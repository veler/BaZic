using System;

namespace BaZic.Runtime.BaZic.Runtime
{
    [Serializable]
    internal class BaZicInterpreterStateChangedBridge : MarshalByRefObject
    {
        internal event BaZicInterpreterStateEventHandler StateChanged;

        internal void RaiseStateChange(BaZicInterpreterStateChangeEventArgs e)
        {
            StateChanged?.Invoke(this, e);
        }
    }
}
