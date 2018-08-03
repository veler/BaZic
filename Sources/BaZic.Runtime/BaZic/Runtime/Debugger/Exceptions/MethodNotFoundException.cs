using System;
using System.Runtime.Serialization;

namespace BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions
{
    /// <summary>
    /// Represents an exception thrown when a method is not found.
    /// </summary>
    [Serializable]
    internal class MethodNotFoundException : BaZicInterpreterException, ISerializable
    {
        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        internal string MethodName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodNotFoundException"/> class.
        /// </summary>
        /// <param name="methodName">The method name.</param>
        /// <param name="message">The error message.</param>
        internal MethodNotFoundException(string methodName, string message)
            : base(message)
        {
            MethodName = methodName;
        }

        public MethodNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            MethodName = info.GetString(nameof(MethodName));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(MethodName), MethodName);

            base.GetObjectData(info, context);
        }
    }
}
