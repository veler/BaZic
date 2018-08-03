using System;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents an assignment in an algorithm
    /// </summary>
    [Serializable]
    public sealed class AssignStatement : Statement
    {
        #region Properties

        /// <summary>
        /// Gets or sets the expression on the left of the assign symbol in the algorithm
        /// </summary>
        public Expression LeftExpression { get; set; }

        /// <summary>
        /// Gets or sets the expression on the right of the assign symbol in the algorithm
        /// </summary>
        public Expression RightExpression { get; set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignStatement"/> class.
        /// </summary>
        public AssignStatement()
        {
        }

        /// <summary>  
        /// Initializes a new instance of the <see cref="AssignStatement"/> class.
        /// </summary>
        /// <param name="leftExpression">The expression on the left of the assign symbol in the algorithm</param>
        /// <param name="rightExpression">The expression on the right of the assign symbol in the algorithm</param>
        public AssignStatement(Expression leftExpression, Expression rightExpression)
        {
            LeftExpression = leftExpression;
            RightExpression = rightExpression;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new AssignStatement(LeftExpression, RightExpression)
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
