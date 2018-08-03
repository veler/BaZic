using System;

namespace BaZic.Core.Logs
{
    /// <summary>
    /// Represents the event data sent when notifying that a log has been added.
    /// </summary>
    public class LogAddedEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// The log added.
        /// </summary>
        public string Log { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of the <see cref="LogAddedEventArgs"/> class.
        /// </summary>
        /// <param name="log">The log added.</param>
        internal LogAddedEventArgs(string log)
        {
            Log = log;
        }

        #endregion
    }
}
