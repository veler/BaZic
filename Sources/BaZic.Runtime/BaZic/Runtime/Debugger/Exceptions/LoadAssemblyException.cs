using System;
using System.Runtime.Serialization;

namespace BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions
{
    /// <summary>
    /// Represents an exception thrown when the interpreter fails to load a required assembly for the execution of the program.
    /// </summary>
    [Serializable]
    internal sealed class LoadAssemblyException : BaZicInterpreterException, ISerializable
    {
        internal string AssemblyPath { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadAssemblyException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="assemblyPath">The path to the supposed assembly to load.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        internal LoadAssemblyException(string message, string assemblyPath, Exception innerException)
            : base(message, innerException)
        {
            AssemblyPath = assemblyPath;
        }

        public LoadAssemblyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            AssemblyPath = info.GetString(nameof(AssemblyPath));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(AssemblyPath), AssemblyPath);

            base.GetObjectData(info, context);
        }
    }
}
