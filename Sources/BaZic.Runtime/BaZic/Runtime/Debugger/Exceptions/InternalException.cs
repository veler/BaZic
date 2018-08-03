using BaZic.Runtime.Localization;
using System;
using System.Runtime.Serialization;

namespace BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions
{
    /// <summary>
    /// Represents an exception that is thrown in case of expected internal interpreter error or incoherence.
    /// </summary>
    [Serializable]
    internal sealed class InternalException : BaZicInterpreterException, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InternalException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        internal InternalException(string message)
            : base(L.BaZic.Runtime.Debugger.Exception.FormattedInternalException(message))
        {
        }

        public InternalException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
