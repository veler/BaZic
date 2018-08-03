using System;
using System.Runtime.Serialization;

namespace BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions
{
    /// <summary>
    /// Represents an exception thrown when a null value is not expected at a moment of the execution.
    /// </summary>
    [Serializable]
    internal sealed class NullValueException : BaZicInterpreterException, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NullValueException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        internal NullValueException(string message)
            : base(message)
        {
        }

        public NullValueException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
