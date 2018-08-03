using BaZic.Core.Enums;
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
        /// Parse a list of parameter declaration separated by a comma. This could be included between parenthesis.
        /// 
        /// Corresponding grammar :
        ///     Parameter_Declaration? ( ',' Parameter_Declaration)*
        /// </summary>
        /// <returns>Returns an array of parameter declaration.</returns>
        private ParameterDeclaration[] ParseParameterList()
        {
            var list = new List<ParameterDeclaration>();

            ParameterDeclaration parameterDeclaration = null;
            do
            {
                parameterDeclaration = ParseParameterDeclaration();
                if (parameterDeclaration != null)
                {
                    list.Add(parameterDeclaration);

                    if (CurrentToken.TokenType == TokenType.Comma)
                    {
                        DiscardToken();
                    }
                }
            } while (parameterDeclaration != null);

            return list.ToArray();
        }

        /// <summary>
        /// Parse a parameter declaration.
        /// 
        /// Corresponding grammar:
        ///     Identifier ('[]')?
        /// </summary>
        /// <returns>Returns a parameter declaration if it is found, or a null value.</returns>
        private ParameterDeclaration ParseParameterDeclaration()
        {
            if (CurrentToken.TokenType == TokenType.Identifier)
            {
                var identifierToken = CurrentToken;
                var parameterName = string.Empty;
                var parameterArray = false;

                TokenIdentificationHelper.CheckIdentifier(CurrentToken.Value, CurrentToken, false, _issues);
                parameterName = CurrentToken.Value;
                DiscardToken();

                if (CurrentToken.TokenType == TokenType.LeftBracket)
                {
                    DiscardToken();
                    DiscardToken(TokenType.RightBracket);
                    parameterArray = true;
                }

                var parameterDeclaration = new ParameterDeclaration(parameterName, parameterArray)
                {
                    Line = identifierToken.Line,
                    Column = identifierToken.Column,
                    StartOffset = identifierToken.StartOffset,
                    NodeLength = identifierToken.ParsedLength
                };
                AddParameter(parameterDeclaration);
                return parameterDeclaration;
            }
            else if (CurrentToken.TokenType != TokenType.RightParenth)
            {
                AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.UnexpectedOrMissingCharacter));
            }

            return null;
        }

        /// <summary>
        /// Parse an expression.
        /// </summary>
        /// <param name="isRequired">Defines whether the expression is expected or not and throw an exception if it is required but not parsed.</param>
        /// <param name="expectedEndToken">Defines the expected tokens that defines the end of the expression.</param>
        /// <returns>An expression</returns>
        private Expression ParseExpression(bool isRequired, params TokenType[] expectedEndToken)
        {
            if (TokenIdentificationHelper.IsOperator(CurrentToken))
            {
                AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Expressions.CannotStartWithOperator));
            }

            if (isRequired && TokenIdentificationHelper.IsStatementSeparator(CurrentToken))
            {
                AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Expressions.NotValid));
            }

            var expression = ParseConditionalOrExpression(isRequired);

            if (expression == null && isRequired)
            {
                AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Expressions.NotValid));
            }

            if (expectedEndToken.Any() && expression != null)
            {
                if (expectedEndToken.All(token => token != CurrentToken.TokenType))
                {
                    var expectedEndTokenStrings = new List<string>();
                    foreach (var token in expectedEndToken)
                    {
                        expectedEndTokenStrings.Add(token.GetDescription());
                    }
                    AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Expressions.FormattedEndTokenExpected(string.Join("', '", expectedEndTokenStrings))));
                }
            }

            return expression;
        }

        /// <summary>
        /// Parse expression that contains a logical 'or' operator.
        /// 
        /// Corresponding grammar :
        ///     Conditional_And_Expression ('OR' Conditional_And_Expression)*
        /// </summary>
        /// <param name="isRequired">Defines whether it is required/expected to parse an expression. If true, throw an exception if no expression is parsed.</param>
        /// <returns>Returns an expression.</returns>
        private Expression ParseConditionalOrExpression(bool isRequired)
        {
            var expression = ParseConditionalAndExpression(isRequired);

            while (CurrentToken.TokenType == TokenType.Or)
            {
                var operatorToken = CurrentToken;
                DiscardToken();
                var rightExpression = ParseConditionalAndExpression(true);
                expression = new BinaryOperatorExpression(expression, BinaryOperatorType.LogicalOr, rightExpression)
                {
                    Line = operatorToken.Line,
                    Column = operatorToken.Column,
                    StartOffset = operatorToken.StartOffset,
                    NodeLength = operatorToken.ParsedLength
                };
            }

            return expression;
        }

        /// <summary>
        /// Parse expression that contains a logical 'and' operator.
        /// 
        /// Corresponding grammar :
        ///     Negative_Expression ('AND' Negative_Expression)*
        /// </summary>
        /// <param name="isRequired">Defines whether it is required/expected to parse an expression. If true, throw an exception if no expression is parsed.</param>
        /// <returns>Returns an expression.</returns>
        private Expression ParseConditionalAndExpression(bool isRequired)
        {
            var expression = ParseNegativeExpression(isRequired);

            while (CurrentToken.TokenType == TokenType.And)
            {
                var operatorToken = CurrentToken;
                DiscardToken();
                var rightExpression = ParseNegativeExpression(true);
                expression = new BinaryOperatorExpression(expression, BinaryOperatorType.LogicalAnd, rightExpression)
                {
                    Line = operatorToken.Line,
                    Column = operatorToken.Column,
                    StartOffset = operatorToken.StartOffset,
                    NodeLength = operatorToken.ParsedLength
                };
            }

            return expression;
        }

        /// <summary>
        /// Parse expression that starts with logical negative expression.
        /// 
        /// Corresponding grammar :
        ///     ('NOT')? Equality_Expression
        /// </summary>
        /// <param name="isRequired">Defines whether it is required/expected to parse an expression. If true, throw an exception if no expression is parsed.</param>
        /// <returns>Returns an expression.</returns>
        private Expression ParseNegativeExpression(bool isRequired)
        {
            if (CurrentToken.TokenType == TokenType.Not)
            {
                var operatorToken = CurrentToken;
                DiscardToken();
                return new NotOperatorExpression(ParseEqualityExpression(true))
                {
                    Line = operatorToken.Line,
                    Column = operatorToken.Column,
                    StartOffset = operatorToken.StartOffset,
                    NodeLength = operatorToken.ParsedLength
                };
            }

            return ParseEqualityExpression(isRequired);
        }

        /// <summary>
        /// Parse expression that contains equality symbols.
        /// 
        /// Corresponding grammar :
        ///     Relational_Expression ('=' Relational_Expression)*
        /// </summary>
        /// <param name="isRequired">Defines whether it is required/expected to parse an expression. If true, throw an exception if no expression is parsed.</param>
        /// <returns>Returns an expression.</returns>
        private Expression ParseEqualityExpression(bool isRequired)
        {
            var expression = ParseRelationalExpression(isRequired);

            while (CurrentToken.TokenType == TokenType.Equal)
            {
                var operatorToken = CurrentToken;
                DiscardToken();
                var rightExpression = ParseRelationalExpression(true);
                expression = new BinaryOperatorExpression(expression, BinaryOperatorType.Equality, rightExpression)
                {
                    Line = operatorToken.Line,
                    Column = operatorToken.Column,
                    StartOffset = operatorToken.StartOffset,
                    NodeLength = operatorToken.ParsedLength
                };
            }

            return expression;
        }

        /// <summary>
        /// Parse expression that contains lesser than or greater than symbols.
        /// 
        /// Corresponding grammar :
        ///     Additive_Expression (('<' | '>' | '<=' | '>=') Additive_Expression)*
        /// </summary>
        /// <param name="isRequired">Defines whether it is required/expected to parse an expression. If true, throw an exception if no expression is parsed.</param>
        /// <returns>Returns an expression.</returns>
        private Expression ParseRelationalExpression(bool isRequired)
        {
            var expression = ParseAdditiveExpression(isRequired);

            while (CurrentToken.TokenType == TokenType.LesserThan || CurrentToken.TokenType == TokenType.GreaterThan)
            {
                BinaryOperatorType binaryOperator;
                var operatorToken = CurrentToken;
                if (CurrentToken.TokenType == TokenType.LesserThan)
                {
                    if (NextToken.TokenType == TokenType.Equal)
                    {
                        binaryOperator = BinaryOperatorType.LessThanOrEqual;
                        DiscardToken();
                    }
                    else
                    {
                        binaryOperator = BinaryOperatorType.LessThan;
                    }
                }
                else
                {
                    if (NextToken.TokenType == TokenType.Equal)
                    {
                        binaryOperator = BinaryOperatorType.GreaterThan;
                        DiscardToken();
                    }
                    else
                    {
                        binaryOperator = BinaryOperatorType.GreaterThanOrEqual;
                    }
                }

                DiscardToken();
                var rightExpression = ParseAdditiveExpression(true);
                expression = new BinaryOperatorExpression(expression, binaryOperator, rightExpression)
                {
                    Line = operatorToken.Line,
                    Column = operatorToken.Column,
                    StartOffset = operatorToken.StartOffset,
                    NodeLength = operatorToken.ParsedLength
                };
            }

            return expression;
        }

        /// <summary>
        /// Parse expression that contains addition, substraction symbols.
        /// 
        /// Corresponding grammar :
        ///     Multiplicative_Expression (('+' | '-') Multiplicative_Expression)*
        /// </summary>
        /// <param name="isRequired">Defines whether it is required/expected to parse an expression. If true, throw an exception if no expression is parsed.</param>
        /// <returns>Returns an expression.</returns>
        private Expression ParseAdditiveExpression(bool isRequired)
        {
            var expression = ParseMultiplicativeExpression(isRequired);

            while (CurrentToken.TokenType == TokenType.Plus || CurrentToken.TokenType == TokenType.Minus)
            {
                BinaryOperatorType binaryOperator;
                var operatorToken = CurrentToken;
                if (CurrentToken.TokenType == TokenType.Plus)
                {
                    binaryOperator = BinaryOperatorType.Addition;
                }
                else
                {
                    binaryOperator = BinaryOperatorType.Subtraction;
                }

                DiscardToken();
                var rightExpression = ParseMultiplicativeExpression(true);
                expression = new BinaryOperatorExpression(expression, binaryOperator, rightExpression)
                {
                    Line = operatorToken.Line,
                    Column = operatorToken.Column,
                    StartOffset = operatorToken.StartOffset,
                    NodeLength = operatorToken.ParsedLength
                };
            }

            return expression;
        }

        /// <summary>
        /// Parse expression that contains multiply, division and modulus symbols.
        /// 
        /// Corresponding grammar :
        ///     Unary_Expression (('*' | '/' | '%') Unary_Expression)*
        /// </summary>
        /// <param name="isRequired">Defines whether it is required/expected to parse an expression. If true, throw an exception if no expression is parsed.</param>
        /// <returns>Returns an expression.</returns>
        private Expression ParseMultiplicativeExpression(bool isRequired)
        {
            var expression = ParseUnaryExpression(isRequired);

            while (CurrentToken.TokenType == TokenType.Asterisk || CurrentToken.TokenType == TokenType.Slash || CurrentToken.TokenType == TokenType.Percent)
            {
                BinaryOperatorType binaryOperator;
                var operatorToken = CurrentToken;
                if (CurrentToken.TokenType == TokenType.Asterisk)
                {
                    binaryOperator = BinaryOperatorType.Multiply;
                }
                else if (CurrentToken.TokenType == TokenType.Slash)
                {
                    binaryOperator = BinaryOperatorType.Division;
                }
                else
                {
                    binaryOperator = BinaryOperatorType.Modulus;
                }

                DiscardToken();
                var rightExpression = ParseUnaryExpression(true);
                expression = new BinaryOperatorExpression(expression, binaryOperator, rightExpression)
                {
                    Line = operatorToken.Line,
                    Column = operatorToken.Column,
                    StartOffset = operatorToken.StartOffset,
                    NodeLength = operatorToken.ParsedLength
                };
            }

            return expression;
        }

        /// <summary>
        /// Parse a unary expression.
        /// 
        /// Corresponding grammar :
        ///     Primary_Expression
        ///     | 'AWAIT' Unary_Expression
        /// </summary>
        /// <param name="isRequired">Defines whether it is required/expected to parse an expression. If true, throw an exception if no expression is parsed.</param>
        /// <returns>Returns an expression.</returns>
        private Expression ParseUnaryExpression(bool isRequired)
        {
            if (CurrentToken.TokenType == TokenType.Await)
            {
                DiscardToken();

                var line = CurrentToken.Line;
                var column = CurrentToken.Column;
                var startOffset = CurrentToken.StartOffset;
                var parsedLength = CurrentToken.ParsedLength;
                var asyncExpression = ParseUnaryExpression(isRequired);

                var methodInvocation = asyncExpression as InvokeMethodExpression;
                if (methodInvocation == null)
                {
                    AddIssue(new BaZicParserException(line, column, startOffset, parsedLength, L.BaZic.Parser.Expressions.AsyncMethodExpected));
                    return null;
                }

                methodInvocation.Await = true;
                if (asyncExpression.GetType() != typeof(InvokeCoreMethodExpression))
                {
                    AddMethodInvocation(methodInvocation);
                }

                return asyncExpression;
            }

            return ParsePrimaryExpression(isRequired);
        }

        /// <summary>
        /// Parse a part of an expression that can be a reference or primary value followed by an accesser like array indexer or method invocation.
        /// 
        /// Corresponding grammar :
        ///     Primary_Expression_Start Bracket_Expression* ((Member_Access | Method_Invocation) Bracket_Expression* )*
        /// </summary>
        /// <param name="isRequired">Defines whether it is required/expected to parse an expression. If true, throw an exception if no expression is parsed.</param>
        /// <returns>Returns an expression.</returns>
        private Expression ParsePrimaryExpression(bool isRequired)
        {
            Expression[] bracketExpression = null;
            var expressionLine = CurrentToken.Line;
            var expressionColumn = CurrentToken.Column;
            var expressionStartOffset = CurrentToken.StartOffset;
            var expressionParsedLength = CurrentToken.ParsedLength;

            // Primary_Expression_Start
            var expression = ParsePrimaryExpressionStart(isRequired);

            // Bracket_Expression *
            do
            {
                var bracketToken = CurrentToken;
                bracketExpression = ParseBracketExpression();
                if (bracketExpression != null)
                {
                    var referenceExpression = expression as ReferenceExpression;

                    if (referenceExpression == null)
                    {
                        AddIssue(new BaZicParserException(expressionLine, expressionColumn, expressionStartOffset, expressionParsedLength, L.BaZic.Parser.Expressions.UnexpectedIndexer));
                    }

                    if (bracketExpression.Length == 0)
                    {
                        AddIssue(new BaZicParserException(bracketToken.Line, bracketToken.Column, expressionStartOffset, expressionParsedLength, L.BaZic.Parser.Expressions.IndexerExpected));
                        return null;
                    }

                    var arrayIndexer = new ArrayIndexerExpression(referenceExpression, bracketExpression)
                    {
                        Line = bracketToken.Line,
                        Column = bracketToken.Column,
                        StartOffset = bracketToken.StartOffset,
                        NodeLength = bracketToken.ParsedLength
                    };

                    ValidateArrayIndexerExpression(arrayIndexer);
                    expression = arrayIndexer;
                }
            } while (bracketExpression != null);

            // ((Member_Access | Method_Invocation) Bracket_Expression* )*
            while (CurrentToken.TokenType == TokenType.Dot || CurrentToken.TokenType == TokenType.LeftParenth)
            {
                if (CurrentToken.TokenType == TokenType.Dot)
                {
                    // Member_Access
                    var memberNameToken = CurrentToken;
                    var memberAccess = ParseMemberAccessPart(true);

                    if (!string.IsNullOrEmpty(memberAccess))
                    {
                        var referenceExpression = expression as ReferenceExpression;

                        if (referenceExpression == null)
                        {
                            AddIssue(new BaZicParserException(expressionLine, expressionColumn, expressionStartOffset, expressionParsedLength, L.BaZic.Parser.Expressions.IllegalPropertyAccess));
                        }

                        expression = new PropertyReferenceExpression(referenceExpression, memberAccess)
                        {
                            Line = memberNameToken.Line,
                            Column = memberNameToken.Column + 1, // +1 because we don't want to show a potential error on the dot.
                            StartOffset = memberNameToken.StartOffset + 1, // +1 because we don't want to show a potential error on the dot.
                            NodeLength = memberNameToken.ParsedLength
                        };
                    }
                }
                else if (CurrentToken.TokenType == TokenType.LeftParenth)
                {
                    // Method_Invocation
                    var methodInvocationParameters = ParseMethodInvocation();
                    var propertyReferenceExpression = expression as PropertyReferenceExpression;

                    if (expression is VariableReferenceExpression variableReferenceExpression)
                    {
                        var methodInvoke = new InvokeMethodExpression(variableReferenceExpression.Name.ToString(), false)
                        {
                            Line = variableReferenceExpression.Line,
                            Column = variableReferenceExpression.Column,
                            StartOffset = variableReferenceExpression.StartOffset,
                            NodeLength = variableReferenceExpression.NodeLength
                        }
                        .WithParameters(methodInvocationParameters);

                        AddMethodInvocation(methodInvoke);
                        expression = methodInvoke;
                    }
                    else if (propertyReferenceExpression != null)
                    {
                        var methodInvoke = new InvokeCoreMethodExpression(propertyReferenceExpression.TargetObject, propertyReferenceExpression.PropertyName.ToString(), false)
                        {
                            Line = propertyReferenceExpression.Line,
                            Column = propertyReferenceExpression.Column,
                            StartOffset = propertyReferenceExpression.StartOffset,
                            NodeLength = propertyReferenceExpression.NodeLength
                        };
                        methodInvoke.WithParameters(methodInvocationParameters);

                        ValidateCoreMethodInvocation(methodInvoke);
                        expression = methodInvoke;
                    }
                    else
                    {
                        AddIssue(new BaZicParserException(expressionLine, expressionColumn, expressionStartOffset, expressionParsedLength, L.BaZic.Parser.Expressions.MethodNameExpected));
                    }
                }
                else
                {
                    AddIssue(new BaZicParserException(expressionLine, expressionColumn, expressionStartOffset, expressionParsedLength, L.BaZic.Parser.Expressions.MethodNameExpected));
                }

                // Bracket_Expression*
                do
                {
                    var bracketToken = CurrentToken;
                    bracketExpression = ParseBracketExpression();
                    if (bracketExpression != null)
                    {
                        var referenceExpression = expression as ReferenceExpression;

                        if (referenceExpression == null)
                        {
                            AddIssue(new BaZicParserException(expressionLine, expressionColumn, expressionStartOffset, expressionParsedLength, L.BaZic.Parser.Expressions.UnexpectedIndexer));
                        }

                        if (bracketExpression.Length == 0)
                        {
                            AddIssue(new BaZicParserException(bracketToken.Line, bracketToken.Column, expressionStartOffset, expressionParsedLength, L.BaZic.Parser.Expressions.IndexerExpected));
                        }

                        var arrayIndexer = new ArrayIndexerExpression(referenceExpression, bracketExpression)
                        {
                            Line = bracketToken.Line,
                            Column = bracketToken.Column,
                            StartOffset = bracketToken.StartOffset,
                            NodeLength = bracketToken.ParsedLength
                        };

                        ValidateArrayIndexerExpression(arrayIndexer);
                        expression = arrayIndexer;
                    }
                } while (bracketExpression != null);
            }

            return expression;
        }

        /// <summary>
        /// Parse an expression that can be either a primitive value, a variable reference, an expression between parenthesis or instantiation.
        /// 
        /// Corresponding grammar :
        ///     Primitive_Value
        ///     | Identifier
        ///     | '(' Expression ')'
        ///     | 'NEW' (Bracket_Expressio | Namespace_Or_Type_Name Method_Invocation)
        /// </summary>
        /// <param name="isRequired">Defines whether it is required/expected to parse an expression. If true, throw an exception if no expression is parsed.</param>
        /// <returns>Returns an expression that corresponds to a primitive value, a variable reference, an expression between parenthesis or instantiation.</returns>
        private Expression ParsePrimaryExpressionStart(bool isRequired)
        {
            if (TokenIdentificationHelper.IsPrimitiveValue(CurrentToken))
            {
                return ParsePrimitiveExpression();
            }
            else if (CurrentToken.TokenType == TokenType.Exception)
            {
                var exceptionToken = CurrentToken;
                if (GetCatchIndicator() < 1)
                {
                    AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Expressions.IllegalExceptionKeyword));
                }
                DiscardToken();
                return new ExceptionReferenceExpression()
                {
                    Line = exceptionToken.Line,
                    Column = exceptionToken.Column,
                    StartOffset = exceptionToken.StartOffset,
                    NodeLength = exceptionToken.ParsedLength
                };
            }
            else if (CurrentToken.TokenType == TokenType.Identifier)
            {
                var identifierToken = CurrentToken;
                TokenIdentificationHelper.CheckIdentifier(CurrentToken.Value, CurrentToken, false, _issues);
                var variableReference = new VariableReferenceExpression(CurrentToken.Value)
                {
                    Line = identifierToken.Line,
                    Column = identifierToken.Column,
                    StartOffset = identifierToken.StartOffset,
                    NodeLength = identifierToken.ParsedLength
                };

                if (NextToken.TokenType != TokenType.LeftParenth)
                {
                    // We validate the variable reference only if it looks like it's not a method invocation.
                    if (ValidateVariableReferenceExpression(variableReference, throwIssueForVariableNotFound: false) == null)
                    {
                        // If the variable does not exists, it's maybe a reference to a Class.
                        return ParseStaticPropertyOrMethod();
                    }
                }

                DiscardToken();
                return variableReference;
            }
            else if (CurrentToken.TokenType == TokenType.LeftParenth)
            {
                DiscardToken();
                var expression = ParseExpression(true, TokenType.RightParenth);
                DiscardToken(TokenType.RightParenth);
                return expression;
            }
            else if (CurrentToken.TokenType == TokenType.New)
            {
                DiscardToken();

                if (CurrentToken.TokenType == TokenType.LeftBracket)
                {
                    // Array creation
                    var bracketToken = CurrentToken;
                    var bracketExpression = ParseBracketExpression();
                    return new ArrayCreationExpression()
                    {
                        Line = bracketToken.Line,
                        Column = bracketToken.Column,
                        StartOffset = bracketToken.StartOffset,
                        NodeLength = bracketToken.ParsedLength
                    }
                    .WithValues(bracketExpression);
                }
                else
                {
                    // Type creation
                    var typeName = ParseNamespaceOrTypeName();

                    if (typeName == null)
                    {
                        AddIssue(new BaZicParserException(PreviousToken.Line, PreviousToken.Column, PreviousToken.StartOffset, PreviousToken.ParsedLength, L.BaZic.Parser.Expressions.TypeExpected));
                    }

                    if (CurrentToken.TokenType != TokenType.LeftParenth)
                    {
                        AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Expressions.InstantiateExpectParenths));
                    }

                    var arguments = ParseMethodInvocation();
                    return new InstantiateExpression(typeName)
                    {
                        Line = typeName.Line,
                        Column = typeName.Column,
                        StartOffset = typeName.StartOffset,
                        NodeLength = typeName.NodeLength
                    }
                    .WithParameters(arguments);
                }
            }
            else if (TokenIdentificationHelper.IsStatementSeparator(CurrentToken) && isRequired)
            {
                AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Expressions.ExpressionOnOneLine));
            }

            return null;
        }

        /// <summary>
        /// Try to parse a primitive value.
        /// 
        /// Corresponding grammar :
        ///     'NULL'
        ///     | 'TRUE'
        ///     | 'FALSE'
        ///     | String
        ///     | (+|-)?Integer.Integer
        ///     | (+|-)?Integer
        /// </summary>
        /// <returns>If succeed, returns a <see cref="PrimitiveExpression"/>.</returns>
        private PrimitiveExpression ParsePrimitiveExpression()
        {
            PrimitiveExpression primitiveValue;
            switch (CurrentToken.TokenType)
            {
                case TokenType.Null:
                    primitiveValue = new PrimitiveExpression();
                    break;

                case TokenType.Integer:
                    primitiveValue = new PrimitiveExpression(int.Parse(CurrentToken.Value));
                    break;

                case TokenType.Double:
                    primitiveValue = new PrimitiveExpression(double.Parse(CurrentToken.Value));
                    break;

                case TokenType.String:
                    primitiveValue = new PrimitiveExpression(FormatVerbatimString(CurrentToken.Value));
                    break;

                case TokenType.True:
                case TokenType.False:
                    primitiveValue = new PrimitiveExpression(bool.Parse(CurrentToken.Value));
                    break;

                default:
                    AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Expressions.NotPrimitive));
                    DiscardToken();
                    return null;
            }

            primitiveValue.Line = CurrentToken.Line;
            primitiveValue.Column = CurrentToken.Column;
            primitiveValue.StartOffset = CurrentToken.StartOffset;
            primitiveValue.NodeLength = CurrentToken.ParsedLength;

            DiscardToken();
            return primitiveValue;
        }

        /// <summary>
        /// Parse a static reference to a property or a static method invocation.
        /// 
        /// Corresponding grammar :
        ///     Namespace_Or_Type_Name (Member_Access | Method_Invocation)
        /// </summary>
        /// <returns>Returns a <see cref="PropertyReferenceExpression"/> or <see cref="InvokeCoreMethodExpression"/>. Returns null if it doesn't looks like a valid reference or invocation.</returns>
        private Expression ParseStaticPropertyOrMethod()
        {
            var identifierToken = CurrentToken;
            if (CurrentToken.TokenType != TokenType.Identifier)
            {
                AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Expressions.TypeExpected));
            }

            var typeName = ParseNamespaceOrTypeName();

            if (typeName == null)
            {
                AddIssue(new BaZicParserException(identifierToken.Line, identifierToken.Column, identifierToken.StartOffset, identifierToken.ParsedLength, L.BaZic.Optimizer.FormattedUndeclaredName(identifierToken.Value)));
                return null;
            }

            if (!typeName.Namespace.Contains("."))
            {
                AddIssue(new BaZicParserException(typeName.Line, typeName.Column, typeName.StartOffset, typeName.NodeLength, L.BaZic.Parser.Expressions.FormattedInvalidNamespaceOrVariable(typeName.Namespace)));
                return null;
            }

            var lastDotIndex = typeName.Namespace.LastIndexOf('.');
            var propertyOrMethodName = typeName.ClassName;
            var className = typeName.Namespace.Substring(lastDotIndex + 1);
            var @namespace = typeName.Namespace.Substring(0, lastDotIndex);

            if (CurrentToken.TokenType == TokenType.LeftParenth)
            {
                var invocationToken = PreviousToken;
                var arguments = ParseMethodInvocation();
                var methodInvoke = new InvokeCoreMethodExpression(new ClassReferenceExpression(@namespace, className), propertyOrMethodName.Identifier, false)
                {
                    Line = invocationToken.Line,
                    Column = invocationToken.Column,
                    StartOffset = invocationToken.StartOffset,
                    NodeLength = invocationToken.ParsedLength
                };
                methodInvoke.WithParameters(arguments);

                ValidateCoreMethodInvocation(methodInvoke);
                return methodInvoke;
            }

            return new PropertyReferenceExpression(new ClassReferenceExpression(@namespace, className), propertyOrMethodName.Identifier)
            {
                Line = PreviousToken.Line,
                Column = PreviousToken.Column,
                StartOffset = PreviousToken.StartOffset,
                NodeLength = PreviousToken.ParsedLength
            };
        }

        /// <summary>
        /// Parse a namespace and/or a full type name.
        /// 
        /// Corresponding grammar :
        ///     Identifier Member_Access*
        /// </summary>
        /// <returns>Returns a namespace and/or type name represented by a <see cref="ClassReferenceExpression"/>.</returns>
        private ClassReferenceExpression ParseNamespaceOrTypeName()
        {
            if (CurrentToken.TokenType != TokenType.Identifier)
            {
                AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Expressions.TypeExpected));
            }

            var memberPart = string.Empty;
            var typeFullName = string.Empty;
            var identifierToken = CurrentToken;
            TokenIdentificationHelper.CheckIdentifier(CurrentToken.Value, CurrentToken, false, _issues);
            typeFullName += CurrentToken.Value;
            DiscardToken();

            var typeNameFound = false;
            do
            {
                memberPart = ParseMemberAccessPart(false);
                if (!string.IsNullOrEmpty(memberPart))
                {
                    typeFullName += "." + memberPart;
                }
                else
                {
                    typeNameFound = true;
                }
            } while (!typeNameFound);

            var @namespace = string.Empty;
            var className = string.Empty;
            var typeLimit = typeFullName.LastIndexOf(".", StringComparison.Ordinal);

            if (typeLimit != -1)
            {
                @namespace = typeFullName.Substring(0, typeLimit);
                className = typeFullName.Substring(typeLimit + 1);
            }
            else
            {
                return null;
            }

            return new ClassReferenceExpression(@namespace, className)
            {
                Line = identifierToken.Line,
                Column = identifierToken.Column,
                StartOffset = identifierToken.StartOffset,
                NodeLength = identifierToken.ParsedLength
            };
        }

        /// <summary>
        /// Parse the access to a member. It could be a property or a method. This method only returns the parsed member name. The caller method will determines whether this member is a property of method invocation.
        /// 
        /// Corresponding grammar :
        ///     '.' Identifier
        /// </summary>
        /// <returns>Returns a member name.</returns>
        private string ParseMemberAccessPart(bool isExpected)
        {
            if (CurrentToken.TokenType == TokenType.Dot)
            {
                var identifier = NextToken.Value;
                DiscardToken();
                if (CurrentToken.TokenType != TokenType.Identifier && CurrentToken.TokenType != TokenType.Exception)
                {
                    AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Expressions.ReferenceExpected));
                }
                else
                {
                    DiscardToken();
                    TokenIdentificationHelper.CheckIdentifier(identifier, PreviousToken, false, _issues);
                    return identifier;
                }
            }

            if (isExpected)
            {
                AddIssue(new BaZicParserException(CurrentToken.Line, CurrentToken.Column, CurrentToken.StartOffset, CurrentToken.ParsedLength, L.BaZic.Parser.Expressions.MemberAccessExpected));
            }

            return string.Empty;
        }

        /// <summary>
        /// Parse a list of expressions between parenthesis. This normally corresponds to a method invocation.
        /// 
        /// Corresponding grammar :
        ///     '(' Expression_List ')'
        /// </summary>
        /// <returns>Returns an array of expressions.</returns>
        private Expression[] ParseMethodInvocation()
        {
            if (CurrentToken.TokenType != TokenType.LeftParenth)
            {
                return Array.Empty<Expression>();
            }

            DiscardToken(TokenType.LeftParenth);
            var result = ParseExpressionList();
            DiscardToken(TokenType.RightParenth);
            return result;
        }

        /// <summary>
        /// Parse a list of expressions between brackets. This normally corresponds to the access to an array's item or the creation of a new array.
        /// 
        /// Corresponding grammar :
        ///     '[' Expression_List ']'
        /// </summary>
        /// <returns>Returns an array of expressions.</returns>
        private Expression[] ParseBracketExpression()
        {
            if (CurrentToken.TokenType != TokenType.LeftBracket)
            {
                return null;
            }

            DiscardToken(TokenType.LeftBracket);
            var result = ParseExpressionList();
            DiscardToken(TokenType.RightBracket);
            return result;
        }

        /// <summary>
        /// Parse a list of expression separated by a comma. This could be included between brackets or parenthesis.
        /// 
        /// Corresponding grammar :
        ///     Expression? ( ',' Expression)*
        /// </summary>
        /// <returns>Returns an array of expressions.</returns>
        private Expression[] ParseExpressionList()
        {
            var list = new List<Expression>();

            Expression expression = null;
            do
            {
                expression = ParseExpression(false, TokenType.Comma, TokenType.RightBracket, TokenType.RightParenth);
                if (expression != null)
                {
                    list.Add(expression);

                    if (CurrentToken.TokenType == TokenType.Comma)
                    {
                        DiscardToken();
                    }
                }
            } while (expression != null);

            return list.ToArray();
        }

        /// <summary>
        /// Transform a literal string to a verbatim string.
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>A literal string.</returns>
        private string FormatVerbatimString(string input)
        {
            return input.Replace("\\\"", "\"").Replace("\\r", "\r").Replace("\\n", "\n").Replace("\\\\", "\\");
        }

        #endregion
    }
}
