using BaZic.Core.ComponentModel;
using System;
using System.Runtime.CompilerServices;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter.Statement
{
    /// <summary>
    /// Provides the bases of a statement interpreter.
    /// </summary>
    internal abstract class StatementInterpreter<T> where T : Code.AbstractSyntaxTree.Statement
    {
        #region Properties

        /// <summary>
        /// Gets the parent BaZic interpreter.
        /// </summary>
        protected BaZicInterpreterCore BaZicInterpreter { get; }

        /// <summary>
        /// Gets the parent interpreter.
        /// </summary>
        protected BlockInterpreter ParentInterpreter { get; }

        /// <summary>
        /// Gets a GUID that defines in which callstack is linked
        /// </summary>
        protected Guid ExecutionFlowId { get; }

        /// <summary>
        /// Gets the statement to interpret.
        /// </summary>
        protected T Statement { get; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StatementInterpreter"/> class.
        /// </summary>
        /// <param name="baZicInterpreter">The main interpreter.</param>
        /// <param name="parentInterpreter">The parent interpreter.</param>
        /// <param name="executionFlowId">A GUID that defines in which callstack is linked.</param>
        /// <param name="statement">The statement to interpret.</param>
        protected StatementInterpreter(BaZicInterpreterCore baZicInterpreter, BlockInterpreter parentInterpreter, Guid executionFlowId, T statement)
        {
            Requires.NotNull(baZicInterpreter, nameof(baZicInterpreter));
            Requires.NotNull(parentInterpreter, nameof(parentInterpreter));
            Requires.NotNull(statement, nameof(statement));
            BaZicInterpreter = baZicInterpreter;
            ParentInterpreter = parentInterpreter;
            Statement = statement;
            ExecutionFlowId = executionFlowId;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Execute the interpretation of the statement.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void Run();

        #endregion
    }
}
