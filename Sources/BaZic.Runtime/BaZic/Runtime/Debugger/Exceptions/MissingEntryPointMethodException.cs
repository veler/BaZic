using BaZic.Runtime.Localization;
using System;
using System.Runtime.Serialization;

namespace BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions
{
    /// <summary>
    /// Represents an excpetion thrown when no entry point has been found in a program.
    /// </summary>
    [Serializable]
    internal class MissingEntryPointMethodException : BaZicInterpreterException, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MissingEntryPointMethodException"/> class.
        /// </summary>
        internal MissingEntryPointMethodException()
            : base(L.BaZic.Runtime.Debugger.Exception.MissingEntryPointMethodException)
        {
        }

        public MissingEntryPointMethodException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
