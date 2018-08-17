using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Debugger;
using BaZic.Runtime.Localization;
using System;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter.Statement
{
    /// <summary>
    /// Provide the interpreter for a return statement.
    /// </summary>
    internal sealed class ReturnInterpreter : StatementInterpreter<ReturnStatement>
    {
        internal ReturnInterpreter(BaZicInterpreterCore baZicInterpreter, BlockInterpreter parentInterpreter, Guid executionFlowId, ReturnStatement statement)
            : base(baZicInterpreter, parentInterpreter, executionFlowId, statement)
        {
        }

        /// <inheritdoc/>
        internal override void Run()
        {
            var methodInterpreter = ParentInterpreter.GetParentMethodInterpreter();

            if (ParentInterpreter.IsAborted)
            {
                return;
            }

            var returnValue = ParentInterpreter.RunExpression(Statement.Expression);

            if (ParentInterpreter.IsAborted)
            {
                return;
            }

            methodInterpreter.ReturnedValue = returnValue;
            ParentInterpreter.State.ExitMethod = true;

            if (BaZicInterpreter.Verbose && !ParentInterpreter.IsAborted)
            {
                var valueString = methodInterpreter.ReturnedValue == null ? L.BaZic.Runtime.Debugger.ValueInfo.Null : $"{returnValue} ({ValueInfo.GetValueInfo(returnValue)})";
                ParentInterpreter.VerboseLog(L.BaZic.Runtime.Interpreters.Statements.ReturnInterpreter.FormattedReturn(valueString));
            }
        }
    }
}
