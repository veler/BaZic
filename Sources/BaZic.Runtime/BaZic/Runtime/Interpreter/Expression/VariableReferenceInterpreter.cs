using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Memory;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter.Expression
{
    /// <summary>
    /// Provide the interpreter for a variable reference expression.
    /// </summary>
    internal sealed class VariableReferenceInterpreter : ExpressionInterpreter<VariableReferenceExpression>
    {
        internal VariableReferenceInterpreter(BaZicInterpreterCore baZicInterpreter, Interpreter parentInterpreter, VariableReferenceExpression expression)
            : base(baZicInterpreter, parentInterpreter, expression)
        {
        }

        /// <inheritdoc/>
        internal override object Run()
        {
            var variable = ParentInterpreter.GetVariable(Expression.VariableDeclarationID, Expression.Name.Identifier, true);

            if (variable != null)
            {
                return variable.Value;
            }

            return null;
        }

        /// <summary>
        /// Retrieves the variable in memory to assign it.
        /// </summary>
        /// <returns>A variable.</returns>
        internal Variable GetAssignableObject()
        {
            return ParentInterpreter.GetVariable(Expression.VariableDeclarationID, Expression.Name.Identifier, true);
        }
    }
}
