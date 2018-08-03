using System;
using System.Runtime.Serialization;

namespace BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions
{
    /// <summary>
    /// Represents an exception thrown when an error related to a method argument is detected.
    /// </summary>
    [Serializable]
    internal sealed class BadArgumentException : BaZicInterpreterException, ISerializable
    {
        /// <summary>
        /// Gets the name of the argument.
        /// </summary>
        internal string ArgumentName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadArgumentException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="argumentName">The name of the argument.</param>
        internal BadArgumentException(string message, string argumentName = "")
            : base(message)
        {
            ArgumentName = argumentName;
        }

        public BadArgumentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ArgumentName = info.GetString(nameof(ArgumentName));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(ArgumentName), ArgumentName);

            base.GetObjectData(info, context);
        }
    }
}
