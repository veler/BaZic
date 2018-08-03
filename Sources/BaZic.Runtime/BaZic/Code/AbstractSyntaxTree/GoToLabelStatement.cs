using System;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents a GoTo label in an algorithm.
    /// </summary>
    [Serializable]
    public sealed class GoToLabelStatement : Statement
    {
        #region Properties

        /// <summary>
        /// Gets the label name
        /// </summary>
        public MemberIdentifier Name { get; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GoToLabelStatement"/> class.
        /// </summary>
        public GoToLabelStatement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GoToLabelStatement"/> class.
        /// </summary>
        /// <param name="name">The label name</param>
        public GoToLabelStatement(string name)
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
            return new GoToLabelStatement(Name.Identifier)
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
