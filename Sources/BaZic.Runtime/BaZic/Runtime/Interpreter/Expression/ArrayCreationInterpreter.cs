using BaZicProgramReleaseMode;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter.Expression
{
    /// <summary>
    /// Provide the interpreter for a variable reference expression.
    /// </summary>
    internal sealed class ArrayCreationInterpreter : ExpressionInterpreter<ArrayCreationExpression>
    {
        internal ArrayCreationInterpreter(BaZicInterpreterCore baZicInterpreter, Interpreter parentInterpreter, ArrayCreationExpression expression)
            : base(baZicInterpreter, parentInterpreter, expression)
        {
        }

        /// <inheritdoc/>
        internal override object Run()
        {
            var array = new ObservableDictionary();

            foreach (var value in Expression.Values)
            {
                array.Add(ParentInterpreter.RunExpression(value));
            }

            return array;
        }
    }
}
