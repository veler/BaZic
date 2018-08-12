using BaZic.Runtime.BaZic.Code.Parser;
using BaZic.Runtime.BaZic.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace BaZic.Runtime.Tests.BaZic.Runtime.Interpreter.Statement
{
    [TestClass]
    public class ReturnInterpreterTest
    {
        [TestInitialize]
        public void Initialize()
        {
            TestUtilities.InitializeLogs();
        }

        [TestMethod]
        public async Task ReturnInterpreter()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
    RETURN NULL
    VARIABLE var1 = 123 # Should not be executed
END FUNCTION";
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, true).Program);
            await interpreter.StartDebugAsync(true);

            var expectedLogs = @"[State] Ready
[State] Preparing
[Log] Reference assembly 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' loaded in the application domain.
[Log] Reference assembly 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' loaded in the application domain.
[Log] Reference assembly 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' loaded in the application domain.
[Log] Reference assembly 'System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' loaded in the application domain.
[Log] Reference assembly 'Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' loaded in the application domain.
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
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '' ({Null}).
[Log] Return : {Null}
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] End of the execution of the method 'Main'. Returned value :  ({Null})
[State] Idle
[State] Stopped
";

            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());
            await TestUtilities.TestAllRunningMode(null, inputCode);



            parser = new BaZicParser();

            inputCode =
@"EXTERN FUNCTION Main(args[])
    RETURN
END FUNCTION";
            interpreter = new BaZicInterpreter(parser.Parse(inputCode, true).Program);
            await interpreter.StartDebugAsync(true);

            expectedLogs = @"[State] Ready
[State] Preparing
[Log] Reference assembly 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' loaded in the application domain.
[Log] Reference assembly 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' loaded in the application domain.
[Log] Reference assembly 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' loaded in the application domain.
[Log] Reference assembly 'System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' loaded in the application domain.
[Log] Reference assembly 'Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' loaded in the application domain.
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
[Log] Return : {Null}
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] End of the execution of the method 'Main'. Returned value :  ({Null})
[State] Idle
[State] Stopped
";

            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());





            parser = new BaZicParser();

            inputCode =
@"EXTERN FUNCTION Main(args[])
    VARIABLE var1
    IF var1 = NULL THEN
        var1 = 1
        RETURN var1
        var1 = 2
    END IF

    RETURN False
END FUNCTION # Should return 1";
            interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program);
            await interpreter.StartDebugAsync(true);

            expectedLogs = @"[State] Ready
[State] Preparing
[Log] Reference assembly 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' loaded in the application domain.
[Log] Reference assembly 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' loaded in the application domain.
[Log] Reference assembly 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' loaded in the application domain.
[Log] Reference assembly 'System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' loaded in the application domain.
[Log] Reference assembly 'Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' loaded in the application domain.
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
[Log] Variable 'var1' declared. Default value : {Null}
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 == {null}'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '' ({Null}).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '' ({Null}).
[Log] Doing an operation 'Equality'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to ''1' (type:System.Int32)'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Variable 'var1' value set to : 1 (System.Int32)
[Log] 'var1' is now equal to '1'(type:System.Int32)
[Log] Executing a statement of type 'ReturnStatement'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Return : 1 (System.Int32)
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] End of the execution of the condition 'var1 == {null}'.
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] End of the execution of the method 'Main'. Returned value : 1 (System.Int32)
[State] Idle
[State] Stopped
";

            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());
            await TestUtilities.TestAllRunningMode("1", inputCode);
        }
    }
}
