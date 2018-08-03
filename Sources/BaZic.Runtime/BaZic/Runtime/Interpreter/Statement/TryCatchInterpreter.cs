using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Debugger;
using BaZic.Runtime.Localization;
using System;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter.Statement
{
    /// <summary>
    /// Provide the interpreter for a try catch statement.
    /// </summary>
    internal sealed class TryCatchInterpreter : StatementInterpreter<TryCatchStatement>
    {
        /// <summary>
        /// Gets the state of the child block interpreter once it's done.
        /// </summary>
        internal BlockState ChildBlockState { get; private set; }

        internal TryCatchInterpreter(BaZicInterpreterCore baZicInterpreter, BlockInterpreter parentInterpreter, TryCatchStatement statement)
            : base(baZicInterpreter, parentInterpreter, statement)
        {
        }

        /// <inheritdoc/>
        internal override void Run()
        {
            try
            {
                // Execute statements
                var block = new BlockInterpreter(BaZicInterpreter, ParentInterpreter, ParentInterpreter.State.IsInIteration, ParentInterpreter.CaughtException, Statement.TryStatements);
                block.Run();
                ChildBlockState = block.State;
            }
            catch (Exception exception)
            {
                if (BaZicInterpreter.Verbose)
                {
                    ParentInterpreter.VerboseLog(L.BaZic.Runtime.Interpreters.Statements.TryCatchInterpreter.FormattedExceptionCaught(exception.GetType().FullName));
                }

                // Execute statements
                var block = new BlockInterpreter(BaZicInterpreter, ParentInterpreter, ParentInterpreter.State.IsInIteration, exception, Statement.CatchStatements);
                block.Run();
                ChildBlockState = block.State;
            }
        }
    }
}
