using System.Linq;
using System.Threading.Tasks;
using BaZic.Runtime.BaZic.Code.Parser;
using BaZic.Runtime.BaZic.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Runtime.Interpreter.Expression
{
    [TestClass]
    public class InvokeCoreMethodInterpreterTest
    {
        [TestInitialize]
        public void Initialize()
        {
            TestUtilities.InitializeLogs();
        }

        [TestMethod]
        public async Task InvokeCoreMethodInterpreter1()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Main(args[])
    VARIABLE var1 = 123.ToString()
    VARIABLE var2 = 123.ToString(""X"") # to Hexadecimal
    VARIABLE var3 = System.Int32.Parse(""123"")
END FUNCTION";
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, true).Program);
            await interpreter.StartDebugAsync(true);

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
[Log] The expression returned the value 'BaZicProgramReleaseMode.ObservableDictionary' (BaZicProgramReleaseMode.ObservableDictionary (length: 0)).
[Log] Invoking the synchronous method 'Main'.
[Log] Variable 'args' declared. Default value : {Null}
[Log] Variable 'args' value set to : BaZicProgramReleaseMode.ObservableDictionary (BaZicProgramReleaseMode.ObservableDictionary (length: 0))
[Log] Registering labels.
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'InvokeCoreMethodExpression'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '123' (System.Int32).
[Log] Executing the argument values of the method.
[Log] The expression returned the value '123' (System.String).
[Log] Variable 'var1' declared. Default value : 123 (System.String)
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'InvokeCoreMethodExpression'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '123' (System.Int32).
[Log] Executing the argument values of the method.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'X' (System.String).
[Log] The expression returned the value '7B' (System.String).
[Log] Variable 'var2' declared. Default value : 7B (System.String)
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'InvokeCoreMethodExpression'.
[Log] Executing an expression of type 'ClassReferenceExpression'.
[Log] The expression returned the value 'System.Int32' (System.RuntimeType).
[Log] Executing the argument values of the method.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '123' (System.String).
[Log] The expression returned the value '123' (System.Int32).
[Log] Variable 'var3' declared. Default value : 123 (System.Int32)
[Log] End of the execution of the method 'Main'. Returned value :  ({Null})
[State] Stopped
";

            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());
            await TestUtilities.TestAllRunningMode(null, inputCode);
        }

        [TestMethod]
        public async Task InvokeCoreMethodInterpreter2()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Main(args[])
    VARIABLE var1 = AWAIT System.Threading.Tasks.Task.Delay(System.TimeSpan.FromMilliseconds(500.0))
    VARIABLE client = NEW System.Net.WebClient()
    RETURN AWAIT client.DownloadStringTaskAsync(""LocalWebPage.html"")
END FUNCTION";
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, true).Program);
            await interpreter.StartDebugAsync(true);

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
[Log] The expression returned the value 'BaZicProgramReleaseMode.ObservableDictionary' (BaZicProgramReleaseMode.ObservableDictionary (length: 0)).
[Log] Invoking the synchronous method 'Main'.
[Log] Variable 'args' declared. Default value : {Null}
[Log] Variable 'args' value set to : BaZicProgramReleaseMode.ObservableDictionary (BaZicProgramReleaseMode.ObservableDictionary (length: 0))
[Log] Registering labels.
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'InvokeCoreMethodExpression'.
[Log] Executing an expression of type 'ClassReferenceExpression'.
[Log] The expression returned the value 'System.Threading.Tasks.Task' (System.RuntimeType).
[Log] Executing the argument values of the method.
[Log] Executing an expression of type 'InvokeCoreMethodExpression'.
[Log] Executing an expression of type 'ClassReferenceExpression'.
[Log] The expression returned the value 'System.TimeSpan' (System.RuntimeType).
[Log] Executing the argument values of the method.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '500' (System.Double).
[Log] The expression returned the value '00:00:00.5000000' (System.TimeSpan).
[Log] The expression returned the value '' ({Null}).
[Log] Variable 'var1' declared. Default value : {Null}
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'InstantiateExpression'.
[Log] Executing an expression of type 'ClassReferenceExpression'.
[Log] The expression returned the value 'System.Net.WebClient' (System.RuntimeType).
[Log] Creating a new instance of 'System.Net.WebClient'
[Log] Executing the argument values of the method.
[Log] The expression returned the value 'System.Net.WebClient' (System.Net.WebClient).
[Log] Variable 'client' declared. Default value : System.Net.WebClient (System.Net.WebClient)
[Log] Executing a statement of type 'ReturnStatement'.
[Log] Executing an expression of type 'InvokeCoreMethodExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value 'System.Net.WebClient' (System.Net.WebClient).
[Log] Executing the argument values of the method.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'LocalWebPage.html' (System.String).
[Log] The expression returned the value '<!DOCTYPE html>' (System.String).
[Log] Return : <!DOCTYPE html> (System.String)
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] End of the execution of the method 'Main'. Returned value : <!DOCTYPE html> (System.String)
[State] Stopped
";

            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());
            await TestUtilities.TestAllRunningMode("<!DOCTYPE html>", inputCode);
        }

        [TestMethod]
        public async Task InvokeCoreMethodInterpreter3()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Main(args[])
    VARIABLE var1 = 123.toString()
END FUNCTION";
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, true).Program);
            await interpreter.StartDebugAsync(true);

            Assert.AreEqual("The method 'toString' does not exist in the type 'System.Int32'.", interpreter.StateChangedHistory.Last().Error.Exception.InnerException.Message);
        }
    }
}
