using System;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents a variable declaration in a statement. In this structure, variables will always be of type <see cref="object"/> or <see cref="Collection{T}"/> of <see cref="object"/>
    /// </summary>
    [Serializable]
    public sealed class VariableDeclaration : Statement
    {
        #region Properties

        /// <summary>
        /// Gets the name of the variable 
        /// </summary>
        public MemberIdentifier Name { get; }

        /// <summary>
        /// Gets whether the variable is of type <see cref="object"/> or <see cref="Collection{T}"/> of <see cref="object"/>
        /// </summary>
        public bool IsArray { get; }

        /// <summary>
        /// Gets the default value of the variable
        /// </summary>
        public Expression DefaultValue { get; private set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableDeclaration"/> class.
        /// </summary>
        public VariableDeclaration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableDeclaration"/> class.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <param name="isArray">Define whether the variable is of type <see cref="object"/> or <see cref="Collection{T}"/> of <see cref="object"/></param>
        public VariableDeclaration(string name, bool isArray = false)
        {
            Name = new MemberIdentifier(name);
            IsArray = isArray;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Defines the default value of the variable.
        /// </summary>
        /// <param name="defaultValue">The <see cref="Expression"/> that returns the default value of the variable.</param>
        /// <returns>The variable declaration</returns>
        public VariableDeclaration WithDefaultValue(Expression defaultValue)
        {
            DefaultValue = defaultValue;
            return this;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new VariableDeclaration(Name.Identifier, IsArray)
            {
                Column = Column,
                Id = Id,
                Line = Line,
                StartOffset = StartOffset,
                NodeLength = NodeLength
            }
            .WithDefaultValue(DefaultValue);
        }

        #endregion
    }
}
