using System.Threading.Tasks;
using BaZic.Runtime.BaZic.Code.Parser;
using BaZic.Runtime.BaZic.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Runtime.Interpreter.Expression
{
    [TestClass]
    public class PrimitiveInterpreterTest
    {
        [TestInitialize]
        public void Initialize()
        {
            TestUtilities.InitializeLogs();
        }

        [TestMethod]
        public async Task PrimitiveInterpreter()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Main(args[])
    VARIABLE var1 = 1
    VARIABLE var2 = 1.2
    VARIABLE var3 = TRUE
    VARIABLE var4 = false
    VARIABLE var5 = NULL
    VARIABLE var6 = ""Hello""
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
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Variable 'var1' declared. Default value : 1 (System.Int32)
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1.2' (System.Double).
[Log] Variable 'var2' declared. Default value : 1.2 (System.Double)
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Variable 'var3' declared. Default value : True (System.Boolean)
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Variable 'var4' declared. Default value : False (System.Boolean)
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '' ({Null}).
[Log] Variable 'var5' declared. Default value : {Null}
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'Hello' (System.String).
[Log] Variable 'var6' declared. Default value : Hello (System.String)
[Log] End of the execution of the method 'Main'. Returned value :  ({Null})
[State] Stopped
";

            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());
        }
    }
}
