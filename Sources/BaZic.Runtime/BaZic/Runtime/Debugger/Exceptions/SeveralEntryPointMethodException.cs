using BaZic.Runtime.Localization;
using System;
using System.Runtime.Serialization;

namespace BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions
{
    /// <summary>
    /// Represents an excpetion thrown when several entry points have been found in a program.
    /// </summary>
    [Serializable]
    internal class SeveralEntryPointMethodException : BaZicInterpreterException, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SeveralEntryPointMethodException"/> class.
        /// </summary>
        internal SeveralEntryPointMethodException()
            : base(L.BaZic.Runtime.Debugger.Exception.SeveralEntryPointMethodException)
        {
        }

        public SeveralEntryPointMethodException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
