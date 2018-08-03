using System;
using System.Collections.Generic;
using System.Linq;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents the instantiation of an array.
    /// </summary>
    [Serializable]
    public sealed class ArrayCreationExpression : ReferenceExpression
    {
        #region Properties

        /// <summary>
        /// Gets the arguments to pass in the class's constructor
        /// </summary>
        public IReadOnlyList<Expression> Values { get; private set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of <see cref="ArrayCreationExpression"/>
        /// </summary>
        public ArrayCreationExpression()
        {
            Values = new List<Expression>().AsReadOnly();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set array's values.
        /// </summary>
        /// <param name="values">The values</param>
        /// <returns>The current array</returns>
        public ArrayCreationExpression WithValues(params Expression[] values)
        {
            Values = new List<Expression>(values).AsReadOnly();
            return this;
        }

        /// <summary>
        /// Gets a string representation of the array
        /// </summary>
        /// <returns>String that reprensents the array</returns>
        public override string ToString()
        {
            var values = new List<string>();
            foreach (var item in Values)
            {
                values.Add(item.ToString());
            }

            return $"NEW [{string.Join(", ", values)}]";
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new ArrayCreationExpression()
            {
                Column = Column,
                Id = Id,
                Line = Line,
                StartOffset = StartOffset,
                NodeLength = NodeLength
            }
            .WithValues(Values.ToArray());
        }

        #endregion
    }
}
