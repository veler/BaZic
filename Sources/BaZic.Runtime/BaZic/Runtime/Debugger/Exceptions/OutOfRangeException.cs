using System;
using System.Runtime.Serialization;

namespace BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions
{
    /// <summary>
    /// Represents an exception thrown when a value is not in a expected range at a moment of the execution.
    /// </summary>
    [Serializable]
    internal sealed class OutOfRangeException : BaZicInterpreterException, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutOfRangeException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        internal OutOfRangeException(string message)
            : base(message)
        {
        }

        public OutOfRangeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
