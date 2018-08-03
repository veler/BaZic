using System;
using System.Runtime.Serialization;

namespace BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions
{
    /// <summary>
    /// Represents an exception thrown a statement is placed at a illegal position in a block of statements.
    /// </summary>
    [Serializable]
    internal sealed class IncoherentStatementException : BaZicInterpreterException, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IncoherentStatementException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        internal IncoherentStatementException(string message)
            : base(message)
        {
        }

        public IncoherentStatementException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
