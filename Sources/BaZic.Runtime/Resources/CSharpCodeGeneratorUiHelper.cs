// Helper for CSharp generated code.

namespace BaZicProgramReleaseMode
{
    /// <summary>
    /// Provides a set of methods designed to help the generated program to run with the same behavior than with a BaZic code.
    /// </summary>
    public partial class ProgramHelper
    {
        #region Fields & Constants

        private System.Windows.FrameworkElement _userInterface;

        private string _xamlCode = "{XAMLCode}";

        #endregion

        #region Properties

        /// <summary>
        /// Sets the result of the user interface when the window is closing.
        /// </summary>
        internal dynamic UiResult { private get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Raised when the Idle state can be set in the BaZicInterpreter.
        /// </summary>
        public event System.EventHandler IdleStateOccured;

        #endregion

        #region Methods

        /// <summary>
        /// Close the UI.
        /// </summary>
        public void CloseUserInterface()
        {
            try
            {
                UIDispatcher?.Invoke(() =>
                {
                    if (_userInterface is System.Windows.Window window)
                    {
                        window?.Close();
                    }
                    System.Windows.Threading.Dispatcher.CurrentDispatcher?.InvokeShutdown();
                }, System.Windows.Threading.DispatcherPriority.Send);
            }
            catch { }
        }

        /// <summary>
        /// Load the user interface in memory.
        /// </summary>
        internal void LoadUserInterface()
        {
            ProgramResourceManager.LoadResources();
            _userInterface = System.Windows.Markup.XamlReader.Parse(_xamlCode) as System.Windows.FrameworkElement;
            UIDispatcher = _userInterface.Dispatcher;
        }

        /// <summary>
        /// Show the user interface of the program.
        /// </summary>
        /// <returns>Returns the result of the Window.Closed event from the user interface.</returns>
        internal dynamic ShowUserInterface()
        {
            System.Exception eventException = null;

            var window = _userInterface as System.Windows.Window;

            if (window != null)
            {
                window.Closed += (sender, e) =>
                {
                    UIDispatcher?.InvokeShutdown();
                };

                window.Loaded += (sender, e) =>
                {
                    IdleStateOccured?.Invoke(this, e);
                };
            }
            else
            {
                IdleStateOccured?.Invoke(this, new System.EventArgs());
            }

            try
            {
                window?.Show();
                System.Windows.Threading.Dispatcher.Run();
            }
            catch (System.Exception exception)
            {
                eventException = exception;
            }
            finally
            {
                try
                {
                    window?.Close();
                }
                catch { }
                window = null;
                _userInterface = null;
            }

            if (eventException != null)
            {
                throw eventException;
            }

            return UiResult;
        }

        /// <summary>
        /// Gets the specified control from the user interface.
        /// </summary>
        /// <param name="controlName">The name of the control to retrieves.</param>
        /// <returns>Returns null if the control does not exist.</returns>
        internal dynamic GetControl(System.String controlName)
        {
            dynamic dynamic = _userInterface.FindName(controlName);
            return dynamic;
        }

        #endregion
    }
}