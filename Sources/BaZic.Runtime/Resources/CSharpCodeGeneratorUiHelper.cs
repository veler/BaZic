// Helper for CSharp generated code.

namespace BaZicProgramReleaseMode
{
    /// <summary>
    /// Provides a set of methods designed to help the generated program to run with the same behavior than with a BaZic code.
    /// </summary>
    internal sealed class ProgramUiHelper
    {
        #region Fields & Constants

        private string _xamlCode = "{XAMLCode}";
        private System.Windows.Window _userInterface;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current instance of the helper.
        /// </summary>
        internal static ProgramUiHelper Instance { get; private set; }

        /// <summary>
        /// Sets the result of the user interface when the window is closing.
        /// </summary>
        internal dynamic UiResult { private get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new static instance of <see cref="ProgramUiHelper"/>.
        /// </summary>
        internal static void CreateNewInstance()
        {
            Instance = new ProgramUiHelper();
        }

        /// <summary>
        /// Load the user interface in memory.
        /// </summary>
        internal void LoadWindow()
        {
            _userInterface = System.Windows.Markup.XamlReader.Parse(_xamlCode) as System.Windows.Window;
        }

        /// <summary>
        /// Show the user interface of the program.
        /// </summary>
        /// <returns>Returns the result of the Window.Closed event from the user interface.</returns>
        internal dynamic ShowWindow()
        {
            System.Exception eventException = null;

            _userInterface.Closed += (sender, e) =>
            {
                _userInterface?.Dispatcher?.InvokeShutdown();
            };

            try
            {
                _userInterface.Show();
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
                    _userInterface.Close();
                }
                catch { }
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