using System.Threading.Tasks;
using BaZic.Runtime.BaZic.Code.Parser;
using BaZic.Runtime.BaZic.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Runtime.Interpreter.Expression
{
    [TestClass]
    public class InstantiateInterpreterTest
    {
        [TestInitialize]
        public void Initialize()
        {
            TestUtilities.InitializeLogs();
        }

        [TestMethod]
        public async Task InstantiateInterpreter()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
    VARIABLE var1 = NEW System.Windows.UIElement()
    VARIABLE var2 = NEW System.Windows.Data.Binding("""")
    VARIABLE var3 = NEW System.Windows.UIElement(""Bad argument"")
END FUNCTION";

            var program = parser.Parse(inputCode, true).Program;
            program.WithAssemblies("PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            var interpreter = new BaZicInterpreter(program);
            await interpreter.StartDebugAsync(true);

            var expectedLogs = @"[State] Ready
[State] Preparing
[Log] Reference assembly 'PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' loaded in the application domain.
[Log] Reference assembly 'PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' loaded in the application domain.
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
[Log] Executing an expression of type 'InstantiateExpression'.
[Log] Executing an expression of type 'ClassReferenceExpression'.
[Log] The expression returned the value 'System.Windows.UIElement' (System.RuntimeType).
[Log] Creating a new instance of 'System.Windows.UIElement'
[Log] Executing the argument values of the method.
[Log] The expression returned the value 'System.Windows.UIElement' (System.Windows.UIElement).
[Log] Variable 'var1' declared. Default value : System.Windows.UIElement (System.Windows.UIElement)
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'InstantiateExpression'.
[Log] Executing an expression of type 'ClassReferenceExpression'.
[Log] The expression returned the value 'System.Windows.Data.Binding' (System.RuntimeType).
[Log] Creating a new instance of 'System.Windows.Data.Binding'
[Log] Executing the argument values of the method.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '' (System.String).
[Log] The expression returned the value 'System.Windows.Data.Binding' (System.Windows.Data.Binding).
[Log] Variable 'var2' declared. Default value : System.Windows.Data.Binding (System.Windows.Data.Binding)
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'InstantiateExpression'.
[Log] Executing an expression of type 'ClassReferenceExpression'.
[Log] The expression returned the value 'System.Windows.UIElement' (System.RuntimeType).
[Log] Creating a new instance of 'System.Windows.UIElement'
[Log] Executing the argument values of the method.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'Bad argument' (System.String).
[Error] Unexpected and unmanaged error has been detected : There is no constructor of 'System.Windows.UIElement' that accept arguments of types 'String'.
";

            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());
        }
    }
}
