using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter.Expression
{
    /// <summary>
    /// Provide the interpreter for a primitive expression.
    /// </summary>
    internal sealed class PrimitiveInterpreter : ExpressionInterpreter<PrimitiveExpression>
    {
        internal PrimitiveInterpreter(BaZicInterpreterCore baZicInterpreter, Interpreter parentInterpreter, PrimitiveExpression expression)
            : base(baZicInterpreter, parentInterpreter, expression)
        {
        }

        /// <inheritdoc/>
        internal override object Run()
        {
            return Expression.Value;
        }
    }
}
