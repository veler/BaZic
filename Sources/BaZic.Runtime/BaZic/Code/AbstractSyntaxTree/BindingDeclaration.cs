using System;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents binding between a control from the user interface and a variable in a BaZic program.
    /// </summary>
    [Serializable]
    public sealed class BindingDeclaration : Statement
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name of the control in the user interface.
        /// </summary>
        public string ControlName { get; set; }

        /// <summary>
        /// Gets or sets the name of the property of the control.
        /// </summary>
        public string ControlPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the variable that must be used to get or set the value of the property of the control.
        /// </summary>
        public VariableDeclaration Variable { get; set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingDeclaration"/> class.
        /// </summary>
        public BindingDeclaration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingDeclaration"/> class.
        /// </summary>
        /// <param name="controlName">The name of the control in the user interface.</param>
        /// <param name="controlPropertyName">The name of the property of the control.</param>
        /// <param name="variable">The variable that must be used to get or set the value of the property of the control.</param>
        public BindingDeclaration(string controlName, string controlPropertyName, VariableDeclaration variable)
        {
            ControlName = controlName?.Trim();
            ControlPropertyName = controlPropertyName;
            Variable = variable;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new BindingDeclaration(ControlName, ControlPropertyName, Variable)
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
