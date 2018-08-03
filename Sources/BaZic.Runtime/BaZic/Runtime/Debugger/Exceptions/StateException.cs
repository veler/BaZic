using BaZic.Runtime.Localization;
using System;
using System.Runtime.Serialization;

namespace BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions
{
    /// <summary>
    /// Represents an exception thrown when the current state of the algorithm interpreter is not correct
    /// </summary>
    [Serializable]
    internal sealed class StateException : BaZicInterpreterException, ISerializable
    {
        /// <summary>
        /// Gets or sets the state
        /// </summary>
        internal BaZicInterpreterState Sate { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="StateException"/>
        /// </summary>
        /// <param name="state">The state</param>
        internal StateException(BaZicInterpreterState state)
            : base(L.BaZic.Runtime.Debugger.Exception.StateException)
        {
            Sate = state;
        }

        public StateException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Sate = (BaZicInterpreterState)info.GetValue(nameof(Sate), typeof(BaZicInterpreterState));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Sate), Sate);

            base.GetObjectData(info, context);
        }
    }
}
