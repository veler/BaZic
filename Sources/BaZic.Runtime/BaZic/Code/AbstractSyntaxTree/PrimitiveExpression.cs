using System;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents a primitive value (like True, 10, 3.14) in an algorithm.
    /// </summary>
    [Serializable]
    public sealed class PrimitiveExpression : ReferenceExpression
    {
        #region Properties

        /// <summary>
        /// Gets the primitive value
        /// </summary>
        public object Value { get; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimitiveExpression"/> class.
        /// </summary>
        public PrimitiveExpression()
        {
            Value = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimitiveExpression"/> class.
        /// </summary>
        /// <param name="value">The primitive value</param>
        public PrimitiveExpression(ValueType value)
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimitiveExpression"/> class.
        /// </summary>
        /// <param name="value">The primitive value</param>
        public PrimitiveExpression(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimitiveExpression"/> class.
        /// </summary>
        /// <param name="value">The primitive value</param>
        public PrimitiveExpression(Array value)
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimitiveExpression"/> class.
        /// </summary>
        /// <param name="value">The primitive value</param>
        public PrimitiveExpression(object value)
        {
            Value = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a string representation of the reference
        /// </summary>
        /// <returns>String that reprensents the reference</returns>
        public override string ToString()
        {
            return Value == null ? "{null}" : $"'{Value}' (type:{Value.GetType().FullName})";
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new PrimitiveExpression(Value)
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
