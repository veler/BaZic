using BaZic.Core.ComponentModel;
using BaZic.Core.ComponentModel.Reflection;
using BaZic.Core.Enums;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Code.Lexer;
using BaZic.Runtime.BaZic.Code.Lexer.Tokens;
using BaZic.Runtime.BaZic.Code.Optimizer;
using BaZic.Runtime.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Markup;

namespace BaZic.Runtime.BaZic.Code.Parser
{
    /// <summary>
    /// Provides a BaZic code parser
    /// </summary>
    public sealed partial class BaZicParser
    {
        #region Fields & Constants

        private readonly BaZicLexer _lexer = new BaZicLexer();
        private readonly Stack<Token> _tokenStack = new Stack<Token>();
        private readonly Stack<ExpressionGroupSeparator> _expectedExpressionGroupSeparator = new Stack<ExpressionGroupSeparator>();
        private readonly List<List<VariableStatistics>> _declaredVariables = new List<List<VariableStatistics>>();
        private readonly List<ParameterDeclaration> _declaredParameterDeclaration = new List<ParameterDeclaration>();
        private readonly List<MethodDeclaration> _declaredMethods = new List<MethodDeclaration>();
        private readonly List<Event> _declaredEvents = new List<Event>();
        private readonly List<InvokeMethodExpression> _methodInvocations = new List<InvokeMethodExpression>();
        private readonly List<Exception> _issues = new List<Exception>();

        private FastReflection _reflectionHelper;
        private Window _parsedXamlRoot;
        private int _catchIndicator;
        private int _doLoopIndicator;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the previous parsed token.
        /// </summary>
        private Token PreviousToken { get; set; }

        /// <summary>
        /// Gets the current parsed token.
        /// </summary>
        private Token CurrentToken { get; set; }

        /// <summary>
        /// Gets the next parsed token.
        /// </summary>
        private Token NextToken { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Parse a BaZic code and returns a syntax tree representation of the algorithm.
        /// </summary>
        /// <param name="inputCode">The BaZic code to analyze.</param>
        /// <param name="optimize">(optional) Defines whether the generated syntax tree must be optimized for the interpreter or not.</param>
        /// <returns>A <see cref="BaZicProgram"/> that represents the syntax tree that corresponds to the input code.</returns>
        public ParserResult Parse(string inputCode, bool optimize = false)
        {
            return Parse(inputCode, string.Empty, optimize);
        }

        /// <summary>
        /// Parse a BaZic code and returns a syntax tree representation of the algorithm.
        /// </summary>
        /// <param name="inputCode">The BaZic code to analyze.</param>
        /// <param name="xamlCode">The XAML code to analyze that represents the user interface.</param>
        /// <param name="optimize">(optional) Defines whether the generated syntax tree must be optimized for the interpreter or not.</param>
        /// <returns>A <see cref="BaZicProgram"/> that represents the syntax tree that corresponds to the input code.</returns>
        public ParserResult Parse(string inputCode, string xamlCode, bool optimize = false)
        {
            _issues.Clear();

            try
            {
                var tokens = _lexer.Tokenize(inputCode);
                return Parse(tokens, xamlCode, optimize);
            }
            catch (Exception exception)
            {
                _issues.Add(exception);
                return new ParserResult(null, new AggregateException(_issues));
            }
        }

        /// <summary>
        /// Parse a BaZic code and returns a syntax tree representation of the algorithm.
        /// </summary>
        /// <param name="tokens">The BaZic code represented by tokens to analyze.</param>
        /// <param name="xamlCode">The XAML code to analyze that represents the user interface.</param>
        /// <param name="optimize">(optional) Defines whether the generated syntax tree must be optimized for the interpreter or not.</param>
        /// <returns>A <see cref="BaZicProgram"/> that represents the syntax tree that corresponds to the input code.</returns>
        public ParserResult Parse(List<Token> tokens, string xamlCode, bool optimize = false)
        {
            Requires.NotNull(tokens, nameof(tokens));

            _issues.Clear();
            BaZicProgram program = null;

            if (tokens.Count == 0)
            {
                return new ParserResult(program, new AggregateException(_issues));
            }

            try
            {
                _reflectionHelper = new FastReflection();

                // Parse BaZic user interface code. (XAML)
                _parsedXamlRoot = ParseXaml(xamlCode);

                // Parse BaZic code.
                _catchIndicator = 0;
                _doLoopIndicator = 0;
                _tokenStack.Clear();

                for (var i = tokens.Count - 1; i >= 0; i--)
                {
                    _tokenStack.Push(tokens[i]);
                }

                if (_tokenStack.Peek().TokenType != TokenType.StartCode)
                {
                    AddIssue(new BaZicParserException(L.BaZic.Parser.FormattedBadFirstToken(TokenType.StartCode)));
                }

                PreviousToken = _tokenStack.Pop();
                CurrentToken = _tokenStack.Pop();
                NextToken = _tokenStack.Pop();

                program = ParseProgram(xamlCode);

                if (optimize && _issues.OfType<BaZicParserException>().Count(issue => issue.Level == BaZicParserExceptionLevel.Error) == 0)
                {
                    var optimizer = new BaZicOptimizer();
                    program = optimizer.Optimize(program);
                }

                tokens.Clear();
            }
            catch (Exception exception)
            {
                _issues.Add(exception);
            }
            finally
            {
                _expectedExpressionGroupSeparator.Clear();
                _declaredVariables.Clear();
                _declaredParameterDeclaration.Clear();
                _declaredMethods.Clear();
                _declaredEvents.Clear();
                _methodInvocations.Clear();
                _catchIndicator = 0;
                _doLoopIndicator = 0;
                _parsedXamlRoot?.Close();
                _parsedXamlRoot = null;
                _reflectionHelper.Dispose();
                _reflectionHelper = null;
            }

            return new ParserResult(program, new AggregateException(_issues));
        }

        /// <summary>
        /// Add an issue to the list of detected issues in the code.
        /// </summary>
        /// <param name="nodeObject">The object of the syntax tree related to the issue.</param>
        /// <param name="issueLevel">The level of importance of the issue.</param>
        /// <param name="message">The message related to the issue.</param>
        private void AddIssue(NodeObject nodeObject, BaZicParserExceptionLevel issueLevel, string message)
        {
            Requires.NotNull(nodeObject, nameof(nodeObject));
            Requires.NotNullOrWhiteSpace(message, nameof(message));

            if (!_issues.OfType<BaZicParserException>().Any(issue => issue.Line == nodeObject.Line && issue.Column == nodeObject.Column && string.Compare(issue.Message, message, StringComparison.Ordinal) == 0))
            {
                _issues.Add(new BaZicParserException(nodeObject.Line, nodeObject.Column, nodeObject.StartOffset, nodeObject.NodeLength, issueLevel, message));
            }
        }

        /// <summary>
        /// Add an issue to the list of detected issues in the code.
        /// </summary>
        /// <param name="baZicParserException">The error.</param>
        private void AddIssue(BaZicParserException baZicParserException)
        {
            Requires.NotNull(baZicParserException, nameof(baZicParserException));

            _issues.Add(baZicParserException);
        }

        /// <summary>
        /// Discard the current token and switch to the next one.
        /// </summary>
        private void DiscardToken()
        {
            if (CurrentToken != null)
            {
                if (CurrentToken.TokenType == TokenType.LeftBracket)
                {
                    _expectedExpressionGroupSeparator.Push(ExpressionGroupSeparator.RightBracket);
                }
                else if (CurrentToken.TokenType == TokenType.LeftParenth)
                {
                    _expectedExpressionGroupSeparator.Push(ExpressionGroupSeparator.RightParenth);
                }
                else if (CurrentToken.TokenType == TokenType.RightBracket)
                {
                    CheckExpectedExpressionGroupSeparator(ExpressionGroupSeparator.RightBracket);
                }
                else if (CurrentToken.TokenType == TokenType.RightParenth)
                {
                    CheckExpectedExpressionGroupSeparator(ExpressionGroupSeparator.RightParenth);
                }

                PreviousToken = CurrentToken;
            }

            if (NextToken != null)
            {
                CurrentToken = NextToken;

                if (CurrentToken.TokenType == TokenType.NotDefined)
                {
                    AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.UnexpectedOrMissingCharacter));
                }
            }

            if (_tokenStack.Any())
            {
                NextToken = _tokenStack.Pop();
            }
            else
            {
                if (CurrentToken.ParsedLength == 0)
                {
                    NextToken = new Token(TokenType.EndCode, string.Empty, PreviousToken.Line, PreviousToken.Column + PreviousToken.ParsedLength, PreviousToken.StartOffset + +PreviousToken.ParsedLength - 1, 1);
                }
                else
                {
                    NextToken = new Token(TokenType.EndCode, string.Empty, CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength);
                }
            }
        }

        /// <summary>
        /// Discard the current token and switch to the next one.
        /// </summary>
        /// <param name="tokenType">The token type that is expect to be discarded</param>
        private void DiscardToken(TokenType tokenType)
        {
            if (CurrentToken.TokenType != tokenType)
            {
                AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.FormattedUnexpectedToken(tokenType.GetDescription(), CurrentToken.TokenType.GetDescription())));
            }

            DiscardToken();
        }

        /// <summary>
        /// Indicates that a Try Catch has been detected and that the Catch block is actually treated.
        /// </summary>
        private void IncreaseCatchIndicator()
        {
            _catchIndicator++;
        }

        /// <summary>
        /// Indicates that a Try Catch has been detected and that the Catch block has been treated.
        /// </summary>
        private void DecreaseCatchIndicator()
        {
            _catchIndicator--;
        }

        /// <summary>
        /// Retrieves in how many Catch block the parser is actually.
        /// </summary>
        /// <returns>Returns a number that indicates in how many catch block the parser is.</returns>
        private int GetCatchIndicator()
        {
            return _catchIndicator;
        }

        /// <summary>
        /// Indicates that a Do Loop has been detected and is actually treated.
        /// </summary>
        private void IncreaseDoLoopIndicator()
        {
            _doLoopIndicator++;
        }

        /// <summary>
        /// Indicates that a Do Loop has been detected and has been treated.
        /// </summary>
        private void DecreaseDoLoopIndicator()
        {
            _doLoopIndicator--;
        }

        /// <summary>
        /// Retrieves in how many Do Loop block the parser is actually.
        /// </summary>
        /// <returns>Returns a number that indicates in how many Do Loop block the parser is.</returns>
        private int GetDoLoopIndicator()
        {
            return _doLoopIndicator;
        }

        /// <summary>
        /// Set that a new block of code has been created.
        /// </summary>
        private void EnteringScope()
        {
            _declaredVariables.Insert(0, new List<VariableStatistics>());
        }

        /// <summary>
        /// Set that the analyzer is exiting a block of code.
        /// </summary>
        private void ExitingScope()
        {
            foreach (var variable in _declaredVariables[0])
            {
                if (variable.ReferenceCount == 0)
                {
                    AddIssue(variable.Declaration, BaZicParserExceptionLevel.Warning, L.BaZic.Parser.FormattedVariableNeverUsed(variable.Declaration.Name));
                }
            }

            _declaredVariables.RemoveAt(0);
        }

        /// <summary>
        /// Check whether the detected right bracket or right parenthesis match with a previous opened bracket/parenth, and throw a <see cref="BaZicParserException"/> if it is Unexpected.
        /// </summary>
        /// <param name="expressionGroupSeparator">The expected separator (bracket or parenth).</param>
        private void CheckExpectedExpressionGroupSeparator(ExpressionGroupSeparator expressionGroupSeparator)
        {
            if (_expectedExpressionGroupSeparator.Any())
            {
                var separator = _expectedExpressionGroupSeparator.Pop();

                if (separator == expressionGroupSeparator)
                {
                    return;
                }
            }

            AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.UnexpectedRightBracket));
        }

        /// <summary>
        /// Parse the user interface XAML code.
        /// </summary>
        /// <param name="xamlCode">The XAML code to analyze that represents the user interface.</param>
        /// <returns>Returns a <see cref="Window"/> is the parsing succeed. Otherwise, returns null if the XAML code is empty, or an exception if it can't parse it.</returns>
        private Window ParseXaml(string xamlCode)
        {
            if (string.IsNullOrWhiteSpace(xamlCode))
            {
                return null;
            }

            Window result = null;

            try
            {
                result = XamlReader.Parse(xamlCode) as Window;
            }
            catch (Exception exception)
            {
                AddIssue(new BaZicParserException(L.BaZic.Parser.FormattedXamlParsingError(exception.Message)));
            }

            if (result == null)
            {
                AddIssue(new BaZicParserException(L.BaZic.Parser.XamlUnknownParsingError));
            }

            return result;
        }

        /// <summary>
        /// Parse the program's root context.
        /// </summary>
        /// <param name="xamlCode">The XAML code to analyze that represents the user interface.</param>
        /// <returns>A <see cref="BaZicProgram"/> that represents the syntax tree that corresponds to the input code.</returns>
        private BaZicProgram ParseProgram(string xamlCode)
        {
            var variables = new List<VariableDeclaration>();
            var bindings = new List<BindingDeclaration>();
            var methods = new List<MethodDeclaration>();

            var statements = ParseStatements(true, TokenType.EndCode);
            DiscardToken(TokenType.EndCode);

            foreach (var statement in statements)
            {
                switch (statement)
                {
                    case VariableDeclaration variable:
                        ValidateGlobalVariableDeclarationDefaultValue(variable.DefaultValue);
                        variables.Add(variable);
                        break;

                    case BindingDeclaration binding:
                        ValidateGlobalVariableDeclarationDefaultValue(binding.Variable.DefaultValue);
                        bindings.Add(binding);
                        break;

                    case MethodDeclaration method:
                        methods.Add(method);
                        break;

                    default:
                        AddIssue(new BaZicParserException(L.BaZic.Parser.ForbiddenMember));
                        break;
                }
            }

            foreach (var methodInvocation in _methodInvocations)
            {
                ValidateMethodInvocation(methodInvocation);
            }

            if (bindings.Count > 0 || _declaredEvents.Count > 0 || _parsedXamlRoot != null)
            {
                var uiProgram = new BaZicUiProgram();
                uiProgram.Xaml = xamlCode;
                uiProgram.WithUiBindings(bindings.ToArray());
                uiProgram.WithUiEvents(_declaredEvents.ToArray());
                uiProgram.WithVariables(variables.ToArray());
                uiProgram.WithMethods(methods.ToArray());
                return uiProgram;
            }
            else
            {
                var program = new BaZicProgram();
                program.WithVariables(variables.ToArray());
                program.WithMethods(methods.ToArray());
                return program;
            }
        }

        /// <summary>
        /// Register a variable to the current scope (current block of code).
        /// </summary>
        /// <param name="variableDeclaration">The variable to add.</param>
        private void AddVariableToScope(VariableDeclaration variableDeclaration)
        {
            var identicalVariableName = _declaredVariables.SelectMany(var => var).LastOrDefault(variable => string.Compare(variable.Declaration.Name.Identifier, variableDeclaration.Name.Identifier, StringComparison.Ordinal) == 0); // The name is case sensitive.

            if (identicalVariableName != null)
            {
                AddIssue(variableDeclaration, BaZicParserExceptionLevel.Error, L.BaZic.Parser.FormattedDuplicatedVariable(variableDeclaration.Name, identicalVariableName.Declaration.Line));
            }

            var identicalParameterName = _declaredParameterDeclaration.LastOrDefault(variable => string.Compare(variable.Name.Identifier, variableDeclaration.Name.Identifier, StringComparison.Ordinal) == 0); // The name is case sensitive.

            if (identicalParameterName != null)
            {
                AddIssue(variableDeclaration, BaZicParserExceptionLevel.Error, L.BaZic.Parser.FormattedDuplicatedParameter(variableDeclaration.Name, identicalParameterName.Line));
            }

            _declaredVariables.First().Add(new VariableStatistics(variableDeclaration));
        }

        /// <summary>
        /// Add a parameter to the list of parameters accessible in the scope.
        /// </summary>
        /// <param name="parameterDeclaration">The parameter to add.</param>
        private void AddParameter(ParameterDeclaration parameterDeclaration)
        {
            var identicalParameterName = _declaredParameterDeclaration.LastOrDefault(variable => string.Compare(variable.Name.Identifier, parameterDeclaration.Name.Identifier, StringComparison.Ordinal) == 0); // The name is case sensitive.

            if (identicalParameterName != null)
            {
                AddIssue(parameterDeclaration, BaZicParserExceptionLevel.Error, L.BaZic.Parser.FormattedDuplicatedParameter(parameterDeclaration.Name, identicalParameterName.Line));
            }

            _declaredParameterDeclaration.Add(parameterDeclaration);
        }

        /// <summary>
        /// Add a method to the list of method declared in the program.
        /// </summary>
        /// <param name="methodDeclaration">The method to add.</param>
        /// <param name="isEvent">Defines whether the method to add is an EVENT FUNCTION or not.</param>
        private void AddMethod(MethodDeclaration methodDeclaration, bool isEvent)
        {
            var identicalMethodName = _declaredMethods.LastOrDefault(method => string.Compare(method.Name.Identifier, methodDeclaration.Name.Identifier, StringComparison.Ordinal) == 0); // The name is case sensitive.

            if (identicalMethodName != null)
            {
                AddIssue(methodDeclaration, BaZicParserExceptionLevel.Error, L.BaZic.Parser.FormattedDuplicatedMethod(methodDeclaration.Name, identicalMethodName.Line));
            }

            if (isEvent)
            {
                var splittedMethodName = methodDeclaration.Name.Identifier.Split('_');
                var controlName = splittedMethodName[0];
                var eventName = splittedMethodName[1];
                var @event = new Event(controlName, eventName, methodDeclaration.Id);
                @event.Line = methodDeclaration.Line;
                @event.Column = methodDeclaration.Column;
                @event.StartOffset = methodDeclaration.StartOffset;
                @event.NodeLength = methodDeclaration.NodeLength;
                ValidateEvent(@event);
                _declaredEvents.Add(@event);
            }

            _declaredMethods.Add(methodDeclaration);
            _declaredParameterDeclaration.Clear();
        }

        /// <summary>
        /// Add a method invocation expression to the list of method invocation in the program to analyze it later when all the declared method have been discovered.
        /// </summary>
        /// <param name="invokeMethod">The method invocation to add.</param>
        private void AddMethodInvocation(InvokeMethodExpression invokeMethod)
        {
            foreach (var argument in invokeMethod.Arguments)
            {
                AnalyzeExpression(argument);
            }

            _methodInvocations.Add(invokeMethod);
        }

        /// <summary>
        /// Valudate a given expression.
        /// </summary>
        /// <param name="expression">The expression to analyze.</param>
        private void AnalyzeExpression(AbstractSyntaxTree.Expression expression)
        {
            if (expression == null)
            {
                return;
            }

            switch (expression)
            {
                case ArrayIndexerExpression arrayIndexer:
                    ValidateArrayIndexerExpression(arrayIndexer);
                    break;

                case ClassReferenceExpression classReference:
                    break;

                case PropertyReferenceExpression propertyReference:
                    AnalyzeExpression(propertyReference.TargetObject);
                    break;

                case VariableReferenceExpression variableReference:
                    ValidateVariableReferenceExpression(variableReference);
                    break;

                case ExceptionReferenceExpression exception:
                    break;

                case InvokeCoreMethodExpression invokeCoreMethod:
                    ValidateCoreMethodInvocation(invokeCoreMethod);
                    break;

                case InvokeMethodExpression invokeMethod:
                    break;

                case PrimitiveExpression primitive:
                    break;

                case BinaryOperatorExpression binaryOperator:
                    AnalyzeExpression(binaryOperator.LeftExpression);
                    AnalyzeExpression(binaryOperator.RightExpression);
                    break;

                case NotOperatorExpression notOperator:
                    AnalyzeExpression(notOperator.Expression);
                    break;

                case InstantiateExpression instantiate:
                    foreach (var value in instantiate.Arguments)
                    {
                        AnalyzeExpression(value);
                    }
                    break;

                case ArrayCreationExpression arrayCreation:
                    foreach (var value in arrayCreation.Values)
                    {
                        AnalyzeExpression(value);
                    }
                    break;

                default:
                    throw new NotImplementedException(L.BaZic.Parser.FormattedNoExpressionAnalyzer(expression.GetType().FullName));
            }
        }

        /// <summary>
        /// Analyze a method invocation.
        /// </summary>
        /// <param name="invokeMethod">The expression to analyze.</param>
        private void ValidateMethodInvocation(InvokeMethodExpression invokeMethod)
        {
            var methodDeclaration = _declaredMethods.LastOrDefault(method => string.Compare(method.Name.Identifier, invokeMethod.MethodName.Identifier, StringComparison.Ordinal) == 0); // The name is case sensitive.

            if (methodDeclaration == null)
            {
                AddIssue(invokeMethod, BaZicParserExceptionLevel.Error, L.BaZic.Optimizer.FormattedUndeclaredName(invokeMethod.MethodName));
            }
            else
            {
                if (invokeMethod.Await)
                {
                    if (!methodDeclaration.IsAsync)
                    {
                        AddIssue(invokeMethod, BaZicParserExceptionLevel.Error, L.BaZic.Parser.CannotAwait);
                    }
                }

                if (methodDeclaration.Arguments.Count != invokeMethod.Arguments.Count)
                {
                    AddIssue(invokeMethod, BaZicParserExceptionLevel.Error, L.BaZic.Optimizer.FormattedMethodNoMatchArguments(invokeMethod.MethodName, invokeMethod.Arguments.Count));
                }
            }
        }

        /// <summary>
        /// Analyze a method invocation.
        /// </summary>
        /// <param name="invokeMethod">The expression to analyze.</param>
        private void ValidateCoreMethodInvocation(InvokeCoreMethodExpression invokeMethod)
        {
            AnalyzeExpression(invokeMethod.TargetObject);

            foreach (var argument in invokeMethod.Arguments)
            {
                AnalyzeExpression(argument);
            }
        }

        /// <summary>
        /// Analyze a reference to a variable.
        /// </summary>
        /// <param name="variableReference">The reference.</param>
        /// <param name="isArrayExpected">Defines whether an array must be expected or not. A null value means that we don't care.</param>
        /// <param name="throwIssueForVariableNotFound">Defines whether an issue must be added if the variable is not found</param>
        /// <returns>Returns whether the variable is an array or not (if the variable exists).</returns>
        private bool? ValidateVariableReferenceExpression(VariableReferenceExpression variableReference, bool? isArrayExpected = null, bool throwIssueForVariableNotFound = true)
        {
            bool? isArray = null;
            var identicalVariableName = _declaredVariables.SelectMany(var => var).LastOrDefault(variable => string.Compare(variable.Declaration.Name.Identifier, variableReference.Name.Identifier, StringComparison.Ordinal) == 0); // The name is case sensitive.

            if (identicalVariableName == null)
            {
                var identicalParameterName = _declaredParameterDeclaration.LastOrDefault(variable => string.Compare(variable.Name.Identifier, variableReference.Name.Identifier, StringComparison.Ordinal) == 0); // The name is case sensitive.

                if (identicalParameterName == null)
                {
                    if (throwIssueForVariableNotFound)
                    {
                        AddIssue(variableReference, BaZicParserExceptionLevel.Error, L.BaZic.Optimizer.FormattedUndeclaredName(variableReference.Name));
                    }
                }
                else
                {
                    variableReference.VariableDeclarationID = identicalParameterName.Id;
                    isArray = identicalParameterName.IsArray;
                }
            }
            else
            {
                variableReference.VariableDeclarationID = identicalVariableName.Declaration.Id;
                isArray = identicalVariableName.Declaration.IsArray;
                identicalVariableName.IncreaseReference();
            }

            if (isArrayExpected.HasValue && isArray.HasValue)
            {
                if (isArrayExpected.Value)
                {
                    if (!isArray.Value)
                    {
                        AddIssue(variableReference, BaZicParserExceptionLevel.Error, L.BaZic.Parser.FormattedNotArrayVariable(variableReference.Name));
                    }
                }
                else
                {
                    if (isArray.Value)
                    {
                        AddIssue(variableReference, BaZicParserExceptionLevel.Error, L.BaZic.Parser.FormattedArrayVariable(variableReference.Name));
                    }
                }
            }

            return isArray;
        }

        /// <summary>
        /// Analyze an array indexer. 
        /// </summary>
        /// <param name="arrayIndexer">The expression to analyze.</param>
        private void ValidateArrayIndexerExpression(ArrayIndexerExpression arrayIndexer)
        {
            if (arrayIndexer.TargetObject is VariableReferenceExpression variableReference)
            {
                ValidateVariableReferenceExpression(variableReference, true);

                if (arrayIndexer.Indexes.Length != 1)
                {
                    AddIssue(arrayIndexer, BaZicParserExceptionLevel.Error, L.BaZic.Parser.FormattedOneDimensionVariable(variableReference.Name));
                }
            }
            else
            {
                AnalyzeExpression(arrayIndexer.TargetObject);
            }

            foreach (var expression in arrayIndexer.Indexes)
            {
                AnalyzeExpression(expression);
            }
        }

        /// <summary>
        /// Analyze an assignment.
        /// </summary>
        /// <param name="assign">The assignment.</param>
        private void ValidateAssignStatement(AssignStatement assign)
        {
            AnalyzeExpression(assign.LeftExpression);
            AnalyzeExpression(assign.RightExpression);

            if (assign.LeftExpression is VariableReferenceExpression variableReferenceLeft)
            {
                if (assign.RightExpression is VariableReferenceExpression variableReferenceRight)
                {
                    var leftIsArray = ValidateVariableReferenceExpression(variableReferenceLeft);
                    var rightIsArray = ValidateVariableReferenceExpression(variableReferenceRight);

                    if (leftIsArray.HasValue && rightIsArray.HasValue)
                    {
                        if (leftIsArray.Value != rightIsArray.Value)
                        {
                            AddIssue(variableReferenceRight, BaZicParserExceptionLevel.Error, L.BaZic.Parser.ArrayExpectedOnBothSide);
                        }
                    }
                }
                else
                {
                    if (assign.RightExpression is ArrayCreationExpression)
                    {
                        var leftIsArray = ValidateVariableReferenceExpression(variableReferenceLeft);
                        if (leftIsArray.HasValue && !leftIsArray.Value)
                        {
                            AddIssue(variableReferenceLeft, BaZicParserExceptionLevel.Error, L.BaZic.Parser.ArrayAssign);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check that a variable declared in the root of a BaZic program is and only is an expression that contains only primitive values.
        /// </summary>
        /// <param name="expression">The expression to check,</param>
        private void ValidateGlobalVariableDeclarationDefaultValue(AbstractSyntaxTree.Expression expression)
        {
            if (expression == null)
            {
                return;
            }

            switch (expression)
            {
                case ArrayCreationExpression arrayCreation:
                    foreach (var value in arrayCreation.Values)
                    {
                        ValidateGlobalVariableDeclarationDefaultValue(value);
                    }
                    break;

                case ArrayIndexerExpression arrayIndexer:
                    ValidateGlobalVariableDeclarationDefaultValue(arrayIndexer.TargetObject);
                    break;

                case BinaryOperatorExpression binaryOperator:
                    ValidateGlobalVariableDeclarationDefaultValue(binaryOperator.LeftExpression);
                    ValidateGlobalVariableDeclarationDefaultValue(binaryOperator.RightExpression);
                    break;

                case NotOperatorExpression notOperator:
                    ValidateGlobalVariableDeclarationDefaultValue(notOperator.Expression);
                    break;

                case PrimitiveExpression primitive:
                    // It's OK.
                    break;

                default:
                    AddIssue(new BaZicParserException(expression.Line, expression.Column, expression.StartOffset, expression.NodeLength, L.BaZic.Parser.VariableDefaultValue));
                    break;
            }
        }

        /// <summary>
        /// Check that a UI Binding is correct and that the requested control property exists in the XAML.
        /// </summary>
        /// <param name="binding">The binding to check.</param>
        private void ValidateUiBinding(BindingDeclaration binding)
        {
            if (_parsedXamlRoot == null)
            {
                AddIssue(new BaZicParserException(binding.Line, binding.Column, binding.StartOffset, binding.NodeLength, L.BaZic.Parser.NoXamlBinding));
                return;
            }

            var control = _parsedXamlRoot.FindName(binding.ControlName) as UIElement;

            if (control == null)
            {
                AddIssue(new BaZicParserException(binding.Line, binding.Column, binding.StartOffset, binding.NodeLength, L.BaZic.Parser.FormattedXamlControlNotFoundBinding(binding.ControlName)));
                return;
            }

            if (!_reflectionHelper.PropertyHasGetterAndSetter(control, binding.ControlPropertyName))
            {
                AddIssue(new BaZicParserException(binding.Line, binding.Column, binding.StartOffset, binding.NodeLength, L.BaZic.Parser.FormattedNotAccessibleProperty(binding.Variable.Name, binding.ControlName, binding.ControlPropertyName)));
            }
        }

        /// <summary>
        /// Check that a UI Binding is correct and that the requested control property exists in the XAML.
        /// </summary>
        /// <param name="event">The binding to check.</param>
        private void ValidateEvent(Event @event)
        {
            if (_parsedXamlRoot == null)
            {
                AddIssue(new BaZicParserException(@event.Line, @event.Column, @event.StartOffset, @event.NodeLength, L.BaZic.Parser.NoXamlEvent));
                return;
            }

            var control = _parsedXamlRoot.FindName(@event.ControlName) as UIElement;

            if (control == null)
            {
                AddIssue(new BaZicParserException(@event.Line, @event.Column, @event.StartOffset, @event.NodeLength, L.BaZic.Parser.FormattedXamlControlNotFoundEvent(@event.ControlName)));
                return;
            }

            if (control.GetType().GetEvent(@event.ControlEventName, Consts.LimitedBindingFlags) == null)
            {
                AddIssue(new BaZicParserException(@event.Line, @event.Column, @event.StartOffset, @event.NodeLength, L.BaZic.Parser.FormattedNotAccessibleEvent(@event.ControlName, @event.ControlEventName)));
            }
        }

        #endregion
    }
}