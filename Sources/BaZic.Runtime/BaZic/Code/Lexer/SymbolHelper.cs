using System.Collections.Generic;

namespace BaZic.Runtime.BaZic.Code.Lexer
{
    /// <summary>
    /// Provides a set of list of separators required by the Lexer.
    /// </summary>
    internal static class SymbolHelper
    {
        #region Fields & Constants

        /// <summary>
        /// Gets a list of special separators that can be used to detect a keyword or expression.
        /// </summary>
        internal static IReadOnlyList<string> Separators = new List<string> { " ", "\r", "\n", "#", ",", "[", "]", "(", ")" }.AsReadOnly();

        /// <summary>
        /// Gets a list of special separators with symbols that can be used to detect a keyword or expression.
        /// </summary>
        internal static char[] SymbolSeparators = new char[] { ' ', '\r', '\n', '#', ',', '[', ']', '(', ')', '-', '+', '/', '*', '=', '%', '<', '>', '.' };

        #endregion
    }
}
