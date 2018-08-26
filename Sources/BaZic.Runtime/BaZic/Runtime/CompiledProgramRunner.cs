using BaZic.Core.ComponentModel;
using BaZic.Core.ComponentModel.Assemblies;
using BaZic.Core.Enums;
using BaZic.Runtime.BaZic.Code;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.Localization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        private object _compiledProgramInstance;

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
        /// 
        internal CompilerResult Build(BaZicCompilerOutputType outputType)
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

            var outputKind = OutputKind.ConsoleApplication;
            if (outputType == BaZicCompilerOutputType.WindowsApp)
            {
                outputKind = OutputKind.WindowsApplication;
            }
            else if (outputType == BaZicCompilerOutputType.DynamicallyLinkedLibrary)
            {
                outputKind = OutputKind.DynamicallyLinkedLibrary;
            }

            var assemblyName = Guid.NewGuid().ToString();
            var references = GetAssemblyReferences();
            var options = new CSharpCompilationOptions(outputKind)
                .WithAllowUnsafe(true)
                .WithOptimizationLevel(OptimizationLevel.Debug)
                .WithPlatform(Platform.AnyCpu);

            var cSharpCompilation = CSharpCompilation.Create(assemblyName, new[] { syntaxTree }, references, options);

            var assemblyStream = new MemoryStream();
            var pdbStream = new MemoryStream();

            var result = cSharpCompilation.Emit(peStream: assemblyStream, pdbStream: pdbStream);
            var errors = GetCompilationErrors(result);

            if (errors != null)
            {
                return new CompilerResult
                {
                    BuildErrors = errors
                };
            }

            assemblyStream.Seek(0, SeekOrigin.Begin);
            pdbStream.Seek(0, SeekOrigin.Begin);

            if (_baZicInterpreter.Verbose)
            {
                _baZicInterpreter.ChangeState(this, new BaZicInterpreterStateChangeEventArgs(L.BaZic.Runtime.CompiledProgramRunner.BuiltSucceed));
            }

            return new CompilerResult
            {
                Assembly = assemblyStream,
                Pdb = pdbStream
            };
        }

        /// <summary>
        /// Runs the compiled program.
        /// </summary>
        /// <param name="arguments">The arguments to pass to the program.</param>
        internal void Run(params object[] arguments)
        {
            _baZicInterpreter.CheckState(BaZicInterpreterState.Preparing);

            _baZicInterpreter.ChangeState(this, new BaZicInterpreterStateChangeEventArgs(BaZicInterpreterState.Running));

            if (_baZicInterpreter.Verbose)
            {
                _baZicInterpreter.ChangeState(this, new BaZicInterpreterStateChangeEventArgs(L.BaZic.Runtime.CompiledProgramRunner.RunningProgram));
            }

            var argumentValues = new object[] { arguments };

            if (_program is BaZicUiProgram)
            {
                ThreadHelper.RunOnStaThread(() =>
                {
                    InitializeProgram();

                    var programHelper = _assemblySandbox.Reflection.GetProperty(_compiledProgramInstance, Consts.CompiledProgramHelperInstance);

                    _assemblySandbox.Reflection.SubscribeEvent(programHelper, Consts.CompiledProgramIdleStateOccuredEvent, () =>
                    {
                        _baZicInterpreter.ChangeState(this, new BaZicInterpreterStateChangeEventArgs(BaZicInterpreterState.Idle)); // Go to Idle mode.
                    });

                    ProgramResult = InvokeMethod(Consts.EntryPointMethodName, argumentValues);
                });
            }
            else
            {
                InitializeProgram();
                ProgramResult = InvokeMethod(Consts.EntryPointMethodName, argumentValues);
            }

            if (_baZicInterpreter.Verbose)
            {
                _baZicInterpreter.ChangeState(this, new BaZicInterpreterStateChangeEventArgs(L.BaZic.Runtime.CompiledProgramRunner.ExecutionEnded));
            }
        }

        /// <summary>
        /// Invokes a public method in the program.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="arguments">The arguments to pass to the program.</param>
        /// <returns>Returns the result of the method.</returns>
        internal object InvokeMethod(string methodName, params object[] arguments)
        {
            InitializeProgram();
            return _assemblySandbox.Reflection.InvokeMethod(_compiledProgramInstance, methodName, arguments);
        }

        /// <summary>
        /// Request to close the user interface of the running program.
        /// </summary>
        internal void CloseUserInterface()
        {
            InitializeProgram();
            var programHelper = _assemblySandbox.Reflection.GetProperty(_compiledProgramInstance, Consts.CompiledProgramHelperInstance);
            _assemblySandbox.Reflection.InvokeMethod(programHelper, Consts.CompiledCloseUserInterface);
        }

        /// <summary>
        /// Creates an instance of the compiled program class, if it is not already instanciated.
        /// </summary>
        private void InitializeProgram()
        {
            if (_compiledProgramInstance == null)
            {
                _compiledProgramInstance = _assemblySandbox.Reflection.Instantiate(_assemblySandbox.GetTypeRef(Consts.CompiledProgramClassName));
            }
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
        /// <returns>Returns an <see cref="AggregateException"/> or null if the build succeeded.</returns>
        private AggregateException GetCompilationErrors(EmitResult result)
        {
            if (result.Success)
            {
                return null;
            }

            var compilationErrors = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error).ToList();

            if (!compilationErrors.Any())
            {
                return null;
            }

            var errorList = new List<Exception>();
            foreach (var error in compilationErrors)
            {
                var errorNumber = error.Id;
                var errorDescription = error.GetMessage();
                var ErrorMessage = $"{errorNumber}: {errorDescription};";
                errorList.Add(new Exception(L.BaZic.Runtime.CompiledProgramRunner.FormattedBuildFaild(errorNumber, errorDescription)));
            }

            return new AggregateException(errorList);
        }

        #endregion
    }
}
