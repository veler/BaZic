using System;
using System.Reflection;

namespace BaZic.Core.ComponentModel.Assemblies
{
    /// <summary>
    /// Represents a loaded assembly with its details.
    /// </summary>
    [Serializable]
    public class LoadedAssemblyDetails
    {
        #region Properties

        /// <summary>
        /// Gets the assembly in memory.
        /// </summary>
        public Assembly Assembly { get; internal set; }

        /// <summary>
        /// Gets the details of the assembly.
        /// </summary>
        public AssemblyDetails Details { get; internal set; }

        #endregion
    }
}
