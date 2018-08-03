using BaZic.Runtime.BaZic.Runtime.Memory;
using System;
using System.Collections.Generic;

namespace BaZic.Runtime.BaZic.Runtime.Debugger
{
    /// <summary>
    /// Provides the debug information when the program is paused in debug mode.
    /// </summary>
    [Serializable]
    public sealed class DebugInfo
    {
        #region Properties

        /// <summary>
        /// Gets the complete call stack.
        /// </summary>
        public List<Call> CallStack { get; }

        /// <summary>
        /// Gets or sets the global variables.
        /// </summary>
        public IReadOnlyList<Variable> GlobalVariables { get; set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugInfo"/> class.
        /// </summary>
        public DebugInfo()
        {
            CallStack = new List<Call>();
        }

        #endregion
    }
}
