using System.Threading.Tasks;
using BaZic.Runtime.BaZic.Code.Parser;
using BaZic.Runtime.BaZic.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Runtime.Interpreter.Expression
{
    [TestClass]
    public class InvokeMethodInterpreterTest
    {
        [TestInitialize]
        public void Initialize()
        {
            TestUtilities.InitializeLogs();
        }

        [TestMethod]
        public async Task InvokeMethodInterpreter()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
    VARIABLE var1 = Method1(""Hello"")
    VARIABLE var2 = MethodAsync(""Hello Async"", 3.0)
    VARIABLE var3 = AWAIT MethodAsync(""Hello Await Async"", 1.0)
    RETURN ""END OF MAIN METHOD""
END FUNCTION

FUNCTION Method1(value)
    RETURN value
END FUNCTION

ASYNC FUNCTION MethodAsync(value, timeToWait)
    VARIABLE var1 = AWAIT System.Threading.Tasks.Task.Delay(System.TimeSpan.FromSeconds(timeToWait))
    RETURN value
END FUNCTION";

            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program);
            await interpreter.StartDebugAsync(true);

            var expected1 = "[Log] Variable 'var1' declared. Default value : Hello (System.String)";
            var expected2 = "[Log] Variable 'var2' declared. Default value : System.Threading.Tasks.Task (System.Threading.Tasks.Task)";
            var expected3 = "[Log] Variable 'var3' declared. Default value : Hello Await Async (System.String)";

            var result = interpreter.GetStateChangedHistoryString();

            Assert.IsTrue(result.Contains(expected1));
            Assert.IsTrue(result.Contains(expected2));
            Assert.IsTrue(result.Contains(expected3));

            // The call of MethodAsync on var2 must finish after var3.
            Assert.IsTrue(result.IndexOf("[Log] Return : Hello Async (System.String)") > result.IndexOf("[Log] Return : Hello Await Async (System.String)"));
            Assert.IsTrue(result.IndexOf("[Log] Return : END OF MAIN METHOD (System.String)") > result.IndexOf("[Log] Return : Hello Await Async (System.String)"));
            Assert.IsTrue(result.IndexOf("[Log] Return : Hello Async (System.String)") > result.IndexOf("[Log] Return : END OF MAIN METHOD (System.String)"));
            Assert.AreEqual("END OF MAIN METHOD", interpreter.ProgramResult);

            await TestUtilities.TestAllRunningMode("END OF MAIN METHOD", inputCode);
        }

        [TestMethod]
        public async Task InvokeMethodInterpreterRecursivity()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
    RETURN FirstMethod(100)
END FUNCTION

FUNCTION FirstMethod(num)
    IF num > 1 THEN
        RETURN FirstMethod(num - 1)
    END IF
    RETURN num
END FUNCTION";

            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program);
            await interpreter.StartDebugAsync(true);

            var expected1 = "[Log] End of the execution of the method 'FirstMethod'. Returned value : 0 (System.Int32)";

            var result = interpreter.GetStateChangedHistoryString();

            Assert.IsTrue(result.Contains(expected1));
            Assert.AreEqual(0, interpreter.ProgramResult);

            await TestUtilities.TestAllRunningMode("0", inputCode);
        }

        [TestMethod]
        public async Task InvokeMethodInterpreterRecursivityStackoverflowAsync()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
    FirstMethod(1000) # A Stackoverflow must never happen if FirstMethod is ASYNC.
END FUNCTION

ASYNC FUNCTION FirstMethod(num)
    IF num > 1 THEN
        RETURN FirstMethod(num - 1)
    END IF
    RETURN num
END FUNCTION";

            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program);
            await interpreter.StartDebugAsync(true);

            var expected1 = "[Log] End of the execution of the method 'FirstMethod'. Returned value : 0 (System.Int32)";

            var result = interpreter.GetStateChangedHistoryString();

            Assert.IsTrue(result.Contains(expected1));
            Assert.AreEqual(null, interpreter.ProgramResult);

            await TestUtilities.TestAllRunningMode(null, inputCode);
        }

        [TestMethod]
        public async Task InvokeMethodInterpreterRecursivityStackoverflow()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
    FirstMethod(1000) # A Stackoverflow must never happen because every 100 recursive call, a new thread is created.
END FUNCTION

FUNCTION FirstMethod(num)
    IF num > 1 THEN
        RETURN FirstMethod(num - 1)
    END IF
    RETURN num
END FUNCTION";

            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program);
            await interpreter.StartDebugAsync(true);

            var expected1 = "[Log] End of the execution of the method 'FirstMethod'. Returned value : 0 (System.Int32)";

            var result = interpreter.GetStateChangedHistoryString();

            Assert.IsTrue(result.Contains(expected1));
            Assert.AreEqual(null, interpreter.ProgramResult);

            await TestUtilities.TestAllRunningMode(null, inputCode);
        }

        [TestMethod]
        public async Task InvokeMethodInterpreterInlined()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
    VARIABLE var1 = Method1(""Hello"")
    RETURN var1
END FUNCTION

FUNCTION Method1(value)
    RETURN value
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
[Log] Reference assembly 'PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' loaded in the application domain.
[Log] Reference assembly 'PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' loaded in the application domain.
[Log] Reference assembly 'WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' loaded in the application domain.
[Log] Declaring global variables.
[Log] Program's entry point detected.
[State] Running
[Log] Preparing to invoke the method 'Main'.
[Log] Executing the argument values of the method.
[Log] Executing an expression of type 'ArrayCreationExpression'.
[Log] The expression returned the value 'BaZic.StandaloneRuntime.ObservableDictionary' (BaZic.StandaloneRuntime.ObservableDictionary (length: 0)).
[Log] Invoking the synchronous method 'Main'.
[Log] Variable 'args' declared. Default value : {Null}
[Log] Variable 'args' value set to : BaZic.StandaloneRuntime.ObservableDictionary (BaZic.StandaloneRuntime.ObservableDictionary (length: 0))
[Log] Registering labels.
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Variable 'var1' declared. Default value : {Null}
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'Hello' (System.String).
[Log] Variable 'value' declared. Default value : Hello (System.String)
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Variable 'RET_A' declared. Default value : {Null}
[Log] Executing a statement of type 'LabelDeclaration'.
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'RET_A' to 'value'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value 'Hello' (System.String).
[Log] Variable 'RET_A' value set to : Hello (System.String)
[Log] 'RET_A' is now equal to 'Hello'(type:System.String)
[Log] Executing a statement of type 'GoToLabelStatement'.
[Log] Jumping to the label '_B'.
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to 'RET_A'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value 'Hello' (System.String).
[Log] Variable 'var1' value set to : Hello (System.String)
[Log] 'var1' is now equal to 'Hello'(type:System.String)
[Log] Executing a statement of type 'ReturnStatement'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value 'Hello' (System.String).
[Log] Return : Hello (System.String)
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] End of the execution of the method 'Main'. Returned value : Hello (System.String)
[State] Idle
";


            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());
            Assert.AreEqual("Hello", interpreter.ProgramResult);

            await TestUtilities.TestAllRunningMode("Hello", inputCode);
        }
    }
}
