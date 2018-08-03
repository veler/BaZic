using BaZic.Core.ComponentModel;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Code.Lexer.Tokens;
using BaZic.Runtime.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BaZic.Runtime.BaZic.Code.Parser
{
    /// <summary>
    /// Provides a BaZic code parser
    /// </summary>
    public sealed partial class BaZicParser
    {
        #region Methods

        /// <summary>
        /// Parse statements.
        /// </summary>
        /// <param name="methodOrBindDeclarationAllowed">Defines whether a method or binding can be declared in the current block.</param>
        /// <param name="expectedEndTokens">Defines the expected tokens that defines the end of a statement.</param>
        /// <returns>Returns a list of parsed statements.</returns>
        private Statement[] ParseStatements(bool methodOrBindDeclarationAllowed, params TokenType[] expectedEndTokens)
        {
            var statements = new List<Statement>();

            EnteringScope();

            while (expectedEndTokens.All(token => token != CurrentToken.TokenType) && CurrentToken.TokenType != TokenType.EndCode)
            {
                if (!TokenIdentificationHelper.IsStatementSeparator(PreviousToken))
                {
                    AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Statements.StatementExpected));
                }

                while (TokenIdentificationHelper.IsStatementSeparator(CurrentToken) && CurrentToken.TokenType != TokenType.EndCode)
                {
                    DiscardToken();
                }

                switch (CurrentToken.TokenType)
                {
                    case TokenType.Variable:
                    case TokenType.Bind:
                        statements.Add(ParseBindAndVariableDeclaration(methodOrBindDeclarationAllowed));
                        break;

                    case TokenType.Do:
                        statements.Add(ParseLoopStatement());
                        break;

                    case TokenType.If:
                        statements.Add(ParseConditionStatement());
                        break;

                    case TokenType.Try:
                        statements.Add(ParseTryCatchStatement());
                        break;

                    case TokenType.Throw:
                        statements.Add(ParseThrowStatement());
                        break;

                    case TokenType.Return:
                        statements.Add(ParseReturnStatement());
                        break;

                    case TokenType.Break:
                        statements.Add(ParseBreakStatement());
                        break;

                    case TokenType.Breakpoint:
                        statements.Add(ParseBreakpointStatement());
                        break;

                    case TokenType.Async:
                    case TokenType.Event:
                    case TokenType.Function:
                        if (!methodOrBindDeclarationAllowed)
                        {
                            AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Statements.InvalidMethodDeclaration));
                        }

                        statements.Add(ParseMethodDeclaration());
                        break;

                    default:
                        var statement = ParseAssignOrExpressionStatement();
                        if (statement != null)
                        {
                            statements.Add(statement);
                        }
                        else if (expectedEndTokens.All(token => token != CurrentToken.TokenType) && CurrentToken.TokenType != TokenType.EndCode)
                        {
                            AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Statements.InvalidStatement));
                            DiscardToken();
                        }
                        break;
                }
            }

            ExitingScope();

            return statements.ToArray();
        }

        /// <summary>
        /// Try to parse a variable delcaration statement.
        /// 
        /// Corresponding grammar :
        ///     ('VARIABLE' | 'BIND') Identifier '[]'? ('=' Expression)?
        /// </summary>
        /// <param name="bindDeclarationAllowed">Defines whether a binding can be declared in the current block.</param>
        /// <returns>If succeed, returns a <see cref="VariableDeclaration"/> if it's a VARIABLE statement or a <see cref="BindingDeclaration"/> if it's a BIND statement.</returns>
        private Statement ParseBindAndVariableDeclaration(bool bindDeclarationAllowed)
        {
            var isBinding = CurrentToken.TokenType == TokenType.Bind;

            if (isBinding)
            {
                if (!bindDeclarationAllowed)
                {
                    AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Statements.InvalidBindingDeclaration));
                }

                DiscardToken(TokenType.Bind);
            }
            else
            {
                DiscardToken(TokenType.Variable);
            }

            var identifierToken = CurrentToken;
            var variableName = CurrentToken.Value;
            var variableIsArray = false;
            Expression variableDefaultValue = null;

            TokenIdentificationHelper.CheckIdentifier(variableName, CurrentToken, isBinding, _issues);
            DiscardToken();

            if (CurrentToken.TokenType == TokenType.LeftBracket)
            {
                DiscardToken();
                DiscardToken(TokenType.RightBracket);
                variableIsArray = true;
            }

            if (CurrentToken.TokenType == TokenType.Equal)
            {
                DiscardToken();
                var line = CurrentToken.Line;
                var column = CurrentToken.Column;
                var startOffset = CurrentToken.StartOffset;
                var parsedLength = CurrentToken.ParsedLength;
                variableDefaultValue = ParseExpression(true, TokenIdentificationHelper.GetStatementSeparatorsTokens());

                if (variableDefaultValue != null)
                {
                    if (variableIsArray && (variableDefaultValue.GetType() == typeof(BinaryOperatorExpression) || variableDefaultValue.GetType() == typeof(PrimitiveExpression) || variableDefaultValue.GetType() == typeof(ClassReferenceExpression) || variableDefaultValue.GetType() == typeof(ExceptionReferenceExpression) || variableDefaultValue.GetType() == typeof(InstantiateExpression) || variableDefaultValue.GetType() == typeof(NotOperatorExpression)))
                    {
                        AddIssue(new BaZicParserException(line, column, startOffset, parsedLength, L.BaZic.Parser.Statements.FormattedDefaultValueArrayExpected(variableName)));
                    }
                    else if (!variableIsArray && variableDefaultValue.GetType() == typeof(ArrayCreationExpression))
                    {
                        AddIssue(new BaZicParserException(line, column, startOffset, parsedLength, L.BaZic.Parser.Statements.FormattedDefaultValueNoArrayExpected(variableName)));
                    }
                    else if (variableDefaultValue.GetType() == typeof(VariableReferenceExpression))
                    {
                        ValidateVariableReferenceExpression((VariableReferenceExpression)variableDefaultValue, variableIsArray);
                    }
                }
            }

            if (TokenIdentificationHelper.IsStatementSeparator(CurrentToken))
            {
                DiscardToken();
                var variableDeclaration = new VariableDeclaration(variableName, variableIsArray);
                variableDeclaration.WithDefaultValue(variableDefaultValue);
                variableDeclaration.Line = identifierToken.Line;
                variableDeclaration.Column = identifierToken.Column;
                variableDeclaration.StartOffset = identifierToken.StartOffset;
                variableDeclaration.NodeLength = identifierToken.ParsedLength;

                AddVariableToScope(variableDeclaration);

                if (isBinding)
                {
                    var splittedVariableName = variableName.Split('_');
                    var controlName = splittedVariableName[0];
                    var propertyName = splittedVariableName[1];

                    var binding = new BindingDeclaration(controlName, propertyName, variableDeclaration);
                    binding.Line = identifierToken.Line;
                    binding.Column = identifierToken.Column;
                    binding.StartOffset = identifierToken.StartOffset;
                    binding.NodeLength = identifierToken.ParsedLength;

                    ValidateUiBinding(binding);
                    return binding;
                }

                return variableDeclaration;
            }

            AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Statements.AssignOrLineExpected));
            return null;
        }

        /// <summary>
        /// Try to parse a loop iteration statement.
        /// 
        /// Corresponding grammar:
        ///     'DO' ('WHILE' Expression)?
        ///         Statement_List
        ///     'LOOP' ('WHILE' Expression)?
        /// </summary>
        /// <returns>If succeed, returns a <see cref="IterationStatement"/>.</returns>
        private IterationStatement ParseLoopStatement()
        {
            var doToken = CurrentToken;
            DiscardToken(TokenType.Do);

            if (CurrentToken.TokenType == TokenType.While)
            {
                // DO WHILE () ... LOOP
                DiscardToken(TokenType.While);

                var conditionExpression = ParseExpression(true, TokenIdentificationHelper.GetStatementSeparatorsTokens());

                DiscardToken(TokenType.NewLine);
                IncreaseDoLoopIndicator();
                var statements = ParseStatements(false, TokenType.Loop);
                DecreaseDoLoopIndicator();

                DiscardToken(TokenType.Loop);

                if (TokenIdentificationHelper.IsStatementSeparatorButNotEndCode(CurrentToken))
                {
                    DiscardToken();
                    return new IterationStatement(conditionExpression, false)
                    {
                        Line = doToken.Line,
                        Column = doToken.Column,
                        StartOffset = doToken.StartOffset,
                        NodeLength = doToken.ParsedLength
                    }
                    .WithBody(statements);
                }
                else
                {
                    AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Statements.LineExpected));
                    return null;
                }
            }
            else
            {
                // DO ... LOOP WHILE
                DiscardToken(TokenType.NewLine);
                IncreaseDoLoopIndicator();
                var statements = ParseStatements(false, TokenType.Loop);
                DecreaseDoLoopIndicator();

                DiscardToken(TokenType.Loop);
                DiscardToken(TokenType.While);

                var conditionExpression = ParseExpression(true, TokenIdentificationHelper.GetStatementSeparatorsTokens());

                if (TokenIdentificationHelper.IsStatementSeparatorButNotEndCode(CurrentToken))
                {
                    DiscardToken();
                    return new IterationStatement(conditionExpression, true)
                    {
                        Line = doToken.Line,
                        Column = doToken.Column,
                        StartOffset = doToken.StartOffset,
                        NodeLength = doToken.ParsedLength
                    }
                    .WithBody(statements);
                }
                else
                {
                    AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Statements.LineExpected));
                    return null;
                }
            }
        }

        /// <summary>
        /// Try to parse a condition statement.
        /// 
        /// Corresponding grammar:
        ///     'IF' Expression 'THEN'
        ///         Statement_List
        ///     'ELSE'?
        ///         Statement_List
        ///     'END' 'IF'
        /// </summary>
        /// <returns>If succeed, returns a <see cref="ConditionStatement"/>.</returns>
        private ConditionStatement ParseConditionStatement()
        {
            var ifToken = CurrentToken;
            DiscardToken(TokenType.If);
            var conditionExpression = ParseExpression(true, TokenType.Then);
            DiscardToken(TokenType.Then);
            DiscardToken(TokenType.NewLine);

            var trueStatements = ParseStatements(false, TokenType.Else, TokenType.End);

            if (CurrentToken.TokenType == TokenType.Else)
            {
                DiscardToken(TokenType.Else);
                DiscardToken(TokenType.NewLine);
                var falseStatements = ParseStatements(false, TokenType.End);
                DiscardToken(TokenType.End);
                DiscardToken(TokenType.If);
                if (TokenIdentificationHelper.IsStatementSeparatorButNotEndCode(CurrentToken))
                {
                    DiscardToken();
                    return new ConditionStatement(conditionExpression)
                    {
                        Line = ifToken.Line,
                        Column = ifToken.Column,
                        StartOffset = ifToken.StartOffset,
                        NodeLength = ifToken.ParsedLength
                    }
                    .WithThenBody(trueStatements)
                    .WithElseBody(falseStatements);
                }
                else
                {
                    AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Statements.LineExpected));
                    return null;
                }
            }
            else
            {
                DiscardToken(TokenType.End);
                DiscardToken(TokenType.If);
                if (TokenIdentificationHelper.IsStatementSeparatorButNotEndCode(CurrentToken))
                {
                    DiscardToken();
                    return new ConditionStatement(conditionExpression)
                    {
                        Line = ifToken.Line,
                        Column = ifToken.Column,
                        StartOffset = ifToken.StartOffset,
                        NodeLength = ifToken.ParsedLength
                    }
                    .WithThenBody(trueStatements);
                }
                else
                {
                    AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Statements.LineExpected));
                    return null;
                }
            }
        }

        /// <summary>
        /// Try to parse a try catch statement.
        /// 
        /// Corresponding grammar:
        ///     'TRY'
        ///         Statement_List
        ///     'CATCH'?
        ///         Statement_List
        ///     'END' 'TRY'
        /// </summary>
        /// <returns>If succeed, returns a <see cref="TryCatchStatement"/>.</returns>
        private TryCatchStatement ParseTryCatchStatement()
        {
            var tryCatchToken = CurrentToken;
            DiscardToken(TokenType.Try);
            DiscardToken(TokenType.NewLine);

            var tryStatements = ParseStatements(false, TokenType.Catch, TokenType.End);

            if (CurrentToken.TokenType == TokenType.Catch)
            {
                IncreaseCatchIndicator();
                DiscardToken(TokenType.Catch);
                DiscardToken(TokenType.NewLine);
                var catchStatements = ParseStatements(false, TokenType.End);
                DiscardToken(TokenType.End);
                DiscardToken(TokenType.Try);
                DecreaseCatchIndicator();
                if (TokenIdentificationHelper.IsStatementSeparatorButNotEndCode(CurrentToken))
                {
                    DiscardToken();
                    return new TryCatchStatement()
                    {
                        Line = tryCatchToken.Line,
                        Column = tryCatchToken.Column,
                        StartOffset = tryCatchToken.StartOffset,
                        NodeLength = tryCatchToken.ParsedLength
                    }
                    .WithTryBody(tryStatements)
                    .WithCatchBody(catchStatements);
                }
                else
                {
                    AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Statements.LineExpected));
                    return null;
                }
            }
            else
            {
                DiscardToken(TokenType.End);
                DiscardToken(TokenType.Try);
                if (TokenIdentificationHelper.IsStatementSeparatorButNotEndCode(CurrentToken))
                {
                    DiscardToken();
                    return new TryCatchStatement()
                    {
                        Line = tryCatchToken.Line,
                        Column = tryCatchToken.Column,
                        StartOffset = tryCatchToken.StartOffset,
                        NodeLength = tryCatchToken.ParsedLength
                    }
                    .WithTryBody(tryStatements);
                }
                else
                {
                    AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Statements.LineExpected));
                    return null;
                }
            }
        }

        /// <summary>
        /// Try to parse a throw statement.
        /// 
        /// Corresponding grammar :
        ///     'THROW' Expression
        /// </summary>
        /// <returns>If succeed, returns a <see cref="ThrowStatement"/>.</returns>
        private ThrowStatement ParseThrowStatement()
        {
            var throwToken = CurrentToken;
            DiscardToken(TokenType.Throw);

            var thrownValue = ParseExpression(true, TokenIdentificationHelper.GetStatementSeparatorsTokens());

            if (TokenIdentificationHelper.IsStatementSeparator(CurrentToken))
            {
                DiscardToken();
                return new ThrowStatement(thrownValue)
                {
                    Line = throwToken.Line,
                    Column = throwToken.Column,
                    StartOffset = throwToken.StartOffset,
                    NodeLength = throwToken.ParsedLength
                };
            }
            else
            {
                AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Statements.LineExpected));
                return null;
            }
        }

        /// <summary>
        /// Try to parse a return statement.
        /// 
        /// Corresponding grammar :
        ///     'RETURN' Expression?
        /// </summary>
        /// <returns>If succeed, returns a <see cref="ReturnStatement"/>.</returns>
        private ReturnStatement ParseReturnStatement()
        {
            var returnToken = CurrentToken;
            DiscardToken(TokenType.Return);

            var returnedValue = ParseExpression(false, TokenIdentificationHelper.GetStatementSeparatorsTokens());

            if (TokenIdentificationHelper.IsStatementSeparator(CurrentToken))
            {
                DiscardToken();
                return new ReturnStatement(returnedValue)
                {
                    Line = returnToken.Line,
                    Column = returnToken.Column,
                    StartOffset = returnToken.StartOffset,
                    NodeLength = returnToken.ParsedLength
                };
            }
            else
            {
                AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Statements.LineExpected));
                return null;
            }
        }

        /// <summary>
        /// Try to parse a break statement.
        /// 
        /// Corresponding grammar :
        ///     'BREAK'
        /// </summary>
        /// <returns>If succeed, returns a <see cref="BreakStatement"/>.</returns>
        private BreakStatement ParseBreakStatement()
        {
            if (GetDoLoopIndicator() < 1)
            {
                AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Statements.InvalidBreak));
            }

            var breakToken = CurrentToken;
            DiscardToken(TokenType.Break);

            if (TokenIdentificationHelper.IsStatementSeparator(CurrentToken))
            {
                DiscardToken();
                return new BreakStatement()
                {
                    Line = breakToken.Line,
                    Column = breakToken.Column,
                    StartOffset = breakToken.StartOffset,
                    NodeLength = breakToken.ParsedLength
                };
            }
            else
            {
                AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Statements.LineExpected));
                return null;
            }
        }

        /// <summary>
        /// Try to parse a breakpoint statement.
        /// 
        /// Corresponding grammar :
        ///     'BREAKPOINT'
        /// </summary>
        /// <returns>If succeed, returns a <see cref="BreakpointStatement"/>.</returns>
        private BreakpointStatement ParseBreakpointStatement()
        {
            var breakpointToken = CurrentToken;
            DiscardToken(TokenType.Breakpoint);

            if (TokenIdentificationHelper.IsStatementSeparator(CurrentToken))
            {
                DiscardToken();
                return new BreakpointStatement()
                {
                    Line = breakpointToken.Line,
                    Column = breakpointToken.Column,
                    StartOffset = breakpointToken.StartOffset,
                    NodeLength = breakpointToken.ParsedLength
                };
            }
            else
            {
                AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Statements.LineExpected));
                return null;
            }
        }

        /// <summary>
        /// Try to parse a assign or expression statement.
        /// 
        /// Corresponding grammar :
        ///     Expression ('=' Expression)?
        /// </summary>
        /// <returns>If succeed, returns a <see cref="AssignStatement"/> or <see cref="ExpressionStatement"/>.</returns>
        private Statement ParseAssignOrExpressionStatement()
        {
            var leftExpressionLine = CurrentToken.Line;
            var leftExpressionColumn = CurrentToken.Column;
            var leftExpressionStartOffset = CurrentToken.StartOffset;
            var leftExpressionParsedLength = CurrentToken.ParsedLength;

            var expression = ParseExpression(false, TokenIdentificationHelper.GetStatementSeparatorsTokens());

            if (expression == null)
            {
                return null;
            }
            else
            {
                Statement statement = null;

                if (expression is BinaryOperatorExpression binaryOperatorExpression)
                {
                    if (binaryOperatorExpression.Operator == BinaryOperatorType.Equality)
                    {
                        if (!(binaryOperatorExpression.LeftExpression is IAssignable))
                        {
                            AddIssue(new BaZicParserException(leftExpressionLine, leftExpressionColumn, leftExpressionStartOffset, leftExpressionParsedLength, L.BaZic.Parser.Statements.NotAssignable));
                        }

                        var assignStatement = new AssignStatement(binaryOperatorExpression.LeftExpression, binaryOperatorExpression.RightExpression)
                        {
                            Line = leftExpressionLine,
                            Column = leftExpressionColumn,
                            StartOffset = leftExpressionStartOffset,
                            NodeLength = leftExpressionParsedLength
                        };

                        ValidateAssignStatement(assignStatement);
                        statement = assignStatement;
                    }
                    else
                    {
                        AddIssue(new BaZicParserException(leftExpressionLine, leftExpressionColumn, leftExpressionStartOffset, leftExpressionParsedLength, L.BaZic.Parser.Statements.AssignOrCallExpected));
                        return null;
                    }
                }
                else
                {
                    if (expression is IAssignable)
                    {
                        AddIssue(new BaZicParserException(leftExpressionLine, leftExpressionColumn, leftExpressionStartOffset, leftExpressionParsedLength, L.BaZic.Parser.Statements.AssignOrCallExpected));
                    }
                    statement = new ExpressionStatement(expression)
                    {
                        Line = leftExpressionLine,
                        Column = leftExpressionColumn,
                        StartOffset = leftExpressionStartOffset,
                        NodeLength = leftExpressionParsedLength
                    };
                }

                if (TokenIdentificationHelper.IsStatementSeparator(CurrentToken))
                {
                    DiscardToken();
                    return statement;
                }
                else
                {
                    AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Statements.LineExpected));
                    return null;
                }
            }
        }

        /// <summary>
        /// Try to parse a method delcaration statement.
        /// 
        /// Corresponding grammar :
        ///     ('ASYNC' | 'EVENT')? 'FUNCTION' Identifier '(' Parameter_List ')'
        ///         Statement_List
        ///     'END' 'FUNCTION'
        /// </summary>
        /// <returns>If succeed, returns a <see cref="VariableDeclaration"/>.</returns>
        private MethodDeclaration ParseMethodDeclaration()
        {
            var isAsync = false;
            var isEvent = false;

            if (CurrentToken.TokenType == TokenType.Async)
            {
                if (NextToken.TokenType == TokenType.Event)
                {
                    AddIssue(new BaZicParserException(NextToken.Line, NextToken.Column, NextToken.StartOffset, NextToken.ParsedLength, L.BaZic.Parser.Statements.AsyncEvent));
                }

                isAsync = true;
                DiscardToken();
            }
            else if (CurrentToken.TokenType == TokenType.Event)
            {
                if (NextToken.TokenType == TokenType.Async)
                {
                    AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Statements.AsyncEvent));
                }

                isEvent = true;
                DiscardToken();
            }

            DiscardToken(TokenType.Function);

            var identifierToken = CurrentToken;
            var methodName = CurrentToken.Value;
            TokenIdentificationHelper.CheckIdentifier(methodName, CurrentToken, isEvent, _issues);
            DiscardToken();

            DiscardToken(TokenType.LeftParenth);
            var methodParameters = ParseParameterList();
            DiscardToken(TokenType.RightParenth);

            if (!TokenIdentificationHelper.IsStatementSeparatorButNotEndCode(CurrentToken))
            {
                AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Statements.LineExpected));
            }

            if (isEvent && methodParameters.Length != 0)
            {
                AddIssue(new BaZicParserException(identifierToken.Line, identifierToken.Column, identifierToken.StartOffset, identifierToken.ParsedLength, L.BaZic.Parser.Statements.EventNoParameter));
            }

            while (TokenIdentificationHelper.IsStatementSeparatorButNotEndCode(CurrentToken))
            {
                DiscardToken();
            }

            var statements = ParseStatements(false, TokenType.End);

            DiscardToken(TokenType.End);
            DiscardToken(TokenType.Function);
            var endToken = PreviousToken;

            if (TokenIdentificationHelper.IsStatementSeparator(CurrentToken))
            {
                DiscardToken();
            }
            else
            {
                AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Statements.LineExpected));
            }

            if (string.Compare(methodName, Consts.EntryPointMethodName, StringComparison.Ordinal) == 0) // The name is case sensitive.
            {
                if (isAsync)
                {
                    AddIssue(new BaZicParserException(identifierToken.Line, identifierToken.Column, identifierToken.StartOffset, identifierToken.ParsedLength, L.BaZic.Parser.Statements.AsyncEntryPoint));
                }
                else if (methodParameters.Length != 1)
                {
                    AddIssue(new BaZicParserException(identifierToken.Line, identifierToken.Column, identifierToken.StartOffset, identifierToken.ParsedLength, L.BaZic.Parser.Statements.UniqueArgumentEntryPoint));
                }
                else if (!methodParameters.Single().IsArray)
                {
                    AddIssue(new BaZicParserException(identifierToken.Line, identifierToken.Column, identifierToken.StartOffset, identifierToken.ParsedLength, L.BaZic.Parser.Statements.EntryPointArgumentArrayExpected));
                }

                var entryPointDeclaration = new EntryPointMethod()
                {
                    Line = identifierToken.Line,
                    Column = identifierToken.Column,
                    StartOffset = identifierToken.StartOffset,
                    NodeLength = identifierToken.ParsedLength,
                    EndOffset = endToken.StartOffset + endToken.ParsedLength
                }
                .WithParameters(methodParameters);

                AddMethod(entryPointDeclaration, false);

                return entryPointDeclaration.WithBody(statements);
            }

            var methodDeclaration = new MethodDeclaration(methodName, isAsync)
            {
                Line = identifierToken.Line,
                Column = identifierToken.Column,
                StartOffset = identifierToken.StartOffset,
                NodeLength = identifierToken.ParsedLength,
                EndOffset = endToken.StartOffset + endToken.ParsedLength
            }
            .WithParameters(methodParameters);

            AddMethod(methodDeclaration, isEvent);

            return methodDeclaration.WithBody(statements);
        }

        #endregion
    }
}
