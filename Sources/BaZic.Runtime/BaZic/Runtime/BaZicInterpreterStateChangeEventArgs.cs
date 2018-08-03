using BaZic.Runtime.BaZic.Runtime.Debugger;
using BaZic.Runtime.Localization;
using System;

namespace BaZic.Runtime.BaZic.Runtime
{
    /// <summary>
    /// Provide an <see cref="EventHandler"/> for a BaZic algorithm interpreter state change
    /// </summary>
    /// <param name="sender">The source from where the state has changed</param>
    /// <param name="e">The arguments that describe the change</param>
    public delegate void BaZicInterpreterStateEventHandler(object sender, BaZicInterpreterStateChangeEventArgs e);

    /// <summary>
    /// Represents an event data for a BaZic algorithm interpreter state update
    /// </summary>
    [Serializable]
    public sealed class BaZicInterpreterStateChangeEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the <see cref="BaZicInterpreterState"/>
        /// </summary>
        public BaZicInterpreterState State { get; }

        /// <summary>
        /// Gets an error that occured.
        /// </summary>
        public Error Error { get; }

        /// <summary>
        /// Gets the message related to the log.
        /// </summary>
        public string LogMessage { get; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BaZicInterpreterStateChangeEventArgs"/> class.
        /// </summary>
        /// <param name="state">The <see cref="BaZicInterpreterState"/></param>
        public BaZicInterpreterStateChangeEventArgs(BaZicInterpreterState state)
        {
            if (state == BaZicInterpreterState.StoppedWithError)
            {
                throw new ArgumentOutOfRangeException(nameof(state), L.BaZic.Runtime.BaZicInterpreterStateChangeEventArgs.StoppedWithErrorForbidden);
            }

            State = state;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaZicInterpreterStateChangeEventArgs"/> class.
        /// </summary>
        /// <param name="error">An <see cref="Error"/></param>
        public BaZicInterpreterStateChangeEventArgs(Error error)
        {
            State = BaZicInterpreterState.StoppedWithError;
            Error = error;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BaZicInterpreterStateChangeEventArgs"/>
        /// </summary>
        /// <param name="log">A log message</param>
        public BaZicInterpreterStateChangeEventArgs(string log)
        {
            State = BaZicInterpreterState.Log;
            LogMessage = log;
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            if (Error != null)
            {
                return L.BaZic.Runtime.BaZicInterpreterStateChangeEventArgs.FormattedError(Error.Exception.Message);
            }

            if (State == BaZicInterpreterState.Log)
            {
                return L.BaZic.Runtime.BaZicInterpreterStateChangeEventArgs.FormattedLog(LogMessage);
            }

            return L.BaZic.Runtime.BaZicInterpreterStateChangeEventArgs.FormattedState(State);
        }

        #endregion
    }
}
