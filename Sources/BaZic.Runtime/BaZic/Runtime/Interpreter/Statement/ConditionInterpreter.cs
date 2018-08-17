using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Debugger;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using BaZic.Runtime.Localization;
using System;
using System.Collections.Generic;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter.Statement
{
    /// <summary>
    /// Provide the interpreter for a condition statement.
    /// </summary>
    internal sealed class ConditionInterpreter : StatementInterpreter<ConditionStatement>
    {
        /// <summary>
        /// Gets the state of the child block interpreter once it's done.
        /// </summary>
        internal BlockState ChildBlockState { get; private set; }

        internal ConditionInterpreter(BaZicInterpreterCore baZicInterpreter, BlockInterpreter parentInterpreter, Guid executionFlowId, ConditionStatement statement)
            : base(baZicInterpreter, parentInterpreter, executionFlowId, statement)
        {
        }

        /// <inheritdoc/>
        internal override void Run()
        {
            if (BaZicInterpreter.Verbose)
            {
                ParentInterpreter.VerboseLog(L.BaZic.Runtime.Interpreters.Statements.ConditionInterpreter.FormattedExecutingCondition(Statement.Condition));
            }

            var conditionResult = RunCondition(BaZicInterpreter, ParentInterpreter, Statement.Condition);

            if (ParentInterpreter.IsAborted || conditionResult == null)
            {
                return;
            }

            IReadOnlyList<Code.AbstractSyntaxTree.Statement> statements;

            if (conditionResult.Value)
            {
                statements = Statement.TrueStatements;
            }
            else
            {
                statements = Statement.FalseStatements;
            }

            if (statements == null || statements.Count == 0)
            {
                return;
            }

            // Execute statements
            var block = new BlockInterpreter(BaZicInterpreter, ParentInterpreter, ExecutionFlowId, ParentInterpreter.State.IsInIteration, ParentInterpreter.CaughtException, statements);
            block.Run();
            ChildBlockState = block.State;

            if (BaZicInterpreter.Verbose)
            {
                ParentInterpreter.VerboseLog(L.BaZic.Runtime.Interpreters.Statements.ConditionInterpreter.FormattedEndExecutingCondition(Statement.Condition));
            }
        }

        /// <summary>
        /// Execute the condition
        /// </summary>
        /// <param name="baZicInterpreter">The <see cref="BaZicInterpreterCore"/>.</param>
        /// <param name="parentInterpreter">The parent block interpreter</param>
        /// <param name="condition">The condition expression</param>
        /// <returns>Return true, false, or null in case of error</returns>
        internal static bool? RunCondition(BaZicInterpreterCore baZicInterpreter, BlockInterpreter parentInterpreter, Code.AbstractSyntaxTree.Expression condition)
        {
            if (condition == null)
            {
                baZicInterpreter.ChangeState(parentInterpreter, new NullValueException(L.BaZic.Runtime.Interpreters.Statements.ConditionInterpreter.MissingCondition));
                return null;
            }

            var conditionResult = parentInterpreter.RunExpression(condition);

            if (parentInterpreter.IsAborted)
            {
                return null;
            }

            var boolResult = conditionResult as bool?;
            if (boolResult != null)
            {
                return boolResult.Value;
            }

            var intResult = conditionResult as int?;
            if (intResult != null)
            {
                switch (intResult.Value)
                {
                    case 1:
                        return true;
                    case 0:
                        return false;
                    default:
                        baZicInterpreter.ChangeState(parentInterpreter, new OutOfRangeException(L.BaZic.Runtime.Interpreters.Statements.ConditionInterpreter.CastToBool), condition);
                        return null;
                }
            }

            baZicInterpreter.ChangeState(parentInterpreter, new OutOfRangeException(L.BaZic.Runtime.Interpreters.Statements.ConditionInterpreter.BooleanExpected), condition);
            return null;
        }
    }
}
