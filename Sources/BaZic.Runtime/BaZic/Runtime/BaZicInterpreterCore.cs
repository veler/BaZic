using BaZic.Core.ComponentModel;
using BaZic.Core.ComponentModel.Assemblies;
using BaZic.Core.ComponentModel.Reflection;
using BaZic.Core.Enums;
using BaZic.Core.Logs;
using BaZic.Runtime.BaZic.Code;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Code.Parser;
using BaZic.Runtime.BaZic.Runtime.Debugger;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using BaZic.Runtime.BaZic.Runtime.Interpreter;
using BaZic.Runtime.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BaZic.Runtime.BaZic.Runtime
{
    /// <summary>
    /// Provides an interpreter for a BaZic program.
    /// </summary>
    [Serializable]
    internal sealed class BaZicInterpreterCore : MarshalByRefObject, Core.ComponentModel.IDisposable
    {
        #region Fields & Constants

        private readonly List<BaZicInterpreterStateChangeEventArgs> _stateChangedHistory;
        private readonly List<Task> _unwaitedMethodInvocation;

        private AssemblySandbox _assemblySandbox;
        private object[] _programArguments;
        private Task _mainInterpreterTask;
        private Thread _mainInterpreterThread;
        private CompiledProgramRunner _releaseModeRuntime;
        private CompilerResult _compilerResult;

        private AutoResetEvent _pauseModeWaiter;
        private bool _ignoreException;
        private bool _externMethodInvoked;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets the current state of the interpreter.
        /// </summary>
        internal BaZicInterpreterState State { get; private set; }

        /// <summary>
        /// Gets whether the execution of the program should generates logs.
        /// </summary>
        internal bool Verbose { get; private set; }

        /// <summary>
        /// Gets whether the execution of the program is in debug mode. If it is, the program is interpreted. If it is not, the program is compiled and run.
        /// </summary>
        internal bool DebugMode { get; private set; }

        /// <summary>
        /// Gets the current interpreter error.
        /// </summary>
        public Error Error { get; private set; }

        /// <summary>
        /// Gets the result of the execution of the program.
        /// </summary>
        internal object ProgramResult { get; private set; }

        /// <summary>
        /// Gets the historic of state changes.
        /// </summary>
        internal IReadOnlyList<BaZicInterpreterStateChangeEventArgs> StateChangedHistory => _stateChangedHistory.AsReadOnly();

        /// <summary>
        /// Gets a way to perform reflection quickly with acceptable performances.
        /// </summary>
        internal FastReflection Reflection { get; }

        /// <summary>
        /// Gets whether the interpreted program has been optimized.
        /// </summary>
        internal bool ProgramIsOptimized => Program.IsOptimized;

        /// <summary>
        /// Gets the list of method declared in the program.
        /// </summary>
        internal IReadOnlyList<MethodDeclaration> MethodDeclarations => Program.Methods;

        /// <summary>
        /// Gets the current program interpreter.
        /// </summary>
        internal ProgramInterpreter ProgramInterpreter { get; set; }

        /// <summary>
        /// Gets the list of debug information for every running thread in the program.
        /// </summary>
        internal DebugInfo DebugInfos { get; private set; }

        /// <summary>
        /// Gets the program actually used for running.
        /// </summary>
        internal BaZicProgram Program { get; }

        /// <summary>
        /// Gets a proxy used to get and listen to the interpreter state in a cross-AppDomain context.
        /// </summary>
        internal BaZicInterpreterStateChangedBridge StateBridgeProxy { get; private set; }

        #endregion

        #region Events

        private event BaZicInterpreterStateEventHandler LocalStateChanged;

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BaZicInterpreterCore"/> class.
        /// </summary>
        /// <param name="baZicInterpreterStateChangedBridge">The state changed bridge.</param>
        /// <param name="assemblySandbox">The assembly sandbox.</param>
        private BaZicInterpreterCore(BaZicInterpreterStateChangedBridge baZicInterpreterStateChangedBridge, AssemblySandbox assemblySandbox)
        {
            Requires.NotNull(baZicInterpreterStateChangedBridge, nameof(baZicInterpreterStateChangedBridge));
            Requires.NotNull(assemblySandbox, nameof(assemblySandbox));

            StateBridgeProxy = baZicInterpreterStateChangedBridge;
            _stateChangedHistory = new List<BaZicInterpreterStateChangeEventArgs>();
            ChangeState(this, new BaZicInterpreterStateChangeEventArgs(BaZicInterpreterState.Ready));

            _unwaitedMethodInvocation = new List<Task>();
            _assemblySandbox = assemblySandbox;
            Reflection = _assemblySandbox.Reflection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaZicInterpreterCore"/> class.
        /// </summary>
        /// <param name="baZicInterpreterStateChangedBridge">The state changed bridge.</param>
        /// <param name="assemblySandbox">The assembly sandbox.</param>
        /// <param name="inputCode">The BaZic code to interpret.</param>
        /// <param name="xamlCode">The XAML code to interpret that represents the user interface.</param>
        /// <param name="optimize">(optional) Defines whether the generated syntax tree must be optimized for the interpreter or not.</param>
        private BaZicInterpreterCore(BaZicInterpreterStateChangedBridge baZicInterpreterStateChangedBridge, AssemblySandbox assemblySandbox, string inputCode, string xamlCode, bool optimize = false)
            : this(baZicInterpreterStateChangedBridge, assemblySandbox)
        {
            var parser = new BaZicParser();
            var parsingResult = parser.Parse(inputCode, xamlCode, optimize);

            if (parsingResult.Issues.InnerExceptions.OfType<BaZicParserException>().Count(issue => issue.Level == BaZicParserExceptionLevel.Error) != 0 || (!parsingResult.Issues.InnerExceptions.OfType<BaZicParserException>().Any() && parsingResult.Issues.InnerExceptions.Count > 0))
            {
                throw parsingResult.Issues;
            }

            Program = parsingResult.Program;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaZicInterpreterCore"/> class.
        /// </summary>
        /// <param name="baZicInterpreterStateChangedBridge">The state changed bridge.</param>
        /// <param name="assemblySandbox">The assembly sandbox.</param>
        /// <param name="program">The <see cref="BaZicProgram"/> to interpret.</param>
        private BaZicInterpreterCore(BaZicInterpreterStateChangedBridge baZicInterpreterStateChangedBridge, AssemblySandbox assemblySandbox, BaZicProgram program)
            : this(baZicInterpreterStateChangedBridge, assemblySandbox)
        {
            Requires.NotNull(program, nameof(program));

            _stateChangedHistory = new List<BaZicInterpreterStateChangeEventArgs>();
            ChangeState(this, new BaZicInterpreterStateChangeEventArgs(BaZicInterpreterState.Ready));

            Program = program;
        }

        /// <summary>
        /// Finalizes the instance of the class.
        /// </summary>
        ~BaZicInterpreterCore()
        {
            OnDispose(false);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            OnDispose(true);
            IsDisposed = true;
        }

        /// <summary>
        /// Compiles the program in memory and save it on the hard drive or returns the build errors.
        /// </summary>
        /// <param name="callback">The cross-AppDomain task proxy.</param>
        /// <param name="outputType">Defines the type of assembly to generate</param>
        /// <param name="outputPath">The full path to the .exe or .dll file to create if the build succeed.</param>
        /// <returns>Returns the build errors, or null if it succeed.</returns>
        internal void Build(MarshaledResultSetter<AggregateException> callback, BaZicCompilerOutputType outputType, string outputPath)
        {
            Requires.NotNullOrWhiteSpace(outputPath, nameof(outputPath));

            CheckState(BaZicInterpreterState.Ready, BaZicInterpreterState.Stopped, BaZicInterpreterState.StoppedWithError);

            if (ProgramIsOptimized)
            {
                throw new InvalidOperationException(L.BaZic.Runtime.BaZicInterpreter.CannotRunOptimizedProgramInRelease);
            }

            ChangeState(this, new BaZicInterpreterStateChangeEventArgs(BaZicInterpreterState.Running));

            AggregateException buildErrors = null;
            var currentCulture = LocalizationHelper.GetCurrentCulture();

            _mainInterpreterTask = Task.Run(() =>
            {
                LocalizationHelper.SetCurrentCulture(currentCulture, false);

                var outputFile = new FileInfo(outputPath);
                var directory = outputFile.Directory;

                if (string.IsNullOrWhiteSpace(outputFile.Extension))
                {
                    if (outputType == BaZicCompilerOutputType.DynamicallyLinkedLibrary)
                    {
                        outputFile = new FileInfo(outputPath + ".dll");
                    }
                    else
                    {
                        outputFile = new FileInfo(outputPath + ".exe");
                    }
                }

                var outputFileName = Path.GetFileNameWithoutExtension(outputPath);
                var outputPdbFile = new FileInfo(Path.Combine(directory.FullName, outputFileName + ".pdb"));

                using (var compileResult = Build(outputType))
                {
                    if (compileResult.BuildErrors != null)
                    {
                        buildErrors = compileResult.BuildErrors;
                        ChangeState(this, new UnexpectedException(buildErrors));
                        return;
                    }

                    try
                    {
                        if (!directory.Exists)
                        {
                            directory.Create();
                        }

                        foreach (var assembly in _assemblySandbox.GetAssemblies().Where(a => a.CopyToLocal))
                        {
                            if (File.Exists(assembly.Location))
                            {
                                File.Copy(assembly.Location, Path.Combine(directory.FullName, Path.GetFileName(assembly.Location)));

                                var pdbFilePath = Path.Combine(Directory.GetParent(assembly.Location).FullName, Path.GetFileNameWithoutExtension(assembly.Location) + ".pdb");
                                var xmlFilePath = Path.Combine(Directory.GetParent(assembly.Location).FullName, Path.GetFileNameWithoutExtension(assembly.Location) + ".xml");

                                if (File.Exists(pdbFilePath))
                                {
                                    File.Copy(pdbFilePath, Path.Combine(directory.FullName, Path.GetFileName(pdbFilePath)));
                                }

                                if (File.Exists(xmlFilePath))
                                {
                                    File.Copy(xmlFilePath, Path.Combine(directory.FullName, Path.GetFileName(xmlFilePath)));
                                }
                            }

                        }

                        using (var assemblyFileStream = new FileStream(outputFile.FullName, FileMode.Create))
                        using (var pdbFileStream = new FileStream(outputPdbFile.FullName, FileMode.Create))
                        {
                            compileResult.Assembly.WriteTo(assemblyFileStream);
                            compileResult.Pdb.WriteTo(pdbFileStream);
                        }

                        ChangeState(this, new BaZicInterpreterStateChangeEventArgs(BaZicInterpreterState.Stopped));
                    }
                    catch (Exception exception)
                    {
                        buildErrors = new AggregateException(new List<Exception> { exception });
                        ChangeState(this, new UnexpectedException(exception));
                    }
                }
            });
            _mainInterpreterTask.ContinueWith((task) =>
            {
                callback.SetResult(buildErrors);
            });
        }

        /// <summary>
        /// Compiles the program in memory and returns either the generated assembly or the errors.
        /// </summary>
        /// <param name="outputType">Defines the type of assembly to generate</param>
        /// <returns>Returns a <seealso cref="CompilerResult"/>.</returns>
        private CompilerResult Build(BaZicCompilerOutputType outputType)
        {
            LoadAssemblies();

            if (_releaseModeRuntime == null)
            {
                _releaseModeRuntime = new CompiledProgramRunner(this, Program, _assemblySandbox);
            }

            return _releaseModeRuntime.Build(outputType);
        }

        /// <summary>
        /// Starts the interpreter in release mode. The program will be compiled (emitted) and run quickly. Breakpoint statements will be ignored.
        /// </summary>
        /// <param name="callback">The cross-AppDomain task proxy.</param>
        /// <param name="verbose">Defines if the verbose mode must be enabled or not.</param>
        /// <param name="args">The arguments to pass to the entry point.</param>
        /// <returns>Returns an awaitable task that can wait the end of the program execution</returns>
        internal void StartRelease(MarshaledResultSetter callback, bool verbose, params object[] args)
        {
            var action = new Action(() =>
            {
                if (_releaseModeRuntime == null || _compilerResult == null || _compilerResult.BuildErrors != null)
                {
                    if (State == BaZicInterpreterState.Preparing)
                    {
                        LoadAssemblies();
                        _compilerResult = Build(BaZicCompilerOutputType.DynamicallyLinkedLibrary);

                        if (_compilerResult.BuildErrors != null)
                        {
                            ChangeState(this, new UnexpectedException(_compilerResult.BuildErrors));
                        }
                        else
                        {
                            _assemblySandbox.LoadAssembly(_compilerResult.Assembly);
                            _compilerResult.Dispose();
                        }
                    }
                }

                if (State == BaZicInterpreterState.Preparing)
                {
                    _releaseModeRuntime.Run(_programArguments);

                    if (State == BaZicInterpreterState.Running || State == BaZicInterpreterState.Idle)
                    {
                        ProgramResult = _releaseModeRuntime.ProgramResult;
                        Stop(false).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                }
            });

            Start(callback, action, verbose, args);
        }

        /// <summary>
        /// Starts the interpreter in debug mode. The program will be interpreted and support the beakpoint and step by step debugging.
        /// </summary>
        /// <param name="callback">The cross-AppDomain task proxy.</param>
        /// <param name="verbose">Defines if the verbose mode must be enabled or not.</param>
        /// <param name="args">The arguments to pass to the entry point.</param>
        /// <returns>Returns an awaitable task that can wait the end of the program execution</returns>
        internal void StartDebug(MarshaledResultSetter callback, bool verbose, params object[] args)
        {
            DebugMode = true;

            var action = new Action(() =>
            {
                LoadAssemblies();

                if (State == BaZicInterpreterState.Preparing)
                {
                    ProgramInterpreter = new ProgramInterpreter(this, Program);
                    ProgramInterpreter.Start(_programArguments);

                    var waitThreads = true;
                    do
                    {
                        Task[] threads = null;
                        lock (_unwaitedMethodInvocation)
                        {
                            threads = _unwaitedMethodInvocation.ToArray();
                        }

                        Task.WhenAll(threads).ConfigureAwait(false).GetAwaiter().GetResult();

                        lock (_unwaitedMethodInvocation)
                        {
                            waitThreads = _unwaitedMethodInvocation.Any(t => !t.IsCanceled && !t.IsCompleted && !t.IsFaulted);
                        }
                    } while (waitThreads);

                    foreach (var task in _unwaitedMethodInvocation)
                    {
                        task.Dispose();
                    }
                    _unwaitedMethodInvocation.Clear();

                    if (!_externMethodInvoked)
                    {
                        foreach (var variable in ProgramInterpreter.Variables)
                        {
                            variable.Dispose();
                        }

                        if (State == BaZicInterpreterState.Running || State == BaZicInterpreterState.Idle)
                        {
                            ProgramResult = ProgramInterpreter.ProgramResult;
                            Stop(false).ConfigureAwait(false).GetAwaiter().GetResult();
                        }
                    }
                }
            });

            Start(callback, action, verbose, args);
        }

        /// <summary>
        /// Invoke a public method accessible from outside of the interpreter (EXTERN FUNCTION).
        /// </summary>
        /// <param name="callback">The cross-AppDomain task proxy.</param>
        /// <param name="verbose">Defines if the verbose mode must be enabled or not.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="awaitIfAsync">Await if the method is maked as asynchronous.</param>
        /// <param name="args">The arguments to pass to the method.</param>
        /// <returns>Returns the result of the invocation (a <see cref="Task"/> in the case of a not awaited asynchronous method, or the value returned by the method).</returns>
        internal async void InvokeMethod(MarshaledResultSetter<object> callback, bool verbose, string methodName, bool awaitIfAsync, Expression[] args)
        {
            // TODO : RELEASE MODE

            DebugMode = true;
            _externMethodInvoked = true;

            if (State == BaZicInterpreterState.Ready || State == BaZicInterpreterState.Stopped || State == BaZicInterpreterState.StoppedWithError)
            {
                // Creates a new ProgramInterpreter.

                var privateCallback = new MarshaledResultSetter();
                var action = new Action(() =>
                {
                    LoadAssemblies();

                    if (State == BaZicInterpreterState.Preparing)
                    {
                        ProgramInterpreter = new ProgramInterpreter(this, Program);
                        ProgramInterpreter.InitializeGlobalState();
                        ChangeState(this, new BaZicInterpreterStateChangeEventArgs(BaZicInterpreterState.Idle));
                    }
                });

                Start(privateCallback, action, verbose, args);
                await privateCallback.Task;
            }
            else if (State == BaZicInterpreterState.Preparing)
            {
                // Wait for running.
                WaitForState(BaZicInterpreterState.Running);
            }

            if (State == BaZicInterpreterState.Running)
            {
                // Wait for having being idle.
                WaitForState(BaZicInterpreterState.Idle);
            }

            CheckState(BaZicInterpreterState.Idle);

            var currentCulture = LocalizationHelper.GetCurrentCulture();

            var thread = Task.Run(() =>
            {
                LocalizationHelper.SetCurrentCulture(currentCulture, false);
                return ProgramInterpreter.InvokeMethod(methodName, awaitIfAsync, args);
            });
            AddUnwaitedMethodInvocation(thread);

            var t = thread.ContinueWith((task) =>
            {
                callback.SetResult(task.Result);
                _externMethodInvoked = false;
            });
        }

        /// <summary>
        /// Ask the program to stop.
        /// </summary>
        /// <param name="callback">The cross-AppDomain task proxy.</param>
        internal void Stop(MarshaledResultSetter callback)
        {
            if (Verbose)
            {
                ChangeState(this, new BaZicInterpreterStateChangeEventArgs(L.BaZic.Runtime.BaZicInterpreter.StopRequested));
            }

            Task.Run(async () =>
            {
                await Stop(true);
                callback.NotifyEndTask();
            });
        }

        /// <summary>
        /// Ask the program to pause.
        /// </summary>
        internal void Pause()
        {
            if (!DebugMode)
            {
                throw new InternalException("Unable to pause a program in release mode.");
            }

            CheckState(BaZicInterpreterState.Running, BaZicInterpreterState.Idle, BaZicInterpreterState.Preparing);
            _pauseModeWaiter = new AutoResetEvent(false);
            _pauseModeWaiter.Reset();
        }

        /// <summary>
        /// Resume a paused program.
        /// </summary>
        internal void Resume()
        {
            if (!DebugMode)
            {
                throw new InternalException("Unable to resume a program in release mode.");
            }

            CheckState(BaZicInterpreterState.Pause);
            ChangeState(this, new BaZicInterpreterStateChangeEventArgs(BaZicInterpreterState.Running));
            FreePauseModeWaiter();
        }

        /// <summary>
        /// Resume a paused program, run the current statement and pause again. Similar behavior to a Step Into command.
        /// </summary>
        internal void NextStep()
        {
            Resume();
            Pause();
        }

        /// <summary>
        /// If the interpreter is requested to be in pause, then pause the thread.
        /// </summary>
        /// <param name="blockInterpreter">The current block interpreter that check whether it must pause the thread.</param>
        internal void AttemptPauseIfRequired(BlockInterpreter blockInterpreter)
        {
            if (_pauseModeWaiter != null)
            {
                ChangeState(this, new BaZicInterpreterStateChangeEventArgs(BaZicInterpreterState.Pause));
                DebugInfos = blockInterpreter.GetDebugInfo();
                _pauseModeWaiter.WaitOne();
            }
        }

        /// <summary>
        /// Check whether the <see cref="State"/> of the interpreter corresponds to one of the specified states. Throws a <see cref="StateException"/> if the state is not one of the expected states.
        /// </summary>
        /// <param name="expectedStates">The list of expected states.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void CheckState(params BaZicInterpreterState[] expectedStates)
        {
            if (expectedStates.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(expectedStates), L.BaZic.Runtime.BaZicInterpreter.OneStateMinimum);
            }

            var stateValidated = false;
            for (var i = 0; i < expectedStates.Length && !stateValidated; i++)
            {
                if (State == expectedStates[i])
                {
                    stateValidated = true;
                }
            }

            if (!stateValidated)
            {
                throw new StateException(State);
            }
        }

        /// <summary>
        /// Change the state of the interpreter.
        /// </summary>
        /// <param name="source">The source from where we changed the state (an interpreter usually).</param>
        /// <param name="exception">The exception thrown.</param>
        /// <param name="syntaxTreeObject">(optional) The algorithm object where the problem comes.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ChangeState(object source, BaZicInterpreterException exception, NodeObject syntaxTreeObject = null)
        {
            ChangeState(source, new BaZicInterpreterStateChangeEventArgs(new Error(exception, syntaxTreeObject)));
        }

        /// <summary>
        /// Change the state of the interpreter.
        /// </summary>
        /// <param name="source">The source from where we changed the state (an interpreter usually).</param>
        /// <param name="e">The new state.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ChangeState(object source, BaZicInterpreterStateChangeEventArgs e)
        {
            lock (_stateChangedHistory)
            {
                if (e.State == BaZicInterpreterState.StoppedWithError || e.State == BaZicInterpreterState.Stopped)
                {
                    ProgramInterpreter?.CloseUserInterface();
                }

                switch (e.State)
                {
                    case BaZicInterpreterState.StoppedWithError:
                        if (State != BaZicInterpreterState.StoppedWithError)
                        {
                            Error = e.Error;
                            State = e.State;
                            _stateChangedHistory.Add(e);
                            StateBridgeProxy.RaiseStateChange(e);
                            LocalStateChanged?.Invoke(this, e);
                        }
                        break;

                    case BaZicInterpreterState.Pause:
                    case BaZicInterpreterState.Preparing:
                    case BaZicInterpreterState.Ready:
                    case BaZicInterpreterState.Running:
                    case BaZicInterpreterState.Idle:
                    case BaZicInterpreterState.Stopped:
                        State = e.State;
                        break;

                    case BaZicInterpreterState.Log:
                        Logger.Instance.Debug($"BaZic Interpreter : {e.LogMessage}");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(e.State));
                }

                if (State != BaZicInterpreterState.StoppedWithError)
                {
                    _stateChangedHistory.Add(e);
                    StateBridgeProxy.RaiseStateChange(e);
                    LocalStateChanged?.Invoke(this, e);
                }
            }
        }

        /// <summary>
        /// Generates a string representation of the <see cref="StateChangedHistory"/>.
        /// </summary>
        /// <returns>Returns a string representation of <see cref="StateChangedHistory"/>.</returns>
        internal string GetStateChangedHistoryString()
        {
            lock (_stateChangedHistory)
            {
                var builder = new StringBuilder();
                foreach (var stateChange in _stateChangedHistory)
                {
                    builder.AppendLine(stateChange?.ToString());
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Add an unwaited task to the list of task to wait to allow the program to consider itself as done.
        /// </summary>
        /// <param name="task">The task to add.</param>
        internal void AddUnwaitedMethodInvocation(Task task)
        {
            Requires.NotNull(task, nameof(task));
            lock (_unwaitedMethodInvocation)
            {
                _unwaitedMethodInvocation.Add(task);
            }
        }

        /// <summary>
        /// Sets the program required dependencies.
        /// </summary>
        /// <param name="assemblies">The assemblies (can be a full name or a location).</param>
        internal void SetDependencies(string[] assemblies)
        {
            Program.WithAssemblies(assemblies);
        }

        /// <summary>
        /// Sets the program required dependencies.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        internal void SetDependencies(AssemblyDetails[] assemblies)
        {
            Program.WithAssemblies(assemblies);
        }

        private void Start(MarshaledResultSetter callback, Action action, bool verbose, params object[] args)
        {
            CheckState(BaZicInterpreterState.Ready, BaZicInterpreterState.Stopped, BaZicInterpreterState.StoppedWithError);

            ChangeState(this, new BaZicInterpreterStateChangeEventArgs(BaZicInterpreterState.Preparing));

            _programArguments = args;
            Verbose = verbose;
            DebugInfos = null;

            var currentCulture = LocalizationHelper.GetCurrentCulture();

            _mainInterpreterTask = Task.Run(() =>
            {
                _mainInterpreterThread = Thread.CurrentThread;
                LocalizationHelper.SetCurrentCulture(currentCulture, false);
                RuntimeHelpers.EnsureSufficientExecutionStack();
                GCSettings.LatencyMode = GCLatencyMode.LowLatency;

                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    if (!_ignoreException)
                    {
                        ChangeState(this, new UnexpectedException(exception));
                    }

                    _ignoreException = false;
                }
                finally
                {
                    if (!_externMethodInvoked)
                    {
                        ProgramInterpreter = null;
                    }
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            });
            _mainInterpreterTask.ContinueWith((task) =>
            {
                callback.NotifyEndTask();
            });
        }

        /// <summary>
        /// Ask the program to stop.
        /// </summary>
        /// <param name="waitForMainInterpreterThread">Defines whether the method should wait the end of the interpretation to finish.</param>
        private async Task Stop(bool waitForMainInterpreterThread)
        {
            CheckState(BaZicInterpreterState.Pause, BaZicInterpreterState.Running, BaZicInterpreterState.Idle, BaZicInterpreterState.Preparing);
            ChangeState(this, new BaZicInterpreterStateChangeEventArgs(BaZicInterpreterState.Stopped));

            if (DebugMode)
            {
                FreePauseModeWaiter();

                if (waitForMainInterpreterThread)
                {
                    await _mainInterpreterTask;
                }
            }
            else
            {
                _ignoreException = true;
                _mainInterpreterThread.Abort();
                await Task.Delay(500); // Let the time to the _mainInterpreterThread to stop.
            }

            DebugInfos = null;
        }

        /// <summary>
        /// Throw a flag to tell the interpreter to continue its process after being in paused.
        /// </summary>
        private void FreePauseModeWaiter()
        {
            if (_pauseModeWaiter != null)
            {
                DebugInfos = null;
                _pauseModeWaiter.Set();
                _pauseModeWaiter.Dispose();
                _pauseModeWaiter = null;
            }
        }

        /// <summary>
        /// Load in the application domain all the required assemblies.
        /// </summary>
        private void LoadAssemblies()
        {
            var assemblies = Program.Assemblies.ToList();

            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            var assembliesPath = new List<String>();
            assembliesPath.Add("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            assembliesPath.Add("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            assembliesPath.Add("System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            assembliesPath.Add("System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            assembliesPath.Add("Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");

            if (Program is BaZicUiProgram)
            {
                assembliesPath.Add("PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                assembliesPath.Add("PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                assembliesPath.Add("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            }

            foreach (var path in assembliesPath)
            {
                if (assemblies.All(a => string.CompareOrdinal(a.ToLocationOrFullName(), path) != 0))
                {
                    assemblies.Add(AssemblyDetails.GetAssemblyDetailsFromName(path));
                }
            }

            AssemblyDetails details = null;
            try
            {
                for (int i = 0; i < assemblies.Count; i++)
                {
                    details = assemblies[i];
                    _assemblySandbox.LoadAssembly(details, false);
                    if (Verbose)
                    {
                        ChangeState(this, new BaZicInterpreterStateChangeEventArgs(L.BaZic.Runtime.BaZicInterpreter.FormattedAssemblyLoaded(details.ToLocationOrFullName())));
                    }
                }
            }
            catch (Exception exception)
            {
                ChangeState(this, new LoadAssemblyException(L.BaZic.Runtime.BaZicInterpreter.FormattedAssemblyFailedLoad(details.ToLocationOrFullName()), details.ToLocationOrFullName(), exception));
            }
        }

        /// <summary>
        /// Blocks the current execution flow until the specified <see cref="BaZicInterpreterState"/> is encounted.
        /// </summary>
        /// <param name="expectedState">The expected state</param>
        private void WaitForState(BaZicInterpreterState expectedState)
        {
            using (var resetEvent = new AutoResetEvent(false))
            {
                resetEvent.Reset();

                BaZicInterpreterStateEventHandler stateChanged = (s, e) =>
                {
                    if (e.State == expectedState)
                    {
                        resetEvent.Set();
                    }
                };

                LocalStateChanged += stateChanged;

                resetEvent.WaitOne();

                LocalStateChanged -= stateChanged;
            }
        }

        /// <summary>
        /// Should be called when the object is being disposed.
        /// </summary>
        /// <param name="disposing">Was Dispose() called or did we get here from the finalizer?</param>
        private void OnDispose(bool disposing)
        {
            if (disposing)
            {
                if (!IsDisposed)
                {
                    FreePauseModeWaiter();
                    _mainInterpreterTask?.Dispose();
                    _stateChangedHistory.Clear();
                    DebugInfos = null;
                }
            }

            IsDisposed = true;
        }

        #endregion
    }
}
