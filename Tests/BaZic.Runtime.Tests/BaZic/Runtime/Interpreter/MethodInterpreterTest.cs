using System.Threading.Tasks;
using BaZic.Runtime.BaZic.Code.Parser;
using BaZic.Runtime.BaZic.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Runtime.Interpreter
{
    [TestClass]
    public class MethodInterpreterTest
    {
        [TestInitialize]
        public void Initialize()
        {
            TestUtilities.InitializeLogs();
        }

        [TestMethod]
        public async Task MethodInterpreterEntryPointMethod()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Main(args[])
    RETURN args[0]
END FUNCTION";
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, true).Program);
            await interpreter.StartDebugAsync(true, "Hello World");

            var expectedLogs = @"[State] Ready
[State] Preparing
[Log] Reference assembly 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll' loaded in the application domain.
[Log] Reference assembly 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.dll' loaded in the application domain.
[Log] Reference assembly 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Core.dll' loaded in the application domain.
[Log] Reference assembly 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Runtime.dll' loaded in the application domain.
[Log] Reference assembly 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\Microsoft.CSharp.dll' loaded in the application domain.
[Log] Declaring global variables.
[Log] Program's entry point detected.
[State] Running
[Log] Preparing to invoke the method 'Main'.
[Log] Executing the argument values of the method.
[Log] Executing an expression of type 'ArrayCreationExpression'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'Hello World' (System.String).
[Log] The expression returned the value 'BaZicProgramReleaseMode.ObservableDictionary' (BaZicProgramReleaseMode.ObservableDictionary (length: 1)).
[Log] Invoking the synchronous method 'Main'.
[Log] Variable 'args' declared. Default value : {Null}
[Log] Variable 'args' value set to : BaZicProgramReleaseMode.ObservableDictionary (BaZicProgramReleaseMode.ObservableDictionary (length: 1))
[Log] Registering labels.
[Log] Executing a statement of type 'ReturnStatement'.
[Log] Executing an expression of type 'ArrayIndexerExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value 'BaZicProgramReleaseMode.ObservableDictionary' (BaZicProgramReleaseMode.ObservableDictionary (length: 1)).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '0' (System.Int32).
[Log] The expression returned the value 'Hello World' (System.String).
[Log] Return : Hello World (System.String)
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] End of the execution of the method 'Main'. Returned value : Hello World (System.String)
[State] Stopped
";

            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());
            await TestUtilities.TestAllRunningMode("Hello World", inputCode, string.Empty, "Hello World");
        }

        [TestMethod]
        public async Task MethodInterpreterAwait()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Main(args[])
    AWAIT Main(NEW [1, 2, 3])
END FUNCTION";
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, true).Program);
            await interpreter.StartDebugAsync(true);

            Assert.AreEqual("Unable to await the method 'Main' because it is not marked as asynchronous.", interpreter.Error.Exception.Message);
        }
    }
}
