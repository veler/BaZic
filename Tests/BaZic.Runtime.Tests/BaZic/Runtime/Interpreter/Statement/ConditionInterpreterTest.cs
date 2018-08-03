using BaZic.Runtime.BaZic.Code.Parser;
using BaZic.Runtime.BaZic.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace BaZic.Runtime.Tests.BaZic.Runtime.Interpreter.Statement
{
    [TestClass]
    public class ConditionInterpreterTest
    {
        [TestInitialize]
        public void Initialize()
        {
            TestUtilities.InitializeLogs();
        }

        [TestMethod]
        public async Task ConditionInterpreter()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Main(args[])
    VARIABLE var1

    IF Not True THEN
        var1 = 1
    ELSE
        IF var1 = NULL THEN
            var1 = 2 # Should go there
        END IF
    END IF

    RETURN var1
END FUNCTION";
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program);
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
[Log] Variable 'var1' declared. Default value : {Null}
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'NOT 'True' (type:System.Boolean)'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Registering labels.
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
[Log] Assign 'var1' to ''2' (type:System.Int32)'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '2' (System.Int32).
[Log] Variable 'var1' value set to : 2 (System.Int32)
[Log] 'var1' is now equal to '2'(type:System.Int32)
[Log] End of the execution of the condition 'var1 == {null}'.
[Log] End of the execution of the condition 'NOT 'True' (type:System.Boolean)'.
[Log] Executing a statement of type 'ReturnStatement'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '2' (System.Int32).
[Log] Return : 2 (System.Int32)
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] End of the execution of the method 'Main'. Returned value : 2 (System.Int32)
[State] Stopped
";

            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());
            await TestUtilities.TestAllRunningMode("2", inputCode);





            parser = new BaZicParser();

            inputCode =
@"FUNCTION Main(args[])
    IF ""Hello"" THEN
    END IF
END FUNCTION";
            interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program);
            await interpreter.StartDebugAsync(true);
            expectedLogs = @"[Error] Unable to perform a condition statement without a boolean value as conditional expression result.";

            Assert.IsTrue(interpreter.GetStateChangedHistoryString().Contains(expectedLogs));
        }

        [TestMethod]
        public async Task ConditionInterpreterInlinedLabelCondition()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Main(args[])
    VARIABLE var1

    IF Not True THEN
        var1 = 1
    ELSE
        IF var1 = NULL THEN
            var1 = 2 # Should go there
        END IF
    END IF

    RETURN var1
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
[Log] Variable 'var1' declared. Default value : {Null}
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] The expression returned the value 'False' (System.Boolean).
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Jumping to the label '_A'.
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '' ({Null}).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '' ({Null}).
[Log] Doing an operation 'Equality'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to ''2' (type:System.Int32)'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '2' (System.Int32).
[Log] Variable 'var1' value set to : 2 (System.Int32)
[Log] 'var1' is now equal to '2'(type:System.Int32)
[Log] Executing a statement of type 'LabelDeclaration'.
[Log] Executing a statement of type 'LabelDeclaration'.
[Log] Executing a statement of type 'ReturnStatement'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '2' (System.Int32).
[Log] Return : 2 (System.Int32)
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] End of the execution of the method 'Main'. Returned value : 2 (System.Int32)
[State] Stopped
";


            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());
            await TestUtilities.TestAllRunningMode("2", inputCode);
        }
    }
}
