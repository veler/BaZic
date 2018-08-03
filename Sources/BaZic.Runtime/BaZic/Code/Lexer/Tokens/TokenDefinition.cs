using BaZic.Core.ComponentModel;
using BaZic.Runtime.Localization;
using System;
using System.Linq;

namespace BaZic.Runtime.BaZic.Code.Lexer.Tokens
{
    /// <summary>
    /// Define a token and the required information to match.
    /// </summary>
    internal class TokenDefinition
    {
        #region Fields & Constants

        private string _keyword;
        private int _keywordLength;
        private bool _expectSpaceAfter;
        private bool _keepOriginalValue;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of the token.
        /// </summary>
        protected TokenType TokenType { get; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenDefinition"/> class.
        /// </summary>
        /// <param name="tokenType">The type of the token.</param>
        internal protected TokenDefinition(TokenType tokenType)
        {
            TokenType = tokenType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenDefinition"/> class.
        /// </summary>
        /// <param name="tokenType">The type of the token.</param>
        /// <param name="keyword">The keyword to detect</param>
        /// <param name="expectSpaceAfter">Defines whether a whitespace must be found after the keyword to consider it as a match</param>
        /// <param name="keepOriginalValue">Defines whether instead of keeping in memory the provided keyword, the original text from the code must be kept.</param>
        internal TokenDefinition(TokenType tokenType, string keyword, bool expectSpaceAfter = true, bool keepOriginalValue = false)
            : this(tokenType)
        {
            Requires.NotNullOrEmpty(keyword, nameof(keyword));

            _keyword = keyword.ToUpperInvariant();
            _keywordLength = keyword.Length;
            _expectSpaceAfter = expectSpaceAfter;
            _keepOriginalValue = keepOriginalValue;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Try to match the current token with the code
        /// </summary>
        /// <param name="lexer">The instance of the <see cref="BaZicLexer"/> that contains the input code.</param>
        /// <param name="startIndex">The index in the input code where the search must start.</param>
        /// <param name="previousTokenType">The token type that has been detected previously this match tentative</param>
        /// <returns>A <see cref="TokenMatch"/> that contains information about the match result.</returns>
        internal virtual TokenMatch Match(BaZicLexer lexer, int startIndex, TokenType previousTokenType)
        {
            if (string.IsNullOrEmpty(_keyword))
            {
                throw new NotSupportedException(L.BaZic.Lexer.NoMatchImplemented);
            }

            if (lexer.InputCode.IndexOf(_keyword, startIndex, StringComparison.OrdinalIgnoreCase) == startIndex)
            {
                if (_expectSpaceAfter)
                {
                    var separatorIndex = startIndex + _keywordLength;
                    if (lexer.InputCode.Length == separatorIndex || SymbolHelper.Separators.Any(c => char.Parse(c) == lexer.InputCode[separatorIndex]))
                    {
                        var value = _keyword;
                        if (_keepOriginalValue)
                        {
                            value = lexer.InputCode.Substring(startIndex, _keyword.Length);
                        }

                        return new TokenMatch
                        {
                            IsMatch = true,
                            TokenType = TokenType,
                            Value = value,
                            ParsedLength = _keywordLength
                        };
                    }
                }
                else
                {
                    var value = _keyword;
                    if (_keepOriginalValue)
                    {
                        value = lexer.InputCode.Substring(startIndex, _keyword.Length);
                    }

                    return new TokenMatch
                    {
                        IsMatch = true,
                        TokenType = TokenType,
                        Value = value,
                        ParsedLength = _keywordLength
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
