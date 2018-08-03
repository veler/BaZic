using System.Threading.Tasks;
using BaZic.Runtime.BaZic.Code.Parser;
using BaZic.Runtime.BaZic.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Runtime.Interpreter.Expression
{
    [TestClass]
    public class BinaryOperatorInterpreterTest
    {
        [TestInitialize]
        public void Initialize()
        {
            TestUtilities.InitializeLogs();
        }

        [TestMethod]
        public async Task BinaryOperatorInterpreterOrders()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Main(args[])
    VARIABLE var1 = 2 * 3 + 1 # Should be 7
    VARIABLE var2 = 1 + 3 * 2 # Should be 7
    VARIABLE var3 = var1 = var2 # Should be True
    VARIABLE var4 = 2 * (3 + 1) # Should be 8
END FUNCTION";
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, true).Program);
            await interpreter.StartDebugAsync(true);

            var result = interpreter.GetStateChangedHistoryString();
            Assert.IsTrue(result.Contains("[Log] Variable 'var1' declared. Default value : 7 (System.Int32)"));
            Assert.IsTrue(result.Contains("[Log] Variable 'var2' declared. Default value : 7 (System.Int32)"));
            Assert.IsTrue(result.Contains("[Log] Variable 'var3' declared. Default value : True (System.Boolean)"));
            Assert.IsTrue(result.Contains("[Log] Variable 'var4' declared. Default value : 8 (System.Int32)"));
        }

        [TestMethod]
        public async Task BinaryOperatorInterpreterAddition()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Main(args[])
    VARIABLE var1 = 2 + 3 # Should be 5
    VARIABLE var2 = ""2"" + 3 # Should be 23
    VARIABLE var3 = 2 + 3.0 # Should be 5
    VARIABLE var4 = ""Hello"" + true # Should be HelloTrue
END FUNCTION";
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, true).Program);
            await interpreter.StartDebugAsync(true);
            var result = interpreter.GetStateChangedHistoryString();

            Assert.IsNull(interpreter.Error);
            Assert.IsTrue(result.Contains("[Log] Variable 'var1' declared. Default value : 5 (System.Int32)"));
            Assert.IsTrue(result.Contains("[Log] Variable 'var2' declared. Default value : 23 (System.String)"));
            Assert.IsTrue(result.Contains("[Log] Variable 'var3' declared. Default value : 5 (System.Double)"));
            Assert.IsTrue(result.Contains("[Log] Variable 'var4' declared. Default value : HelloTrue (System.String)"));




            inputCode =
@"FUNCTION Main(args[])
    VARIABLE var4 = true + false # Should fail
END FUNCTION";
            interpreter = new BaZicInterpreter(parser.Parse(inputCode, true).Program);
            await interpreter.StartDebugAsync(true);
            result = interpreter.GetStateChangedHistoryString();

            Assert.IsTrue(result.Contains("[Error] Unexpected and unmanaged error has been detected : Operator '+' cannot be applied to operands of type 'bool' and 'bool'"));
        }

        [TestMethod]
        public async Task BinaryOperatorInterpreterDivision()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Main(args[])
    VARIABLE var1 = 2 / 0 # Should fail
END FUNCTION";
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, true).Program);
            await interpreter.StartDebugAsync(true);
            var result = interpreter.GetStateChangedHistoryString();

            Assert.IsTrue(result.Contains("[Error] Attempted to divide by zero."));
        }

        [TestMethod]
        public async Task BinaryOperatorInterpreterLogic()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Main(args[])
    VARIABLE var1 = False OR (True AND False) # Should be False
    VARIABLE var2 = False OR (True AND True AND True) # Should be True
    VARIABLE var3 = False OR (True OR False) # Should be True
    VARIABLE var4 = True AND (True OR False) # Should be True
    VARIABLE var5 = True AND (False OR False) # Should be True
END FUNCTION";
            var interpreter = new BaZicInterpreter(parser.Parse(inputCode, true).Program);
            await interpreter.StartDebugAsync(true);
            var result = interpreter.GetStateChangedHistoryString();

            Assert.IsNull(interpreter.Error);
            Assert.IsTrue(result.Contains("[Log] Variable 'var1' declared. Default value : False (System.Boolean)"));
            Assert.IsTrue(result.Contains("[Log] Variable 'var2' declared. Default value : True (System.Boolean)"));
            Assert.IsTrue(result.Contains("[Log] Variable 'var3' declared. Default value : True (System.Boolean)"));
            Assert.IsTrue(result.Contains("[Log] Variable 'var4' declared. Default value : True (System.Boolean)"));
            Assert.IsTrue(result.Contains("[Log] Variable 'var5' declared. Default value : False (System.Boolean)"));




            inputCode =
@"FUNCTION Main(args[])
    VARIABLE var4 = ""Hello"" AND True # Should fail
END FUNCTION";
            interpreter = new BaZicInterpreter(parser.Parse(inputCode, true).Program);
            await interpreter.StartDebugAsync(true);
            result = interpreter.GetStateChangedHistoryString();

            Assert.IsTrue(result.Contains("[Error] Unexpected and unmanaged error has been detected : Cannot implicitly convert type 'string' to 'bool'"));
        }
    }
}
