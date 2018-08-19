using BaZic.Runtime.BaZic.Code.Lexer;
using BaZic.Runtime.BaZic.Code.Lexer.Tokens;
using BaZic.Runtime.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BaZic.Runtime.BaZic.Code.Parser
{
    /// <summary>
    /// Provides a set of functions designed to help to identify a token in a category
    /// </summary>
    internal static class TokenIdentificationHelper
    {
        #region Fields & Constants

        private static TokenType[] statementSeparatorsTokens;
        private static TokenType[] statementSeparatorsTokensExcludingEndCode;
        private static TokenType[] primitiveValueTokens;
        private static TokenType[] expressionBeginningTokens;
        private static TokenType[] binaryOperatorTokens;
        private static TokenType[] operationTokens;
        private static TokenType[] restrictedTokens;

        #endregion

        #region Methods

        /// <summary>
        /// Retrieves the list of statement separators.
        /// </summary>
        /// <returns>An array of <see cref="TokenType"/>.</returns>
        internal static TokenType[] GetStatementSeparatorsTokens()
        {
            if (statementSeparatorsTokens == null)
            {
                statementSeparatorsTokens = new TokenType[]
                {
                    TokenType.NewLine,
                    TokenType.StartCode,
                    TokenType.EndCode,
                    TokenType.Comment
                };
            }

            return statementSeparatorsTokens;
        }

        /// <summary>
        /// Retrieves the list of statement separators.
        /// </summary>
        /// <returns>An array of <see cref="TokenType"/>.</returns>
        internal static TokenType[] GetStatementSeparatorsTokensExcludingEndCode()
        {
            if (statementSeparatorsTokensExcludingEndCode == null)
            {
                statementSeparatorsTokensExcludingEndCode = new TokenType[]
                {
                    TokenType.NewLine,
                    TokenType.StartCode,
                    TokenType.Comment
                };
            }

            return statementSeparatorsTokensExcludingEndCode;
        }

        /// <summary>
        /// Retrieves the list of primitive value token type.
        /// </summary>
        /// <returns>An array of <see cref="TokenType"/>.</returns>
        internal static TokenType[] GetPrimitiveValueTokens()
        {
            if (primitiveValueTokens == null)
            {
                primitiveValueTokens = new TokenType[]
                {
                    TokenType.Integer,
                    TokenType.Double,
                    TokenType.True,
                    TokenType.False,
                    TokenType.String,
                    TokenType.Null
                };
            }

            return primitiveValueTokens;
        }

        /// <summary>
        /// Retrieves the list of token type that refer to the beginning of an expression.
        /// </summary>
        /// <returns>An array of <see cref="TokenType"/>.</returns>
        internal static TokenType[] GetExpressionBeginningTokens()
        {
            if (expressionBeginningTokens == null)
            {
                expressionBeginningTokens = GetPrimitiveValueTokens().Concat(new TokenType[]
                {
                    TokenType.LeftBracket,
                    TokenType.LeftParenth,
                    TokenType.Identifier,
                    TokenType.New,
                    TokenType.Await
                }).ToArray();
            }

            return expressionBeginningTokens;
        }

        /// <summary>
        /// Retrieves the list of binary operator tokens.
        /// </summary>
        /// <returns>An array of <see cref="TokenType"/>.</returns>
        internal static TokenType[] GetBinaryOperatorTokens()
        {
            if (binaryOperatorTokens == null)
            {
                binaryOperatorTokens = new TokenType[]
                {
                    TokenType.And,
                    TokenType.Equal,
                    TokenType.GreaterThan,
                    TokenType.LesserThan,
                    TokenType.Or
                };
            }

            return binaryOperatorTokens;
        }

        /// <summary>
        /// Retrieves the list of mathematical operation token type.
        /// </summary>
        /// <returns>An array of <see cref="TokenType"/>.</returns>
        internal static TokenType[] GetOperationTokens()
        {
            if (operationTokens == null)
            {
                operationTokens = new TokenType[]
                {
                    TokenType.Asterisk,
                    TokenType.Minus,
                    TokenType.Percent,
                    TokenType.Plus,
                    TokenType.Slash
                };
            }

            return operationTokens;
        }

        /// <summary>
        /// Retrieves the list of keywords that cannot be used as an identifier.
        /// </summary>
        /// <returns>An array of <see cref="TokenType"/>.</returns>
        internal static TokenType[] GetRestrictedTokens()
        {
            if (restrictedTokens == null)
            {
                restrictedTokens = new TokenType[]
                {
                    TokenType.And,
                    TokenType.Async,
                    TokenType.Await,
                    TokenType.Break,
                    TokenType.Breakpoint,
                    TokenType.Catch,
                    TokenType.Do,
                    TokenType.Else,
                    TokenType.Event,
                    TokenType.False,
                    TokenType.Function,
                    TokenType.If,
                    TokenType.Loop,
                    TokenType.New,
                    TokenType.Not,
                    TokenType.Or,
                    TokenType.Return,
                    TokenType.Then,
                    TokenType.Throw,
                    TokenType.True,
                    TokenType.Try,
                    TokenType.Variable
                };
            }

            return restrictedTokens;
        }

        /// <summary>
        /// Determines whether the specified token is a primitive value
        /// </summary>
        /// <param name="token">The token to check.</param>
        /// <returns>Returns True if the token is a primitive value.</returns>
        internal static bool IsPrimitiveValue(Token token)
        {
            var ttype = token.TokenType;
            return GetPrimitiveValueTokens().Any(t => t == ttype);
        }

        /// <summary>
        /// Determines whether the specified token is a logic operator (and, equal, greater than, lesser than, or)
        /// </summary>
        /// <param name="token">The token to check.</param>
        /// <returns>Returns True if the token is a logic operator.</returns>
        internal static bool IsOperator(Token token)
        {
            var ttype = token.TokenType;
            return GetBinaryOperatorTokens().Concat(GetOperationTokens()).Any(t => t == ttype);
        }

        /// <summary>
        /// Determines whether the specified token is a statement separator (beginning/end of the code, comment or new line)
        /// </summary>
        /// <param name="token">The token to check.</param>
        /// <returns>Returns True if the token is a statement separator.</returns>
        internal static bool IsStatementSeparator(Token token)
        {
            var ttype = token.TokenType;
            return GetStatementSeparatorsTokens().Any(t => t == ttype);
        }

        /// <summary>
        /// Determines whether the specified token is a statement separator (beginning of the code, comment or new line, but not end of code)
        /// </summary>
        /// <param name="token">The token to check.</param>
        /// <returns>Returns True if the token is a statement separator.</returns>
        internal static bool IsStatementSeparatorButNotEndCode(Token token)
        {
            var ttype = token.TokenType;
            return GetStatementSeparatorsTokensExcludingEndCode().Any(t => t == ttype);
        }

        /// <summary>
        /// Determines whether the specified token is a keyword that cannot be used as an identifier.
        /// </summary>
        /// <param name="token">The token to check.</param>
        /// <returns>Returns True if the token is a restricted keyword.</returns>
        internal static bool IsRestrictedKeyword(Token token)
        {
            var ttype = token.TokenType;
            return GetRestrictedTokens().Any(t => t == ttype);
        }

        /// <summary>
        /// Check whether an identifier has a good syntax.
        /// </summary>
        /// <param name="identifier">The identifier</param>
        /// <param name="token">The token to check.</param>
        /// <param name="isEvent">Defines whether the identifier is expected to be an identifier in a Event function.</param>
        /// <param name="issues">A reference to the list of issues that must be used in case of problem with the identifier.</param>
        internal static void CheckIdentifier(string identifier, Token token, bool isEvent, List<Exception> issues)
        {
            var ttype = token.TokenType;
            if (ttype != TokenType.Identifier && ttype != TokenType.Exception)
            {
                if (IsRestrictedKeyword(token))
                {
                    issues.Add(new BaZicParserException(token.Line, token.Column, token.StartOffset, token.ParsedLength, L.BaZic.TokenIdentificationHelper.FormattedRestrictedToken(token.Value)));
                    return;
                }

                issues.Add(new BaZicParserException(token.Line, token.Column, token.StartOffset, token.ParsedLength, L.BaZic.TokenIdentificationHelper.IdentifierExpected));
                return;
            }

            if (!identifier.All(c => char.IsLetterOrDigit(c) || c == '_'))
            {
                issues.Add(new BaZicParserException(token.Line, token.Column, token.StartOffset, token.ParsedLength, L.BaZic.TokenIdentificationHelper.InvalidIdentifier));
                return;
            }

            if (isEvent)
            {
                if (identifier.StartsWith("_") || identifier.EndsWith("_") || identifier.Count(c => c == '_') != 1)
                {
                    issues.Add(new BaZicParserException(token.Line, token.Column, token.StartOffset, token.ParsedLength, L.BaZic.TokenIdentificationHelper.InvalidEventName));
                    return;
                }
            }
        }

        #endregion
    }
}
