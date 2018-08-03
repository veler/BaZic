using System;
using System.Runtime.Serialization;

namespace BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions
{
    /// <summary>
    /// Represents an exception thrown when too many BaZic mathod calls happen.
    /// </summary>
    [Serializable]
    internal sealed class StackOverflowException : BaZicInterpreterException, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StackOverflowException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        internal StackOverflowException(string message)
            : base(message)
        {
        }

        public StackOverflowException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
