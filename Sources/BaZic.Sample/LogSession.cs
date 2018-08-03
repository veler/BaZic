using BaZic.Core.Logs;
using System;
using System.Diagnostics;
using System.Text;
using System.Windows;

namespace BaZic.Sample
{
    /// <summary>
    /// Provides a set of functions designed to perform action when a log session starts, stop or wants to save the logs.
    /// </summary>
    internal sealed class LogSession : Logger
    {
        #region Methods

        /// <inheritdoc/>
        public override void SessionStarted()
        {
        }

        /// <inheritdoc/>
        public override void Persist(StringBuilder logs)
        {
        }

        /// <inheritdoc/>
        public override void SessionStopped()
        {
        }

        /// <inheritdoc/>
        public override string GetFatalErrorAdditionalInfo()
        {
            var result = new StringBuilder();
            result.AppendLine($"Memory used : {Process.GetCurrentProcess().PrivateMemorySize64 / 1024} KB");
            result.AppendLine($"Operating system : {Environment.OSVersion.VersionString}");
            MessageBox.Show(result.ToString());
            return result.ToString();
        }

        #endregion
    }
}
