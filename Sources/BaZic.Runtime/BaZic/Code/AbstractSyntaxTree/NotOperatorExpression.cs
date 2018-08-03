using System;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents a Not operator expression
    /// </summary>
    [Serializable]
    public sealed class NotOperatorExpression : Expression
    {
        #region Properties

        /// <summary>
        /// Gets or sets the expression
        /// </summary>
        public Expression Expression { get; set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NotOperatorExpression"/> class.
        /// </summary>
        public NotOperatorExpression()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotOperatorExpression"/> class.
        /// </summary>
        /// <param name="expression">The expression</param>
        public NotOperatorExpression(Expression expression)
        {
            Expression = expression;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a string representation of the reference
        /// </summary>
        /// <returns>String that reprensents the reference</returns>
        public override string ToString()
        {
            return $"NOT {Expression}";
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new NotOperatorExpression(Expression)
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
