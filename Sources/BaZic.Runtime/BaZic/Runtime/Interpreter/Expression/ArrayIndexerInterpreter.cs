using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Debugger;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using BaZic.Runtime.Localization;
using BaZic.StandaloneRuntime;
using System;
using System.Collections;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter.Expression
{
    /// <summary>
    /// Provide the interpreter for an array indexer expression.
    /// </summary>
    internal sealed class ArrayIndexerInterpreter : ExpressionInterpreter<ArrayIndexerExpression>
    {
        internal ArrayIndexerInterpreter(BaZicInterpreterCore baZicInterpreter, Interpreter parentInterpreter, ArrayIndexerExpression expression)
            : base(baZicInterpreter, parentInterpreter, expression)
        {
        }

        /// <inheritdoc/>
        internal override object Run()
        {
            var expressionValue = ParentInterpreter.RunExpression(Expression.TargetObject);

            if (ParentInterpreter.IsAborted)
            {
                return null;
            }

            if (expressionValue == null)
            {
                BaZicInterpreter.ChangeState(this, new NullValueException(L.BaZic.Runtime.Interpreters.Expressions.ArrayIndexerInterpreter.NullValue), Expression);
                return null;
            }

            var valueInfo = ValueInfo.GetValueInfo(expressionValue);

            if (!valueInfo.IsArray)
            {
                BaZicInterpreter.ChangeState(this, new BadTypeException(L.BaZic.Runtime.Interpreters.Expressions.ArrayIndexerInterpreter.FormattedIndexerForbidden(valueInfo.Type.Name)), Expression);
                return null;
            }

            if (Expression.Indexes.Length != 1)
            {
                BaZicInterpreter.ChangeState(this, new OutOfRangeException(L.BaZic.Runtime.Interpreters.Expressions.ArrayIndexerInterpreter.OneIndexerAllowed), Expression);
                return null;
            }

            var index = ParentInterpreter.RunExpression(Expression.Indexes[0]);

            if (ParentInterpreter.IsAborted)
            {
                return null;
            }

            if (index == null)
            {
                BaZicInterpreter.ChangeState(this, new OutOfRangeException(L.BaZic.Runtime.Interpreters.Expressions.ArrayIndexerInterpreter.IndexMustNotBeNull), Expression);
                return null;
            }

            if (valueInfo.Type == typeof(ObservableDictionary))
            {
                var dictionary = (ObservableDictionary)expressionValue;
                object val = null;
                if (dictionary.TryGetValue(index, out val))
                {
                    return val;
                }

                BaZicInterpreter.ChangeState(this, new OutOfRangeException(L.BaZic.Runtime.Interpreters.Expressions.ArrayIndexerInterpreter.FormattedKeyDoesNotExist(index?.ToString())), Expression);
                return null;
            }
            else if (typeof(IDictionary).IsAssignableFrom(valueInfo.Type))
            {
                return ((IDictionary)expressionValue)[index];
            }
            else
            {
                var indexValue = index as int?;

                if (indexValue == null)
                {
                    BaZicInterpreter.ChangeState(this, new BadArgumentException(L.BaZic.Runtime.Interpreters.Expressions.ArrayIndexerInterpreter.CastToNumber), Expression);
                    return null;
                }

                if (indexValue < 0 || indexValue >= valueInfo.Length)
                {
                    BaZicInterpreter.ChangeState(this, new OutOfRangeException(L.BaZic.Runtime.Interpreters.Expressions.ArrayIndexerInterpreter.FormattedOutOfRange(indexValue.ToString(), (valueInfo.Length - 1).ToString())), Expression);
                    return null;
                }

                if (valueInfo.Type.IsArray || valueInfo.Type == typeof(Array))
                {
                    return ((Array)expressionValue).GetValue(indexValue.Value);
                }
                else if (typeof(IList).IsAssignableFrom(valueInfo.Type))
                {
                    return ((IList)expressionValue)[indexValue.Value];
                }
            }

            BaZicInterpreter.ChangeState(this, new InternalException(L.BaZic.Runtime.Interpreters.Expressions.ArrayIndexerInterpreter.FormattedUnsupportedArray(valueInfo.Type.FullName)), Expression);
            return null;
        }
    }
}
