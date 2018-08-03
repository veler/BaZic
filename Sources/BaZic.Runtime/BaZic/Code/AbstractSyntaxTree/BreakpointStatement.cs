using System;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents a breakpoint in an algorithm 
    /// </summary>
    [Serializable]
    public sealed class BreakpointStatement : Statement
    {
        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BreakpointStatement"/> class.
        /// </summary>
        public BreakpointStatement()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new BreakpointStatement()
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
