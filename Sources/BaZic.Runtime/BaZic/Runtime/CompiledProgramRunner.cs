using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using BaZic.Core.ComponentModel;
using BaZic.Core.ComponentModel.Assemblies;
using BaZic.Runtime.BaZic.Code;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.Localization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace BaZic.Runtime.BaZic.Runtime
{
    /// <summary>
    /// Provide a sets of method to compile and run a BaZic program.
    /// </summary>
    internal sealed class CompiledProgramRunner : MarshalByRefObject
    {
        #region Fields

        private readonly BaZicInterpreterCore _baZicInterpreter;
        private readonly BaZicProgram _program;
        private readonly AssemblySandbox _assemblySandbox;

        private Thread _currentThread;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the result of the Main method or the result of the Window.Closed event from the user interface.
        /// </summary>
        internal object ProgramResult { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompiledProgramRunner"/> class.
        /// </summary>
        /// <param name="baZicInterpreter">The <see cref="BaZicInterpreterCore"/> that called this class.</param>
        /// <param name="program">The <see cref="BaZicProgram"/> to compile and run.</param>
        /// <param name="assemblySandbox">The sandbox that contains the required assemblies.</param>
        internal CompiledProgramRunner(BaZicInterpreterCore baZicInterpreter, BaZicProgram program, AssemblySandbox assemblySandbox)
        {
            Requires.NotNull(baZicInterpreter, nameof(baZicInterpreter));
            Requires.NotNull(program, nameof(program));
            Requires.NotNull(assemblySandbox, nameof(assemblySandbox));
            _baZicInterpreter = baZicInterpreter;
            _program = program;
            _assemblySandbox = assemblySandbox;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Convert the BaZic program to CSharp code and build it in memory.
        /// </summary>
        internal void Compile()
        {
            if (_baZicInterpreter.Verbose)
            {
                _baZicInterpreter.ChangeState(this, new BaZicInterpreterStateChangeEventArgs(L.BaZic.Runtime.CompiledProgramRunner.GenerationCSharp));
            }

            var codeGen = new CSharpCodeGenerator();
            var syntaxTree = CSharpSyntaxTree.ParseText(codeGen.Generate(_program));

            if (_baZicInterpreter.Verbose)
            {
                _baZicInterpreter.ChangeState(this, new BaZicInterpreterStateChangeEventArgs(L.BaZic.Runtime.CompiledProgramRunner.Compiling));
            }

#if DEBUG
            var optimizationlevel = OptimizationLevel.Debug;
#else
            var optimizationlevel = OptimizationLevel.Release;
#endif
            var assemblyName = Guid.NewGuid().ToString();
            var references = GetAssemblyReferences();
            var cSharpCompilation = CSharpCompilation.Create(assemblyName, new[] { syntaxTree }, references, new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary, optimizationLevel: optimizationlevel, platform: Platform.AnyCpu));

            using (var memoryStream = new MemoryStream())
            {
                var result = cSharpCompilation.Emit(memoryStream);
                ThrowExceptionIfCompilationFailure(result);
                memoryStream.Seek(0, SeekOrigin.Begin);

                if (_baZicInterpreter.Verbose)
                {
                    _baZicInterpreter.ChangeState(this, new BaZicInterpreterStateChangeEventArgs(L.BaZic.Runtime.CompiledProgramRunner.BuiltSucceed));
                }
                _assemblySandbox.LoadAssembly(memoryStream);
            }
        }

        /// <summary>
        /// Runs the compiled program.
        /// </summary>
        /// <param name="arguments">The arguments to pass to the program.</param>
        internal void Run(params object[] arguments)
        {
            _currentThread = Thread.CurrentThread;
            _baZicInterpreter.CheckState(BaZicInterpreterState.Preparing);

            _baZicInterpreter.ChangeState(this, new BaZicInterpreterStateChangeEventArgs(BaZicInterpreterState.Running));

            if (_baZicInterpreter.Verbose)
            {
                _baZicInterpreter.ChangeState(this, new BaZicInterpreterStateChangeEventArgs(L.BaZic.Runtime.CompiledProgramRunner.RunningProgram));
            }

            var argumentValues = new object[] { arguments };
            ProgramResult = _assemblySandbox.CreateInstanceAndInvoke("BaZicProgramReleaseMode.Program", "Main", argumentValues);

            if (_baZicInterpreter.Verbose)
            {
                _baZicInterpreter.ChangeState(this, new BaZicInterpreterStateChangeEventArgs(L.BaZic.Runtime.CompiledProgramRunner.ExecutionEnded));
            }
        }

        internal void Stop()
        {
            _currentThread.Abort();
        }

        /// <summary>
        /// Gets a reference to the required assemblies.
        /// </summary>
        /// <returns>A list of reference to the assemblies.</returns>
        private IReadOnlyList<MetadataReference> GetAssemblyReferences()
        {
            var references = new List<MetadataReference>();

            references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

            foreach (var assembly in _assemblySandbox.GetAssemblies())
            {
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }

            return references.AsReadOnly();
        }

        /// <summary>
        /// Throw exceptions if the compilation failed.
        /// </summary>
        /// <param name="result">The result of the compilation.</param>
        private void ThrowExceptionIfCompilationFailure(EmitResult result)
        {
            if (result.Success)
            {
                return;
            }

            var compilationErrors = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error).ToList();

            if (!compilationErrors.Any())
            {
                return;
            }

            var errorList = new List<Exception>();
            foreach (var error in compilationErrors)
            {
                var errorNumber = error.Id;
                var errorDescription = error.GetMessage();
                var ErrorMessage = $"{errorNumber}: {errorDescription};";
                errorList.Add(new Exception(L.BaZic.Runtime.CompiledProgramRunner.FormattedBuildFaild(errorNumber, errorDescription)));
            }
            throw new AggregateException(errorList);
        }

        #endregion
    }
}
