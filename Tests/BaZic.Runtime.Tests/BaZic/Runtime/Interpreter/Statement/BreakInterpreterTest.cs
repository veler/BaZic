using BaZic.Runtime.BaZic.Code.Parser;
using BaZic.Runtime.BaZic.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace BaZic.Runtime.Tests.BaZic.Runtime.Interpreter.Statement
{
    [TestClass]
    public class BreakInterpreterTest
    {
        [TestInitialize]
        public void Initialize()
        {
            TestUtilities.InitializeLogs();
        }

        [TestMethod]
        public async Task BreakInterpreter()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Main(args[])
    VARIABLE var1 = 0

    DO WHILE var1 < 10
        DO
            IF var1 > 5 THEN
                BREAK
            END IF

            var1 = var1 + 1
        LOOP WHILE True
        var1 = var1 + 1
    LOOP

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
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '0' (System.Int32).
[Log] Variable 'var1' declared. Default value : 0 (System.Int32)
[Log] Executing a statement of type 'IterationStatement'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '0' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'IterationStatement'.
[Log] Registering labels.
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 >= '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '0' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'GreaterThanOrEqual'.
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to 'var1 + '1' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '0' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Doing an operation 'Addition'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Variable 'var1' value set to : 1 (System.Int32)
[Log] 'var1' is now equal to '1'(type:System.Int32)
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 >= '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'GreaterThanOrEqual'.
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to 'var1 + '1' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Doing an operation 'Addition'.
[Log] The expression returned the value '2' (System.Int32).
[Log] Variable 'var1' value set to : 2 (System.Int32)
[Log] 'var1' is now equal to '2'(type:System.Int32)
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 >= '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '2' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'GreaterThanOrEqual'.
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to 'var1 + '1' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '2' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Doing an operation 'Addition'.
[Log] The expression returned the value '3' (System.Int32).
[Log] Variable 'var1' value set to : 3 (System.Int32)
[Log] 'var1' is now equal to '3'(type:System.Int32)
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 >= '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '3' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'GreaterThanOrEqual'.
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to 'var1 + '1' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '3' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Doing an operation 'Addition'.
[Log] The expression returned the value '4' (System.Int32).
[Log] Variable 'var1' value set to : 4 (System.Int32)
[Log] 'var1' is now equal to '4'(type:System.Int32)
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 >= '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '4' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'GreaterThanOrEqual'.
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to 'var1 + '1' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '4' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Doing an operation 'Addition'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Variable 'var1' value set to : 5 (System.Int32)
[Log] 'var1' is now equal to '5'(type:System.Int32)
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 >= '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'GreaterThanOrEqual'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'BreakStatement'.
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] End of the execution of the condition 'var1 >= '5' (type:System.Int32)'.
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to 'var1 + '1' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Doing an operation 'Addition'.
[Log] The expression returned the value '6' (System.Int32).
[Log] Variable 'var1' value set to : 6 (System.Int32)
[Log] 'var1' is now equal to '6'(type:System.Int32)
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '6' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'IterationStatement'.
[Log] Registering labels.
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 >= '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '6' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'GreaterThanOrEqual'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'BreakStatement'.
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] End of the execution of the condition 'var1 >= '5' (type:System.Int32)'.
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to 'var1 + '1' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '6' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Doing an operation 'Addition'.
[Log] The expression returned the value '7' (System.Int32).
[Log] Variable 'var1' value set to : 7 (System.Int32)
[Log] 'var1' is now equal to '7'(type:System.Int32)
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '7' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'IterationStatement'.
[Log] Registering labels.
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 >= '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '7' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'GreaterThanOrEqual'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'BreakStatement'.
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] End of the execution of the condition 'var1 >= '5' (type:System.Int32)'.
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to 'var1 + '1' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '7' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Doing an operation 'Addition'.
[Log] The expression returned the value '8' (System.Int32).
[Log] Variable 'var1' value set to : 8 (System.Int32)
[Log] 'var1' is now equal to '8'(type:System.Int32)
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '8' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'IterationStatement'.
[Log] Registering labels.
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 >= '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '8' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'GreaterThanOrEqual'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'BreakStatement'.
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] End of the execution of the condition 'var1 >= '5' (type:System.Int32)'.
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to 'var1 + '1' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '8' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Doing an operation 'Addition'.
[Log] The expression returned the value '9' (System.Int32).
[Log] Variable 'var1' value set to : 9 (System.Int32)
[Log] 'var1' is now equal to '9'(type:System.Int32)
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '9' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'IterationStatement'.
[Log] Registering labels.
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 >= '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '9' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'GreaterThanOrEqual'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'BreakStatement'.
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] End of the execution of the condition 'var1 >= '5' (type:System.Int32)'.
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to 'var1 + '1' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '9' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Doing an operation 'Addition'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Variable 'var1' value set to : 10 (System.Int32)
[Log] 'var1' is now equal to '10'(type:System.Int32)
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'ReturnStatement'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Return : 10 (System.Int32)
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] End of the execution of the method 'Main'. Returned value : 10 (System.Int32)
[State] Stopped
";

            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());
            await TestUtilities.TestAllRunningMode("10", inputCode);
        }

        [TestMethod]
        public async Task BreakInterpreterInlined()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Main(args[])
    VARIABLE var1 = 0

    DO WHILE var1 < 10
        DO
            IF var1 > 5 THEN
                BREAK
            END IF

            var1 = var1 + 1
        LOOP WHILE True
        var1 = var1 + 1
    LOOP

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
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '0' (System.Int32).
[Log] Variable 'var1' declared. Default value : 0 (System.Int32)
[Log] Executing a statement of type 'LabelDeclaration'.
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '0' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'LabelDeclaration'.
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '0' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'GreaterThanOrEqual'.
[Log] The expression returned the value 'False' (System.Boolean).
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Jumping to the label '_E'.
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to 'var1 + '1' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '0' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Doing an operation 'Addition'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Variable 'var1' value set to : 1 (System.Int32)
[Log] 'var1' is now equal to '1'(type:System.Int32)
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'GoToLabelStatement'.
[Log] Jumping to the label '_C'.
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'GreaterThanOrEqual'.
[Log] The expression returned the value 'False' (System.Boolean).
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Jumping to the label '_E'.
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to 'var1 + '1' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Doing an operation 'Addition'.
[Log] The expression returned the value '2' (System.Int32).
[Log] Variable 'var1' value set to : 2 (System.Int32)
[Log] 'var1' is now equal to '2'(type:System.Int32)
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'GoToLabelStatement'.
[Log] Jumping to the label '_C'.
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '2' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'GreaterThanOrEqual'.
[Log] The expression returned the value 'False' (System.Boolean).
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Jumping to the label '_E'.
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to 'var1 + '1' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '2' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Doing an operation 'Addition'.
[Log] The expression returned the value '3' (System.Int32).
[Log] Variable 'var1' value set to : 3 (System.Int32)
[Log] 'var1' is now equal to '3'(type:System.Int32)
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'GoToLabelStatement'.
[Log] Jumping to the label '_C'.
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '3' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'GreaterThanOrEqual'.
[Log] The expression returned the value 'False' (System.Boolean).
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Jumping to the label '_E'.
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to 'var1 + '1' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '3' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Doing an operation 'Addition'.
[Log] The expression returned the value '4' (System.Int32).
[Log] Variable 'var1' value set to : 4 (System.Int32)
[Log] 'var1' is now equal to '4'(type:System.Int32)
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'GoToLabelStatement'.
[Log] Jumping to the label '_C'.
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '4' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'GreaterThanOrEqual'.
[Log] The expression returned the value 'False' (System.Boolean).
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Jumping to the label '_E'.
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to 'var1 + '1' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '4' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Doing an operation 'Addition'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Variable 'var1' value set to : 5 (System.Int32)
[Log] 'var1' is now equal to '5'(type:System.Int32)
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'GoToLabelStatement'.
[Log] Jumping to the label '_C'.
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'GreaterThanOrEqual'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'GoToLabelStatement'.
[Log] Jumping to the label '_D'.
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to 'var1 + '1' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Doing an operation 'Addition'.
[Log] The expression returned the value '6' (System.Int32).
[Log] Variable 'var1' value set to : 6 (System.Int32)
[Log] 'var1' is now equal to '6'(type:System.Int32)
[Log] Executing a statement of type 'GoToLabelStatement'.
[Log] Jumping to the label '_A'.
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '6' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'LabelDeclaration'.
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '6' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'GreaterThanOrEqual'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'GoToLabelStatement'.
[Log] Jumping to the label '_D'.
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to 'var1 + '1' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '6' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Doing an operation 'Addition'.
[Log] The expression returned the value '7' (System.Int32).
[Log] Variable 'var1' value set to : 7 (System.Int32)
[Log] 'var1' is now equal to '7'(type:System.Int32)
[Log] Executing a statement of type 'GoToLabelStatement'.
[Log] Jumping to the label '_A'.
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '7' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'LabelDeclaration'.
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '7' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'GreaterThanOrEqual'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'GoToLabelStatement'.
[Log] Jumping to the label '_D'.
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to 'var1 + '1' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '7' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Doing an operation 'Addition'.
[Log] The expression returned the value '8' (System.Int32).
[Log] Variable 'var1' value set to : 8 (System.Int32)
[Log] 'var1' is now equal to '8'(type:System.Int32)
[Log] Executing a statement of type 'GoToLabelStatement'.
[Log] Jumping to the label '_A'.
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '8' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'LabelDeclaration'.
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '8' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'GreaterThanOrEqual'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'GoToLabelStatement'.
[Log] Jumping to the label '_D'.
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to 'var1 + '1' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '8' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Doing an operation 'Addition'.
[Log] The expression returned the value '9' (System.Int32).
[Log] Variable 'var1' value set to : 9 (System.Int32)
[Log] 'var1' is now equal to '9'(type:System.Int32)
[Log] Executing a statement of type 'GoToLabelStatement'.
[Log] Jumping to the label '_A'.
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '9' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'LabelDeclaration'.
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '9' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'GreaterThanOrEqual'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'GoToLabelStatement'.
[Log] Jumping to the label '_D'.
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to 'var1 + '1' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '9' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Doing an operation 'Addition'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Variable 'var1' value set to : 10 (System.Int32)
[Log] 'var1' is now equal to '10'(type:System.Int32)
[Log] Executing a statement of type 'GoToLabelStatement'.
[Log] Jumping to the label '_A'.
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'False' (System.Boolean).
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Jumping to the label '_B'.
[Log] Executing a statement of type 'ReturnStatement'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Return : 10 (System.Int32)
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] End of the execution of the method 'Main'. Returned value : 10 (System.Int32)
[State] Stopped
";

            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());
            await TestUtilities.TestAllRunningMode("10", inputCode);
        }
    }
}
