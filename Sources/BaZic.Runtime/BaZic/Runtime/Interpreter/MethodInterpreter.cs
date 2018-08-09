using BaZic.Core.ComponentModel;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Debugger;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using BaZic.Runtime.Localization;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter
{
    /// <summary>
    /// Provide a sets of method to interpret a method.
    /// </summary>
    internal sealed class MethodInterpreter : Interpreter
    {
        #region Fields & Constants

        private readonly MethodDeclaration _methodDeclaration;
        private readonly InvokeMethodExpression _invokeMethod;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the value returned by the method.
        /// </summary>
        internal object ReturnedValue { get; set; }

        /// <summary>
        /// Gets the debug information about the current call.
        /// </summary>
        internal Call DebugCallInfo { get; private set; }

        /// <summary>
        /// Gets whether the method is asynchrone and has been invoked without the await operator.
        /// </summary>
        internal bool AsyncMethodCalledWithoutAwait { get; private set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodInterpreter"/> class.
        /// </summary>
        /// <param name="baZicInterpreter">The main interpreter.</param>
        /// <param name="parentInterpreter">The parent interpreter.</param>
        /// <param name="methodDeclaration">The declaration of the method to interpret.</param>
        /// <param name="invokeMethod">The expression that performs the invocation.</param>
        internal MethodInterpreter(BaZicInterpreterCore baZicInterpreter, Interpreter parentInterpreter, MethodDeclaration methodDeclaration, InvokeMethodExpression invokeMethod)
            : base(baZicInterpreter, parentInterpreter)
        {
            Requires.NotNull(methodDeclaration, nameof(methodDeclaration));
            Requires.NotNull(invokeMethod, nameof(invokeMethod));
            _methodDeclaration = methodDeclaration;
            _invokeMethod = invokeMethod;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Run the method.
        /// </summary>
        /// <returns>Returns the result of the invocation (a <see cref="Task"/> in the case of a not awaited asynchronous method, or the value returned by the method).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal object Invoke()
        {
            if (BaZicInterpreter.Verbose)
            {
                VerboseLog(L.BaZic.Runtime.Interpreters.MethodInterpreter.FormattedPreparing(_invokeMethod.MethodName));
            }

            DebugCallInfo = new Call(_invokeMethod);

            var awaitCall = _invokeMethod.Await;

            if (awaitCall && !_methodDeclaration.IsAsync)
            {
                BaZicInterpreter.ChangeState(this, new MethodNotAwaitableException(_methodDeclaration.Name.Identifier), _invokeMethod);
                return ReturnedValue;
            }

            if (_methodDeclaration.Arguments.Count != _invokeMethod.Arguments.Count)
            {
                BaZicInterpreter.ChangeState(this, new MethodNotFoundException(_methodDeclaration.Name.Identifier, L.BaZic.Runtime.Interpreters.MethodInterpreter.FormattedBadArgumentCount(_methodDeclaration.Name, _invokeMethod.Arguments.Count)), _invokeMethod);
                return ReturnedValue;
            }

            // Execute argument's values.
            if (BaZicInterpreter.Verbose)
            {
                VerboseLog(L.BaZic.Runtime.Interpreters.MethodInterpreter.ExecutingArguments);
            }
            var argumentValues = new List<object>();
            for (var i = 0; i < _invokeMethod.Arguments.Count; i++)
            {
                var argumentValue = RunExpression(_invokeMethod.Arguments[i]);
                argumentValues.Add(argumentValue);
            }

            if (IsAborted)
            {
                return ReturnedValue;
            }

            if (BaZicInterpreter.ProgramInterpreter.TotalSynchronousCallCount > 0 && BaZicInterpreter.ProgramInterpreter.TotalSynchronousCallCount % 500 == 0)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            // Execute the method.
            if (_methodDeclaration.IsAsync)
            {
                if (awaitCall)
                {
                    if (BaZicInterpreter.Verbose)
                    {
                        VerboseLog(L.BaZic.Runtime.Interpreters.MethodInterpreter.FormattedAwait(_invokeMethod.MethodName));
                    }
                    RunAsync(argumentValues).Wait();
                    return ReturnedValue;
                }
                else
                {
                    if (BaZicInterpreter.Verbose)
                    {
                        VerboseLog(L.BaZic.Runtime.Interpreters.MethodInterpreter.FormattedAsync(_invokeMethod.MethodName));
                    }
                    AsyncMethodCalledWithoutAwait = true;
                    var task = RunAsync(argumentValues);
                    BaZicInterpreter.AddUnwaitedMethodInvocation(task);
                    return task;
                }
            }
            else
            {
                if (BaZicInterpreter.Verbose)
                {
                    VerboseLog(L.BaZic.Runtime.Interpreters.MethodInterpreter.FormattedSync(_invokeMethod.MethodName));
                }

                BaZicInterpreter.ProgramInterpreter.SynchronousCallCount++;
                BaZicInterpreter.ProgramInterpreter.TotalSynchronousCallCount++;

                if (BaZicInterpreter.ProgramInterpreter.TotalSynchronousCallCount >= Consts.StackOverflowLimit)
                {
                    BaZicInterpreter.ChangeState(this, new Debugger.Exceptions.StackOverflowException(L.BaZic.Runtime.Interpreters.MethodInterpreter.FormattedStackOverflow(Consts.StackOverflowLimit)));
                    return ReturnedValue;
                }

                if (BaZicInterpreter.ProgramInterpreter.SynchronousCallCount >= Consts.CallLimitBeforeNewThread)
                {
                    BaZicInterpreter.ProgramInterpreter.SynchronousCallCount = 0;
                    CallMethodNewThread(argumentValues).Wait();
                }
                else
                {
                    RunSync(argumentValues);
                }
                return ReturnedValue;
            }
        }

        /// <summary>
        /// Invoke a method in a new thread to avoid a stack overflow exception.
        /// </summary>
        /// <param name="argumentValues">The arguments values.</param>
        /// <returns>Returns a task associated to the execution.</returns>
        private async Task CallMethodNewThread(List<object> argumentValues)
        {
            var thread = new Thread((ThreadStart)delegate { RunSync(argumentValues); }, 0);
            thread.Start();
            thread.Join();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            await Task.CompletedTask;
        }

        /// <summary>
        /// Execute a method in a new thread.
        /// </summary>
        /// <returns>Returns a task associated to the execution.</returns>
        private Task RunAsync(IReadOnlyList<object> argumentValues)
        {
            return Task.Run(() => RunSync(argumentValues));
        }

        /// <summary>
        /// Execute a method in the current thread
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RunSync(IReadOnlyList<object> argumentValues)
        {
            // Defines arguments.
            for (var i = 0; i < _methodDeclaration.Arguments.Count; i++)
            {
                var argumentDeclaration = _methodDeclaration.Arguments[i];
                var variableDeclaration = new VariableDeclaration(argumentDeclaration.Name.Identifier, argumentDeclaration.IsArray)
                {
                    Id = argumentDeclaration.Id,
                    Line = argumentDeclaration.Line,
                    Column = argumentDeclaration.Column,
                    StartOffset = argumentDeclaration.StartOffset,
                    NodeLength = argumentDeclaration.NodeLength
                };
                var variableRef = new VariableReferenceExpression(variableDeclaration)
                {
                    VariableDeclarationID = argumentDeclaration.Id,
                    Line = argumentDeclaration.Line,
                    Column = argumentDeclaration.Column,
                    StartOffset = argumentDeclaration.StartOffset,
                    NodeLength = argumentDeclaration.NodeLength
                };

                AddVariable(variableDeclaration, searchInParents: false);
                SetVariable(variableRef, argumentValues[i]);
            }

            if (IsAborted)
            {
                return;
            }

            // Execute statements
            var block = new BlockInterpreter(BaZicInterpreter, this, false, null, _methodDeclaration.Statements);
            block.Run();

            if (BaZicInterpreter.Verbose)
            {
                VerboseLog(L.BaZic.Runtime.Interpreters.MethodInterpreter.FormattedEndExecution(_invokeMethod.MethodName, ReturnedValue, ValueInfo.GetValueInfo(ReturnedValue)));
            }

            foreach (var variable in Variables)
            {
                if (DebugCallInfo.Variables.Remove(variable))
                {
                    variable.Dispose();
                }
            }

            DebugCallInfo = null;

            if (!_invokeMethod.Await && _methodDeclaration.IsAsync)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        #endregion
    }
}
