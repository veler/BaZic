using System;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents a return statement in an algorithm 
    /// </summary>
    [Serializable]
    public sealed class ReturnStatement : ExpressionStatement
    {
        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ReturnStatement"/> class.
        /// </summary>
        public ReturnStatement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReturnStatement"/> class.
        /// </summary>
        /// <param name="expression">The expression to return</param>
        public ReturnStatement(Expression expression)
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
            return new ReturnStatement(Expression)
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
