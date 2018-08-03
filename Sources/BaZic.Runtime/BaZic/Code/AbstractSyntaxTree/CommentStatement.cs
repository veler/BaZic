using System;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents a comment in an algorithm
    /// </summary>
    [Serializable]
    public sealed class CommentStatement : Statement
    {
        #region Properties

        /// <summary>
        /// Gets the comment
        /// </summary>
        public string Comment { get; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentStatement"/> class.
        /// </summary>
        public CommentStatement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentStatement"/> class.
        /// </summary>
        /// <param name="comment">The comment</param>
        public CommentStatement(string comment)
        {
            Comment = comment;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new CommentStatement(Comment)
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
