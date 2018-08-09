using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Debugger;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using BaZic.Runtime.BaZic.Runtime.Interpreter.Statement;
using BaZic.Runtime.Localization;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter
{
    /// <summary>
    /// Provide a sets of method to interpret a block of statements.
    /// </summary>
    internal sealed class BlockInterpreter : Interpreter
    {
        #region Fields & Constants

        private readonly IReadOnlyList<Code.AbstractSyntaxTree.Statement> _statements;

        private Dictionary<string, int> _labelRegistry;
        private int _executionCursor;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the state of the block and the reason why it stopped.
        /// </summary>
        internal BlockState State { get; }

        /// <summary>
        /// Gets an exception that has been thrown and caught by a <see cref="TryCatchStatement"/>.
        /// </summary>
        internal Exception CaughtException { get; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockInterpreter"/> class.
        /// </summary>
        /// <param name="baZicInterpreter">The main interpreter.</param>
        /// <param name="parentInterpreter">The parent interpreter.</param>
        /// <param name="isInIteration">Defines whether the current block of statement is executed inside of a iteration statement.</param>
        /// <param name="caughtException">Defines the caught exception.</param>
        /// <param name="statements">The list of statements to interpret.</param>
        internal BlockInterpreter(BaZicInterpreterCore baZicInterpreter, Interpreter parentInterpreter, bool isInIteration, Exception caughtException, IReadOnlyList<Code.AbstractSyntaxTree.Statement> statements)
            : base(baZicInterpreter, parentInterpreter)
        {
            CaughtException = caughtException;
            State = new BlockState(isInIteration);
            _statements = statements;
            FillLabelRegistry();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Execute the statements of the block.
        /// </summary>
        internal void Run()
        {
            _executionCursor = 0;
            while (_executionCursor < _statements.Count)
            {
                RunStatement(_statements[_executionCursor]);

                BaZicInterpreter.AttemptPauseIfRequired(this);

                if (IsAborted || State.ExitMethod || State.ExitIteration || State.ExitBlockBecauseOfLabelJump)
                {
                    break;
                }

                _executionCursor++;
            }

            var parentMethodInterpreter = GetParentMethodInterpreter();
            foreach (var variable in Variables)
            {
                if (parentMethodInterpreter.DebugCallInfo.Variables.Remove(variable))
                {
                    variable.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets the debug information of the current thread. If the program is optimized, returns null.
        /// </summary>
        /// <returns>The debug information</returns>
        internal DebugInfo GetDebugInfo()
        {
            if (BaZicInterpreter.ProgramIsOptimized)
            {
                return null;
            }

            var debugInfo = new DebugInfo();
            var stackDeep = 0;
            var parentMethodInterpreter = GetParentMethodInterpreter();
            while (parentMethodInterpreter != null)
            {
                var parentOfParentMethodInterpreter = parentMethodInterpreter.GetParentMethodInterpreter(throwIfNotFound: false);
                if (parentOfParentMethodInterpreter != null && parentOfParentMethodInterpreter.AsyncMethodCalledWithoutAwait && stackDeep > 0)
                {
                    debugInfo.CallStack.Add(null);
                }
                else
                {
                    debugInfo.CallStack.Add(parentMethodInterpreter.DebugCallInfo);
                }

                parentMethodInterpreter = parentOfParentMethodInterpreter;
                stackDeep++;
            }

            debugInfo.GlobalVariables = BaZicInterpreter.ProgramInterpreter.Variables;

            return debugInfo;
        }

        /// <summary>
        /// Move the execution cursor to the specified label. If the label is in a parent block, it sets a flag to exit the current block.
        /// </summary>
        /// <param name="labelName">The name of the label to jump to.</param>
        private void JumpToLabel(string labelName)
        {
            if (BaZicInterpreter.Verbose)
            {
                VerboseLog(L.BaZic.Runtime.Interpreters.BlockInterpreter.FormattedGoTo(labelName));
            }

            var cursor = -1;
            if (!_labelRegistry.TryGetValue(labelName, out cursor))
            {
                if (ParentInterpreter is BlockInterpreter blockInterpreter)
                {
                    State.ExitBlockBecauseOfLabelJump = true;
                    blockInterpreter.JumpToLabel(labelName);
                    return;
                }

                BaZicInterpreter.ChangeState(this, new InternalException(L.BaZic.Runtime.Interpreters.BlockInterpreter.FormattedLabelNotFound(labelName)));
            }

            _executionCursor = cursor;
        }

        /// <summary>
        /// Save in memory the index of execution of each label declaration.
        /// </summary>
        private void FillLabelRegistry()
        {
            if (BaZicInterpreter.Verbose)
            {
                VerboseLog(L.BaZic.Runtime.Interpreters.BlockInterpreter.DeclaringLabels);
            }

            _labelRegistry = new Dictionary<string, int>();

            for (var i = 0; i < _statements.Count; i++)
            {
                if (_statements[i] is LabelDeclaration labelDeclaration)
                {
                    _labelRegistry.Add(labelDeclaration.Name.Identifier, i);
                }
            }
        }

        /// <summary>
        /// Execute a statement.
        /// </summary>
        /// <param name="statement">The statement to interpret.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RunStatement(Code.AbstractSyntaxTree.Statement statement)
        {
            if (BaZicInterpreter.Verbose)
            {
                VerboseLog(L.BaZic.Runtime.Interpreters.BlockInterpreter.FormattedExecutingStatement(statement.GetType().Name));
            }

            switch (statement)
            {
                case AssignStatement assign:
                    new AssignInterpreter(BaZicInterpreter, this, assign).Run();
                    break;

                case BreakStatement @break:
                    new BreakInterpreter(BaZicInterpreter, this, @break).Run();
                    break;

                case CommentStatement comment:
                    // Just ignore
                    break;

                case ConditionStatement condition:
                    var conditionInterpreter = new ConditionInterpreter(BaZicInterpreter, this, condition);
                    conditionInterpreter.Run();
                    ApplyChildBlockState(conditionInterpreter.ChildBlockState);
                    break;

                case GoToLabelStatement goToLabel:
                    JumpToLabel(goToLabel.Name.Identifier);
                    break;

                case IterationStatement iteration:
                    var iterationInterpreter = new IterationInterpreter(BaZicInterpreter, this, iteration);
                    iterationInterpreter.Run();
                    ApplyChildBlockState(iterationInterpreter.ChildBlockState);
                    break;

                case LabelConditionStatement labelCondition:
                    var labelConditionInterpreter = new LabelConditionInterpreter(BaZicInterpreter, this, labelCondition);
                    labelConditionInterpreter.Run();
                    if (!IsAborted && labelConditionInterpreter.ConditionValidated)
                    {
                        JumpToLabel(labelCondition.LabelName.Identifier);
                    }
                    break;

                case LabelDeclaration labelDeclaration:
                    // Just ignore
                    break;

                case ReturnStatement @return:
                    new ReturnInterpreter(BaZicInterpreter, this, @return).Run();
                    break;

                case ThrowStatement @throw:
                    new ThrowInterpreter(BaZicInterpreter, this, @throw).Run();
                    break;

                case TryCatchStatement tryCatch:
                    var tryCatchInterpreter = new TryCatchInterpreter(BaZicInterpreter, this, tryCatch);
                    tryCatchInterpreter.Run();
                    ApplyChildBlockState(tryCatchInterpreter.ChildBlockState);
                    break;

                case VariableDeclaration variableDeclaration:
                    new VariableDeclarationInterpreter(BaZicInterpreter, this, variableDeclaration).Run();
                    break;

                case ExpressionStatement expressionStatement:
                    new ExpressionStatementInterpreter(BaZicInterpreter, this, expressionStatement).Run();
                    break;

                case BreakpointStatement breakpoint:
                    if (BaZicInterpreter.Verbose)
                    {
                        VerboseLog(L.BaZic.Runtime.Interpreters.BlockInterpreter.BreakpointIntercepted);
                    }
                    BaZicInterpreter.Pause();
                    Task.Delay(100).Wait();
                    break;

                default:
                    BaZicInterpreter.ChangeState(this, new InternalException(L.BaZic.Runtime.Interpreters.BlockInterpreter.FormattedInterpreterNotFound(statement.GetType().FullName)), statement);
                    break;
            }

            if (BaZicInterpreter.Verbose && (State.ExitMethod || State.ExitIteration || State.ExitBlockBecauseOfLabelJump || IsAborted))
            {
                VerboseLog(L.BaZic.Runtime.Interpreters.BlockInterpreter.ExitingBlock);
            }
        }

        /// <summary>
        /// Apply a child block state from a statement (condition, iteration...etc) to the current block interpreter.
        /// </summary>
        /// <param name="state">The child block state.</param>
        private void ApplyChildBlockState(BlockState state)
        {
            if (state == null)
            {
                return;
            }

            State.ExitMethod = state.ExitMethod;
            State.ExitIteration = state.ExitIteration;
        }

        #endregion
    }
}
