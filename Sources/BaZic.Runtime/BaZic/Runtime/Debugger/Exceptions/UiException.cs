using System;
using System.Runtime.Serialization;

namespace BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions
{
    /// <summary>
    /// Represents an exception thrown when an error happens in the user interface.
    /// </summary>
    [Serializable]
    internal sealed class UiException : BaZicInterpreterException, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UiException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        internal UiException(string message)
            : base(message)
        {
        }

        public UiException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
