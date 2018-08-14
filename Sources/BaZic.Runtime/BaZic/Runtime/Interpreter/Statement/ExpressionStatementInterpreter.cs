using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using System;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter.Statement
{
    /// <summary>
    /// Provide the interpreter for an expression statement.
    /// </summary>
    internal sealed class ExpressionStatementInterpreter : StatementInterpreter<ExpressionStatement>
    {
        internal ExpressionStatementInterpreter(BaZicInterpreterCore baZicInterpreter, BlockInterpreter parentInterpreter, Guid executionFlowId, ExpressionStatement statement)
            : base(baZicInterpreter, parentInterpreter, executionFlowId, statement)
        {
        }

        /// <inheritdoc/>
        internal override void Run()
        {
            ParentInterpreter.RunExpression(Statement.Expression);
        }
    }
}
