using BaZic.Core.ComponentModel;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.Localization;

namespace BaZic.Runtime.BaZic.Runtime.Memory
{
    /// <summary>
    /// Represents a binded variable at runtime.
    /// </summary>
    internal sealed class Binding : Variable
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
        /// Gets the name of the control's property.
        /// </summary>
        internal string PropertyName { get; }

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

                return _baZicInterpreter.ProgramInterpreter.UIDispatcher.Invoke(() =>
                {
                    return _baZicInterpreter.Reflection.GetProperty(_control, PropertyName);
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
            protected set
            {
                if (_isDisposing)
                {
                    return;
                }

                _baZicInterpreter.ProgramInterpreter.UIDispatcher.Invoke(() =>
                {
                    _baZicInterpreter.Reflection.SetProperty(_control, PropertyName, value);
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Binding"/> class.
        /// </summary>
        /// <param name="bindingDeclaration">The <see cref="BindingDeclaration"/> used to create the new variable in memory at runtime.</param>
        /// <param name="control">The UI component where the binding must be performed.</param>
        /// <param name="baZicInterpreter">The main BaZic interpreter.</param>
        internal Binding(BindingDeclaration bindingDeclaration, object control, BaZicInterpreterCore baZicInterpreter)
            : base(bindingDeclaration.Variable)
        {
            Requires.NotNull(control, nameof(control));
            Requires.NotNull(baZicInterpreter, nameof(baZicInterpreter));

            ControlName = bindingDeclaration.ControlName;
            PropertyName = bindingDeclaration.ControlPropertyName;
            _control = control;
            _baZicInterpreter = baZicInterpreter;
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            if (Value == null)
            {
                return L.BaZic.Runtime.Debugger.ValueInfo.Null;
            }

            return $"{Value} ({ControlName}.{PropertyName} : {Info})";
        }

        #endregion
    }
}
