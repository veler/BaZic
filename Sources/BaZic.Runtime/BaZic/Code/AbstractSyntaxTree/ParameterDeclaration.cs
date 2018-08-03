using System;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents a parameter declaration for a method in an algorithm
    /// </summary>
    [Serializable]
    public sealed class ParameterDeclaration : NodeObject
    {
        #region Properties

        /// <summary>
        /// Gets the name of the argument
        /// </summary>
        public MemberIdentifier Name { get; }

        /// <summary>
        /// Gets whether the argument is of type <see cref="object"/> or <see cref="Collection{T}"/> of <see cref="object"/>
        /// </summary>
        public bool IsArray { get; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterDeclaration"/> class.
        /// </summary>
        public ParameterDeclaration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterDeclaration"/> class.
        /// </summary>
        /// <param name="name">The name of the argument</param>
        /// <param name="isArray">Define whether the argument is of type <see cref="object"/> or <see cref="Collection{T}"/> of <see cref="object"/></param>
        public ParameterDeclaration(string name, bool isArray = false)
        {
            Name = new MemberIdentifier(name);
            IsArray = isArray;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new ParameterDeclaration(Name.Identifier, IsArray)
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
