using System;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Basic class for algorithm representation.
    /// </summary>
    [Serializable]
    public abstract class NodeObject : ICloneable
    {
        #region Properties

        /// <summary>
        /// Gets or sets a unique GUID to identify a part of an algorithm
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the line in the code where the syntax tree element is. By default the value is -1.
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// Gets or sets the column in the code where the syntax tree element is. By default the value is -1.
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// Gets the character number where the node starts in the code.
        /// </summary>
        public int StartOffset { get; set; }

        /// <summary>
        /// Gets or sets the length of the node in the code.
        /// </summary>
        public int NodeLength { get; set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeObject"/> class.
        /// </summary>
        protected NodeObject()
        {
            Line = -1;
            Column = -1;
            StartOffset = -1;
            NodeLength = 0;
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public abstract object Clone();

        #endregion
    }
}
