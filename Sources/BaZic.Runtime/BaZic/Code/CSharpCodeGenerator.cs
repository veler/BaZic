using BaZic.Core.ComponentModel;
using BaZic.Core.Enums;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;

namespace BaZic.Runtime.BaZic.Code
{
    /// <summary>
    /// Provides a CSharp code generator
    /// </summary>
    public class CSharpCodeGenerator
    {
        #region Fields & Constants

        private int _indentSpaceCount;
        private bool _currentMethodIsAsync;
        private bool _currentProgramHasUi;
        private StringBuilder _uiLoadingStatements;
        private IReadOnlyList<MethodDeclaration> _methodDeclarations;
        private Window _userInterface;

        #endregion

        #region Methods

        /// <summary>
        /// Generates a BaZic code from a syntax tree.
        /// </summary>
        /// <param name="syntaxTree">The syntax tree that represents the algorithm</param>
        /// <returns>A CSharp code</returns>
        public string Generate(BaZicProgram syntaxTree)
        {
            if (syntaxTree is BaZicUiProgram)
            {
                return Generate((BaZicUiProgram)syntaxTree);
            }

            _indentSpaceCount = 0;
            _currentProgramHasUi = false;
            _currentMethodIsAsync = false;
            _userInterface = null;
            _methodDeclarations = syntaxTree.Methods;

            IncreaseIndent();
            var indent = IncreaseIndent();

            var globalVariables = new List<string>();
            foreach (var variable in syntaxTree.GlobalVariables)
            {
                globalVariables.Add(indent + GenerateVariableDeclaration(variable, true, true));
            }

            var globalVariablesString = string.Join(Environment.NewLine, globalVariables);
            if (!string.IsNullOrWhiteSpace(globalVariablesString))
            {
                globalVariablesString += Environment.NewLine + Environment.NewLine;
            }

            var methods = new List<string>();
            foreach (var method in syntaxTree.Methods)
            {
                methods.Add(GenerateMethodDeclaration(method));
            }

            var methodsString = string.Join(Environment.NewLine + Environment.NewLine, methods);

            DecreaseIndent();
            DecreaseIndent();

            var csharpCodeGeneratorHelper = new StreamReader(typeof(CSharpCodeGenerator).Assembly.GetManifestResourceStream("BaZic.Runtime.Resources.CSharpCodeGeneratorHelper.cs"));
            var observableConcurrentDictionary = new StreamReader(typeof(CSharpCodeGenerator).Assembly.GetManifestResourceStream("BaZic.Runtime.Resources.ObservableDictionary.cs"));

            return $"// CSharp code generated automatically" + Environment.NewLine + Environment.NewLine +
                   $"namespace BaZicProgramReleaseMode" + Environment.NewLine +
                   $"{{" + Environment.NewLine +
                   $"    [System.Serializable]" + Environment.NewLine +
                   $"    public static class Program" + Environment.NewLine +
                   $"    {{" + Environment.NewLine +
                   $"{globalVariablesString}" +
                   $"{methodsString}" + Environment.NewLine +
                   $"    }}" + Environment.NewLine +
                   $"}}" + Environment.NewLine + Environment.NewLine +
                   csharpCodeGeneratorHelper.ReadToEnd() + Environment.NewLine + Environment.NewLine +
                   observableConcurrentDictionary.ReadToEnd();
        }

        /// <summary>
        /// Generates a BaZic code with UI from a syntax tree.
        /// </summary>
        /// <param name="syntaxTree">The syntax tree that represents the algorithm</param>
        /// <returns>A CSharp code</returns>
        public string Generate(BaZicUiProgram syntaxTree)
        {
            _indentSpaceCount = 0;
            _currentProgramHasUi = true;
            _currentMethodIsAsync = false;
            _uiLoadingStatements = new StringBuilder();
            _methodDeclarations = syntaxTree.Methods;

            var methodsString = string.Empty;
            var bindingsString = string.Empty;
            var globalVariablesString = string.Empty;

            ThreadHelper.RunOnStaThread(() =>
            {
                try
                {
                    _userInterface = XamlReader.Parse(syntaxTree.Xaml) as Window;
                }
                catch (Exception exception)
                {
                    throw new BaZicParserException(L.BaZic.Parser.FormattedXamlParsingError(exception.Message));
                }

                if (_userInterface == null)
                {
                    throw new BaZicParserException(L.BaZic.Parser.XamlUnknownParsingError);
                }

                IncreaseIndent();
                var indent = IncreaseIndent();

                var globalVariables = new List<string>();
                foreach (var variable in syntaxTree.GlobalVariables)
                {
                    globalVariables.Add(indent + GenerateVariableDeclaration(variable, true, true));
                }

                globalVariablesString = string.Join(Environment.NewLine, globalVariables);
                if (!string.IsNullOrWhiteSpace(globalVariablesString))
                {
                    globalVariablesString += Environment.NewLine + Environment.NewLine;
                }

                var bindings = new List<string>();
                foreach (var binding in syntaxTree.UiBindings)
                {
                    bindings.Add(GenerateBindingDeclaration(binding, indent));
                }

                bindingsString = string.Join(Environment.NewLine, bindings);
                if (!string.IsNullOrWhiteSpace(bindingsString))
                {
                    bindingsString += Environment.NewLine + Environment.NewLine;
                }

                var methods = new List<string>();
                foreach (var method in syntaxTree.Methods.OrderBy(m => m.GetType() == typeof(EntryPointMethod)))
                {
                    methods.Add(GenerateMethodDeclaration(method, syntaxTree.UiEvents.SingleOrDefault(e => e.MethodId == method.Id)));
                }

                methodsString = string.Join(Environment.NewLine + Environment.NewLine, methods);

                DecreaseIndent();
                DecreaseIndent();

                if (_userInterface != null)
                {
                    _userInterface.Close();
                    _userInterface = null;
                }
            });

            var csharpCodeGeneratorHelper = new StreamReader(typeof(CSharpCodeGenerator).Assembly.GetManifestResourceStream("BaZic.Runtime.Resources.CSharpCodeGeneratorHelper.cs"));
            var csharpCodeGeneratorUiHelper = new StreamReader(typeof(CSharpCodeGenerator).Assembly.GetManifestResourceStream("BaZic.Runtime.Resources.CSharpCodeGeneratorUiHelper.cs"));
            var observableConcurrentDictionary = new StreamReader(typeof(CSharpCodeGenerator).Assembly.GetManifestResourceStream("BaZic.Runtime.Resources.ObservableDictionary.cs"));

            return $"// CSharp code generated automatically" + Environment.NewLine + Environment.NewLine +
                   $"namespace BaZicProgramReleaseMode" + Environment.NewLine +
                   $"{{" + Environment.NewLine +
                   $"    [System.Serializable]" + Environment.NewLine +
                   $"    public static class Program" + Environment.NewLine +
                   $"    {{" + Environment.NewLine +
                   $"{globalVariablesString}" +
                   $"{bindingsString}" +
                   $"        static Program()" + Environment.NewLine +
                   $"        {{" + Environment.NewLine +
                   $"            ProgramHelper.CreateNewInstance();" + Environment.NewLine +
                   $"        }}" + Environment.NewLine + Environment.NewLine +
                   $"{methodsString}" + Environment.NewLine +
                   $"    }}" + Environment.NewLine +
                   $"}}" + Environment.NewLine + Environment.NewLine +
                   csharpCodeGeneratorHelper.ReadToEnd() + Environment.NewLine + Environment.NewLine +
                   csharpCodeGeneratorUiHelper.ReadToEnd().Replace("{XAMLCode}", FormatLiteralString(syntaxTree.Xaml)) + Environment.NewLine + Environment.NewLine +
                   observableConcurrentDictionary.ReadToEnd();
        }

        /// <summary>
        /// Generates the code for an <see cref="Expression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A CSharp code</returns>
        private string GenerateExpression(AbstractSyntaxTree.Expression expression)
        {
            if (expression == null)
            {
                return string.Empty;
            }

            switch (expression)
            {
                case ReferenceExpression reference:
                    return GenerateReferenceExpression(reference);

                case BinaryOperatorExpression binaryOperator:
                    return GenerateBinaryOperatorExpression(binaryOperator);

                case NotOperatorExpression notOperator:
                    return GenerateNotOperatorExpression(notOperator);

                default:
                    throw new NotImplementedException(L.BaZic.BaZicCodeGenerator.FormattedNoGeneratorForExpressionImplemented(expression.GetType().FullName));
            }
        }

        /// <summary>
        /// Generates the code for a <see cref="ReferenceExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A CSharp code</returns>
        private string GenerateReferenceExpression(ReferenceExpression expression)
        {
            switch (expression)
            {
                case ArrayCreationExpression arrayCreation:
                    return GenerateArrayCreationExpression(arrayCreation);

                case ArrayIndexerExpression arrayIndexer:
                    return GenerateArrayIndexerExpression(arrayIndexer);

                case ClassReferenceExpression classReference:
                    return GenerateClassReferenceExpression(classReference);

                case PropertyReferenceExpression propertyReference:
                    return GeneratePropertyReferenceExpression(propertyReference);

                case VariableReferenceExpression variableReference:
                    return GenerateVariableReferenceExpression(variableReference);

                case ExceptionReferenceExpression exception:
                    return GenerateExceptionReferenceExpression(exception);

                case InstantiateExpression instantiate:
                    return GenerateInstantiateExpression(instantiate);

                case InvokeCoreMethodExpression invokeCoreMethod:
                    return GenerateInvokeCoreMethodExpression(invokeCoreMethod);

                case InvokeMethodExpression invokeMethod:
                    return GenerateInvokeMethodExpression(invokeMethod);

                case PrimitiveExpression primitive:
                    return GeneratePrimitiveExpression(primitive);

                default:
                    throw new NotImplementedException(L.BaZic.BaZicCodeGenerator.FormattedNoGeneratorForExpressionImplemented(expression.GetType().FullName));
            }
        }

        /// <summary>
        /// Generates the code for a <see cref="Statement"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A CSharp code</returns>
        private string GenerateStatement(Statement statement)
        {
            switch (statement)
            {
                case CommentStatement comment:
                    return GenerateCommentStatement(comment);

                case LabelDeclaration label:
                    return GenerateLabelDeclaration(label);

                case GoToLabelStatement gotoLabel:
                    return GenerateGoToLabelStatement(gotoLabel);

                case LabelConditionStatement labelCondition:
                    return GenerateLabelConditionStatement(labelCondition);

                case AssignStatement assign:
                    return GenerateAssignStatement(assign);

                case ConditionStatement condition:
                    return GenerateConditionStatement(condition);

                case IterationStatement iteration:
                    return GenerateIterationStatement(iteration);

                case TryCatchStatement tryCatch:
                    return GenerateTryCatchStatement(tryCatch);

                case VariableDeclaration variable:
                    return GenerateVariableDeclaration(variable, false, false);

                case ReturnStatement @return:
                    return GenerateReturnStatement(@return);

                case ThrowStatement @throw:
                    return GenerateThrowStatement(@throw);

                case BreakStatement @break:
                    return GenerateBreakStatement();

                case BreakpointStatement breakpoint:
                    return GenerateCommentStatement(new CommentStatement("Ignored breakpoint."));

                case ExpressionStatement expression:
                    return GenerateExpressionStatement(expression);

                default:
                    throw new NotImplementedException(L.BaZic.BaZicCodeGenerator.FormattedNoGeneratorForStatementImplemented(statement.GetType().FullName));
            }
        }

        /// <summary>
        /// Generates the code for an <see cref="ArrayIndexerExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A CSharp code</returns>
        private string GenerateArrayIndexerExpression(ArrayIndexerExpression expression)
        {
            Requires.NotNull(expression.TargetObject, nameof(expression.TargetObject));
            Requires.NotNull(expression.Indexes, nameof(expression.Indexes));

            var targetObject = GenerateReferenceExpression(expression.TargetObject);
            var indexes = new List<string>();
            foreach (var index in expression.Indexes)
            {
                indexes.Add(GenerateExpression(index));
            }

            return $"{targetObject}[{string.Join(", ", indexes)}]";
        }

        /// <summary>
        /// Generates the code for an <see cref="ClassReferenceExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A CSharp code</returns>
        private string GenerateClassReferenceExpression(ClassReferenceExpression expression)
        {
            return $"{expression.Namespace}.{expression.ClassName}";
        }

        /// <summary>
        /// Generates the code for an <see cref="PropertyReferenceExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A CSharp code</returns>
        private string GeneratePropertyReferenceExpression(PropertyReferenceExpression expression)
        {
            Requires.NotNull(expression.TargetObject, nameof(expression.TargetObject));

            var targetObject = GenerateReferenceExpression(expression.TargetObject);

            return $"{targetObject}.{expression.PropertyName}";
        }

        /// <summary>
        /// Generates the code for an <see cref="VariableReferenceExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A CSharp code</returns>
        private string GenerateVariableReferenceExpression(VariableReferenceExpression expression)
        {
            Requires.NotNull(expression.Name, nameof(expression.Name));

            return expression.Name.ToString();
        }

        /// <summary>
        /// Generates the code for an <see cref="ExceptionReferenceExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A CSharp code</returns>
        private string GenerateExceptionReferenceExpression(ExceptionReferenceExpression expression)
        {
            return "EXCEPTION";
        }

        /// <summary>
        /// Generates the code for an <see cref="BinaryOperatorExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A CSharp code</returns>
        private string GenerateBinaryOperatorExpression(BinaryOperatorExpression expression)
        {
            Requires.NotNull(expression.LeftExpression, nameof(expression.LeftExpression));
            Requires.NotNull(expression.RightExpression, nameof(expression.RightExpression));

            var leftExpression = GenerateExpression(expression.LeftExpression);
            if (expression.LeftExpression is BinaryOperatorExpression || expression.LeftExpression is NotOperatorExpression)
            {
                leftExpression = $"({leftExpression})";
            }

            var rightExpression = GenerateExpression(expression.RightExpression);
            if (expression.RightExpression is BinaryOperatorExpression || expression.RightExpression is NotOperatorExpression)
            {
                rightExpression = $"({rightExpression})";
            }

            var @operator = string.Empty;
            switch (expression.Operator)
            {
                case BinaryOperatorType.Equality:
                    @operator = "==";
                    break;

                case BinaryOperatorType.BitwiseOr:
                    @operator = "|";
                    break;

                case BinaryOperatorType.BitwiseAnd:
                    @operator = "&";
                    break;

                case BinaryOperatorType.LogicalOr:
                    @operator = "||";
                    break;

                case BinaryOperatorType.LogicalAnd:
                    @operator = "&&";
                    break;

                case BinaryOperatorType.LessThan:
                    @operator = "<";
                    break;

                case BinaryOperatorType.LessThanOrEqual:
                    @operator = "<=";
                    break;

                case BinaryOperatorType.GreaterThan:
                    @operator = ">";
                    break;

                case BinaryOperatorType.GreaterThanOrEqual:
                    @operator = ">=";
                    break;

                case BinaryOperatorType.Addition:
                    @operator = "+";
                    break;

                case BinaryOperatorType.Subtraction:
                    @operator = "-";
                    break;

                case BinaryOperatorType.Multiply:
                    @operator = "*";
                    break;

                case BinaryOperatorType.Division:
                    @operator = "/";
                    break;

                case BinaryOperatorType.Modulus:
                    @operator = "%";
                    break;

                default:
                    throw new NotImplementedException(L.BaZic.BaZicCodeGenerator.FormattedNoGeneratorForOperatorImplemented(expression.Operator.GetDescription()));
            }

            return $"{leftExpression} {@operator} {rightExpression}";
        }

        /// <summary>
        /// Generates the code for an <see cref="NotOperatorExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A CSharp code</returns>
        private string GenerateNotOperatorExpression(NotOperatorExpression expression)
        {
            Requires.NotNull(expression.Expression, nameof(expression.Expression));

            var expressionString = GenerateExpression(expression.Expression);
            if (expression.Expression is BinaryOperatorExpression || expression.Expression is NotOperatorExpression)
            {
                return $"!({expressionString})";
            }

            return $"!{expressionString}";
        }

        /// <summary>
        /// Generates the code for an <see cref="InstantiateExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A CSharp code</returns>
        private string GenerateInstantiateExpression(InstantiateExpression expression)
        {
            Requires.NotNull(expression.CreateType, nameof(expression.CreateType));

            var type = GenerateClassReferenceExpression(expression.CreateType);
            var arguments = new List<string>();

            foreach (var argument in expression.Arguments)
            {
                arguments.Add(GenerateExpression(argument));
            }

            return $"new {type}({string.Join(", ", arguments)})";
        }

        /// <summary>
        /// Generates the code for an <see cref="PrimitiveExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A CSharp code</returns>
        private string GeneratePrimitiveExpression(PrimitiveExpression expression)
        {
            return GenerateObjectStringRepresentation(expression.Value);
        }

        /// <summary>
        /// Generates the code for an <see cref="ArrayCreationExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A CSharp code</returns>
        private string GenerateArrayCreationExpression(ArrayCreationExpression expression)
        {
            var values = new List<string>();
            foreach (var item in expression.Values)
            {
                values.Add(GenerateExpression(item));
            }

            return $"new BaZicProgramReleaseMode.ObservableDictionary() {{ {string.Join(", ", values)} }}";
        }

        /// <summary>
        /// Generates a <see cref="String"/> representation of an <see cref="Object"/>
        /// </summary>
        /// <param name="value">The value to convert to string</param>
        /// <returns>A string representation of the value</returns>
        private string GenerateObjectStringRepresentation(object value)
        {
            if (value == null)
            {
                return "null";
            }

            switch (value)
            {
                case bool typedValue:
                    return typedValue.ToString().ToLowerInvariant();

                case string typedValue:
                    return $"\"{FormatLiteralString(typedValue)}\"";

                case Array typedValue:
                    var values = new List<string>();
                    foreach (var item in typedValue)
                    {
                        values.Add(GenerateObjectStringRepresentation(item));
                    }

                    return $"new BaZicProgramReleaseMode.ObservableDictionary() {{ {string.Join(", ", values)} }}";

                default:
                    return value.ToString();
            }
        }

        /// <summary>
        /// Generates the code for an <see cref="InvokeMethodExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A CSharp code</returns>
        private string GenerateInvokeMethodExpression(InvokeMethodExpression expression)
        {
            Requires.NotNull(expression.MethodName, nameof(expression.MethodName));

            var arguments = new List<string>();

            foreach (var argument in expression.Arguments)
            {
                arguments.Add(GenerateExpression(argument));
            }

            if (expression.Await)
            {
                if (_currentMethodIsAsync)
                {
                    return $"await {expression.MethodName}({string.Join(", ", arguments)})";
                }

                return $"{expression.MethodName}({string.Join(", ", arguments)}).GetAwaiter().GetResult()";
            }

            var declaration = _methodDeclarations.SingleOrDefault(m => string.Compare(m.Name.Identifier, expression.MethodName.Identifier, StringComparison.Ordinal) == 0 && m.Arguments.Count == expression.Arguments.Count);

            if (declaration == null)
            {
                throw new BaZicParserException($"Unable to find a method called '{expression.MethodName}' with {expression.Arguments.Count} argument(s).");
            }

            if (declaration.IsAsync)
            {
                return $"ProgramHelper.AddUnwaitedThread({expression.MethodName}({string.Join(", ", arguments)}))";
            }

            return $"{expression.MethodName}({string.Join(", ", arguments)})";
        }

        /// <summary>
        /// Generates the code for an <see cref="InvokeCoreMethodExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A CSharp code</returns>
        private string GenerateInvokeCoreMethodExpression(InvokeCoreMethodExpression expression)
        {
            Requires.NotNull(expression.TargetObject, nameof(expression.TargetObject));
            Requires.NotNull(expression.MethodName, nameof(expression.MethodName));

            var targetObject = GenerateReferenceExpression(expression.TargetObject);

            var arguments = new List<string>();
            foreach (var argument in expression.Arguments)
            {
                arguments.Add(GenerateExpression(argument));
            }

            var argumentStrings = string.Join(", ", arguments);

            if (expression.Await)
            {
                if (_currentMethodIsAsync)
                {
                    return $"await ProgramHelper.RunTask((System.Threading.Tasks.Task){targetObject}.{expression.MethodName}({argumentStrings}))";
                }

                return $"ProgramHelper.RunTaskSynchronously((System.Threading.Tasks.Task){targetObject}.{expression.MethodName}({argumentStrings}))";
            }

            if (!string.IsNullOrEmpty(argumentStrings))
            {
                argumentStrings = $", {argumentStrings}";
            }

            if (expression.TargetObject is ClassReferenceExpression)
            {
                // static method
                return $"ProgramHelper.AddUnwaitedThreadIfRequired(typeof({targetObject}), \"{expression.MethodName}\"{argumentStrings})";
            }

            return $"ProgramHelper.AddUnwaitedThreadIfRequired({targetObject}, \"{expression.MethodName}\"{argumentStrings})";
        }

        /// <summary>
        /// Generates the code for a <see cref="CommentStatement"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A CSharp code</returns>
        private string GenerateCommentStatement(CommentStatement statement)
        {
            return $"// {statement.Comment}";
        }

        /// <summary>
        /// Generates the code for a <see cref="LabelDeclaration"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A CSharp code</returns>
        private string GenerateLabelDeclaration(LabelDeclaration statement)
        {
            return $"{statement.Name}:";
        }

        /// <summary>
        /// Generates the code for a <see cref="GoToLabelStatement"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A CSharp code</returns>
        private string GenerateGoToLabelStatement(GoToLabelStatement statement)
        {
            return $"goto {statement.Name};";
        }

        /// <summary>
        /// Generates the code for a <see cref="LabelConditionStatement"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A CSharp code</returns>
        private string GenerateLabelConditionStatement(LabelConditionStatement statement)
        {
            Requires.NotNull(statement.Condition, nameof(statement.Condition));

            var condition = GenerateExpression(statement.Condition);
            return $"if ({condition}) goto {statement.LabelName};";
        }

        /// <summary>
        /// Generates the code for a <see cref="AssignStatement"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A CSharp code</returns>
        private string GenerateAssignStatement(AssignStatement statement)
        {
            Requires.NotNull(statement.LeftExpression, nameof(statement.LeftExpression));
            Requires.NotNull(statement.RightExpression, nameof(statement.RightExpression));

            var leftExpression = GenerateExpression(statement.LeftExpression);
            var rightExpression = GenerateExpression(statement.RightExpression);

            return $"{leftExpression} = {rightExpression};";
        }

        /// <summary>
        /// Generates the code for a <see cref="ConditionStatement"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A CSharp code</returns>
        private string GenerateConditionStatement(ConditionStatement statement)
        {
            Requires.NotNull(statement.Condition, nameof(statement.Condition));

            var condition = GenerateExpression(statement.Condition);

            var indent = IncreaseIndent();

            var trueStatements = new List<string>();
            foreach (var stmt in statement.TrueStatements)
            {
                trueStatements.Add($"{indent}{GenerateStatement(stmt)}");
            }

            var trueStatementsString = string.Join(Environment.NewLine, trueStatements);

            if (statement.FalseStatements.Count > 0)
            {
                var falseStatements = new List<string>();
                foreach (var stmt in statement.FalseStatements)
                {
                    falseStatements.Add($"{indent}{GenerateStatement(stmt)}");
                }

                var falseStatementsString = string.Join(Environment.NewLine, falseStatements);

                indent = DecreaseIndent();

                return $"if ({condition})" + Environment.NewLine +
                       $"{indent}{{" + Environment.NewLine +
                       $"{trueStatementsString}" + Environment.NewLine +
                       $"{indent}}}" + Environment.NewLine +
                       $"{indent}else" + Environment.NewLine +
                       $"{indent}{{" + Environment.NewLine +
                       $"{falseStatementsString}" + Environment.NewLine +
                       $"{indent}}}";
            }

            indent = DecreaseIndent();

            return $"if ({condition})" + Environment.NewLine +
                   $"{indent}{{" + Environment.NewLine +
                   $"{trueStatementsString}" + Environment.NewLine +
                   $"{indent}}}";
        }

        /// <summary>
        /// Generates the code for a <see cref="IterationStatement"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A CSharp code</returns>
        private string GenerateIterationStatement(IterationStatement statement)
        {
            Requires.NotNull(statement.Condition, nameof(statement.Condition));

            var condition = GenerateExpression(statement.Condition);

            var indent = IncreaseIndent();

            var statements = new List<string>();
            foreach (var stmt in statement.Statements)
            {
                statements.Add($"{indent}{GenerateStatement(stmt)}");
            }

            var statementsString = string.Join(Environment.NewLine, statements);

            indent = DecreaseIndent();

            if (statement.ConditionAfterBody)
            {
                return $"do" + Environment.NewLine +
                       $"{indent}{{" + Environment.NewLine +
                       $"{statementsString}" + Environment.NewLine +
                       $"{indent}}}" + Environment.NewLine +
                       $"{indent}while ({condition});";
            }
            else
            {
                return $"while ({condition})" + Environment.NewLine +
                       $"{indent}{{" + Environment.NewLine +
                       $"{statementsString}" + Environment.NewLine +
                       $"{indent}}}";
            }
        }

        /// <summary>
        /// Generates the code for a <see cref="TryCatchStatement"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A CSharp code</returns>
        private string GenerateTryCatchStatement(TryCatchStatement statement)
        {
            var indent = IncreaseIndent();

            var tryStatements = new List<string>();
            foreach (var stmt in statement.TryStatements)
            {
                tryStatements.Add($"{indent}{GenerateStatement(stmt)}");
            }

            var tryStatementsString = string.Join(Environment.NewLine, tryStatements);

            if (statement.CatchStatements.Count > 0)
            {
                var catchStatements = new List<string>();
                foreach (var stmt in statement.CatchStatements)
                {
                    catchStatements.Add($"{indent}{GenerateStatement(stmt)}");
                }

                var catchStatementsString = string.Join(Environment.NewLine, catchStatements);

                indent = DecreaseIndent();

                return $"try" + Environment.NewLine +
                       $"{indent}{{" + Environment.NewLine +
                       $"{tryStatementsString}" + Environment.NewLine +
                       $"{indent}}}" + Environment.NewLine +
                       $"{indent}catch (System.Exception EXCEPTION)" + Environment.NewLine +
                       $"{indent}{{" + Environment.NewLine +
                       $"{catchStatementsString}" + Environment.NewLine +
                       $"{indent}}}";
            }

            indent = DecreaseIndent();

            return $"try" + Environment.NewLine +
                   $"{indent}{{" + Environment.NewLine +
                   $"{tryStatementsString}" + Environment.NewLine +
                   $"{indent}}}" + Environment.NewLine +
                   $"catch {{}}";
        }

        /// <summary>
        /// Generates the code for a <see cref="VariableDeclaration"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <param name="isGlobal">Defines whether it is a global variable or not.</param>
        /// <param name="isPrivate">Definew whether the variable is private.</param>
        /// <returns>A CSharp code</returns>
        private string GenerateVariableDeclaration(VariableDeclaration statement, bool isGlobal, bool isPrivate)
        {
            var accessor = string.Empty;
            if (isPrivate)
            {
                accessor += "private ";
            }
            if (isGlobal)
            {
                accessor += "static ";
            }

            if (statement.DefaultValue == null)
            {
                return $"{accessor}dynamic {statement.Name} = null;";
            }

            var defaultValue = GenerateExpression(statement.DefaultValue);
            return $"{accessor}dynamic {statement.Name} = {defaultValue};";
        }

        /// <summary>
        /// Generates the code for a <see cref="BindingDeclaration"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <param name="indent">The indentation</param>
        /// <returns>A CSharp code</returns>
        private string GenerateBindingDeclaration(BindingDeclaration statement, string indent)
        {
            var arrayMarkup = statement.Variable.IsArray ? "[]" : string.Empty;

            if (statement.Variable.DefaultValue != null)
            {
                var defaultValue = GenerateExpression(statement.Variable.DefaultValue);
                _uiLoadingStatements.AppendLine($"            {statement.ControlName}_{statement.ControlPropertyName} = {defaultValue};");
            }

            return $@"{indent}private static dynamic {statement.ControlName}_{statement.ControlPropertyName}
{indent}{{ 
{indent}    get {{
{indent}        dynamic result = ProgramHelper.UIDispatcher.Invoke(() => {{
{indent}            return ProgramHelper.Instance.GetControl(""{statement.ControlName}"")?.{statement.ControlPropertyName};
{indent}        }}, System.Windows.Threading.DispatcherPriority.Background);
{indent}        return result;
{indent}    }}
{indent}    set
{indent}    {{
{indent}        ProgramHelper.UIDispatcher.Invoke(() => {{
{indent}            ProgramHelper.Instance.GetControl(""{statement.ControlName}"").{statement.ControlPropertyName} = value;
{indent}        }}, System.Windows.Threading.DispatcherPriority.Background);
{indent}    }}
{indent}}}";
        }

        /// <summary>
        /// Generates the code for a <see cref="ReturnStatement"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A CSharp code</returns>
        private string GenerateReturnStatement(ReturnStatement statement)
        {
            return $"return {GenerateExpression(statement.Expression)};";
        }

        /// <summary>
        /// Generates the code for a <see cref="ThrowStatement"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A CSharp code</returns>
        private string GenerateThrowStatement(ThrowStatement statement)
        {
            return $"throw {GenerateExpression(statement.Expression)};";
        }

        /// <summary>
        /// Generates the code for a <see cref="BreakStatement"/>.
        /// </summary>
        /// <returns>A CSharp code</returns>
        private string GenerateBreakStatement()
        {
            return "break;";
        }

        /// <summary>
        /// Generates the code for a <see cref="ExpressionStatement"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A CSharp code</returns>
        private string GenerateExpressionStatement(ExpressionStatement statement)
        {
            return GenerateExpression(statement.Expression) + ";";
        }

        /// <summary>
        /// Generates the code for a <see cref="MethodDeclaration"/>.
        /// </summary>
        /// <param name="method">The method declaration</param>
        /// <param name="uiEvent">(optional) The UI event linked to the method.</param>
        /// <returns>A CSharp code</returns>
        private string GenerateMethodDeclaration(MethodDeclaration method, Event uiEvent = null)
        {
            Requires.NotNull(method.Name, nameof(method.Name));

            _currentMethodIsAsync = method.IsAsync;
            var indent = IncreaseIndent();

            var statements = new List<string>();
            foreach (var stmt in method.Statements)
            {
                statements.Add($"{indent}{GenerateStatement(stmt)}");
            }

            var oldIdent = DecreaseIndent();

            var statementsString = string.Join(Environment.NewLine, statements);

            var arguments = new List<string>();
            foreach (var argument in method.Arguments)
            {
                arguments.Add(GenerateParameterDeclaration(argument));
            }

            var accessor = "internal";
            if (method.IsExtern)
            {
                accessor = "public";
            }

            if (method.IsAsync)
            {
                return $"{oldIdent}{accessor} static async System.Threading.Tasks.Task<dynamic> {method.Name}({string.Join(", ", arguments)})" + Environment.NewLine +
                       $"{oldIdent}{{" + Environment.NewLine +
                       $"{statementsString}" + Environment.NewLine +
                       $"{indent}return await System.Threading.Tasks.Task.FromResult<object>(null);" + Environment.NewLine +
                       $"{oldIdent}}}";
            }

            if (method is EntryPointMethod)
            {
                if (_currentProgramHasUi)
                {
                    return $"{oldIdent}{accessor} static dynamic {method.Name}({string.Join(", ", arguments)})" + Environment.NewLine +
                           $"{oldIdent}{{" + Environment.NewLine +
                           $"{indent}try {{" + Environment.NewLine +
                           $"{statementsString}" + Environment.NewLine +
                           $"{indent}//return ProgramHelper.RunOnStaThread(() => {{" + Environment.NewLine +
                           $"{indent}ProgramHelper.Instance.LoadWindow();" + Environment.NewLine +
                           $"{_uiLoadingStatements}" +
                           $"{indent}return ProgramHelper.Instance.ShowWindow();" + Environment.NewLine +
                           $"{indent}//}});" + Environment.NewLine +
                           $"{indent}}} finally {{" + Environment.NewLine +
                           $"{indent}ProgramHelper.WaitAllUnwaitedThreads();" + Environment.NewLine +
                           $"{indent}}}" + Environment.NewLine +
                           $"{indent}return null;" + Environment.NewLine +
                           $"{oldIdent}}}";
                }

                return $"{oldIdent}{accessor} static dynamic {method.Name}({string.Join(", ", arguments)})" + Environment.NewLine +
                       $"{oldIdent}{{" + Environment.NewLine +
                       $"{indent}try {{" + Environment.NewLine +
                       $"{statementsString}" + Environment.NewLine +
                       $"{indent}}} finally {{" + Environment.NewLine +
                       $"{indent}ProgramHelper.WaitAllUnwaitedThreads();" + Environment.NewLine +
                       $"{indent}}}" + Environment.NewLine +
                       $"{indent}return null;" + Environment.NewLine +
                       $"{oldIdent}}}";
            }

            if (uiEvent != null)
            {
                var control = _userInterface.FindName(uiEvent.ControlName);
                if (control == null)
                {
                    control = new UIElement();
                }

                if (control is Window && uiEvent.ControlEventName == nameof(Window.Closed))
                {
                    // The detection could be better by checking that the ControlName corresponds to a Window.
                    _uiLoadingStatements.AppendLine($"            (({control.GetType().FullName})ProgramHelper.Instance.GetControl(\"{uiEvent.ControlName}\")).{uiEvent.ControlEventName} += (sender, e) => {{ ProgramHelper.Instance.UiResult = {method.Name}(); }};");
                }
                else
                {
                    _uiLoadingStatements.AppendLine($"            (({control.GetType().FullName})ProgramHelper.Instance.GetControl(\"{uiEvent.ControlName}\")).{uiEvent.ControlEventName} += (sender, e) => {{ {method.Name}(); }};");
                }
            }

            return $"{oldIdent}{accessor} static dynamic {method.Name}({string.Join(", ", arguments)})" + Environment.NewLine +
                   $"{oldIdent}{{" + Environment.NewLine +
                   $"{statementsString}" + Environment.NewLine +
                   $"{indent}return null;" + Environment.NewLine +
                   $"{oldIdent}}}";
        }

        /// <summary>
        /// Generates the code for a <see cref="ParameterDeclaration"/>.
        /// </summary>
        /// <param name="parameter">The parameter declaration</param>
        /// <returns>A CSharp code</returns>
        private string GenerateParameterDeclaration(ParameterDeclaration parameter)
        {
            Requires.NotNull(parameter.Name, nameof(parameter.Name));

            return $"dynamic {parameter.Name}";
        }

        /// <summary>
        /// Transform a verbatim string to a literal string.
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>A verbatim string.</returns>
        private string FormatLiteralString(string input)
        {
            return input.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
        }

        /// <summary>
        /// Increase the indentation of 4 white spaces.
        /// </summary>
        /// <returns>Returns a string full of white spaces that correspond to the new indentation.</returns>
        private string IncreaseIndent()
        {
            _indentSpaceCount += 4;
            return new string(' ', _indentSpaceCount);
        }

        /// <summary>
        /// Decrease the indentation of 4 white spaces.
        /// </summary>
        /// <returns>Returns a string full of white spaces that correspond to the new indentation.</returns>
        private string DecreaseIndent()
        {
            _indentSpaceCount -= 4;
            return new string(' ', _indentSpaceCount);
        }

        #endregion
    }
}
