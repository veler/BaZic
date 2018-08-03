using BaZic.Core.Enums;
using System;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents a binary conditional expression
    /// </summary>
    [Serializable]
    public sealed class BinaryOperatorExpression : Expression
    {
        #region Properties

        /// <summary>
        /// Gets or sets the left expression
        /// </summary>
        public Expression LeftExpression { get; set; }

        /// <summary>
        /// Gets the binary operator
        /// </summary>
        public BinaryOperatorType Operator { get; }

        /// <summary>
        /// Gets or sets the right expression
        /// </summary>
        public Expression RightExpression { get; set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryOperatorExpression"/> class.
        /// </summary>
        public BinaryOperatorExpression()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryOperatorExpression"/> class.
        /// </summary>
        /// <param name="leftExpression">The left expression</param>
        /// <param name="conditionalOperator">The binary operator</param>
        /// <param name="rightExpression">The right expression</param>
        public BinaryOperatorExpression(Expression leftExpression, BinaryOperatorType conditionalOperator, Expression rightExpression)
        {
            LeftExpression = leftExpression;
            Operator = conditionalOperator;
            RightExpression = rightExpression;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a string representation of the reference
        /// </summary>
        /// <returns>String that reprensents the reference</returns>
        public override string ToString()
        {
            return $"{LeftExpression} {Operator.GetDescription()} {RightExpression}";
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new BinaryOperatorExpression(LeftExpression, Operator, RightExpression)
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
