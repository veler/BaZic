using System;
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

        #endregion
    }
}
