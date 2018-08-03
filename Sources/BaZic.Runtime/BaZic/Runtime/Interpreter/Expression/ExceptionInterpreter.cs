using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using BaZic.Runtime.Localization;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter.Expression
{
    /// <summary>
    /// Provide the interpreter for a exception reference expression.
    /// </summary>
    internal sealed class ExceptionInterpreter : ExpressionInterpreter<ExceptionReferenceExpression>
    {
        internal ExceptionInterpreter(BaZicInterpreterCore baZicInterpreter, Interpreter parentInterpreter, ExceptionReferenceExpression expression)
            : base(baZicInterpreter, parentInterpreter, expression)
        {
        }

        /// <inheritdoc/>
        internal override object Run()
        {
            var parentBlockInterpreter = ParentInterpreter as BlockInterpreter;

            if (parentBlockInterpreter == null)
            {
                BaZicInterpreter.ChangeState(this, new InternalException(L.BaZic.Runtime.Interpreters.Expressions.ExceptionInterpreter.BlockExpected), Expression);
            }

            if (parentBlockInterpreter.CaughtException == null)
            {
                BaZicInterpreter.ChangeState(this, new IncoherentStatementException(L.BaZic.Runtime.Interpreters.Expressions.ExceptionInterpreter.TryCatchExpected), Expression);
            }

            return parentBlockInterpreter.CaughtException;
        }
    }
}
