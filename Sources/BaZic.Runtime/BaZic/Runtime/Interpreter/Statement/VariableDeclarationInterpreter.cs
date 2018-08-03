using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter.Statement
{
    /// <summary>
    /// Provide the interpreter for a variable declaration
    /// </summary>
    internal sealed class VariableDeclarationInterpreter : StatementInterpreter<VariableDeclaration>
    {
        internal VariableDeclarationInterpreter(BaZicInterpreterCore baZicInterpreter, BlockInterpreter parentInterpreter, VariableDeclaration statement)
            : base(baZicInterpreter, parentInterpreter, statement)
        {
        }

        /// <inheritdoc/>
        internal override void Run()
        {
            ParentInterpreter.AddVariable(Statement);
        }
    }
}
