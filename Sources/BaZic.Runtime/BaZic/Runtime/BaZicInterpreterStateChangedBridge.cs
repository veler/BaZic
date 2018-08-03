using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
