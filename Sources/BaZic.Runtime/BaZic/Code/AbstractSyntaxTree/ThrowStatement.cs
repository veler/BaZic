using System;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents a throw statement in an algorithm 
    /// </summary>
    [Serializable]
    public sealed class ThrowStatement : ExpressionStatement
    {
        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrowStatement"/> class.
        /// </summary>
        public ThrowStatement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrowStatement"/> class.
        /// </summary>
        /// <param name="expression">The expression to return</param>
        public ThrowStatement(Expression expression)
            : base(expression)
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
            return new ThrowStatement(Expression)
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
