using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter.Statement
{
    /// <summary>
    /// Provide the interpreter for a label condition statement.
    /// </summary>
    internal sealed class LabelConditionInterpreter : StatementInterpreter<LabelConditionStatement>
    {
        /// <summary>
        /// Gets whether the condition has been validated or not.
        /// </summary>
        internal bool ConditionValidated { get; private set; }

        internal LabelConditionInterpreter(BaZicInterpreterCore baZicInterpreter, BlockInterpreter parentInterpreter, LabelConditionStatement statement)
            : base(baZicInterpreter, parentInterpreter, statement)
        {
        }

        /// <inheritdoc/>
        internal override void Run()
        {
            var conditionResult = ConditionInterpreter.RunCondition(BaZicInterpreter, ParentInterpreter, Statement.Condition);

            if (ParentInterpreter.IsAborted || conditionResult == null)
            {
                return;
            }

            ConditionValidated = conditionResult.Value;
        }
    }
}
