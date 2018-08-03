using System;

namespace BaZic.Core.Exceptions
{
    /// <summary>
    /// Represents an exception thrown when a null value is not expected at a moment of the execution.
    /// </summary>
    internal sealed class BadTypeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BadTypeException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        internal BadTypeException(string message)
            : base(message)
        {
        }
    }
}
