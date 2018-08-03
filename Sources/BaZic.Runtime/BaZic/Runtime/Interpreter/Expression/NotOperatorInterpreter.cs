using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using BaZic.Runtime.Localization;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter.Expression
{
    /// <summary>
    /// Provide the interpreter for a NOT operator expression.
    /// </summary>
    internal sealed class NotOperatorInterpreter : ExpressionInterpreter<NotOperatorExpression>
    {
        internal NotOperatorInterpreter(BaZicInterpreterCore baZicInterpreter, Interpreter parentInterpreter, NotOperatorExpression expression)
            : base(baZicInterpreter, parentInterpreter, expression)
        {
        }

        /// <inheritdoc/>
        internal override object Run()
        {
            if (Expression.Expression == null)
            {
                BaZicInterpreter.ChangeState(this, new NullValueException(L.BaZic.Runtime.Interpreters.Expressions.NotOperatorInterpreter.FormattedExpressionNull(nameof(NotOperatorExpression))), Expression);
                return null;
            }

            var expressionValue = ParentInterpreter.RunExpression(Expression.Expression);

            if (expressionValue == null)
            {
                BaZicInterpreter.ChangeState(this, new NullValueException(L.BaZic.Runtime.Interpreters.Expressions.NotOperatorInterpreter.ValueNull), Expression);
                return null;
            }
            else if (expressionValue.GetType() != typeof(bool))
            {
                BaZicInterpreter.ChangeState(this, new BadTypeException(L.BaZic.Runtime.Interpreters.Expressions.NotOperatorInterpreter.FormattedBooleanExpected(expressionValue.GetType().Name)), Expression);
                return null;
            }

            return !(bool)expressionValue;
        }
    }
}
