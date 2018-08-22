using System;
using System.Diagnostics;
using System.Linq;

namespace BaZic.Core.ComponentModel
{
    /// <summary>
    /// Provides a set of methods used to get information about the application.
    /// </summary>
    public static class CoreHelper
    {
        #region Methods

        /// <summary>
        /// Verify if an identifier is valid (not null, no space, doesn't start with number and doesn't contains special character).
        /// </summary>
        /// <param name="identifier">The identifier to check</param>
        /// <returns>Returns false if the identifier is invalid.</returns>
        public static bool IsValidIdentifier(string identifier)
        {
            return !String.IsNullOrWhiteSpace(identifier) && (char.IsLetter(identifier[0]) || identifier[0] == '_') && identifier.All(ch => char.IsLetterOrDigit(ch) || ch == '_');
        }

        /// <summary>
        /// Log in the Debug console the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public static void ReportException(Exception exception)
        {
#if DEBUG
            Debug.WriteLine($"EXCEPTION : {exception.Message}{Environment.NewLine}Stack Trace : {exception.StackTrace}");
#endif
        }

        #endregion
    }
}
