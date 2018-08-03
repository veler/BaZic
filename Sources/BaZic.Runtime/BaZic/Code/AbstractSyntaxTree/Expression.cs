using System;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Basic class that represents an expression in an algorithm
    /// </summary>
    [Serializable]
    public abstract class Expression : NodeObject
    {
        #region Methods

        /// <summary>
        /// Gets a string representation of the reference
        /// </summary>
        /// <returns>String that reprensents the reference</returns>
        public abstract override string ToString();

        #endregion
    }
}
