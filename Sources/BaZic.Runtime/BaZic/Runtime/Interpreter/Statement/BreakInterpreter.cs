using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using BaZic.Runtime.Localization;
using System;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter.Statement
{
    /// <summary>
    /// Provide the interpreter for a break statement.
    /// </summary>
    internal sealed class BreakInterpreter : StatementInterpreter<BreakStatement>
    {
        internal BreakInterpreter(BaZicInterpreterCore baZicInterpreter, BlockInterpreter parentInterpreter, Guid executionFlowId, BreakStatement statement)
            : base(baZicInterpreter, parentInterpreter, executionFlowId, statement)
        {
        }

        /// <inheritdoc/>
        internal override void Run()
        {
            if (!ParentInterpreter.State.IsInIteration)
            {
                BaZicInterpreter.ChangeState(this, new IncoherentStatementException(L.BaZic.Runtime.Interpreters.Statements.BreakInterpreter.Illegal), Statement);
            }

            ParentInterpreter.State.ExitIteration = true;
        }
    }
}
