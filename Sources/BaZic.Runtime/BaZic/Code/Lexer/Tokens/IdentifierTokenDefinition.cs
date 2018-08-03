namespace BaZic.Runtime.BaZic.Code.Lexer.Tokens
{
    /// <summary>
    /// Provides a token definition for an identifier.
    /// </summary>
    internal sealed class IdentifierTokenDefinition : TokenDefinition
    {
        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentifierTokenDefinition"/> class.
        /// </summary>
        internal IdentifierTokenDefinition()
            : base(TokenType.Identifier)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        internal override TokenMatch Match(BaZicLexer lexer, int startIndex, TokenType previousTokenType)
        {
            if (!char.IsLetter(lexer.InputCode[startIndex]) && lexer.InputCode[startIndex] != '_')
            {
                return new TokenMatch
                {
                    IsMatch = false
                };
            }

            var closestSeparatorIndex = lexer.InputCode.Length;

            var index = lexer.InputCode.IndexOfAny(SymbolHelper.SymbolSeparators, startIndex);
            if (index > startIndex && index < closestSeparatorIndex)
            {
                closestSeparatorIndex = index;
            }

            if (closestSeparatorIndex > startIndex)
            {
                var value = lexer.InputCode.Substring(startIndex, closestSeparatorIndex - startIndex);

                return new TokenMatch
                {
                    IsMatch = true,
                    TokenType = TokenType.Identifier,
                    Value = value.ToString(),
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
