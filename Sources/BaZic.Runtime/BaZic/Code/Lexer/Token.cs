using BaZic.Runtime.BaZic.Code.Lexer.Tokens;

namespace BaZic.Runtime.BaZic.Code.Lexer
{
    /// <summary>
    /// Represents a detected token in a BaZic code.
    /// </summary>
    public sealed class Token
    {
        #region Properties

        /// <summary>
        /// Gets the type of token.
        /// </summary>
        public TokenType TokenType { get; }

        /// <summary>
        /// Gets the value associated to the token.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets the line where the token has been detected.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Gets the column in the line where the token has been detected.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Gets the character number where the token starts.
        /// </summary>
        public int StartOffset { get; }

        /// <summary>
        /// Gets or sets the length that has been parsed to match.
        /// </summary>
        public int ParsedLength { get; set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Token"/> class.
        /// </summary>
        /// <param name="tokenType">The type of token</param>
        /// <param name="value">The associated value</param>
        /// <param name="line">The line where the token has been detected</param>
        /// <param name="column">The column in the line where the token has been detected</param>
        /// <param name="startOffset">The character number where the token starts.</param>
        /// <param name="parsedLength">The length that has been parsed to match.</param>
        public Token(TokenType tokenType, string value, int line, int column, int startOffset, int parsedLength)
        {
            TokenType = tokenType;
            Value = value;
            Line = line;
            Column = column;
            StartOffset = startOffset;
            ParsedLength = parsedLength;
        }

        #endregion
    }
}
