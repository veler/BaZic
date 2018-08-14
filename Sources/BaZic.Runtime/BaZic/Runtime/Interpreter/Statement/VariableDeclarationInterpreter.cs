using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using System;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter.Statement
{
    /// <summary>
    /// Provide the interpreter for a variable declaration
    /// </summary>
    internal sealed class VariableDeclarationInterpreter : StatementInterpreter<VariableDeclaration>
    {
        internal VariableDeclarationInterpreter(BaZicInterpreterCore baZicInterpreter, BlockInterpreter parentInterpreter, Guid executionFlowId, VariableDeclaration statement)
            : base(baZicInterpreter, parentInterpreter, executionFlowId, statement)
        {
        }

        /// <inheritdoc/>
        internal override void Run()
        {
            ParentInterpreter.AddVariable(Statement);
        }
    }
}
