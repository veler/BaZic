using System;
using System.Runtime.Serialization;

namespace BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions
{
    /// <summary>
    /// Represents an exception thrown when we try to assign a data that cannot be assigned in the current context.
    /// </summary>
    [Serializable]
    internal sealed class NotAssignableException : BaZicInterpreterException, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotAssignableException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        internal NotAssignableException(string message)
            : base(message)
        {
        }

        public NotAssignableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
