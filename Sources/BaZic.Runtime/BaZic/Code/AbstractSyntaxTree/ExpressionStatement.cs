using System;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents a statement that consists of a single expression
    /// </summary>
    [Serializable]
    public class ExpressionStatement : Statement
    {
        #region Properties

        /// <summary>
        /// Gets or sets the single expression of the statement
        /// </summary>
        public Expression Expression { get; set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionStatement"/> class.
        /// </summary>
        public ExpressionStatement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionStatement"/> class.
        /// </summary>
        /// <param name="expression">The single expression of the statement</param>
        public ExpressionStatement(Expression expression)
        {
            Expression = expression;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new ExpressionStatement(Expression)
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
