using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using BaZic.Runtime.Localization;
using System;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter.Statement
{
    /// <summary>
    /// Provide the interpreter for a throw statement.
    /// </summary>
    internal sealed class ThrowInterpreter : StatementInterpreter<ThrowStatement>
    {
        internal ThrowInterpreter(BaZicInterpreterCore baZicInterpreter, BlockInterpreter parentInterpreter, ThrowStatement statement)
            : base(baZicInterpreter, parentInterpreter, statement)
        {
        }

        /// <inheritdoc/>
        internal override void Run()
        {
            var expression = ParentInterpreter.RunExpression(Statement.Expression);

            var exception = expression as Exception;

            if (exception == null)
            {
                BaZicInterpreter.ChangeState(this, new BadTypeException(L.BaZic.Runtime.Interpreters.Statements.ThrowInterpreter.FormattedExceptionExpected(typeof(Exception).FullName)));
                return;
            }

            throw exception;
        }
    }
}
