using BaZic.Runtime.BaZic.Code.Parser;
using BaZic.Runtime.BaZic.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

namespace BaZic.Runtime.Tests.BaZic.Runtime.Interpreter.Statement
{
    [TestClass]
    public class IterationInterpreterTest
    {
        [TestInitialize]
        public void Initialize()
        {
            TestUtilities.InitializeLogs();
        }

        [TestMethod]
        public async Task IterationInterpreter()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
    VARIABLE var1 = 0

    DO WHILE var1 < 10
        var1 = var1 + 1
    LOOP

    RETURN var1
END FUNCTION";
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program);
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
            await TestUtilities.TestAllRunningMode("10", inputCode);
        }

        [TestMethod]
        public async Task IterationInterpreterInfiniteLoop()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
    VARIABLE file = NEW System.IO.FileStream(""LocalWebPage.html"", System.IO.FileMode.OpenOrCreate)
    DO
    LOOP WHILE True

    RETURN 1
END FUNCTION";
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program);
            var t = interpreter.StartDebugAsync(true);

            await Task.Delay(3000);

            try
            {
                File.ReadAllText("LocalWebPage.html"); // Must crash.
                Assert.Fail();
            }
            catch (IOException)
            {
            }
            catch
            {
                Assert.Fail();
            }

            await interpreter.Stop();

            var fileContent = File.ReadAllText("LocalWebPage.html");
            Assert.IsTrue(interpreter.GetStateChangedHistoryString().Contains("[Log] The user requests to stop the interpreter as soon as possible."));
            Assert.IsTrue(interpreter.State == BaZicInterpreterState.Stopped);
            Assert.IsFalse(string.IsNullOrWhiteSpace(fileContent));
        }

        [TestMethod]
        public async Task IterationInterpreterInfiniteLoopReleaseMode()
        {
            var parser = new BaZicParser();
            var inputCode =
@"EXTERN FUNCTION Main(args[])
    VARIABLE file = NEW System.IO.FileStream(""LocalWebPage.html"", System.IO.FileMode.OpenOrCreate)
    DO
        # The file LocalWebPage.html must be protected in write as it's a busy resource.
    LOOP WHILE True

    RETURN 1
END FUNCTION";
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program);
            var t = interpreter.StartReleaseAsync(true);

            await Task.Delay(10000);

            try
            {
                File.ReadAllText("LocalWebPage.html"); // Must crash.
                Assert.Fail();
            }
            catch (IOException)
            {
            }
            catch
            {
                Assert.Fail();
            }

            await interpreter.Stop();

            var fileContent = File.ReadAllText("LocalWebPage.html");
            Assert.IsTrue(interpreter.State == BaZicInterpreterState.Stopped);
            Assert.IsFalse(string.IsNullOrWhiteSpace(fileContent));
        }

        [TestMethod]
        public async Task IterationInterpreterInlined()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
    VARIABLE var1 = 0

    DO WHILE var1 < 10
        var1 = var1 + 1
        VARIABLE x = 1
    LOOP

    RETURN var1
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
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Variable 'x' declared. Default value : 1 (System.Int32)
[Log] Executing a statement of type 'GoToLabelStatement'.
[Log] Jumping to the label '_A'.
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'True' (System.Boolean).
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
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Variable 'x' value set to : 1 (System.Int32)
[Log] Executing a statement of type 'GoToLabelStatement'.
[Log] Jumping to the label '_A'.
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '2' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'True' (System.Boolean).
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
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Variable 'x' value set to : 1 (System.Int32)
[Log] Executing a statement of type 'GoToLabelStatement'.
[Log] Jumping to the label '_A'.
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '3' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'True' (System.Boolean).
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
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Variable 'x' value set to : 1 (System.Int32)
[Log] Executing a statement of type 'GoToLabelStatement'.
[Log] Jumping to the label '_A'.
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '4' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'True' (System.Boolean).
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
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Variable 'x' value set to : 1 (System.Int32)
[Log] Executing a statement of type 'GoToLabelStatement'.
[Log] Jumping to the label '_A'.
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '5' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '10' (System.Int32).
[Log] Doing an operation 'LessThan'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] The expression returned the value 'False' (System.Boolean).
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
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Variable 'x' value set to : 1 (System.Int32)
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
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Variable 'x' value set to : 1 (System.Int32)
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
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Variable 'x' value set to : 1 (System.Int32)
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
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Variable 'x' value set to : 1 (System.Int32)
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
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Variable 'x' value set to : 1 (System.Int32)
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
[State] Idle
";

            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());
            await TestUtilities.TestAllRunningMode("10", inputCode);
        }
    }
}
