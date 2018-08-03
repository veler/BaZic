using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using BaZic.Runtime.Localization;
using System;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter.Expression
{
    /// <summary>
    /// Provide the interpreter for a property reference expression.
    /// </summary>
    internal sealed class PropertyReferenceInterpreter : ExpressionInterpreter<PropertyReferenceExpression>
    {
        internal PropertyReferenceInterpreter(BaZicInterpreterCore baZicInterpreter, Interpreter parentInterpreter, PropertyReferenceExpression expression)
            : base(baZicInterpreter, parentInterpreter, expression)
        {
        }

        /// <inheritdoc/>
        internal override object Run()
        {
            if (BaZicInterpreter.Verbose)
            {
                ParentInterpreter.VerboseLog(L.BaZic.Runtime.Interpreters.Expressions.PropertyReferenceInterpreter.FormattedGettingProperty(Expression));
            }

            if (Expression.TargetObject == null)
            {
                BaZicInterpreter.ChangeState(this, new NullValueException(L.BaZic.Runtime.Interpreters.Expressions.PropertyReferenceInterpreter.NullValue), Expression);
                return null;
            }
            else if (string.IsNullOrWhiteSpace(Expression.PropertyName?.Identifier))
            {
                BaZicInterpreter.ChangeState(this, new NullValueException(L.BaZic.Runtime.Interpreters.Expressions.PropertyReferenceInterpreter.UndefinedName), Expression);
                return null;
            }

            var targetObjectValue = ParentInterpreter.RunExpression(Expression.TargetObject);

            if (ParentInterpreter.IsAborted)
            {
                return null;
            }

            if (targetObjectValue == null)
            {
                BaZicInterpreter.ChangeState(this, new NullValueException(L.BaZic.Runtime.Interpreters.Expressions.PropertyReferenceInterpreter.NullValue), Expression);
                return null;
            }

            if (Expression.TargetObject is ClassReferenceExpression && targetObjectValue is Type)
            {
                return BaZicInterpreter.Reflection.GetStaticProperty((Type)targetObjectValue, Expression.PropertyName.Identifier);
            }

            return BaZicInterpreter.Reflection.GetProperty(targetObjectValue, Expression.PropertyName.Identifier);
        }
    }
}
