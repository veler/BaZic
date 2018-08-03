using BaZic.Runtime.Localization;
using System;
using System.Runtime.Serialization;

namespace BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions
{
    /// <summary>
    /// Represents an exception that is thrown when an unmanaged and unexpected exception has been thrown during the interpretation.
    /// </summary>
    [Serializable]
    internal sealed class UnexpectedException : BaZicInterpreterException, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedException"/> class.
        /// </summary>
        /// <param name="exception">The detected exception.</param>
        internal UnexpectedException(Exception exception)
            : base(L.BaZic.Runtime.Debugger.Exception.FormattedUnexpectedException(exception.Message), exception)
        {
        }

        public UnexpectedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
