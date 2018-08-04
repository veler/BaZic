using BaZic.Core.Enums;
using System;

namespace BaZic.Runtime.BaZic.Code
{
    /// <summary>
    /// Represents a BaZic parsing error.
    /// </summary>
    public sealed class BaZicParserException : Exception
    {
        #region Properties

        /// <summary>
        /// Gets the line in the code where the exception happened.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Gets the column in the line in the code where the exception happened.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Gets the character number in the code where the error starts.
        /// </summary>
        public int StartOffset { get; }

        /// <summary>
        /// Gets the length of the text that represents the error.
        /// </summary>
        public int ErrorLength { get; }

        /// <summary>
        /// Gets the importance level of the exception.
        /// </summary>
        public BaZicParserExceptionLevel Level { get; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BaZicParserException"/> class.
        /// </summary>
        /// <param name="message">The message related to the exception.</param>
        public BaZicParserException(string message)
            : base(message)
        {
            Line = -1;
            Column = -1;
            StartOffset = -1;
            ErrorLength = 1;
            Level = BaZicParserExceptionLevel.Error;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaZicParserException"/> class.
        /// </summary>
        /// <param name="line">The line in the code where the exception happened.</param>
        /// <param name="column">The column in the line in the code where the exception happened.</param>
        /// <param name="startOffset">The character number in the code where the error starts.</param>
        /// <param name="errorLength">The length of the text that represents the error.</param>
        /// <param name="message">The message related to the exception.</param>
        public BaZicParserException(int line, int column, int startOffset, int errorLength, string message)
            : base(message)
        {
            Line = line;
            Column = column;
            StartOffset = startOffset;
            ErrorLength = errorLength;
            Level = BaZicParserExceptionLevel.Error;

            if (StartOffset > 0 && ErrorLength == 0)
            {
                StartOffset--;
                ErrorLength++;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaZicParserException"/> class.
        /// </summary>
        /// <param name="line">The line in the code where the exception happened.</param>
        /// <param name="column">The column in the line in the code where the exception happened.</param>
        /// <param name="startOffset">The character number in the code where the error starts.</param>
        /// <param name="errorLength">The length of the text that represents the error.</param>
        /// <param name="level">The importance level of the exception.</param>
        /// <param name="message">The message related to the exception.</param>
        public BaZicParserException(int line, int column, int startOffset, int errorLength, BaZicParserExceptionLevel level, string message)
            : base(message)
        {
            Line = line;
            Column = column;
            StartOffset = startOffset;
            ErrorLength = errorLength;
            Level = level;

            if (StartOffset > 0 && ErrorLength == 0)
            {
                StartOffset--;
                ErrorLength++;
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override string ToString()
        {
            return Localization.L.BaZic.Parser.FormattedParserError(Line, Column, Level.ToString(), Message);
        }

        #endregion
    }
}
