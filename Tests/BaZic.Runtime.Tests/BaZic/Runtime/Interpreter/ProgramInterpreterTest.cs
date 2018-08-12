using System.Threading.Tasks;
using BaZic.Runtime.BaZic.Code.Parser;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, true).Program);

            var result = await interpreter.InvokeMethod(true, "Method1", true, new PrimitiveExpression(123));

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
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, true).Program);

            var result = await interpreter.InvokeMethod(true, "Method1", true, new PrimitiveExpression(123));

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
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, true).Program);

            var t = interpreter.StartDebugAsync(true);
            var result = await interpreter.InvokeMethod(true, "Method1", true, new PrimitiveExpression(123));

            Assert.AreEqual(123, result);
            Assert.AreEqual(BaZicInterpreterState.Idle, interpreter.State);

            await interpreter.Stop();

            Assert.AreEqual(BaZicInterpreterState.Stopped, interpreter.State);
        }
    }
}
