using System.Linq;
using System.Threading.Tasks;
using BaZic.Runtime.BaZic.Code.Parser;
using BaZic.Runtime.BaZic.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Runtime.Interpreter.Expression
{
    [TestClass]
    public class PropertyReferenceInterpreterTest
    {
        [TestInitialize]
        public void Initialize()
        {
            TestUtilities.InitializeLogs();
        }

        [TestMethod]
        public async Task PropertyReferenceInterpreter()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Main(args[])
    VARIABLE var1 = ""Hello"".Length
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
[Log] Executing an expression of type 'PropertyReferenceExpression'.
[Log] Getting the property ''Hello' (type:System.String).Length'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'Hello' (System.String).
[Log] The expression returned the value '5' (System.Int32).
[Log] Variable 'var1' declared. Default value : 5 (System.Int32)
[Log] End of the execution of the method 'Main'. Returned value :  ({Null})
[State] Stopped
";

            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());




            inputCode =
@"FUNCTION Main(args[])
    VARIABLE var1 = ""Hello"".length
END FUNCTION";
            interpreter = new BaZicInterpreter(parser.Parse(inputCode, true).Program);
            await interpreter.StartDebugAsync(true);

            Assert.AreEqual("Unable to access to the property 'length' of the type 'System.String'.", interpreter.StateChangedHistory.Last().Error.Exception.InnerException.Message);
        }

        [TestMethod]
        public async Task PropertyReferenceInterpreterStaticAndAssemblyRequired()
        {
            var parser = new BaZicParser();



            var inputCode =
@"FUNCTION Main(args[])
    RETURN System.Windows.FontStyles.Italic
END FUNCTION";

            var program = parser.Parse(inputCode, true).Program;
            var interpreter = new BaZicInterpreter(program);
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
[Log] Executing a statement of type 'ReturnStatement'.
[Log] Executing an expression of type 'PropertyReferenceExpression'.
[Log] Getting the property 'System.Windows.FontStyles.Italic'.
[Log] Executing an expression of type 'ClassReferenceExpression'.
[Error] Unexpected and unmanaged error has been detected : Unable to load the type 'System.Windows.FontStyles'. Does an assembly is missing?
";

            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());



            inputCode =
@"FUNCTION Main(args[])
    RETURN System.Windows.FontStyles.Italic
END FUNCTION";

            program = parser.Parse(inputCode, true).Program;
            program.WithAssemblies("PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            interpreter = new BaZicInterpreter(program);
            await interpreter.StartDebugAsync(true);

            expectedLogs = @"[State] Ready
[State] Preparing
[Log] Reference assembly 'PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' loaded in the application domain.
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
[Log] Executing a statement of type 'ReturnStatement'.
[Log] Executing an expression of type 'PropertyReferenceExpression'.
[Log] Getting the property 'System.Windows.FontStyles.Italic'.
[Log] Executing an expression of type 'ClassReferenceExpression'.
[Log] The expression returned the value 'System.Windows.FontStyles' (System.RuntimeType).
[Log] The expression returned the value 'Italic' (System.Windows.FontStyle).
[Log] Return : Italic (System.Windows.FontStyle)
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] End of the execution of the method 'Main'. Returned value : Italic (System.Windows.FontStyle)
[State] Stopped
";

            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());



            inputCode =
@"FUNCTION Main(args[])
    VARIABLE System
    RETURN System.Windows.FontStyles.Italic
END FUNCTION";

            program = parser.Parse(inputCode, true).Program;
            program.WithAssemblies("PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            interpreter = new BaZicInterpreter(program);
            await interpreter.StartDebugAsync(true);

            expectedLogs = @"[State] Ready
[State] Preparing
[Log] Reference assembly 'PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' loaded in the application domain.
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
[Log] Variable 'System' declared. Default value : {Null}
[Log] Executing a statement of type 'ReturnStatement'.
[Log] Executing an expression of type 'PropertyReferenceExpression'.
[Log] Getting the property 'System.Windows.FontStyles.Italic'.
[Log] Executing an expression of type 'PropertyReferenceExpression'.
[Log] Getting the property 'System.Windows.FontStyles'.
[Log] Executing an expression of type 'PropertyReferenceExpression'.
[Log] Getting the property 'System.Windows'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '' ({Null}).
[Error] Unable to access to a property of {Null}.
";

            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());
        }
    }
}
