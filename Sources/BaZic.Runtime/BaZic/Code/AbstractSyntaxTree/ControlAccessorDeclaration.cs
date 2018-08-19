using System;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents a global read-only variable declaration used to access a control in the user interface.
    /// </summary>
    [Serializable]
    public sealed class ControlAccessorDeclaration : Statement
    {
        #region Properties

        /// <summary>
        /// Gets the name of the variable, which must match a control name.
        /// </summary>
        public string ControlName { get; }

        /// <summary>
        /// Gets or sets the variable that must be used to get or set the value of the property of the control.
        /// </summary>
        public VariableDeclaration Variable { get; set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlAccessorDeclaration"/> class.
        /// </summary>
        public ControlAccessorDeclaration()
        {
            Column = 0;
            Line = 0;
            StartOffset = 0;
            NodeLength = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlAccessorDeclaration"/> class.
        /// </summary>
        /// <param name="name">The name of the variable, which must match the name of a control.</param>
        public ControlAccessorDeclaration(string name)
            : this()
        {
            ControlName = name;
            Variable = new VariableDeclaration(name)
            {
                Column = 0,
                Line = 0,
                StartOffset = 0,
                NodeLength = 0
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new ControlAccessorDeclaration(ControlName)
            {
                Column = Column,
                Id = Id,
                Line = Line,
                StartOffset = StartOffset,
                NodeLength = NodeLength,
                Variable = Variable
            };
        }

        #endregion
    }
}
