namespace BaZic.Runtime.BaZic.Runtime.Debugger
{
    /// <summary>
    /// Provides a set of properties that represents the execution state and reason why a <see cref="BlockInterpreter"/> stopped.
    /// </summary>
    internal sealed class BlockState
    {
        #region Properties

        /// <summary>
        /// Gets whether the current block of statements is in an iteration statement.
        /// </summary>
        internal bool IsInIteration { get; }

        /// <summary>
        /// Gets or sets whether the block stopped because of a Return statement.
        /// </summary>
        internal bool ExitMethod { get; set; }

        /// <summary>
        /// Gets or sets whether the block stopped because of a Break statement.
        /// </summary>
        internal bool ExitIteration { get; set; }

        /// <summary>
        /// Gets or sets whether a jump to a label has been interpreter and that the declared label is in a parent block or does not exist.
        /// </summary>
        internal bool ExitBlockBecauseOfLabelJump { get; set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockState"/> class.
        /// </summary>
        /// <param name="isInIteration">Defines whether the current block of statement is executed inside of a iteration statement.</param>
        internal BlockState(bool isInIteration)
        {
            IsInIteration = isInIteration;
        }

        #endregion
    }
}
