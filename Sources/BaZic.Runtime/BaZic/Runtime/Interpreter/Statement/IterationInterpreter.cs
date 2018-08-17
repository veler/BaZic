using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Debugger;
using System;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter.Statement
{
    /// <summary>
    /// Provide the interpreter for a iteration statement.
    /// </summary>
    internal sealed class IterationInterpreter : StatementInterpreter<IterationStatement>
    {
        /// <summary>
        /// Gets the state of the child block interpreter once it's done.
        /// </summary>
        internal BlockState ChildBlockState { get; private set; }

        internal IterationInterpreter(BaZicInterpreterCore baZicInterpreter, BlockInterpreter parentInterpreter, Guid executionFlowId, IterationStatement statement)
            : base(baZicInterpreter, parentInterpreter, executionFlowId, statement)
        {
        }

        /// <inheritdoc/>
        internal override void Run()
        {
            var conditionResult = false;


            _IterationLoop:

            if (!Statement.ConditionAfterBody)
            {
                conditionResult = RunCondition();
                if (!conditionResult)
                {
                    return;
                }
            }

            // Execute statements
            var block = new BlockInterpreter(BaZicInterpreter, ParentInterpreter, ExecutionFlowId, true, ParentInterpreter.CaughtException, Statement.Statements);
            block.Run();
            ChildBlockState = block.State;

            if (ChildBlockState.ExitMethod || ChildBlockState.ExitIteration || ChildBlockState.ExitBlockBecauseOfLabelJump || ParentInterpreter.IsAborted)
            {
                ChildBlockState.ExitIteration = false; // prevent to exit the parent iteration, we want to exit just the current one.
                return;
            }

            if (Statement.ConditionAfterBody)
            {
                conditionResult = RunCondition();
            }

            if (conditionResult)
            {
                goto _IterationLoop;
            }
        }

        /// <summary>
        /// Execute the condition
        /// </summary>
        /// <returns>Return true or false, even in case of error</returns>
        private bool RunCondition()
        {
            var conditionResult = ConditionInterpreter.RunCondition(BaZicInterpreter, ParentInterpreter, Statement.Condition);

            if (ParentInterpreter.IsAborted || conditionResult == null)
            {
                return false;
            }

            return conditionResult.Value;
        }
    }
}
