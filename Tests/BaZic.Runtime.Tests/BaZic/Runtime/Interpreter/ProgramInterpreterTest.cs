using System.Threading.Tasks;
using BaZic.Runtime.BaZic.Code.Parser;
using BaZic.Runtime.BaZic.Runtime;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace BaZic.Runtime.Tests.BaZic.Runtime.Interpreter
{
    [TestClass]
    public class ProgramInterpreterTest
    {
        [TestInitialize]
        public void Initialize()
        {
            TestUtilities.InitializeLogs();
        }

        [TestMethod]
        public async Task ProgramInterpreterTestEntryPointMethod()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
END FUNCTION";
            using (var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program))
            {
                await interpreter.StartDebugAsync(true);

                Assert.IsInstanceOfType(interpreter.Error.Exception, typeof(MissingEntryPointMethodException));
            }

            inputCode =
@"EXTERN FUNCTION Main(args[])
END FUNCTION

EXTERN FUNCTION Main(args[])
END FUNCTION";
            using (var interpreter = new BaZicInterpreter(parser.Parse(inputCode, true).Program))
            {
                await interpreter.StartDebugAsync(true);

                Assert.IsInstanceOfType(interpreter.Error.Exception, typeof(SeveralEntryPointMethodException));
            }
        }

        [TestMethod]
        public async Task ProgramInterpreterInvokeExternMethod1()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
END FUNCTION

EXTERN FUNCTION Method1(arg)
    RETURN arg
END FUNCTION";
            using (var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program))
            {
                var result = await interpreter.InvokeMethod(true, "Method1", true, 123);

                Assert.AreEqual(123, result);
                Assert.AreEqual(BaZicInterpreterState.Idle, interpreter.State);
            }
        }

        [TestMethod]
        public async Task ProgramInterpreterInvokeExternMethod2()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
END FUNCTION

FUNCTION Method1(arg)
    RETURN arg
END FUNCTION";
            using (var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program))
            {
                var result = await interpreter.InvokeMethod(true, "Method1", true, 123);

                Assert.AreEqual(null, result);
                Assert.AreEqual("Unable to find a method called 'Method1'.", interpreter.Error.Exception.Message);
                Assert.AreEqual(BaZicInterpreterState.StoppedWithError, interpreter.State);
            }
        }

        [TestMethod]
        public async Task ProgramInterpreterInvokeExternMethod3()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
    AWAIT MethodAsync(345, 5.0)
END FUNCTION

EXTERN FUNCTION Method1(arg)
    RETURN arg
END FUNCTION

ASYNC FUNCTION MethodAsync(value, timeToWait)
    VARIABLE var1 = AWAIT System.Threading.Tasks.Task.Delay(System.TimeSpan.FromSeconds(timeToWait))
    RETURN value
END FUNCTION";
            using (var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program))
            {
                var t = interpreter.StartDebugAsync(true);
                var result = await interpreter.InvokeMethod(true, "Method1", true, 123);

                Assert.AreEqual(123, result);

                // Idle is expected because InvokeMethod waits that the main
                // thread (used by Main()) is free before running. So the async
                // function is complete before reaching this assert.
                Assert.AreEqual(BaZicInterpreterState.Idle, interpreter.State);

                await interpreter.Stop();

                Assert.AreEqual(BaZicInterpreterState.Stopped, interpreter.State);
            }
        }

        [TestMethod]
        public async Task ProgramInterpreterInvokeExternMethod4()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
END FUNCTION

EXTERN FUNCTION Method1(arg)
    RETURN arg
END FUNCTION";
            using (var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program))
            {
                var tempFile = Path.Combine(Path.GetTempPath(), "BaZic_Bin", Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + ".exe");
                var errors = await interpreter.Build();

                Assert.IsNull(errors);

                var result = await interpreter.InvokeMethod(true, "Method1", true, 123);

                Assert.AreEqual(123, result);
                Assert.AreEqual(BaZicInterpreterState.Idle, interpreter.State);
            }
        }

        [TestMethod]
        public async Task ProgramInterpreterInvokeExternMethod5()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
END FUNCTION

FUNCTION Method1(arg)
    RETURN arg
END FUNCTION";
            using (var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program))
            {
                var tempFile = Path.Combine(Path.GetTempPath(), "BaZic_Bin", Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + ".exe");
                var errors = await interpreter.Build();

                Assert.IsNull(errors);

                var result = await interpreter.InvokeMethod(true, "Method1", true, 123);

                Assert.AreEqual(null, result);
                Assert.AreEqual("Unexpected and unmanaged error has been detected : The method 'Method1' does not exist in the type 'BaZicProgramReleaseMode.Program'.", interpreter.Error.Exception.Message);
                Assert.AreEqual(BaZicInterpreterState.StoppedWithError, interpreter.State);
            }
        }

        [TestMethod]
        public async Task ProgramInterpreterInvokeExternMethod6()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
    AWAIT MethodAsync(345, 2.0)
END FUNCTION

EXTERN FUNCTION Method1(arg)
    RETURN arg
END FUNCTION

ASYNC FUNCTION MethodAsync(value, timeToWait)
    VARIABLE var1 = AWAIT System.Threading.Tasks.Task.Delay(System.TimeSpan.FromSeconds(timeToWait))
    RETURN value
END FUNCTION";

            using (var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program))
            {
                var errors = await interpreter.Build();

                Assert.IsNull(errors);

                var t = interpreter.StartReleaseAsync(true);
                var result = await interpreter.InvokeMethod(true, "Method1", true, 123);

                Assert.AreEqual(123, result);

                // Idle is expected because InvokeMethod waits that the main
                // thread (used by Main()) is free before running. So the async
                // function is complete before reaching this assert.
                Assert.AreEqual(BaZicInterpreterState.Idle, interpreter.State);

                await interpreter.Stop();

                Assert.AreEqual(BaZicInterpreterState.Stopped, interpreter.State);
            }

            using (var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program))
            {
                var t = interpreter.StartReleaseAsync(true);
                var result = await interpreter.InvokeMethod(true, "Method1", true, 123);

                Assert.AreEqual(123, result);

                // Idle is expected because InvokeMethod waits that the main
                // thread (used by Main()) is free before running. So the async
                // function is complete before reaching this assert.
                Assert.AreEqual(BaZicInterpreterState.Idle, interpreter.State);

                await interpreter.Stop();

                Assert.AreEqual(BaZicInterpreterState.Stopped, interpreter.State);
            }
        }

        [TestMethod]
        public async Task ProgramInterpreterInvokeExternMethod7()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
END FUNCTION

EXTERN ASYNC FUNCTION Method1(arg)
    VARIABLE var1 = AWAIT System.Threading.Tasks.Task.Delay(System.TimeSpan.FromSeconds(3.0))
    RETURN arg
END FUNCTION";
            using (var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program))
            {
                var errors = await interpreter.Build();

                Assert.IsNull(errors);

                var t = interpreter.StartReleaseAsync(true);
                var result = (Task)interpreter.InvokeMethod(true, "Method1", true, 123);

                Assert.AreEqual(TaskStatus.WaitingForActivation, result.Status);

                await Task.Delay(5000);

                Assert.AreEqual(TaskStatus.RanToCompletion, result.Status);
                Assert.AreEqual(123, ((dynamic)result).Result);
                Assert.AreEqual(BaZicInterpreterState.Idle, interpreter.State);

                await interpreter.Stop();

                Assert.AreEqual(BaZicInterpreterState.Stopped, interpreter.State);
            }
        }

        [TestMethod]
        public async Task ProgramInterpreterInvokeExternMethod8()
        {
            var inputCode =
@"
BIND Button1_Content

EXTERN FUNCTION Main(args[])
END FUNCTION

EVENT FUNCTION Window1_Closed()
    RETURN ""Result of Window.Close""
END FUNCTION

EXTERN FUNCTION Method1()
    VARIABLE var1 = Button1_Content
    IF var1 = ""Hello"" THEN
        Button1_Content = ""Hello World""
        var1 = Button1_Content
        IF var1 = ""Hello World"" THEN
            RETURN TRUE
        END IF
    END IF

    RETURN FALSE
END FUNCTION

# The XAML will be provided separatly";

            var xamlCode = @"
<Window xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Name=""Window1"">
    <StackPanel>
        <Button Name=""Button1"" Content=""Hello""/>
    </StackPanel>
</Window>";

            using (var interpreter = new BaZicInterpreter(inputCode, xamlCode))
            {
                var result = await interpreter.InvokeMethod(true, "Method1", true);

                Assert.IsNull(result);
                Assert.AreEqual("The variable 'Button1_Content' does not exist or is not accessible.", interpreter.Error.Exception.Message);

                var t = interpreter.StartDebugAsync(true);
                result = await interpreter.InvokeMethod(true, "Method1", true);

                Assert.AreEqual(true, result);
            }

            using (var interpreter = new BaZicInterpreter(inputCode, xamlCode))
            {
                var errors = await interpreter.Build();

                Assert.IsNull(errors);

                var result = await interpreter.InvokeMethod(true, "Method1", true);

                Assert.IsNull(result);
                Assert.AreEqual("Object reference not set to an instance of an object.", interpreter.Error.Exception.InnerException.Message);

                var t = interpreter.StartReleaseAsync(true);
                result = await interpreter.InvokeMethod(true, "Method1", true);

                Assert.AreEqual(true, result);
            }

            using (var interpreter = new BaZicInterpreter(inputCode, xamlCode))
            {
                var t = interpreter.StartReleaseAsync(true);
                var result = await interpreter.InvokeMethod(true, "Method1", true);

                Assert.AreEqual(true, result);
            }
        }

        [TestMethod]
        public async Task ProgramInterpreterInvokeExternMethod9()
        {
            var parser = new BaZicParser();

            var inputCode =
@"
VARIABLE globVar = 1

EXTERN FUNCTION Main(args[])
END FUNCTION

EXTERN FUNCTION Method1()
    globVar = globVar + 1
    RETURN globVar
END FUNCTION";

            using (var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program))
            {
                var t = interpreter.StartDebugAsync(true);
                var result = await interpreter.InvokeMethod(true, "Method1", true);

                Assert.AreEqual(2, result);

                result = await interpreter.InvokeMethod(true, "Method1", true);

                Assert.AreEqual(3, result);
            }

            using (var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program))
            {
                var result = await interpreter.InvokeMethod(true, "Method1", true);

                Assert.AreEqual(2, result);

                result = await interpreter.InvokeMethod(true, "Method1", true);

                Assert.AreEqual(3, result);
            }

            using (var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program))
            {
                var errors = await interpreter.Build();

                Assert.IsNull(errors);

                var t = interpreter.StartReleaseAsync(true);
                var result = await interpreter.InvokeMethod(true, "Method1", true);

                Assert.AreEqual(2, result);

                result = await interpreter.InvokeMethod(true, "Method1", true);

                Assert.AreEqual(3, result);
            }

            using (var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program))
            {
                var errors = await interpreter.Build();

                Assert.IsNull(errors);

                var result = await interpreter.InvokeMethod(true, "Method1", true);

                Assert.AreEqual(2, result);

                result = await interpreter.InvokeMethod(true, "Method1", true);

                Assert.AreEqual(3, result);
            }
        }

        [TestMethod]
        public async Task ProgramInterpreterInvokeExternMethod10()
        {
            var parser = new BaZicParser();

            var inputCode =
@"
VARIABLE globVar = 1

EXTERN FUNCTION Main(args[])
END FUNCTION

EXTERN FUNCTION Method1()
    DO WHILE TRUE
    LOOP
END FUNCTION";

            using (var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program))
            {
                var t = interpreter.StartDebugAsync(true);
                t = interpreter.InvokeMethod(true, "Method1", true);

                await Task.Delay(3000);

                Assert.AreEqual(BaZicInterpreterState.Running, interpreter.State);

                await interpreter.Stop();

                Assert.AreEqual(BaZicInterpreterState.Stopped, interpreter.State);
            }

            using (var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program))
            {
                var t = interpreter.StartReleaseAsync(true);
                t = interpreter.InvokeMethod(true, "Method1", true);

                await Task.Delay(10000);

                Assert.AreEqual(BaZicInterpreterState.Running, interpreter.State);

                await interpreter.Stop();

                Assert.AreEqual(BaZicInterpreterState.Stopped, interpreter.State);
            }
        }

        [TestMethod]
        public async Task ProgramInterpreterInvokeExternMethod11()
        {
            var parser = new BaZicParser();

            var inputCode =
@"
VARIABLE globVar = 1

EXTERN FUNCTION Main(args[])
    DO WHILE TRUE
    LOOP
END FUNCTION

EXTERN FUNCTION Method1()
    RETURN TRUE
END FUNCTION";


            using (var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program))
            {
                var t = interpreter.StartDebugAsync(true);
                t = interpreter.InvokeMethod(true, "Method1", true);

                await Task.Delay(3000);

                Assert.AreEqual(BaZicInterpreterState.Running, interpreter.State);

                await interpreter.Stop();

                Assert.AreEqual(BaZicInterpreterState.Stopped, interpreter.State);
            }

            using (var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program))
            {
                var t = interpreter.StartReleaseAsync(true);
                t = interpreter.InvokeMethod(true, "Method1", true);

                await Task.Delay(10000);

                Assert.AreEqual(BaZicInterpreterState.Running, interpreter.State);

                await interpreter.Stop();

                Assert.AreEqual(BaZicInterpreterState.Stopped, interpreter.State);
            }
        }

        [TestMethod]
        public async Task ProgramInterpreterInvokeExternMethod12()
        {
            var parser = new BaZicParser();

            var inputCode =
@"
VARIABLE globVar = 1

EXTERN FUNCTION Main(args[])
END FUNCTION

EXTERN FUNCTION Method1()
    DO WHILE TRUE
    LOOP
END FUNCTION";

            var xamlCode = @"
<Window xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Name=""Window1"">
    <StackPanel>
        <Button Name=""Button1"" Content=""Hello""/>
    </StackPanel>
</Window>";

            using (var interpreter = new BaZicInterpreter(inputCode, xamlCode))
            {
                var t = interpreter.StartDebugAsync(true);
                t = interpreter.InvokeMethod(true, "Method1", true);

                await Task.Delay(3000);

                Assert.AreEqual(BaZicInterpreterState.Running, interpreter.State);

                await interpreter.Stop();

                Assert.AreEqual(BaZicInterpreterState.Stopped, interpreter.State);
            }

            using (var interpreter = new BaZicInterpreter(inputCode, xamlCode))
            {
                var errors = await interpreter.Build();

                Assert.IsNull(errors);

                var t = interpreter.StartReleaseAsync(true);
                t = interpreter.InvokeMethod(true, "Method1", true);

                await Task.Delay(10000);

                Assert.AreEqual(BaZicInterpreterState.Running, interpreter.State);

                await interpreter.Stop();

                Assert.AreEqual(BaZicInterpreterState.Stopped, interpreter.State);
            }
        }

        [TestMethod]
        public async Task ProgramInterpreterInvokeExternMethod13()
        {
            var parser = new BaZicParser();

            var inputCode =
@"
VARIABLE globVar = 1

EXTERN FUNCTION Main(args[])
    DO WHILE TRUE
    LOOP
END FUNCTION

EXTERN FUNCTION Method1()
    RETURN TRUE
END FUNCTION";

            var xamlCode = @"
<Window xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Name=""Window1"">
    <StackPanel>
        <Button Name=""Button1"" Content=""Hello""/>
    </StackPanel>
</Window>";

            using (var interpreter = new BaZicInterpreter(inputCode, xamlCode))
            {
                var t = interpreter.StartDebugAsync(true);
                t = interpreter.InvokeMethod(true, "Method1", true);

                await Task.Delay(3000);

                Assert.AreEqual(BaZicInterpreterState.Running, interpreter.State);

                await interpreter.Stop();

                Assert.AreEqual(BaZicInterpreterState.Stopped, interpreter.State);
            }

            using (var interpreter = new BaZicInterpreter(inputCode, xamlCode))
            {
                var t = interpreter.StartReleaseAsync(true);
                t = interpreter.InvokeMethod(true, "Method1", true);

                await Task.Delay(10000);

                Assert.AreEqual(BaZicInterpreterState.Running, interpreter.State);

                await interpreter.Stop();

                Assert.AreEqual(BaZicInterpreterState.Stopped, interpreter.State);
            }
        }

        [TestMethod]
        public async Task ProgramInterpreterInvokeExternMethod14()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
END FUNCTION

EXTERN ASYNC FUNCTION Method1(arg)
    RETURN arg
END FUNCTION";
            using (var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program))
            {
                var result = await interpreter.InvokeMethod(true, "Method1", true, 123);

                Assert.AreEqual(123, result);
                Assert.AreEqual(BaZicInterpreterState.Idle, interpreter.State);
            }
        }
    }
}
