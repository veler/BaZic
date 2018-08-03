using System;
using System.Runtime.Serialization;

namespace BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions
{
    /// <summary>
    /// Represents an exception thrown when the program attempt a divison by zero.
    /// </summary>
    [Serializable]
    internal sealed class DivideByZeroException : BaZicInterpreterException, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DivideByZeroException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        internal DivideByZeroException(string message)
            : base(message)
        {
        }

        public DivideByZeroException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
