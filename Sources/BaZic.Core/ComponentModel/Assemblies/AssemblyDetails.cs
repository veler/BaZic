using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BaZic.Core.ComponentModel.Assemblies
{
    /// <summary>
    /// Provides informations about an assembly.
    /// </summary>
    [Serializable]
    public class AssemblyDetails
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name of the assembly.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the version of the assembly.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets whether the assembly must be copied to the build folder when the user export its project to a standalone executable package.
        /// </summary>
        public bool CopyToLocal { get; set; }

        /// <summary>
        /// Gets or sets the full name of the assembly.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the path to the file of the assembly.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the culture of the assembly.
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        /// Gets or sets the public key token of the assembly.
        /// </summary>
        public string PublicKeyToken { get; set; }

        /// <summary>
        /// Gets or sets the processor target architecture.
        /// </summary>
        public ProcessorArchitecture ProcessorArchitecture { get; set; }

        /// <summary>
        /// Gets or sets custom information about the assembly.
        /// </summary>
        public string Custom { get; set; }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var value = obj as AssemblyDetails;
            if (value != null)
            {
                return ToString() == value.ToString();
            }

            return base.Equals(obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name}, Version={Version}";
        }

        /// <summary>
        /// Gets either the location if available, or the full name of the assembly.
        /// </summary>
        /// <returns>either the location if available, or the full name of the assembly.</returns>
        public string ToLocationOrFullName()
        {
            if (!string.IsNullOrWhiteSpace(Location) && CopyToLocal)
            {
                return Location;
            }

            return FullName;
        }

        #endregion
    }
}
