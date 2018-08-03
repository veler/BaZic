using System;
using System.Globalization;
using System.Linq;

namespace BaZic.Runtime.BaZic.Code.Lexer.Tokens
{
    /// <summary>
    /// Provides a token definition for a double numeric value.
    /// </summary>
    internal sealed class DoubleTokenDefinition : TokenDefinition
    {
        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleTokenDefinition"/> class.
        /// </summary>
        internal DoubleTokenDefinition()
            : base(TokenType.Double)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        internal override TokenMatch Match(BaZicLexer lexer, int startIndex, TokenType previousTokenType)
        {
            var closestSeparatorIndex = lexer.InputCode.Length;

            var index = lexer.InputCode.IndexOfAny(SymbolHelper.SymbolSeparators.Except(new char[] { '.' }).ToArray(), startIndex + 1);
            if (index > startIndex && index < closestSeparatorIndex)
            {
                closestSeparatorIndex = index;
            }

            if (closestSeparatorIndex > startIndex)
            {
                var valueString = lexer.InputCode.Substring(startIndex, closestSeparatorIndex - startIndex);
                var value = 0.0;
                if (double.TryParse(valueString, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value))
                {
                    if (!valueString.ToString().Contains("."))
                    {
                        return new TokenMatch
                        {
                            IsMatch = false
                        };
                    }
                    else if ((previousTokenType == TokenType.Identifier || previousTokenType == TokenType.Integer || previousTokenType == TokenType.Double || previousTokenType == TokenType.String) && (valueString.StartsWith("+", StringComparison.Ordinal) || valueString.StartsWith("-", StringComparison.Ordinal)))
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
                        TokenType = TokenType.Double,
                        Value = value.ToString(CultureInfo.InvariantCulture),
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
