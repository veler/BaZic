using System;
using System.Collections.Generic;
using System.Linq;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents a method declaration in a class in an algorithm
    /// </summary>
    [Serializable]
    public class MethodDeclaration : Statement
    {
        #region Properties

        /// <summary>
        /// Gets the name of the method
        /// </summary>
        public MemberIdentifier Name { get; }

        /// <summary>
        /// Gets the statements in the method's body
        /// </summary>
        public IReadOnlyList<Statement> Statements { get; private set; }

        /// <summary>
        /// Gets a collection of arguments declaration
        /// </summary>
        public IReadOnlyList<ParameterDeclaration> Arguments { get; private set; }

        /// <summary>
        /// Gets whether the method can be call asynchronously
        /// </summary>
        public bool IsAsync { get; }

        /// <summary>
        /// Gets or sets the position in the code where the END FUNCTION keywords appears.
        /// </summary>
        public int EndOffset { get; set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodDeclaration"/> class.
        /// </summary>
        public MethodDeclaration()
        {
            Statements = new List<Statement>().AsReadOnly();
            Arguments = new List<ParameterDeclaration>().AsReadOnly();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodDeclaration"/> class.
        /// </summary>
        /// <param name="name">The name of the method</param>
        /// <param name="isAsync">Defines whether the method can be call asynchronously</param>
        public MethodDeclaration(string name, bool isAsync)
            : this()
        {
            Name = new MemberIdentifier(name);
            IsAsync = isAsync;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set method's arguments.
        /// </summary>
        /// <param name="parameters">The arguments</param>
        /// <returns>The current method</returns>
        public MethodDeclaration WithParameters(params ParameterDeclaration[] parameters)
        {
            Arguments = new List<ParameterDeclaration>(parameters).AsReadOnly();
            return this;
        }

        /// <summary>
        /// Set method's statements.
        /// </summary>
        /// <param name="statements">The statements</param>
        /// <returns>The current method</returns>
        public MethodDeclaration WithBody(params Statement[] statements)
        {
            Statements = new List<Statement>(statements).AsReadOnly();
            return this;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new MethodDeclaration(Name.Identifier, IsAsync)
            {
                Column = Column,
                Id = Id,
                Line = Line,
                StartOffset = StartOffset,
                NodeLength = NodeLength
            }
            .WithParameters(Arguments.ToArray())
            .WithBody(Statements.ToArray());
        }

        #endregion
    }
}
