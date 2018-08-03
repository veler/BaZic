using BaZic.Runtime.Localization;
using System;
using System.Runtime.Serialization;

namespace BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions
{
    /// <summary>
    /// Represents an exception thrown when we try to await a non-asynchronous method.
    /// </summary>
    [Serializable]
    internal class MethodNotAwaitableException : BaZicInterpreterException, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MethodNotAwaitableException"/> class.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        internal MethodNotAwaitableException(string methodName)
            : base(L.BaZic.Runtime.Debugger.Exception.FormattedMethodNotAwaitableException(methodName))
        {
        }

        public MethodNotAwaitableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
