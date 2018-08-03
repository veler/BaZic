using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using BaZic.Runtime.Localization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter.Expression
{
    /// <summary>
    /// Provide the interpreter for a .Net method invocation expression.
    /// </summary>
    internal sealed class InvokeCoreMethodInterpreter : ExpressionInterpreter<InvokeCoreMethodExpression>
    {
        internal InvokeCoreMethodInterpreter(BaZicInterpreterCore baZicInterpreter, Interpreter parentInterpreter, InvokeCoreMethodExpression expression)
            : base(baZicInterpreter, parentInterpreter, expression)
        {
        }

        /// <inheritdoc/>
        internal override object Run()
        {
            if (Expression.TargetObject == null)
            {
                BaZicInterpreter.ChangeState(this, new NullValueException(L.BaZic.Runtime.Interpreters.Expressions.InvokeCoreMethodInterpreter.TargetObjectNull), Expression);
                return null;
            }
            else if (string.IsNullOrWhiteSpace(Expression.MethodName?.Identifier))
            {
                BaZicInterpreter.ChangeState(this, new NullValueException(L.BaZic.Runtime.Interpreters.Expressions.InvokeCoreMethodInterpreter.UndefinedMethodName), Expression);
                return null;
            }

            var targetObjectValue = ParentInterpreter.RunExpression(Expression.TargetObject);

            if (ParentInterpreter.IsAborted)
            {
                return null;
            }

            if (targetObjectValue == null)
            {
                BaZicInterpreter.ChangeState(this, new NullValueException(L.BaZic.Runtime.Interpreters.Expressions.InvokeCoreMethodInterpreter.TargetObjectNull), Expression);
                return null;
            }


            // Execute argument's values.
            if (BaZicInterpreter.Verbose)
            {
                ParentInterpreter.VerboseLog(L.BaZic.Runtime.Interpreters.MethodInterpreter.ExecutingArguments);
            }
            var argumentValues = new List<object>();
            for (var i = 0; i < Expression.Arguments.Count; i++)
            {
                var argumentValue = ParentInterpreter.RunExpression(Expression.Arguments[i]);
                argumentValues.Add(argumentValue);
            }

            if (ParentInterpreter.IsAborted)
            {
                return null;
            }

            object result = null;

            if (Expression.TargetObject is ClassReferenceExpression && targetObjectValue is Type)
            {
                result = BaZicInterpreter.Reflection.InvokeStaticMethod((Type)targetObjectValue, Expression.MethodName.Identifier, argumentValues.ToArray());
            }
            else
            {
                result = BaZicInterpreter.Reflection.InvokeMethod(targetObjectValue, Expression.MethodName.Identifier, argumentValues.ToArray());
            }

            if (Expression.Await)
            {
                if (result == null || !typeof(Task).IsAssignableFrom(result.GetType()))
                {
                    BaZicInterpreter.ChangeState(this, new MethodNotAwaitableException(Expression.MethodName.Identifier), Expression);
                    return null;
                }
                else
                {
                    var task = (Task)result;
                    task.Wait();
                    var type = task.GetType();
                    if (!type.IsGenericType)
                    {
                        result = null;
                    }
                    else
                    {
                        result = type.GetProperty(nameof(Task<object>.Result)).GetValue(task);
                    }
                    task.Dispose();
                }
            }
            else if (result != null && typeof(Task).IsAssignableFrom(result.GetType()))
            {
                var task = (Task)result;
                BaZicInterpreter.AddUnwaitedMethodInvocation(task);
            }

            return result;
        }
    }
}
