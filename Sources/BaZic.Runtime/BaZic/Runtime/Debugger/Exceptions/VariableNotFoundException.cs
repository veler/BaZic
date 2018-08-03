using BaZic.Runtime.Localization;
using System;
using System.Runtime.Serialization;

namespace BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions
{
    /// <summary>
    /// Represents an exception thrown when we try to access to a variable which is not accessible or does not exist.
    /// </summary>
    [Serializable]
    internal sealed class VariableNotFoundException : BaZicInterpreterException, ISerializable
    {
        /// <summary>
        /// Gets the variable name.
        /// </summary>
        internal string VariableName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableNotFoundException"/> class.
        /// </summary>
        /// <param name="variableName">The variable name</param>
        internal VariableNotFoundException(string variableName)
            : base(L.BaZic.Runtime.Debugger.Exception.FormattedVariableNotFoundException(variableName))
        {
            VariableName = variableName;
        }

        public VariableNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            VariableName = info.GetString(nameof(VariableName));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(VariableName), VariableName);

            base.GetObjectData(info, context);
        }
    }
}
