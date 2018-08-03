using System;

namespace BaZic.Runtime.BaZic.Code.Lexer.Tokens
{
    /// <summary>
    /// Provides a token definition for a string between quotes.
    /// </summary>
    internal sealed class StringTokenDefinition : TokenDefinition
    {
        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StringTokenDefinition"/> class.
        /// </summary>
        internal StringTokenDefinition()
            : base(TokenType.String)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        internal override TokenMatch Match(BaZicLexer lexer, int startIndex, TokenType previousTokenType)
        {
            if (lexer.InputCode.IndexOf("\"", startIndex, StringComparison.OrdinalIgnoreCase) == startIndex)
            {
                var nextSearchStart = startIndex + 1;
                var endQuoteIndex = -1;

                do
                {
                    endQuoteIndex = lexer.InputCode.IndexOf("\"", nextSearchStart, StringComparison.OrdinalIgnoreCase);
                    if (endQuoteIndex > nextSearchStart)
                    {
                        if (lexer.InputCode[endQuoteIndex - 1] == '\\')
                        {
                            nextSearchStart = endQuoteIndex + 1;
                            endQuoteIndex = -1;
                        }
                    }
                    else if (endQuoteIndex == -1)
                    {
                        nextSearchStart = lexer.InputCode.Length;
                    }
                }
                while (endQuoteIndex == -1 && nextSearchStart < lexer.InputCode.Length);

                if (endQuoteIndex > -1)
                {
                    return new TokenMatch
                    {
                        IsMatch = true,
                        TokenType = TokenType.String,
                        Value = lexer.InputCode.Substring(startIndex + 1, endQuoteIndex - startIndex - 1),
                        ParsedLength = 1 + endQuoteIndex - startIndex
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
