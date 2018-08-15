using BaZic.Core.ComponentModel.Assemblies;
using BaZic.Core.Tests.Mocks;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Code.Parser;
using BaZic.Runtime.BaZic.Runtime;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BaZic.Runtime.Tests.BaZic.Runtime
{
    [TestClass]
    public class BaZicInterpreterTest
    {
        [TestInitialize]
        public void Initialize()
        {
            TestUtilities.InitializeLogs();
        }

        [TestMethod]
        public async Task BaZicInterpreterLifeCycle()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
    VARIABLE var1 = 1
    VARIABLE Var1 = 2
    RETURN var1 + Var1
END FUNCTION";
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, true).Program);
            await interpreter.StartDebugAsync(true);

            Assert.AreEqual(BaZicInterpreterState.Ready, interpreter.StateChangedHistory[0].State);
            Assert.AreEqual(BaZicInterpreterState.Preparing, interpreter.StateChangedHistory[1].State);
            Assert.AreEqual(BaZicInterpreterState.Running, interpreter.StateChangedHistory[12].State);
            Assert.AreEqual(BaZicInterpreterState.Idle, interpreter.StateChangedHistory.Last().State);

            await TestUtilities.TestAllRunningMode("3", inputCode);
        }

        [TestMethod]
        public async Task BaZicInterpreterAssembliesLoad()
        {
            var program = new BaZicProgram();
            program.WithAssemblies("FakeAssembly, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

            var interpreter = new BaZicInterpreter(program);
            await interpreter.StartDebugAsync(true);

            var exception = (LoadAssemblyException)interpreter.Error.Exception;
            Assert.AreEqual("FakeAssembly, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", exception.AssemblyPath);
            Assert.AreEqual("Could not load file or assembly 'FakeAssembly, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' or one of its dependencies. The system cannot find the file specified.", exception.InnerException.Message);
        }

        [TestMethod]
        public async Task BaZicInterpreterStepInto()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
    VARIABLE var1 = 0

    BREAKPOINT
    var1 = 1
    var1 = 2
    var1 = 3

    RETURN var1
END FUNCTION";
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program);
            var t = interpreter.StartDebugAsync(true);

            await Task.Delay(3000);

            var expectedLogs = @"[State] Ready
[State] Preparing
[Log] Reference assembly 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' loaded in the application domain.
[Log] Reference assembly 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' loaded in the application domain.
[Log] Reference assembly 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' loaded in the application domain.
[Log] Reference assembly 'System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' loaded in the application domain.
[Log] Reference assembly 'Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' loaded in the application domain.
[Log] Reference assembly 'PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' loaded in the application domain.
[Log] Reference assembly 'PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' loaded in the application domain.
[Log] Reference assembly 'WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' loaded in the application domain.
[Log] Declaring global variables.
[Log] Program's entry point detected.
[State] Running
[Log] Preparing to invoke the method 'Main'.
[Log] Executing the argument values of the method.
[Log] Executing an expression of type 'ArrayCreationExpression'.
[Log] The expression returned the value 'BaZicProgramReleaseMode.ObservableDictionary' (BaZicProgramReleaseMode.ObservableDictionary (length: 0)).
[Log] Invoking the synchronous method 'Main'.
[Log] Variable 'args' declared. Default value : {Null}
[Log] Variable 'args' value set to : BaZicProgramReleaseMode.ObservableDictionary (BaZicProgramReleaseMode.ObservableDictionary (length: 0))
[Log] Registering labels.
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '0' (System.Int32).
[Log] Variable 'var1' declared. Default value : 0 (System.Int32)
[Log] Executing a statement of type 'BreakpointStatement'.
[Log] A Breakpoint has been intercepted.
[State] Pause
";

            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());
            Assert.AreEqual(BaZicInterpreterState.Pause, interpreter.State);

            interpreter.NextStep();
            await Task.Delay(1000);

            Assert.AreEqual(BaZicInterpreterState.Pause, interpreter.State);

            interpreter.NextStep();
            await Task.Delay(1000);

            expectedLogs = @"[State] Ready
[State] Preparing
[Log] Reference assembly 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' loaded in the application domain.
[Log] Reference assembly 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' loaded in the application domain.
[Log] Reference assembly 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' loaded in the application domain.
[Log] Reference assembly 'System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' loaded in the application domain.
[Log] Reference assembly 'Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' loaded in the application domain.
[Log] Reference assembly 'PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' loaded in the application domain.
[Log] Reference assembly 'PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' loaded in the application domain.
[Log] Reference assembly 'WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' loaded in the application domain.
[Log] Declaring global variables.
[Log] Program's entry point detected.
[State] Running
[Log] Preparing to invoke the method 'Main'.
[Log] Executing the argument values of the method.
[Log] Executing an expression of type 'ArrayCreationExpression'.
[Log] The expression returned the value 'BaZicProgramReleaseMode.ObservableDictionary' (BaZicProgramReleaseMode.ObservableDictionary (length: 0)).
[Log] Invoking the synchronous method 'Main'.
[Log] Variable 'args' declared. Default value : {Null}
[Log] Variable 'args' value set to : BaZicProgramReleaseMode.ObservableDictionary (BaZicProgramReleaseMode.ObservableDictionary (length: 0))
[Log] Registering labels.
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '0' (System.Int32).
[Log] Variable 'var1' declared. Default value : 0 (System.Int32)
[Log] Executing a statement of type 'BreakpointStatement'.
[Log] A Breakpoint has been intercepted.
[State] Pause
[State] Running
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to ''1' (type:System.Int32)'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Variable 'var1' value set to : 1 (System.Int32)
[Log] 'var1' is now equal to '1'(type:System.Int32)
[State] Pause
[State] Running
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to ''2' (type:System.Int32)'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '2' (System.Int32).
[Log] Variable 'var1' value set to : 2 (System.Int32)
[Log] 'var1' is now equal to '2'(type:System.Int32)
[State] Pause
";

            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());
            Assert.AreEqual(BaZicInterpreterState.Pause, interpreter.State);

            await interpreter.Stop();
            await Task.Delay(1000);

            expectedLogs = @"[State] Ready
[State] Preparing
[Log] Reference assembly 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' loaded in the application domain.
[Log] Reference assembly 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' loaded in the application domain.
[Log] Reference assembly 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' loaded in the application domain.
[Log] Reference assembly 'System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' loaded in the application domain.
[Log] Reference assembly 'Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' loaded in the application domain.
[Log] Reference assembly 'PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' loaded in the application domain.
[Log] Reference assembly 'PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' loaded in the application domain.
[Log] Reference assembly 'WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' loaded in the application domain.
[Log] Declaring global variables.
[Log] Program's entry point detected.
[State] Running
[Log] Preparing to invoke the method 'Main'.
[Log] Executing the argument values of the method.
[Log] Executing an expression of type 'ArrayCreationExpression'.
[Log] The expression returned the value 'BaZicProgramReleaseMode.ObservableDictionary' (BaZicProgramReleaseMode.ObservableDictionary (length: 0)).
[Log] Invoking the synchronous method 'Main'.
[Log] Variable 'args' declared. Default value : {Null}
[Log] Variable 'args' value set to : BaZicProgramReleaseMode.ObservableDictionary (BaZicProgramReleaseMode.ObservableDictionary (length: 0))
[Log] Registering labels.
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '0' (System.Int32).
[Log] Variable 'var1' declared. Default value : 0 (System.Int32)
[Log] Executing a statement of type 'BreakpointStatement'.
[Log] A Breakpoint has been intercepted.
[State] Pause
[State] Running
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to ''1' (type:System.Int32)'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Variable 'var1' value set to : 1 (System.Int32)
[Log] 'var1' is now equal to '1'(type:System.Int32)
[State] Pause
[State] Running
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to ''2' (type:System.Int32)'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '2' (System.Int32).
[Log] Variable 'var1' value set to : 2 (System.Int32)
[Log] 'var1' is now equal to '2'(type:System.Int32)
[State] Pause
[Log] The user requests to stop the interpreter as soon as possible.
[State] Stopped
[Log] End of the execution of the method 'Main'. Returned value :  ({Null})
";

            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());
            Assert.AreEqual(BaZicInterpreterState.Stopped, interpreter.State);
        }

        [TestMethod]
        public async Task BaZicInterpreterDebugInformation()
        {
            var parser = new BaZicParser();

            var inputCode =
@"
VARIABLE globVar = ""Hello""

EXTERN FUNCTION Main(args[])
    RETURN SimpleRecursivity(5)
END FUNCTION

FUNCTION SimpleRecursivity(num)
    IF num < 1 THEN
        BREAKPOINT
        RETURN num
    ELSE
        RETURN SimpleRecursivity(num - 1)
    END IF
END FUNCTION";
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, true).Program);
            var t = interpreter.StartDebugAsync(true);

            await Task.Delay(2000);

            var debugInfo = interpreter.DebugInfos;

            Assert.IsNull(debugInfo);
            Assert.AreEqual(BaZicInterpreterState.Pause, interpreter.State);


            interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program);
            t = interpreter.StartDebugAsync(true);

            Assert.IsNull(interpreter.DebugInfos);

            await Task.Delay(2000);

            debugInfo = interpreter.DebugInfos;

            Assert.IsNotNull(debugInfo);
            Assert.AreEqual(BaZicInterpreterState.Pause, interpreter.State);
            Assert.AreEqual(7, debugInfo.CallStack.Count);
            Assert.AreEqual(1, debugInfo.GlobalVariables.Count);
            Assert.AreEqual("SimpleRecursivity", debugInfo.CallStack.First().InvokeMethodExpression.MethodName.Identifier);
            Assert.AreEqual("num", debugInfo.CallStack.First().Variables.Single().Name);
            Assert.AreEqual(0, debugInfo.CallStack.First().Variables.Single().Value);
            Assert.AreEqual(5, debugInfo.CallStack[5].Variables.Single().Value);
        }

        [TestMethod]
        public async Task BaZicInterpreterDebugInformationAsync()
        {
            var parser = new BaZicParser();

            var inputCode =
@"
VARIABLE globVar = ""Hello""

EXTERN FUNCTION Main(args[])
    RETURN SimpleRecursivity(5)
END FUNCTION

ASYNC FUNCTION SimpleRecursivity(num)
    IF num < 1 THEN
        BREAKPOINT
        RETURN num
    ELSE
        RETURN SimpleRecursivity(num - 1)
    END IF
END FUNCTION";

            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program);
            var t = interpreter.StartDebugAsync(true);

            Assert.IsNull(interpreter.DebugInfos);

            await Task.Delay(5000);

            var debugInfo = interpreter.DebugInfos;

            Assert.IsNotNull(debugInfo);
            Assert.AreEqual("Hello", debugInfo.GlobalVariables.Single().Value);
            Assert.AreEqual(BaZicInterpreterState.Pause, interpreter.State);
            Assert.AreEqual(1, debugInfo.GlobalVariables.Count);
            Assert.AreEqual("SimpleRecursivity", debugInfo.CallStack.First().InvokeMethodExpression.MethodName.Identifier);
            Assert.AreEqual("num", debugInfo.CallStack.First().Variables.Single().Name);
            Assert.AreEqual(0, debugInfo.CallStack.First().Variables.Single().Value);
            Assert.IsNull(debugInfo.CallStack[1]);




            inputCode =
@"
VARIABLE globVar = ""Hello""

EXTERN FUNCTION Main(args[])
    RETURN AWAIT SimpleRecursivity(5)
END FUNCTION

ASYNC FUNCTION SimpleRecursivity(num)
    IF num < 1 THEN
        BREAKPOINT
        RETURN num
    ELSE
        IF num % 2 = 0 THEN
            RETURN SimpleRecursivity(num - 1)
        ELSE
            RETURN AWAIT SimpleRecursivity(num - 1)
        END IF
    END IF
END FUNCTION";

            interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program);
            t = interpreter.StartDebugAsync(true);

            Assert.IsNull(interpreter.DebugInfos);

            await Task.Delay(5000);

            debugInfo = interpreter.DebugInfos;

            Assert.IsNotNull(debugInfo);
            Assert.AreEqual(BaZicInterpreterState.Pause, interpreter.State);
            Assert.AreEqual(7, debugInfo.CallStack.Count);
            Assert.AreEqual(1, debugInfo.GlobalVariables.Count);
            Assert.AreEqual("SimpleRecursivity", debugInfo.CallStack.First().InvokeMethodExpression.MethodName.Identifier);
            Assert.AreEqual("num", debugInfo.CallStack.First().Variables.Single().Name);
            Assert.AreEqual(0, debugInfo.CallStack.First().Variables.Single().Value);
            Assert.AreEqual("SimpleRecursivity", debugInfo.CallStack[1].InvokeMethodExpression.MethodName.Identifier);
            Assert.AreEqual("num", debugInfo.CallStack[1].Variables.Single().Name);
            Assert.AreEqual(1, debugInfo.CallStack[1].Variables.Single().Value);
            Assert.IsNull(debugInfo.CallStack[2]);
            Assert.IsNull(debugInfo.CallStack[3]);
            Assert.IsNull(debugInfo.CallStack[4]);
            Assert.IsNull(debugInfo.CallStack[5]);
            Assert.IsNull(debugInfo.CallStack[6]);
        }

        [TestMethod]
        public async Task BaZicInterpreterSecurity()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
    VARIABLE var1 = Localization.L.BaZic.AbstractSyntaxTree.InvalidNamespace
    RETURN var1
END FUNCTION";
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program);
            await interpreter.StartDebugAsync(true);

            var expectedLogs = @"[State] Ready
[State] Preparing
[Log] Reference assembly 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' loaded in the application domain.
[Log] Reference assembly 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' loaded in the application domain.
[Log] Reference assembly 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' loaded in the application domain.
[Log] Reference assembly 'System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' loaded in the application domain.
[Log] Reference assembly 'Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' loaded in the application domain.
[Log] Reference assembly 'PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' loaded in the application domain.
[Log] Reference assembly 'PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' loaded in the application domain.
[Log] Reference assembly 'WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' loaded in the application domain.
[Log] Declaring global variables.
[Log] Program's entry point detected.
[State] Running
[Log] Preparing to invoke the method 'Main'.
[Log] Executing the argument values of the method.
[Log] Executing an expression of type 'ArrayCreationExpression'.
[Log] The expression returned the value 'BaZicProgramReleaseMode.ObservableDictionary' (BaZicProgramReleaseMode.ObservableDictionary (length: 0)).
[Log] Invoking the synchronous method 'Main'.
[Log] Variable 'args' declared. Default value : {Null}
[Log] Variable 'args' value set to : BaZicProgramReleaseMode.ObservableDictionary (BaZicProgramReleaseMode.ObservableDictionary (length: 0))
[Log] Registering labels.
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'PropertyReferenceExpression'.
[Log] Getting the property 'Localization.L.BaZic.AbstractSyntaxTree.InvalidNamespace'.
[Log] Executing an expression of type 'ClassReferenceExpression'.
[Error] Unexpected and unmanaged error has been detected : Unable to load the type 'Localization.L.BaZic.AbstractSyntaxTree'. Does an assembly is missing?
";

            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());
        }

        [TestMethod]
        public async Task BaZicCompile()
        {
            var inputCode =
@"EXTERN FUNCTION Main(args[])
    VARIABLE var1 = 1
    VARIABLE Var1 = 2
    VARIABLE result = var1 + Var1
    System.Console.WriteLine(result.ToString())
    RETURN result
END FUNCTION";

            using (var interpreter = new BaZicInterpreter(inputCode, string.Empty, false))
            {
                var mscorlib = AssemblyDetails.GetAssemblyDetailsFromName(typeof(object).Assembly.FullName);
                var baZicCoreTest = AssemblyDetails.GetAssemblyDetailsFromName(typeof(LogMock).Assembly.Location);
                interpreter.SetDependencies(mscorlib, baZicCoreTest);

                var tempFile = Path.Combine(Path.GetTempPath(), "BaZic_Bin", Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + ".exe");
                var errors = await interpreter.Build(Core.Enums.BaZicCompilerOutputType.ConsoleApp, tempFile);

                Assert.IsNull(errors);
                Assert.IsTrue(File.Exists(tempFile));
                Assert.IsTrue(File.Exists(tempFile.Replace(".exe", ".pdb")));
                Assert.IsTrue(File.Exists(Path.Combine(Path.GetTempPath(), "BaZic_Bin", "BaZic.Core.Tests.dll")));

                File.Delete(tempFile);
                File.Delete(tempFile.Replace(".exe", ".pdb"));
                File.Delete(Path.Combine(Path.GetTempPath(), "BaZic_Bin", "BaZic.Core.Tests.dll"));
                Directory.Delete(Path.Combine(Path.GetTempPath(), @"BaZic_Bin"), true);
            }
        }

        [TestMethod]
        public async Task BaZicCompileWithUI()
        {
            var inputCode =
@"
BIND Button1_Content

EXTERN FUNCTION Main(args[])
END FUNCTION

EVENT FUNCTION Window1_Closed()
    RETURN ""Result of Window.Close""
END FUNCTION

EVENT FUNCTION Window1_Loaded()
    VARIABLE var1 = Button1_Content
    IF var1 = ""Hello"" THEN
        Button1_Content = ""Hello World""
        var1 = Button1_Content
        IF var1 = ""Hello World"" THEN
            RETURN TRUE
        END IF
    END IF
END FUNCTION

# The XAML will be provided separatly";

            var xamlCode = @"
<Window xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Name=""Window1"">
    <StackPanel>
        <Button Name=""Button1"" Content=""Hello""/>
    </StackPanel>
</Window>";

            using (var interpreter = new BaZicInterpreter(inputCode, xamlCode, false))
            {
                var tempFile = Path.Combine(Path.GetTempPath(), "BaZic_Bin", Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + ".exe");
                var errors = await interpreter.Build(Core.Enums.BaZicCompilerOutputType.WindowsApp, tempFile);

                Assert.IsNull(errors);
                Assert.IsTrue(File.Exists(tempFile));
                Assert.IsTrue(File.Exists(tempFile.Replace(".exe", ".pdb")));

                File.Delete(tempFile);
                File.Delete(tempFile.Replace(".exe", ".pdb"));
                Directory.Delete(Path.Combine(Path.GetTempPath(), @"BaZic_Bin"), true);
            }
        }
    }
}
