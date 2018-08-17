using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Debugger;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using BaZic.Runtime.Localization;
using BaZicProgramReleaseMode;
using System;
using System.Collections;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter.Statement
{
    /// <summary>
    /// Provide the interpreter for a assign statement.
    /// </summary>
    internal sealed class AssignInterpreter : StatementInterpreter<AssignStatement>
    {
        internal AssignInterpreter(BaZicInterpreterCore baZicInterpreter, BlockInterpreter parentInterpreter, Guid executionFlowId, AssignStatement statement)
            : base(baZicInterpreter, parentInterpreter, executionFlowId, statement)
        {
        }

        /// <inheritdoc/>
        internal override void Run()
        {
            if (BaZicInterpreter.Verbose)
            {
                ParentInterpreter.VerboseLog(L.BaZic.Runtime.Interpreters.Statements.AssignInterpreter.FormattedAssign(Statement.LeftExpression, Statement.RightExpression));
            }

            if (!typeof(IAssignable).IsAssignableFrom(Statement.LeftExpression.GetType()))
            {
                BaZicInterpreter.ChangeState(this, new NotAssignableException(L.BaZic.Runtime.Interpreters.Statements.AssignInterpreter.NotAssignable), Statement);
                return;
            }

            var rightValue = ParentInterpreter.RunExpression(Statement.RightExpression);

            if (ParentInterpreter.IsAborted)
            {
                return;
            }

            switch (Statement.LeftExpression)
            {
                case ArrayIndexerExpression arrayIndexer:
                    AssignArrayValue(arrayIndexer, rightValue);
                    break;

                case PropertyReferenceExpression propertyReference:
                    AssignProperty(propertyReference, rightValue);
                    break;

                case VariableReferenceExpression variableReference:
                    ParentInterpreter.SetVariable(variableReference, rightValue);
                    break;

                default:
                    throw new InternalException(L.BaZic.Runtime.Interpreters.Statements.AssignInterpreter.FormattedNoInterpreter(Statement.LeftExpression.GetType().FullName));
            }

            if (BaZicInterpreter.Verbose && !ParentInterpreter.IsAborted)
            {
                var rightValueString = rightValue == null ? L.BaZic.Runtime.Debugger.ValueInfo.Null : $"'{rightValue}'(type:{ rightValue.GetType().FullName})";
                ParentInterpreter.VerboseLog(L.BaZic.Runtime.Interpreters.Statements.AssignInterpreter.FormattedNowEqualsTo(Statement.LeftExpression, rightValueString));
            }
        }

        /// <summary>
        /// Assigns the specified value to a property.
        /// </summary>
        /// <param name="propertyReference">The reference to the property to set.</param>
        /// <param name="value">The value to assign.</param>
        private void AssignProperty(PropertyReferenceExpression propertyReference, object value)
        {
            if (string.IsNullOrWhiteSpace(propertyReference.PropertyName?.Identifier))
            {
                BaZicInterpreter.ChangeState(this, new NullValueException(L.BaZic.Runtime.Interpreters.Expressions.PropertyReferenceInterpreter.UndefinedName), propertyReference);
            }
            var targetObjectValue = ParentInterpreter.RunExpression(propertyReference.TargetObject);

            if (ParentInterpreter.IsAborted)
            {
                return;
            }

            if (targetObjectValue == null)
            {
                BaZicInterpreter.ChangeState(this, new NullValueException(L.BaZic.Runtime.Interpreters.Expressions.PropertyReferenceInterpreter.NullValue), propertyReference);
                return;
            }

            if (propertyReference.TargetObject is ClassReferenceExpression && targetObjectValue is Type)
            {
                BaZicInterpreter.Reflection.SetStaticProperty((Type)targetObjectValue, propertyReference.PropertyName.Identifier, value);
            }
            else
            {
                BaZicInterpreter.Reflection.SetProperty(targetObjectValue, propertyReference.PropertyName.Identifier, value);
            }
        }

        /// <summary>
        /// Assigns the specified value to an array.
        /// </summary>
        /// <param name="arrayIndexer">The reference to the position in the array to set.</param>
        /// <param name="value">The value to assign.</param>
        private void AssignArrayValue(ArrayIndexerExpression arrayIndexer, object value)
        {
            var expressionValue = ParentInterpreter.RunExpression(arrayIndexer.TargetObject);

            if (ParentInterpreter.IsAborted)
            {
                return;
            }

            if (expressionValue == null)
            {
                BaZicInterpreter.ChangeState(this, new NullValueException(L.BaZic.Runtime.Interpreters.Statements.AssignInterpreter.TargetObjectNull), arrayIndexer);
                return;
            }

            var valueInfo = ValueInfo.GetValueInfo(expressionValue);

            if (!valueInfo.IsArray)
            {
                BaZicInterpreter.ChangeState(this, new BadTypeException(L.BaZic.Runtime.Interpreters.Expressions.ArrayIndexerInterpreter.FormattedIndexerForbidden(valueInfo.Type.Name)), arrayIndexer);
                return;
            }

            if (arrayIndexer.Indexes.Length != 1)
            {
                BaZicInterpreter.ChangeState(this, new OutOfRangeException(L.BaZic.Runtime.Interpreters.Expressions.ArrayIndexerInterpreter.OneIndexerAllowed), arrayIndexer);
                return;
            }

            var index = ParentInterpreter.RunExpression(arrayIndexer.Indexes[0]);

            if (ParentInterpreter.IsAborted)
            {
                return;
            }

            if (index == null)
            {
                BaZicInterpreter.ChangeState(this, new OutOfRangeException(L.BaZic.Runtime.Interpreters.Expressions.ArrayIndexerInterpreter.IndexMustNotBeNull), arrayIndexer.Indexes[0]);
                return;
            }

            if (valueInfo.Type == typeof(ObservableDictionary))
            {
                ((ObservableDictionary)expressionValue)[index] = value;
                return;
            }
            else if (typeof(IDictionary).IsAssignableFrom(valueInfo.Type))
            {
                ((IDictionary)expressionValue)[index] = value;
                return;
            }
            else
            {
                var indexValue = index as int?;

                if (indexValue == null)
                {
                    BaZicInterpreter.ChangeState(this, new BadArgumentException(L.BaZic.Runtime.Interpreters.Expressions.ArrayIndexerInterpreter.CastToNumber), arrayIndexer);
                    return;
                }

                if (indexValue < 0 || indexValue >= valueInfo.Length)
                {
                    BaZicInterpreter.ChangeState(this, new OutOfRangeException(L.BaZic.Runtime.Interpreters.Statements.AssignInterpreter.FormattedOutOfRange(indexValue, valueInfo.Length - 1)), arrayIndexer);
                    return;
                }

                if (valueInfo.Type.IsArray || valueInfo.Type == typeof(Array))
                {
                    ((Array)expressionValue).SetValue(value, indexValue.Value);
                    return;
                }
                else if (typeof(IList).IsAssignableFrom(valueInfo.Type))
                {
                    ((IList)expressionValue)[indexValue.Value] = value;
                    return;
                }
            }

            BaZicInterpreter.ChangeState(this, new InternalException(L.BaZic.Runtime.Interpreters.Statements.AssignInterpreter.FormattedUnsupportedArray(valueInfo.Type.FullName)), arrayIndexer);
        }
    }
}
