using BaZic.Core.ComponentModel;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using BaZic.Runtime.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Markup;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter
{
    /// <summary>
    /// Provide a sets of method to interpret a BaZic program.
    /// </summary>
    internal sealed class ProgramInterpreter : Interpreter
    {
        #region Fields & Constants

        private readonly BaZicProgram _program;
        private readonly BaZicUiProgram _uiProgram;
        private Action _closeUiAction;

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
        internal Window UserInterface { get; private set; }

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
        internal ProgramInterpreter(BaZicInterpreterCore baZicInterpreter, BaZicProgram program)
            : base(baZicInterpreter, null)
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

            if (BaZicInterpreter.Verbose)
            {
                VerboseLog(L.BaZic.Runtime.Interpreters.ProgramInterpreter.DeclaringGlobalVariable);
            }

            foreach (var variable in _program.GlobalVariables)
            {
                AddVariable(variable);
            }

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
            foreach (var argument in args)
            {
                argsExpressions.Add(new PrimitiveExpression(argument));
            }

            var entryPointInvocation = new InvokeMethodExpression(Consts.EntryPointMethodName, false).WithParameters(new ArrayCreationExpression().WithValues(argsExpressions.ToArray()));
            var entryPointInterpreter = new MethodInterpreter(BaZicInterpreter, this, entryPoint, entryPointInvocation);
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

                    UserInterface = XamlReader.Parse(_uiProgram.Xaml) as Window;

                    if (UserInterface == null)
                    {
                        BaZicInterpreter.ChangeState(this, new UiException(L.BaZic.Parser.XamlUnknownParsingError));
                        return;
                    }

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

                            var eventMethodDeclaration = _uiProgram.Methods.Single(m => m.Id == uiEvent.MethodId);
                            var eventInvocation = new InvokeMethodExpression(eventMethodDeclaration.Name.Identifier, false);
                            var eventMethodInterpreter = new MethodInterpreter(BaZicInterpreter, this, eventMethodDeclaration, eventInvocation);

                            if (targetObject is Window && uiEvent.ControlEventName == nameof(Window.Closed))
                            {
                                ProgramResult = eventMethodInterpreter.Invoke();
                            }
                            else
                            {
                                eventMethodInterpreter.Invoke();
                            }
                        });

                        BaZicInterpreter.Reflection.SubscribeEvent(targetObject, uiEvent.ControlEventName, action);
                    }

                    if (BaZicInterpreter.Verbose)
                    {
                        VerboseLog(L.BaZic.Runtime.Interpreters.ProgramInterpreter.DeclaringBindings);
                    }

                    foreach (var binding in _uiProgram.UiBindings)
                    {
                        AddVariable(binding);
                    }

                    if (BaZicInterpreter.Verbose)
                    {
                        VerboseLog(L.BaZic.Runtime.Interpreters.ProgramInterpreter.ShowUi);
                    }

                    UserInterface.Closed += (sender, e) =>
                    {
                        if (BaZicInterpreter.Verbose)
                        {
                            VerboseLog(L.BaZic.Runtime.Interpreters.ProgramInterpreter.CloseUi);
                        }
                        UserInterface?.Dispatcher?.InvokeShutdown();
                    };

                    _closeUiAction = () =>
                    {
                        _closeUiAction = null;
                        UserInterface?.Dispatcher?.InvokeShutdown();
                    };

                    try
                    {
                        UserInterface.Show();
                        System.Windows.Threading.Dispatcher.Run();
                    }
                    catch (Exception exception)
                    {
                        eventException = exception;
                    }
                    finally
                    {
                        try
                        {
                            UserInterface.Close();
                        }
                        catch { }

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
            else
            {
                // Duplicated with above, but important because it is not in the same thread.
                foreach (var variable in Variables)
                {
                    variable.Dispose();
                }
            }
        }

        /// <summary>
        /// Request to close the user interface.
        /// </summary>
        internal void CloseUserInterface()
        {
            _closeUiAction?.Invoke();
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
