using System;
using System.Runtime.Serialization;

namespace BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions
{
    /// <summary>
    /// Represents a base exception for the BaZic interpreter.
    /// </summary>
    [Serializable]
    public abstract class BaZicInterpreterException : Exception, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaZicInterpreterException"/> class.
        /// </summary>
        public BaZicInterpreterException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaZicInterpreterException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        protected BaZicInterpreterException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaZicInterpreterException"/> class with a specified error message and inner exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        protected BaZicInterpreterException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public BaZicInterpreterException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
