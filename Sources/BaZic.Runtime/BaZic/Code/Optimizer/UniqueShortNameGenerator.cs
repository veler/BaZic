using System;

namespace BaZic.Runtime.BaZic.Code.Optimizer
{
    /// <summary>
    /// Provides a way to generate a unique short name like "A", "B", "AA"..etc
    /// </summary>
    internal sealed class UniqueShortNameGenerator
    {
        #region Fields & Constants

        private int _nameId = 0;

        #endregion

        #region Methods

        /// <summary>
        /// Generates the next unique short name.
        /// </summary>
        /// <returns>A unique short name.</returns>
        internal string GetNextName()
        {
            _nameId++;

            var dividend = _nameId;
            var columnName = string.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }

        /// <summary>
        /// Reset the generator.
        /// </summary>
        internal void Reset()
        {
            _nameId = 0;
        }

        #endregion
    }
}
