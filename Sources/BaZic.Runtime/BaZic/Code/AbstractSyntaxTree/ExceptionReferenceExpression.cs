using System;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents the special keyword designed to retrieve a throwned <see cref="Exception"/>
    /// </summary>
    [Serializable]
    public sealed class ExceptionReferenceExpression : ReferenceExpression
    {
        #region Methods

        /// <summary>
        /// Gets a string representation of the reference
        /// </summary>
        /// <returns>String that reprensents the reference</returns>
        public override string ToString()
        {
            return "EXCEPTION";
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new ExceptionReferenceExpression()
            {
                Column = Column,
                Id = Id,
                Line = Line,
                StartOffset = StartOffset,
                NodeLength = NodeLength
            };
        }

        #endregion
    }
}
