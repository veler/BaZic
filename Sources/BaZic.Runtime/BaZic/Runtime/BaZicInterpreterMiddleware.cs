using System;

namespace BaZic.Runtime.BaZic.Runtime
{
    /// <summary>
    /// Provides a middleware to callback the <see cref="BaZicInterpreter"/> from the <see cref="BaZicInterpreterCore"/>.
    /// </summary>
    internal sealed class BaZicInterpreterMiddleware : MarshalByRefObject
    {
        #region Fields & Constants

        private BaZicInterpreter _baZicInterpreter;

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BaZicInterpreterMiddleware"/> class.
        /// </summary>
        /// <param name="baZicInterpreter">The calling <see cref="BaZicInterpreter"/>.</param>
        internal BaZicInterpreterMiddleware(BaZicInterpreter baZicInterpreter)
        {
            _baZicInterpreter = baZicInterpreter;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sends a log to the <see cref="BaZicInterpreter"/>.
        /// </summary>
        /// <param name="sender">The core.</param>
        /// <param name="log">The log.</param>
        internal void SendLog(BaZicInterpreterCore sender, BaZicInterpreterStateChangeEventArgs log)
        {
            _baZicInterpreter.SendLog(log);
        }

        #endregion
    }
}
