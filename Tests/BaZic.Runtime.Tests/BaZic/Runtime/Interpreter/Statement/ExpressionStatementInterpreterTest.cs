using BaZic.Runtime.BaZic.Code.Parser;
using BaZic.Runtime.BaZic.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BaZic.Runtime.Tests.BaZic.Runtime.Interpreter.Statement
{
    [TestClass]
    public class ExpressionStatementInterpreterTest
    {
        [TestInitialize]
        public void Initialize()
        {
            TestUtilities.InitializeLogs();
        }

        [TestMethod]
        public async Task ExpressionStatementInterpreter()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
    AWAIT System.Threading.Tasks.Task.Delay(System.TimeSpan.FromMilliseconds(500.0))
END FUNCTION";
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, true).Program);

            var watch = new Stopwatch();
            watch.Start();
            await interpreter.StartDebugAsync(true);
            watch.Stop();

            Assert.IsTrue(watch.ElapsedMilliseconds > 505);
        }
    }
}
