using BaZic.Runtime.BaZic.Code.Parser;
using BaZic.Runtime.BaZic.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace BaZic.Runtime.Tests.BaZic.Runtime.Interpreter.Statement
{
    [TestClass]
    public class AssignInterpreterTest
    {
        [TestInitialize]
        public void Initialize()
        {
            TestUtilities.InitializeLogs();
        }

        [TestMethod]
        public async Task AssignInterpreter()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
    VARIABLE var1
    VARIABLE var2[] = NEW [1, 2, 3]

    var1 = (1 = 2)
    var1 = NEW System.Text.StringBuilder()
    var1.Capacity = 256
    var2[0] = 1024
    var1 = var2[0] # Should be 1024.

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
[Log] The expression returned the value 'BaZic.StandaloneRuntime.ObservableDictionary' (BaZic.StandaloneRuntime.ObservableDictionary (length: 0)).
[Log] Invoking the synchronous method 'Main'.
[Log] Variable 'args' declared. Default value : {Null}
[Log] Variable 'args' value set to : BaZic.StandaloneRuntime.ObservableDictionary (BaZic.StandaloneRuntime.ObservableDictionary (length: 0))
[Log] Registering labels.
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Variable 'var1' declared. Default value : {Null}
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'ArrayCreationExpression'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '2' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '3' (System.Int32).
[Log] The expression returned the value 'BaZic.StandaloneRuntime.ObservableDictionary' (BaZic.StandaloneRuntime.ObservableDictionary (length: 3)).
[Log] Variable 'var2' declared. Default value : BaZic.StandaloneRuntime.ObservableDictionary (BaZic.StandaloneRuntime.ObservableDictionary (length: 3))
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to ''1' (type:System.Int32) == '2' (type:System.Int32)'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1' (System.Int32).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '2' (System.Int32).
[Log] Doing an operation 'Equality'.
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Variable 'var1' value set to : False (System.Boolean)
[Log] 'var1' is now equal to 'False'(type:System.Boolean)
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to 'new System.Text.StringBuilder()'.
[Log] Executing an expression of type 'InstantiateExpression'.
[Log] Executing an expression of type 'ClassReferenceExpression'.
[Log] The expression returned the value 'System.Text.StringBuilder' (System.RuntimeType).
[Log] Creating a new instance of 'System.Text.StringBuilder'
[Log] Executing the argument values of the method.
[Log] The expression returned the value '' (System.Text.StringBuilder).
[Log] Variable 'var1' value set to :  (System.Text.StringBuilder)
[Log] 'var1' is now equal to ''(type:System.Text.StringBuilder)
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1.Capacity' to ''256' (type:System.Int32)'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '256' (System.Int32).
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '' (System.Text.StringBuilder).
[Log] 'var1.Capacity' is now equal to '256'(type:System.Int32)
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var2['0' (type:System.Int32)]' to ''1024' (type:System.Int32)'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '1024' (System.Int32).
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value 'BaZic.StandaloneRuntime.ObservableDictionary' (BaZic.StandaloneRuntime.ObservableDictionary (length: 3)).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '0' (System.Int32).
[Log] 'var2['0' (type:System.Int32)]' is now equal to '1024'(type:System.Int32)
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to 'var2['0' (type:System.Int32)]'.
[Log] Executing an expression of type 'ArrayIndexerExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value 'BaZic.StandaloneRuntime.ObservableDictionary' (BaZic.StandaloneRuntime.ObservableDictionary (length: 3)).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value '0' (System.Int32).
[Log] The expression returned the value '1024' (System.Int32).
[Log] Variable 'var1' value set to : 1024 (System.Int32)
[Log] 'var1' is now equal to '1024'(type:System.Int32)
[Log] Executing a statement of type 'ReturnStatement'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value '1024' (System.Int32).
[Log] Return : 1024 (System.Int32)
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] End of the execution of the method 'Main'. Returned value : 1024 (System.Int32)
[State] Idle
";


            Assert.AreEqual(expectedLogs, interpreter.GetStateChangedHistoryString());
            await TestUtilities.TestAllRunningMode("1024", inputCode);





            parser = new BaZicParser();

            inputCode =
@"EXTERN FUNCTION Main(args[])
    NEW System.Text.UTF32Encoding().BodyName = 2
END FUNCTION";
            interpreter = new BaZicInterpreter(parser.Parse(inputCode, true).Program);
            await interpreter.StartDebugAsync(true);
            expectedLogs = @"[Error] Unexpected and unmanaged error has been detected : The property 'BodyName' does not have an accessible setter.";

            Assert.IsTrue(interpreter.GetStateChangedHistoryString().Contains(expectedLogs));





            parser = new BaZicParser();

            inputCode =
@"EXTERN FUNCTION Main(args[])
    NEW System.Text.StringBuilder().Capacity = True # Should fail because of incompatible type.
END FUNCTION";
            interpreter = new BaZicInterpreter(parser.Parse(inputCode, true).Program);
            await interpreter.StartDebugAsync(true);
            expectedLogs = @"[Error] Unexpected and unmanaged error has been detected : Specified cast is not valid.";

            Assert.IsTrue(interpreter.GetStateChangedHistoryString().Contains(expectedLogs));
        }
    }
}
