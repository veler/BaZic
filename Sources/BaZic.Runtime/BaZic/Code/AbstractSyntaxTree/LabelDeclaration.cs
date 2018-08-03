using System;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents a label in an algorithm.
    /// </summary>
    [Serializable]
    public sealed class LabelDeclaration : Statement
    {
        #region Properties

        /// <summary>
        /// Gets the label name
        /// </summary>
        public MemberIdentifier Name { get; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelDeclaration"/> class.
        /// </summary>
        public LabelDeclaration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelDeclaration"/> class.
        /// </summary>
        /// <param name="name">The label name</param>
        public LabelDeclaration(string name)
        {
            Name = new MemberIdentifier(name);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new LabelDeclaration(Name.Identifier)
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
