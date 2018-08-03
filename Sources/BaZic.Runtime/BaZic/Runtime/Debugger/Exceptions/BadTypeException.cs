using System;
using System.Runtime.Serialization;

namespace BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions
{
    /// <summary>
    /// Represents an exception thrown when a null value is not expected at a moment of the execution.
    /// </summary>
    [Serializable]
    internal sealed class BadTypeException : BaZicInterpreterException, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BadTypeException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        internal BadTypeException(string message)
            : base(message)
        {
        }

        public BadTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
