using System;

namespace BaZic.Runtime.BaZic.Code.Lexer.Tokens
{
    /// <summary>
    /// Provides a token definition for a comment.
    /// </summary>
    internal sealed class CommentTokenDefinition : TokenDefinition
    {
        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentTokenDefinition"/> class.
        /// </summary>
        internal CommentTokenDefinition()
            : base(TokenType.Comment)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        internal override TokenMatch Match(BaZicLexer lexer, int startIndex, TokenType previousTokenType)
        {
            if (lexer.InputCode.IndexOf("#", startIndex, StringComparison.OrdinalIgnoreCase) == startIndex)
            {
                var endLine = lexer.InputCode.IndexOf("\n", startIndex, StringComparison.OrdinalIgnoreCase);
                var value = string.Empty;
                if (endLine == -1)
                {
                    value = lexer.InputCode;
                }
                else
                {
                    value = lexer.InputCode.Substring(startIndex, endLine - startIndex);
                }

                return new TokenMatch
                {
                    IsMatch = true,
                    TokenType = TokenType.Comment,
                    Value = value,
                    ParsedLength = value.Length
                };
            }

            return new TokenMatch
            {
                IsMatch = false
            };
        }

        #endregion
    }
}
