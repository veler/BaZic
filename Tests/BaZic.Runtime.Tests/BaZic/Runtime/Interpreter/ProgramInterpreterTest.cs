﻿using System.Threading.Tasks;
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
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, true).Program);
            await interpreter.StartDebugAsync(true);

            Assert.IsInstanceOfType(interpreter.Error.Exception, typeof(MissingEntryPointMethodException));

            inputCode =
@"EXTERN FUNCTION Main(args[])
END FUNCTION

EXTERN FUNCTION Main(args[])
END FUNCTION";
            interpreter = new BaZicInterpreter(parser.Parse(inputCode, true).Program);
            await interpreter.StartDebugAsync(true);

            Assert.IsInstanceOfType(interpreter.Error.Exception, typeof(SeveralEntryPointMethodException));
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
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program);

            var result = await interpreter.InvokeMethod(true, "Method1", true, 123);

            Assert.AreEqual(123, result);
            Assert.AreEqual(BaZicInterpreterState.Idle, interpreter.State);
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
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program);

            var result = await interpreter.InvokeMethod(true, "Method1", true, 123);

            Assert.AreEqual(null, result);
            Assert.AreEqual("Unable to find a method called 'Method1'.", interpreter.Error.Exception.Message);
            Assert.AreEqual(BaZicInterpreterState.StoppedWithError, interpreter.State);
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
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program);

            var t = interpreter.StartDebugAsync(true);
            var result = await interpreter.InvokeMethod(true, "Method1", true, 123);

            Assert.AreEqual(123, result);
            Assert.AreEqual(BaZicInterpreterState.Idle, interpreter.State);

            await interpreter.Stop();

            Assert.AreEqual(BaZicInterpreterState.Stopped, interpreter.State);
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
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program);

            var tempFile = Path.Combine(Path.GetTempPath(), "BaZic_Bin", Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + ".exe");
            var errors = await interpreter.Build();

            Assert.IsNull(errors);

            var result = await interpreter.InvokeMethod(true, "Method1", true, 123);

            Assert.AreEqual(123, result);
            Assert.AreEqual(BaZicInterpreterState.Idle, interpreter.State);
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
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program);

            var tempFile = Path.Combine(Path.GetTempPath(), "BaZic_Bin", Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + ".exe");
            var errors = await interpreter.Build();

            Assert.IsNull(errors);

            var result = await interpreter.InvokeMethod(true, "Method1", true, 123);

            Assert.AreEqual(null, result);
            Assert.AreEqual("Unexpected and unmanaged error has been detected : Unable to find a method called 'Method1'.", interpreter.Error.Exception.Message);
            Assert.AreEqual(BaZicInterpreterState.StoppedWithError, interpreter.State);
        }

        [TestMethod]
        public async Task ProgramInterpreterInvokeExternMethod6()
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
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program);
            
            var errors = await interpreter.Build();

            Assert.IsNull(errors);

            var t = interpreter.StartReleaseAsync(true);
            var result = await interpreter.InvokeMethod(true, "Method1", true, 123);

            Assert.AreEqual(123, result);
            Assert.AreEqual(BaZicInterpreterState.Idle, interpreter.State);

            await interpreter.Stop();

            Assert.AreEqual(BaZicInterpreterState.Stopped, interpreter.State);
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
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, false).Program);

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
}
