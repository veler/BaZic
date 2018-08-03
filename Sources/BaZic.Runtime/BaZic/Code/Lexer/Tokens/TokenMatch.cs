namespace BaZic.Runtime.BaZic.Code.Lexer.Tokens
{
    /// <summary>
    /// Represents the result of a token match.
    /// </summary>
    internal sealed class TokenMatch
    {
        #region Properties

        /// <summary>
        /// Gets or sets whether the token matched.
        /// </summary>
        internal bool IsMatch { get; set; }

        /// <summary>
        /// Gets or sets the matched token type.
        /// </summary>
        internal TokenType TokenType { get; set; }

        /// <summary>
        /// Gets or sets the matched value.
        /// </summary>
        internal string Value { get; set; }

        /// <summary>
        /// Gets or sets the length that has been parsed to match.
        /// </summary>
        internal int ParsedLength { get; set; }

        #endregion
    }
}
