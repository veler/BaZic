using BaZic.Core.ComponentModel;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.Localization;

namespace BaZic.Runtime.BaZic.Runtime.Memory
{
    /// <summary>
    /// Represents a control accessor at runtime.
    /// </summary>
    internal sealed class ControlAccessor : Variable
    {
        #region Fields & Constants

        private readonly BaZicInterpreterCore _baZicInterpreter;
        private readonly object _control;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the control.
        /// </summary>
        internal string ControlName { get; }

        /// <summary>
        /// Gets or sets the value of the variable and control's property.
        /// </summary>
        public override object Value
        {
            get
            {
                if (_isDisposing)
                {
                    return null;
                }

                return _control;
            }
        }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlAccessor"/> class.
        /// </summary>
        /// <param name="controlAccessorDeclaration">The <see cref="ControlAccessorDeclaration"/> used to create the new variable in memory at runtime.</param>
        /// <param name="control">The UI component where the binding must be performed.</param>
        /// <param name="baZicInterpreter">The main BaZic interpreter.</param>
        internal ControlAccessor(ControlAccessorDeclaration controlAccessorDeclaration, object control, BaZicInterpreterCore baZicInterpreter)
            : base(controlAccessorDeclaration.Variable)
        {
            Requires.NotNull(control, nameof(control));
            Requires.NotNull(baZicInterpreter, nameof(baZicInterpreter));

            ControlName = controlAccessorDeclaration.ControlName;
            _control = control;
            _baZicInterpreter = baZicInterpreter;

            SetValue(control);
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            if (Value == null)
            {
                return L.BaZic.Runtime.Debugger.ValueInfo.Null;
            }

            return $"{Value} ({ControlName} : {Info})";
        }

        #endregion
    }
}
