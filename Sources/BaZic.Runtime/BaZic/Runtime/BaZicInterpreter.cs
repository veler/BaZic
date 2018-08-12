﻿using BaZic.Core.ComponentModel.Assemblies;
using BaZic.Core.Enums;
using BaZic.Core.IO.Serialization;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Debugger;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BaZic.Runtime.BaZic.Runtime
{
    /// <summary>
    /// Provides an interpreter for a BaZic program.
    /// </summary>
    [Serializable]
    public sealed class BaZicInterpreter : Core.ComponentModel.IDisposable
    {
        #region Fields & Constants

        private readonly AssemblySandbox _assemblySandbox;
        private readonly BaZicInterpreterCore _core;
        private readonly BaZicInterpreterStateChangedBridge _bridge;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets the current state of the interpreter.
        /// </summary>
        public BaZicInterpreterState State => _core.State;

        /// <summary>
        /// Gets the current interpreter error.
        /// </summary>
        public Error Error => GetError();

        /// <summary>
        /// Gets the result of the execution of the program.
        /// </summary>
        public object ProgramResult => _core.ProgramResult;

        /// <summary>
        /// Gets the historic of state changes.
        /// </summary>
        public IReadOnlyList<BaZicInterpreterStateChangeEventArgs> StateChangedHistory => _core.StateChangedHistory;

        /// <summary>
        /// Gets the list of debug information for every running thread in the program.
        /// </summary>
        public DebugInfo DebugInfos => _core.DebugInfos;

        #endregion

        #region Events

        /// <summary>
        /// Raised when the interpretrer state has changed
        /// </summary>
        public event BaZicInterpreterStateEventHandler StateChanged;

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BaZicInterpreter"/> class.
        /// </summary>
        private BaZicInterpreter()
        {
            _bridge = new BaZicInterpreterStateChangedBridge();
            _assemblySandbox = new AssemblySandbox();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaZicInterpreter"/> class.
        /// </summary>
        /// <param name="inputCode">The BaZic code to interpret.</param>
        /// <param name="xamlCode">The XAML code to interpret that represents the user interface.</param>
        /// <param name="optimize">(optional) Defines whether the generated syntax tree must be optimized for the interpreter or not.</param>
        public BaZicInterpreter(string inputCode, string xamlCode, bool optimize = false)
            : this()
        {
            _core = _assemblySandbox.CreateInstanceMarshalByRefObject<BaZicInterpreterCore>(_bridge, _assemblySandbox, inputCode, xamlCode, optimize);
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaZicInterpreter"/> class.
        /// </summary>
        /// <param name="program">The <see cref="BaZicProgram"/> to interpret.</param>
        public BaZicInterpreter(BaZicProgram program)
            : this()
        {
            _core = _assemblySandbox.CreateInstanceMarshalByRefObject<BaZicInterpreterCore>(_bridge, _assemblySandbox, program);
            Initialize();
        }

        /// <summary>
        /// Finalizes the instance of the class.
        /// </summary>
        ~BaZicInterpreter()
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
        /// Compiles the program and save it on the hard drive or returns the build errors.
        /// </summary>
        /// <param name="outputType">Defines the type of assembly to generate.</param>
        /// <param name="outputPath">The full path to the .exe or .dll file to create if the build succeed.</param>
        /// <returns>Returns the build errors, or null if it succeed.</returns>
        public async Task<AggregateException> Build(BaZicCompilerOutputType outputType, string outputPath)
        {
            var callback = new MarshaledResultSetter<AggregateException>();
            _core.Build(callback, outputType, outputPath);
            return await callback.Task;
        }

        /// <summary>
        /// Starts the interpreter in release mode. The program will be compiled (emitted) and run quickly. Breakpoint statements will be ignored.
        /// </summary>
        /// <param name="verbose">Defines if the verbose mode must be enabled or not.</param>
        /// <param name="args">The arguments to pass to the entry point.</param>
        /// <returns>Returns an awaitable task that can wait the end of the program execution</returns>
        public async Task StartReleaseAsync(bool verbose, params object[] args)
        {
            var callback = new MarshaledResultSetter();
            _core.StartRelease(callback, verbose, args);
            await callback.Task;
        }

        /// <summary>
        /// Starts the interpreter in debug mode. The program will be interpreted and support the beakpoint and step by step debugging.
        /// </summary>
        /// <param name="verbose">Defines if the verbose mode must be enabled or not.</param>
        /// <param name="args">The arguments to pass to the entry point.</param>
        /// <returns>Returns an awaitable task that can wait the end of the program execution</returns>
        public async Task StartDebugAsync(bool verbose, params object[] args)
        {
            var callback = new MarshaledResultSetter();
            _core.StartDebug(callback, verbose, args);
            await callback.Task;
        }

        /// <summary>
        /// Invoke a public method accessible from outside of the interpreter (EXTERN FUNCTION).
        /// </summary>
        /// <param name="verbose">Defines if the verbose mode must be enabled or not.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="awaitIfAsync">Await if the method is maked as asynchronous.</param>
        /// <param name="args">The arguments to pass to the method.</param>
        /// <returns>Returns the result of the invocation (a <see cref="Task"/> in the case of a not awaited asynchronous method, or the value returned by the method).</returns>
        public async Task<object> InvokeMethod(bool verbose, string methodName, bool awaitIfAsync, params Expression[] args)
        {
            var callback = new MarshaledResultSetter<object>();
            _core.InvokeMethod(callback, verbose, methodName, awaitIfAsync, args);
            return await callback.Task;
        }

        /// <summary>
        /// Ask the program to stop.
        /// </summary>
        public async Task Stop()
        {
            var callback = new MarshaledResultSetter();
            _core.Stop(callback);
            await callback.Task;
        }

        /// <summary>
        /// Ask the program to pause.
        /// </summary>
        public void Pause()
        {
            _core.Pause();
        }

        /// <summary>
        /// Resume a paused program.
        /// </summary>
        public void Resume()
        {
            _core.Resume();
        }

        /// <summary>
        /// Resume a paused program, run the current statement and pause again. Similar behavior to a Step Into command.
        /// </summary>
        public void NextStep()
        {
            _core.NextStep();
        }

        /// <summary>
        /// Sets the program required dependencies.
        /// </summary>
        /// <param name="assemblies">The assemblies (can be a full name or a location).</param>
        public void SetDependencies(params string[] assemblies)
        {
            _core.SetDependencies(assemblies);
        }

        /// <summary>
        /// Sets the program required dependencies.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        public void SetDependencies(params AssemblyDetails[] assemblies)
        {
            _core.SetDependencies(assemblies);
        }

        /// <summary>
        /// Generates a string representation of the <see cref="StateChangedHistory"/>.
        /// </summary>
        /// <returns>Returns a string representation of <see cref="StateChangedHistory"/>.</returns>
        public string GetStateChangedHistoryString()
        {
            return _core.GetStateChangedHistoryString();
        }

        /// <summary>
        /// Initializes the state of the interpreter.
        /// </summary>
        private void Initialize()
        {
            _bridge.StateChanged += Bridge_StateChanged;
        }

        /// <summary>
        /// Retrieves via serialization the error that comes from the interpreter core.
        /// </summary>
        /// <returns>Returns the error.</returns>
        private Error GetError()
        {
            return SerializationHelper.ConvertFromBinary<Error>(_assemblySandbox.Reflection.GetPropertySerialized(_core, nameof(_core.Error)));
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
                    _core.Dispose();
                    _assemblySandbox.Dispose();
                }
            }

            IsDisposed = true;
        }

        #endregion

        #region Handled Methods

        private void Bridge_StateChanged(object sender, BaZicInterpreterStateChangeEventArgs e)
        {
            StateChanged?.Invoke(this, e);
        }

        #endregion
    }
}
