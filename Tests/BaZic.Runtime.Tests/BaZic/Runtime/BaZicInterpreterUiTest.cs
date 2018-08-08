using BaZic.Runtime.BaZic.Code;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Code.Parser;
using BaZic.Runtime.BaZic.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace BaZic.Runtime.Tests.BaZic.Runtime
{
    [TestClass]
    public class BaZicInterpreterUiTest
    {
        [TestInitialize]
        public void Initialize()
        {
            TestUtilities.InitializeLogs();
        }

        [TestMethod]
        public async Task BaZicInterpreterWithUiProgram()
        {
            var parser = new BaZicParser();

            var inputCode =
@"
BIND Button1_Content

EXTERN FUNCTION Main(args[])
END FUNCTION

EVENT FUNCTION Window1_Closed()
    RETURN ""Result of Window.Close""
END FUNCTION

EVENT FUNCTION Window1_Loaded()
    VARIABLE var1 = Button1_Content
    IF var1 = ""Hello"" THEN
        Button1_Content = ""Hello World""
        var1 = Button1_Content
        IF var1 = ""Hello World"" THEN
            RETURN TRUE
        END IF
    END IF
END FUNCTION

# The XAML will be provided separatly";

            var xamlCode = @"
<Window xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Name=""Window1"" Opacity=""0"">
    <StackPanel>
        <Button Name=""Button1"" Content=""Hello""/>
    </StackPanel>
</Window>";

            var bazicProgram = (BaZicUiProgram)parser.Parse(inputCode, xamlCode, true).Program;


            var interpreter = new BaZicInterpreter(bazicProgram);
            var t = interpreter.StartDebugAsync(true);

            await Task.Delay(5000);

            await interpreter.Stop();

            var expect = @"[State] Ready
[State] Preparing
[Log] Reference assembly 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll' loaded in the application domain.
[Log] Reference assembly 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.dll' loaded in the application domain.
[Log] Reference assembly 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Core.dll' loaded in the application domain.
[Log] Reference assembly 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Runtime.dll' loaded in the application domain.
[Log] Reference assembly 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\Microsoft.CSharp.dll' loaded in the application domain.
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
[Log] End of the execution of the method 'Main'. Returned value :  ({Null})
[Log] Loading user interface.
[Log] Registering events.
[Log] Declaring bindings.
[Log] Variable 'Button1_Content' declared. Default value : Hello (Button1.Content : )
[Log] Showing user interface.
[Log] An event has been raised from an interaction with the user interface.
[Log] Preparing to invoke the method 'Window1_Loaded'.
[Log] Executing the argument values of the method.
[Log] Invoking the synchronous method 'Window1_Loaded'.
[Log] Registering labels.
[Log] Executing a statement of type 'VariableDeclaration'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value 'Hello' (System.String).
[Log] Variable 'var1' declared. Default value : Hello (System.String)
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value 'Hello' (System.String).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'Hello' (System.String).
[Log] Doing an operation 'Equality'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'Button1_Content' to ''Hello World' (type:System.String)'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'Hello World' (System.String).
[Log] Variable 'Button1_Content' value set to : Hello World (Button1.Content : System.String)
[Log] 'Button1_Content' is now equal to 'Hello World'(type:System.String)
[Log] Executing a statement of type 'AssignStatement'.
[Log] Assign 'var1' to 'Button1_Content'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value 'Hello World' (System.String).
[Log] Variable 'var1' value set to : Hello World (System.String)
[Log] 'var1' is now equal to 'Hello World'(type:System.String)
[Log] Executing a statement of type 'LabelConditionStatement'.
[Log] Executing an expression of type 'NotOperatorExpression'.
[Log] Executing an expression of type 'BinaryOperatorExpression'.
[Log] Executing an expression of type 'VariableReferenceExpression'.
[Log] The expression returned the value 'Hello World' (System.String).
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'Hello World' (System.String).
[Log] Doing an operation 'Equality'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] The expression returned the value 'False' (System.Boolean).
[Log] Executing a statement of type 'ReturnStatement'.
[Log] Executing an expression of type 'PrimitiveExpression'.
[Log] The expression returned the value 'True' (System.Boolean).
[Log] Return : True (System.Boolean)
[Log] A Return statement or Break statement or Exception has been detected or thrown. Exiting the current block of statements.
[Log] End of the execution of the method 'Window1_Loaded'. Returned value : True (System.Boolean)
[Log] The user requests to stop the interpreter as soon as possible.
[State] Stopped
";

            Assert.AreEqual(expect, interpreter.GetStateChangedHistoryString());
            Assert.AreEqual(null, interpreter.ProgramResult); // Null because the program has been forced to stop.
        }

        [TestMethod]
        public void BaZicInterpreterWithUiProgramBadXaml()
        {
            var parser = new BaZicParser();

            var inputCode =
@"
BIND Button1_Text

EXTERN FUNCTION Main(args[])
END FUNCTION";

            var xamlCode = @"
<Window xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
    Blabla<FooBar>> Bad XAML
</Window>";

            var result = parser.Parse(inputCode, xamlCode);
            Assert.AreEqual(4, result.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)result.Issues.InnerExceptions.First();
            Assert.AreEqual("Parsing error in the XAML Code : ''Content' property has already been set on 'Window'.' Line number '3' and line position '12'.", exception.Message);
        }

        [TestMethod]
        public void BaZicInterpreterWithUiProgramBadEvent()
        {
            var parser = new BaZicParser();

            var inputCode =
            @"
EVENT FUNCTION Window_BadEventName()
END FUNCTION";

            var xamlCode = @"
<Window xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Name=""Window"">
    <Grid>
        <Button Name=""Button1""/>
    </Grid>
</Window>";

            var result = parser.Parse(inputCode, xamlCode);
            var exception = (BaZicParserException)result.Issues.InnerExceptions.Single();
            Assert.AreEqual("The event function 'Window_BadEventName' is invalid because the event 'Window.BadEventName' does not exist or is not accessible.", exception.Message);
        }

        [TestMethod]
        public async Task BaZicInterpreterWithUiProgramBadBindingUse()
        {
            var parser = new BaZicParser();

            var inputCode =
            @"
BIND Button1_Content

EXTERN FUNCTION Main(args[])
    Button1_Content = ""Hello""
END FUNCTION";

            var xamlCode = @"
<Window xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Name=""Window"">
    <Grid>
        <Button Name=""Button1""/>
    </Grid>
</Window>";

            var bazicProgram = (BaZicUiProgram)parser.Parse(inputCode, xamlCode, true).Program;

            var interpreter = new BaZicInterpreter(bazicProgram);
            await interpreter.StartDebugAsync(true);

            var exception = interpreter.Error.Exception;
            Assert.AreEqual("The variable 'Button1_Content' does not exist or is not accessible.", exception.Message);
        }
    }
}
