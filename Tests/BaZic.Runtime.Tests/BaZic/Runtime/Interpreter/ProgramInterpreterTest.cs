using System.Threading.Tasks;
using BaZic.Runtime.BaZic.Code.Parser;
using BaZic.Runtime.BaZic.Runtime;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
