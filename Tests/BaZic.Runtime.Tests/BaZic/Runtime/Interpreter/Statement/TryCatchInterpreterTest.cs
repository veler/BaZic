using BaZic.Runtime.BaZic.Code.Parser;
using BaZic.Runtime.BaZic.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace BaZic.Runtime.Tests.BaZic.Runtime.Interpreter.Statement
{
    [TestClass]
    public class TryCatchInterpreterTest
    {
        [TestInitialize]
        public void Initialize()
        {
            TestUtilities.InitializeLogs();
        }

        [TestMethod]
        public async Task TryCatchInterpreter()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
    TRY
        THROW new System.ArgumentException(""Hello World"")
    CATCH
        RETURN Exception.Message
    END TRY

    RETURN False
END FUNCTION";
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program);
            await interpreter.StartDebugAsync(true);

            var expectedLogs = interpreter.GetStateChangedHistoryString();

            Assert.IsTrue(expectedLogs.Contains("[Log] End of the execution of the method 'Main'. Returned value : Hello World (System.String)"));
            Assert.IsTrue(expectedLogs.Contains("[State] Stopped"));




            inputCode =
@"EXTERN FUNCTION Main(args[])
    THROW new System.ArgumentException(""Hello World"")
    RETURN False
END FUNCTION";
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
[Log] Executing a statement of type 'ThrowStatement'.
[Log] Executing an expression of type 'InstantiateExpression'.
[Log] Executing an expression of type 'ClassReferenceExpression'.
[Log] The expression returned the value 'System.ArgumentException' (System.RuntimeType).
[Log] Creating a new instance of 'System.ArgumentException'
[Log] Executing the argument values of the method.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'Hello World' (System.String).
[Log] The expression returned the value 'System.ArgumentException: Hello World' (System.ArgumentException).
[Error] Unexpected and unmanaged error has been detected : Hello World
";

            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());
        }

        [TestMethod]
        public async Task TryCatchInterpreterInlined()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
    RETURN Method1()
END FUNCTION

FUNCTION Method1()
    DO WHILE True
        TRY
            TRY
                IF TRUE THEN
                    RETURN ""FOO""
                END IF
            END TRY
        CATCH
            IF TRUE THEN
                BREAK
            END IF
        END TRY
    LOOP

    RETURN ""Hello""
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
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Variable 'RET_A' declared. Default value : {Null}
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Variable 'RET_B' declared. Default value : {Null}
[Log] Executing a statement of type 'LabelDeclaration'.
[Log] Executing a statement of type 'LabelDeclaration'.
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'TryCatchStatement'.
[Log] Registering labels.
[Log] Executing a statement of type 'TryCatchStatement'.
[Log] Registering labels.
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'RET_B' to ''FOO' (type:System.String)'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'FOO' (System.String).
[Log] Variable 'RET_B' value set to : FOO (System.String)
[Log] 'RET_B' is now equal to 'FOO'(type:System.String)
[Log] Executing a statement of type 'GoToLabelStatement'.
[Log] Jumping to the label '_C'.
[Log] Jumping to the label '_C'.
[Log] Jumping to the label '_C'.
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'RET_A' to 'RET_B'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value 'FOO' (System.String).
[Log] Variable 'RET_A' value set to : FOO (System.String)
[Log] 'RET_A' is now equal to 'FOO'(type:System.String)
[Log] Executing a statement of type 'ReturnStatement'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value 'FOO' (System.String).
[Log] Return : FOO (System.String)
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] End of the execution of the method 'Main'. Returned value : FOO (System.String)
[State] Idle
[State] Stopped
";

            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());
            await TestUtilities.TestAllRunningMode("FOO", inputCode);
        }
    }
}
