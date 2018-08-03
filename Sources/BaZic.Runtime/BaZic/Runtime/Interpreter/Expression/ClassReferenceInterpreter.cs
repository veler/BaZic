using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter.Expression
{
    /// <summary>
    /// Provide the interpreter for a class reference expression.
    /// </summary>
    internal sealed class ClassReferenceInterpreter : ExpressionInterpreter<ClassReferenceExpression>
    {
        internal ClassReferenceInterpreter(BaZicInterpreterCore baZicInterpreter, Interpreter parentInterpreter, ClassReferenceExpression expression)
            : base(baZicInterpreter, parentInterpreter, expression)
        {
        }

        /// <inheritdoc/>
        internal override object Run()
        {
            return BaZicInterpreter.Reflection.GetTypeRef(string.Concat(Expression.Namespace, ".", Expression.ClassName));
        }
    }
}
