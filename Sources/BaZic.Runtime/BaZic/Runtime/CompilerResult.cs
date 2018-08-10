using System;
using System.IO;

namespace BaZic.Runtime.BaZic.Runtime
{
    /// <summary>
    /// Provides the streams corresponding to a generated Assembly, PDB and XML documentation file.
    /// </summary>
    public class CompilerResult : IDisposable
    {
        #region Properties

        /// <summary>
        /// Gets the stream that contains the generated assembly.
        /// </summary>
        public MemoryStream Assembly { get; internal set; }

        /// <summary>
        /// Gets the stream that contains the generated PDB.
        /// </summary>
        public MemoryStream Pdb { get; internal set; }

        /// <summary>
        /// Gets the list of error, if the build failed. This property is null if the build succeed.
        /// </summary>
        public AggregateException BuildErrors { get; internal set; }

        #endregion

        #region Methods

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Assembly != null)
            {
                Assembly.Dispose();
            }

            if (Pdb != null)
            {
                Pdb.Dispose();
            }
        }

        #endregion
    }
}
