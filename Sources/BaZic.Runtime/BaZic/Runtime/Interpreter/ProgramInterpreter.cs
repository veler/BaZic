using BaZic.Core.ComponentModel;
using BaZic.Core.IO.Serialization;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using BaZic.Runtime.BaZic.Runtime.Interpreter.Expression;
using BaZic.Runtime.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter
{
    /// <summary>
    /// Provide a sets of method to interpret a BaZic program.
    /// </summary>
    [Serializable]
    internal sealed class ProgramInterpreter : Interpreter
    {
        #region Fields & Constants

        private readonly BaZicProgram _program;
        private readonly BaZicUiProgram _uiProgram;

        private bool _globalStateInitialized;
        private Thread _uiThread;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the number of synchronous BaZic method invocation.
        /// </summary>
        internal int SynchronousCallCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of synchronous BaZic method invocation since the beginning of the execution of the program.
        /// </summary>
        internal int TotalSynchronousCallCount { get; set; }

        /// <summary>
        /// Gets the user interface if the interpreted program is a <see cref="BaZicUiProgram"/>.
        /// </summary>
        internal FrameworkElement UserInterface { get; private set; }

        /// <summary>
        /// Gets the dispatcher of the UI.
        /// </summary>
        internal Dispatcher UIDispatcher { get; private set; }

        /// <summary>
        /// Gets the result of the Main method or the result of the Window.Closed event from the user interface.
        /// </summary>
        internal object ProgramResult { get; private set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramInterpreter"/> class.
        /// </summary>
        /// <param name="baZicInterpreter">The main interpreter.</param>
        /// <param name="program">The <see cref="BaZicProgram"/> to interpret.</param>
        /// <param name="executionFlowId">A GUID that defines in which callstack is linked.</param>
        internal ProgramInterpreter(BaZicInterpreterCore baZicInterpreter, BaZicProgram program, Guid executionFlowId)
            : base(baZicInterpreter, null, executionFlowId)
        {
            Requires.NotNull(program, nameof(program));
            _program = program;
            _uiProgram = program as BaZicUiProgram;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Start the program by finding the entry point and calling it
        /// </summary>
        /// <param name="args">The arguments to pass to the entry point.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Start(object[] args)
        {
            BaZicInterpreter.CheckState(BaZicInterpreterState.Preparing);

            InitializeGlobalState();

            var entryPoint = GetEntryPointMethod();

            if (IsAborted)
            {
                return;
            }

            if (BaZicInterpreter.Verbose)
            {
                VerboseLog(L.BaZic.Runtime.Interpreters.ProgramInterpreter.EntryPointDetected);
            }

            BaZicInterpreter.ChangeState(this, new BaZicInterpreterStateChangeEventArgs(BaZicInterpreterState.Running));

            var argsExpressions = new List<Code.AbstractSyntaxTree.Expression>();
            if (args != null)
            {
                foreach (var argument in args)
                {
                    argsExpressions.Add(new PrimitiveExpression(argument));
                }
            }

            var entryPointInvocation = new InvokeMethodExpression(Consts.EntryPointMethodName, false).WithParameters(new ArrayCreationExpression().WithValues(argsExpressions.ToArray()));
            var entryPointInterpreter = new MethodInterpreter(BaZicInterpreter, this, entryPoint, entryPointInvocation, ExecutionFlowId);
            ProgramResult = entryPointInterpreter.Invoke();

            if (IsAborted)
            {
                return;
            }

            if (_uiProgram != null && ProgramResult == null)
            {
                Exception eventException = null;

                ThreadHelper.RunOnStaThread(() =>
                {
                    if (BaZicInterpreter.Verbose)
                    {
                        VerboseLog(L.BaZic.Runtime.Interpreters.ProgramInterpreter.LoadingUi);
                    }

                    _uiThread = Thread.CurrentThread;
                    UserInterface = SerializationHelper.ConvertFromXaml(_uiProgram.Xaml) as FrameworkElement;

                    if (UserInterface == null)
                    {
                        BaZicInterpreter.ChangeState(this, new UiException(L.BaZic.Parser.XamlUnknownParsingError));
                        return;
                    }

                    UIDispatcher = UserInterface.Dispatcher;

                    if (BaZicInterpreter.Verbose)
                    {
                        VerboseLog(L.BaZic.Runtime.Interpreters.ProgramInterpreter.DeclaringEvents);
                    }

                    foreach (var uiEvent in _uiProgram.UiEvents)
                    {
                        var targetObject = UserInterface.FindName(uiEvent.ControlName);

                        if (targetObject == null)
                        {
                            BaZicInterpreter.ChangeState(this, new UiException($"Unable to find the control named '{uiEvent.ControlName}'."));
                            return;
                        }

                        var action = new Action(() =>
                        {
                            if (BaZicInterpreter.Verbose)
                            {
                                VerboseLog(L.BaZic.Runtime.Interpreters.ProgramInterpreter.EventRaised);
                            }

                            BaZicInterpreter.ChangeState(this, new BaZicInterpreterStateChangeEventArgs(BaZicInterpreterState.Running));
                            var eventMethodDeclaration = _uiProgram.Methods.Single(m => m.Id == uiEvent.MethodId);
                            var eventInvocation = new InvokeMethodExpression(eventMethodDeclaration.Name.Identifier, false);
                            var eventMethodInterpreter = new MethodInterpreter(BaZicInterpreter, this, eventMethodDeclaration, eventInvocation, ExecutionFlowId);

                            if (targetObject is Window && uiEvent.ControlEventName == nameof(Window.Closed))
                            {
                                ProgramResult = eventMethodInterpreter.Invoke();
                            }
                            else
                            {
                                eventMethodInterpreter.Invoke();
                            }

                            BaZicInterpreter.RunningStateManager.SetIsRunningMainFunction(false); // In all cases, at this point, the main function is done. The UI is running. Idle state must be able to be setted.
                            BaZicInterpreter.RunningStateManager.UpdateState();
                        });

                        BaZicInterpreter.Reflection.SubscribeEvent(targetObject, uiEvent.ControlEventName, action);
                    }

                    if (BaZicInterpreter.Verbose)
                    {
                        VerboseLog(L.BaZic.Runtime.Interpreters.ProgramInterpreter.DeclaringBindings);
                    }

                    foreach (var controlAccessor in _uiProgram.UiControlAccessors)
                    {
                        AddVariable(controlAccessor);
                    }

                    if (BaZicInterpreter.Verbose)
                    {
                        VerboseLog(L.BaZic.Runtime.Interpreters.ProgramInterpreter.ShowUi);
                    }

                    var window = UserInterface as Window;

                    try
                    {
                        if (window != null)
                        {
                            window.Closed += (sender, e) =>
                            {
                                if (BaZicInterpreter.Verbose)
                                {
                                    VerboseLog(L.BaZic.Runtime.Interpreters.ProgramInterpreter.CloseUi);
                                }
                                UserInterface?.Dispatcher?.InvokeShutdown();
                            };
                        }

                        BaZicInterpreter.ChangeState(this, new BaZicInterpreterStateChangeEventArgs(BaZicInterpreterState.Idle));
                        window?.Show();
                        Dispatcher.Run();
                    }
                    catch (Exception exception)
                    {
                        CoreHelper.ReportException(exception);
                        eventException = exception;
                    }
                    finally
                    {
                        try
                        {
                            window?.Close();
                        }
                        catch (Exception exception)
                        {
                            CoreHelper.ReportException(exception);
                        }

                        foreach (var variable in Variables)
                        {
                            variable.Dispose();
                        }

                        BaZicInterpreter.Reflection.UnsubscribeAllEvents();
                        UserInterface = null;
                    }
                });

                if (eventException != null)
                {
                    throw eventException;
                }
            }
        }

        /// <summary>
        /// Request to close the user interface.
        /// </summary>
        internal void CloseUserInterface()
        {
            try
            {
                UIDispatcher.Invoke(() =>
                {
                    if (UserInterface is Window window)
                    {
                        window.Close();
                    }
                    Dispatcher.CurrentDispatcher?.InvokeShutdown();
                }, DispatcherPriority.Send);
                UIDispatcher = null;
            }
            catch { }
        }

        /// <summary>
        /// Invoke a public method accessible from outside of the interpreter (EXTERN FUNCTION).
        /// </summary>
        /// <param name="executionFlowId">A GUID that defines in which callstack is linked.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="awaitIfAsync">Await if the method is maked as asynchronous.</param>
        /// <param name="args">The arguments to pass to the method.</param>
        /// <returns>Returns the result of the invocation (a <see cref="Task"/> in the case of a not awaited asynchronous method, or the value returned by the method).</returns>
        internal object InvokeMethod(Guid executionFlowId, string methodName, bool awaitIfAsync, Code.AbstractSyntaxTree.Expression[] args)
        {
            InitializeGlobalState();

            BaZicInterpreter.CheckState(BaZicInterpreterState.Idle, BaZicInterpreterState.Stopped, BaZicInterpreterState.StoppedWithError);

            var invokeExpression = new InvokeMethodExpression(methodName, awaitIfAsync).WithParameters(args);

            BaZicInterpreter.ChangeState(this, new BaZicInterpreterStateChangeEventArgs(BaZicInterpreterState.Running));

            return new InvokeMethodInterpreter(BaZicInterpreter, this, invokeExpression, executionFlowId, true).Run();
        }

        /// <summary>
        /// Declares the global variables if needed.
        /// </summary>
        internal void InitializeGlobalState()
        {
            if (_globalStateInitialized)
            {
                return;
            }

            if (BaZicInterpreter.Verbose)
            {
                VerboseLog(L.BaZic.Runtime.Interpreters.ProgramInterpreter.DeclaringGlobalVariable);
            }

            foreach (var variable in _program.GlobalVariables)
            {
                AddVariable(variable);
            }

            _globalStateInitialized = true;
        }

        /// <summary>
        /// Retrieves and check is accuracy of the entry point of the program.
        /// </summary>
        /// <returns>Returns the entry point founded.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private EntryPointMethod GetEntryPointMethod()
        {
            var entryPoints = _program.Methods.OfType<EntryPointMethod>().ToList();
            if (entryPoints.Count == 0)
            {
                BaZicInterpreter.ChangeState(this, new MissingEntryPointMethodException());
                return null;
            }
            else if (entryPoints.Count > 1)
            {
                BaZicInterpreter.ChangeState(this, new SeveralEntryPointMethodException(), entryPoints.Last());
                return null;
            }

            var entryPoint = entryPoints.Single();

            if (entryPoint.Arguments.Count != 1)
            {
                BaZicInterpreter.ChangeState(this, new BadArgumentException(L.BaZic.Parser.Statements.UniqueArgumentEntryPoint), entryPoint);
                return null;
            }
            else if (!entryPoint.Arguments.Single().IsArray)
            {
                BaZicInterpreter.ChangeState(this, new BadArgumentException(L.BaZic.Parser.Statements.EntryPointArgumentArrayExpected), entryPoint.Arguments.Single());
                return null;
            }

            return entryPoint;
        }

        #endregion
    }
}
