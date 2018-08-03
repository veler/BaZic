using System;
using System.Collections.Generic;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents a reference to an index of an array.
    /// </summary>
    [Serializable]
    public sealed class ArrayIndexerExpression : ReferenceExpression, IAssignable
    {
        #region Properties

        /// <summary>
        /// Gets or sets the reference to the array
        /// </summary>
        public ReferenceExpression TargetObject { get; set; }

        /// <summary>
        /// Gets the indexes of the array.
        /// </summary>
        public Expression[] Indexes { get; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayIndexerExpression"/> class.
        /// </summary>
        public ArrayIndexerExpression()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayIndexerExpression"/> class.
        /// </summary>
        /// <param name="targetObject">The reference to the array</param>
        public ArrayIndexerExpression(ReferenceExpression targetObject)
        {
            TargetObject = targetObject;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayIndexerExpression"/> class.
        /// </summary>
        /// <param name="targetObject">The reference to the array</param>
        /// <param name="indice">The index of the array.</param>
        public ArrayIndexerExpression(ReferenceExpression targetObject, Expression[] indexes)
            : this(targetObject)
        {
            if (indexes?.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(indexes), "At least one index must be specified.");
            }

            Indexes = indexes;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a string representation of the reference
        /// </summary>
        /// <returns>String that reprensents the reference</returns>
        public override string ToString()
        {
            var indexStrings = new List<string>();
            for (var i = 0; i < Indexes.Length; i++)
            {
                indexStrings.Add(Indexes[i].ToString());
            }

            return $"{TargetObject}[{string.Join(", ", indexStrings)}]";
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            if (Indexes == null)
            {
                return new ArrayIndexerExpression(TargetObject)
                {
                    Column = Column,
                    Id = Id,
                    Line = Line,
                    StartOffset = StartOffset,
                    NodeLength = NodeLength
                };
            }

            return new ArrayIndexerExpression(TargetObject, Indexes)
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
