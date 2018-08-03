using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using BaZic.Runtime.Localization;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter.Expression
{
    /// <summary>
    /// Provide the interpreter for a binary operator expression.
    /// </summary>
    internal sealed class BinaryOperatorInterpreter : ExpressionInterpreter<BinaryOperatorExpression>
    {
        internal BinaryOperatorInterpreter(BaZicInterpreterCore baZicInterpreter, Interpreter parentInterpreter, BinaryOperatorExpression expression)
            : base(baZicInterpreter, parentInterpreter, expression)
        {
        }

        /// <inheritdoc/>
        internal override object Run()
        {
            var leftValue = ParentInterpreter.RunExpression(Expression.LeftExpression);
            if (ParentInterpreter.IsAborted)
            {
                return null;
            }

            var rightValue = ParentInterpreter.RunExpression(Expression.RightExpression);
            if (ParentInterpreter.IsAborted)
            {
                return null;
            }

            if (BaZicInterpreter.Verbose)
            {
                ParentInterpreter.VerboseLog(L.BaZic.Runtime.Interpreters.Expressions.BinaryOperatorInterpreter.FormattedPerformOperation(Expression.Operator));
            }

            dynamic dynamicLeftValue = leftValue;
            dynamic dynamicRightValue = rightValue;

            switch (Expression.Operator)
            {
                case BinaryOperatorType.Equality:
                    return dynamicLeftValue == dynamicRightValue;

                case BinaryOperatorType.BitwiseOr:
                    return dynamicLeftValue | dynamicRightValue;

                case BinaryOperatorType.BitwiseAnd:
                    return dynamicLeftValue & dynamicRightValue;

                case BinaryOperatorType.LogicalOr:
                    return dynamicLeftValue || dynamicRightValue;

                case BinaryOperatorType.LogicalAnd:
                    return dynamicLeftValue && dynamicRightValue;

                case BinaryOperatorType.LessThan:
                    return dynamicLeftValue < dynamicRightValue;

                case BinaryOperatorType.LessThanOrEqual:
                    return dynamicLeftValue <= dynamicRightValue;

                case BinaryOperatorType.GreaterThan:
                    return dynamicLeftValue > dynamicRightValue;

                case BinaryOperatorType.GreaterThanOrEqual:
                    return dynamicLeftValue >= dynamicRightValue;

                case BinaryOperatorType.Addition:
                    return dynamicLeftValue + dynamicRightValue;

                case BinaryOperatorType.Subtraction:
                    return dynamicLeftValue - dynamicRightValue;

                case BinaryOperatorType.Multiply:
                    return dynamicLeftValue * dynamicRightValue;

                case BinaryOperatorType.Division:
                    var convertedToLong = long.TryParse(rightValue.ToString(), out long num);
                    if (convertedToLong && num == 0)
                    {
                        BaZicInterpreter.ChangeState(this, new DivideByZeroException(L.BaZic.Runtime.Interpreters.Expressions.BinaryOperatorInterpreter.DivideByZero), Expression.RightExpression);
                        return null;
                    }

                    return dynamicLeftValue / dynamicRightValue;

                case BinaryOperatorType.Modulus:
                    return dynamicLeftValue % dynamicRightValue;

                default:
                    throw new InternalException(L.BaZic.Runtime.Interpreters.Expressions.BinaryOperatorInterpreter.FormattedOperatorNotImplemented(Expression.Operator, nameof(BinaryOperatorInterpreter)));
            }
        }
    }
}