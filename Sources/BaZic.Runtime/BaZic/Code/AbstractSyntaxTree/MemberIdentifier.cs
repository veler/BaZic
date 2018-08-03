using BaZic.Core.ComponentModel;
using BaZic.Runtime.Localization;
using System;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents an identifier
    /// </summary>
    [Serializable]
    public sealed class MemberIdentifier : NodeObject
    {
        #region Properties

        /// <summary>
        /// Gets the identifier
        /// </summary>
        public string Identifier { get; }

        #endregion

        #region Contructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberIdentifier"/> class.
        /// </summary>
        /// <param name="identifier">The identifier</param>
        public MemberIdentifier(string identifier)
        {
            if (!CoreHelper.IsValidIdentifier(identifier))
            {
                throw new ArgumentException(L.BaZic.AbstractSyntaxTree.InvalidIdentifier, nameof(identifier));
            }
            Identifier = identifier;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a string representation of the object 
        /// </summary>
        /// <returns>Returns the identifier</returns>
        public override string ToString()
        {
            return Identifier;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new MemberIdentifier(Identifier)
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
