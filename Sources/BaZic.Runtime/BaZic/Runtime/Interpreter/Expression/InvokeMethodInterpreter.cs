using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using BaZic.Runtime.Localization;
using System;
using System.Linq;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter.Expression
{
    /// <summary>
    /// Provide the interpreter for a local method invocation expression.
    /// </summary>
    internal sealed class InvokeMethodInterpreter : ExpressionInterpreter<InvokeMethodExpression>
    {
        internal InvokeMethodInterpreter(BaZicInterpreterCore baZicInterpreter, Interpreter parentInterpreter, InvokeMethodExpression expression)
            : base(baZicInterpreter, parentInterpreter, expression)
        {
        }

        /// <inheritdoc/>
        internal override object Run()
        {
            var declarations = BaZicInterpreter.MethodDeclarations.Where(m => string.Compare(m.Name.Identifier, Expression.MethodName.Identifier, StringComparison.Ordinal) == 0);

            if (declarations.Count() == 0)
            {
                BaZicInterpreter.ChangeState(this, new MethodNotFoundException(Expression.MethodName.Identifier, L.BaZic.Runtime.Interpreters.Expressions.InvokeMethodInterpreter.FormattedMethodNotFound(Expression.MethodName)), Expression);
                return null;
            }
            else if (declarations.Count() > 1)
            {
                BaZicInterpreter.ChangeState(this, new MethodNotFoundException(Expression.MethodName.Identifier, L.BaZic.Runtime.Interpreters.Expressions.InvokeMethodInterpreter.FormattedSeveralMethods(Expression.MethodName)), Expression);
                return null;
            }

            var methodInterpreter = new MethodInterpreter(BaZicInterpreter, ParentInterpreter, declarations.Single(), Expression);
            return methodInterpreter.Invoke();
        }
    }
}
