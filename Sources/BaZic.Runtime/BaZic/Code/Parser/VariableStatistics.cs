using BaZic.Core.ComponentModel;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;

namespace BaZic.Runtime.BaZic.Code.Parser
{
    /// <summary>
    /// Provides statistics about a variable declaration in a BaZic program for the parser.
    /// </summary>
    internal sealed class VariableStatistics
    {
        #region Properties

        /// <summary>
        /// Gets the variable declaration.
        /// </summary>
        internal VariableDeclaration Declaration { get; }

        /// <summary>
        /// Gets the number of time there is a reference to the variable in the program.
        /// </summary>
        internal int ReferenceCount { get; private set; }

        /// <summary>
        /// Gets whether the variable is a control accessor or not.
        /// </summary>
        internal bool IsControlAccessor { get; private set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableStatistics"/> class.
        /// </summary>
        /// <param name="variableDeclaration">The variable declaration.</param>
        /// <param name="isControlAccessor">Defines whether the variable is a control accessor or not.</param>
        internal VariableStatistics(VariableDeclaration variableDeclaration, bool isControlAccessor)
        {
            Requires.NotNull(variableDeclaration, nameof(variableDeclaration));
            Declaration = variableDeclaration;
            IsControlAccessor = isControlAccessor;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Increase the number of reference to the variable.
        /// </summary>
        internal void IncreaseReference()
        {
            ReferenceCount++;
        }

        #endregion
    }
}
