using System;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents a reference to a variable in an algorithm
    /// </summary>
    [Serializable]
    public sealed class VariableReferenceExpression : ReferenceExpression, IAssignable
    {
        #region Properties

        /// <summary>
        /// Gets the name of the variable
        /// </summary>
        public MemberIdentifier Name { get; }

        /// <summary>
        /// Gets or sets the ID of the <see cref="VariableDeclaration"/> the current object refers too.
        /// </summary>
        public Guid VariableDeclarationID { get; set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableReferenceExpression"/> class.
        /// </summary>
        public VariableReferenceExpression()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableReferenceExpression"/> class.
        /// </summary>
        /// <param name="name">The name of the variable we make reference</param>
        public VariableReferenceExpression(string name)
        {
            Name = new MemberIdentifier(name);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="VariableReferenceExpression"/>
        /// </summary>
        /// <param name="variable"></param>
        public VariableReferenceExpression(VariableDeclaration variable)
        {
            Name = variable.Name;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a string representation of the reference
        /// </summary>
        /// <returns>String that reprensents the reference</returns>
        public override string ToString()
        {
            return Name.ToString();
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new VariableReferenceExpression(Name.Identifier)
            {
                Column = Column,
                Id = Id,
                Line = Line,
                StartOffset = StartOffset,
                NodeLength = NodeLength,
                VariableDeclarationID = VariableDeclarationID
            };
        }

        #endregion
    }
}
