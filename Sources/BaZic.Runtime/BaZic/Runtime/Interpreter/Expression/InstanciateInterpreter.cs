using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using BaZic.Runtime.Localization;
using System;
using System.Collections.Generic;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter.Expression
{
    /// <summary>
    /// Provide the interpreter to instantiate an object.
    /// </summary>
    internal sealed class InstantiateInterpreter : ExpressionInterpreter<InstantiateExpression>
    {
        internal InstantiateInterpreter(BaZicInterpreterCore baZicInterpreter, Interpreter parentInterpreter, InstantiateExpression expression)
            : base(baZicInterpreter, parentInterpreter, expression)
        {
        }

        /// <inheritdoc/>
        internal override object Run()
        {
            if (Expression.CreateType == null)
            {
                BaZicInterpreter.ChangeState(this, new NullValueException(L.BaZic.Runtime.Interpreters.Expressions.InstantiateInterpreter.FormattedCreateTypeNull(nameof(InstantiateExpression.CreateType), nameof(InstantiateExpression))), Expression);
                return null;
            }

            var createType = ParentInterpreter.RunExpression(Expression.CreateType) as Type;

            if (ParentInterpreter.IsAborted)
            {
                return null;
            }

            if (BaZicInterpreter.Verbose)
            {
                ParentInterpreter.VerboseLog(L.BaZic.Runtime.Interpreters.Expressions.InstantiateInterpreter.FormattedCreateInstance(Expression.CreateType));
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

            return BaZicInterpreter.Reflection.Instantiate(createType, argumentValues.ToArray());
        }
    }
}
