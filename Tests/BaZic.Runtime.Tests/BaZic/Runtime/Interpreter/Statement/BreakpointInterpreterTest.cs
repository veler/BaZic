using BaZic.Runtime.BaZic.Code.Parser;
using BaZic.Runtime.BaZic.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace BaZic.Runtime.Tests.BaZic.Runtime.Interpreter.Statement
{
    [TestClass]
    public class BreakpointInterpreterTest
    {
        [TestInitialize]
        public void Initialize()
        {
            TestUtilities.InitializeLogs();
        }

        [TestMethod]
        public async Task BreakpointInterpreter()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
    VARIABLE var1 = 0

    DO WHILE var1 < 10
        IF var1 = 5 THEN
            BREAKPOINT
        END IF

        var1 = var1 + 1
    LOOP

    RETURN var1
END FUNCTION";
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program);
            var t = interpreter.StartDebugAsync(true);

            await Task.Delay(2000);

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
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 == '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '0' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'Equality'.
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
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 == '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'Equality'.
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
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '2' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 == '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '2' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'Equality'.
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
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '3' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 == '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '3' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'Equality'.
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
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '4' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 == '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '4' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'Equality'.
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
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 == '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'Equality'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'BreakpointStatement'.
[Log] A Breakpoint has been intercepted.
[State] Pause
";


            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());

            interpreter.Resume();
            await Task.Delay(2000);

            expectedLogs = @"[State] Ready
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
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 == '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '0' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'Equality'.
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
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 == '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'Equality'.
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
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '2' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 == '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '2' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'Equality'.
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
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '3' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 == '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '3' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'Equality'.
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
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '4' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 == '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '4' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'Equality'.
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
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 == '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'Equality'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Registering labels.
[Log] Executing a statement of type 'BreakpointStatement'.
[Log] A Breakpoint has been intercepted.
[State] Pause
[State] Running
[Log] End of the execution of the condition 'var1 == '5' (type:System.Int32)'.
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
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 == '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '6' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'Equality'.
[Log] The expression returned the value 'False' (System.Boolean).
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
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 == '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '7' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'Equality'.
[Log] The expression returned the value 'False' (System.Boolean).
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
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 == '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '8' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'Equality'.
[Log] The expression returned the value 'False' (System.Boolean).
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
[Log] Executing a statement of type 'ConditionStatement'.
[Log] Executing the condition 'var1 == '5' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '9' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Doing an operation 'Equality'.
[Log] The expression returned the value 'False' (System.Boolean).
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
[State] Idle
";

            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());
        }
    }
}
