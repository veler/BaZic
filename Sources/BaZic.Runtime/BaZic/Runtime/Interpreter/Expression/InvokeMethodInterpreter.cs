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
        private readonly bool _failIfNotExtern;
        private readonly Guid _executionFlowId;

        internal InvokeMethodInterpreter(BaZicInterpreterCore baZicInterpreter, Interpreter parentInterpreter, InvokeMethodExpression expression, Guid executionFlowId, bool failIfNotExtern = false)
            : base(baZicInterpreter, parentInterpreter, expression)
        {
            _executionFlowId = executionFlowId;
            _failIfNotExtern = failIfNotExtern;
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

            // If the invocation is made manually by the user (outisde of the execution flow).
            if (_failIfNotExtern)
            {
                if (!declarations.Single().IsExtern)
                {
                    BaZicInterpreter.ChangeState(this, new MethodNotFoundException(Expression.MethodName.Identifier, L.BaZic.Runtime.Interpreters.Expressions.InvokeMethodInterpreter.FormattedMethodNotFound(Expression.MethodName)), Expression);
                    return null;
                }

                if (!declarations.Single().IsAsync && Expression.Await)
                {
                    Expression.Await = false;
                }
            }

            var methodInterpreter = new MethodInterpreter(BaZicInterpreter, ParentInterpreter, declarations.Single(), Expression, _executionFlowId);
            return methodInterpreter.Invoke();
        }
    }
}
