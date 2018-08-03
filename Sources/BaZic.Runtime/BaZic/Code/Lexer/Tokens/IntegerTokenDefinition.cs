using System;
using System.Globalization;

namespace BaZic.Runtime.BaZic.Code.Lexer.Tokens
{
    /// <summary>
    /// Provides a token definition for an integer value.
    /// </summary>
    internal sealed class IntegerTokenDefinition : TokenDefinition
    {
        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegerTokenDefinition"/> class.
        /// </summary>
        internal IntegerTokenDefinition()
            : base(TokenType.Integer)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        internal override TokenMatch Match(BaZicLexer lexer, int startIndex, TokenType previousTokenType)
        {
            var closestSeparatorIndex = lexer.InputCode.Length;

            var index = lexer.InputCode.IndexOfAny(SymbolHelper.SymbolSeparators, startIndex + 1);
            if (index > startIndex && index < closestSeparatorIndex)
            {
                closestSeparatorIndex = index;
            }

            if (closestSeparatorIndex > startIndex)
            {
                var valueString = lexer.InputCode.Substring(startIndex, closestSeparatorIndex - startIndex);

                var value = 0;
                if (int.TryParse(valueString, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value))
                {
                    if ((previousTokenType == TokenType.Identifier || previousTokenType == TokenType.Integer || previousTokenType == TokenType.Double || previousTokenType == TokenType.String) && (valueString.StartsWith("+", StringComparison.Ordinal) || valueString.StartsWith("-", StringComparison.Ordinal)))
                    {
                        // It's probably a case where we have "1+1" and the current input code is "+1".
                        // We want to consider a + as a Plus instead of a positive number.
                        return new TokenMatch
                        {
                            IsMatch = false
                        };
                    }

                    return new TokenMatch
                    {
                        IsMatch = true,
                        TokenType = TokenType.Integer,
                        Value = value.ToString(),
                        ParsedLength = closestSeparatorIndex - startIndex
                    };
                }
            }

            return new TokenMatch
            {
                IsMatch = false
            };
        }

        #endregion
    }
}
