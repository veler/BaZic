using BaZic.Runtime.BaZic.Code;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Code.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace BaZic.Runtime.Tests.BaZic.Code.Parser
{
    [TestClass]
    public class BaZicParserTest
    {
        [TestInitialize]
        public void Initialize()
        {
            Runtime.TestUtilities.InitializeLogs();
        }


        [TestMethod]
        public void BaZicParserProgram()
        {
            var parser = new BaZicParser();

            var inputCode =
@"

FUNCTION Main(args[])
    MyFunction(1, 2, NULL)
END FUNCTION

VARIABLE myVar[] = NEW [""value1"", ""val2""]


FUNCTION MyFunction(arg1, arg2, arg3[])
    DO
        VARIABLE x = 1 + 2 * (3 + 4 + 5)
        x = myVar[0]
        x = ""hello"" + x.ToString()
        x = 1.ToString()
        BREAK
    LOOP WHILE myVar = arg1 OR(arg1 = arg2 AND arg2 = arg3[0])
    arg3 = NEW System.DateTime()
    RETURN RecursivityFunction(100)
END FUNCTION

ASYNC FUNCTION RecursivityFunction(num)
    IF num > 1 THEN
        num = AWAIT(RecursivityFunction(num – 1))
        TRY
            num.ToString() # this is a comment
        CATCH
            THROW NEW System.Exception(EXCEPTION.Message)
        END TRY
    ELSE
        IF NOT num = 1 THEN
            # another comment
        END IF
    END IF

    RETURN num
END FUNCTION

";

            var program = parser.Parse(inputCode);
            var variableDecl = (VariableDeclaration)((IterationStatement)program.Program.Methods[1].Statements.First()).Statements.First();
            Assert.AreEqual("x", variableDecl.Name.Identifier);
            Assert.AreEqual(12, variableDecl.Line);
            Assert.AreEqual(17, variableDecl.Column);
            Assert.AreEqual(0, program.Issues.InnerExceptions.Count);
        }

        [TestMethod]
        public void BaZicParserProgramErrorRoot1()
        {
            var parser = new BaZicParser();

            var inputCode = "var1 = 1";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(3, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.Last();
            Assert.AreEqual("In a BaZic program's root context, only variable declaration and methods are allowed.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserProgramErrorRoot2()
        {
            var parser = new BaZicParser();

            var inputCode = "RETURN 1";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(1, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.Single();
            Assert.AreEqual("In a BaZic program's root context, only variable declaration and methods are allowed.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserEndOfCodeNotExceptedFunctionNotEnded()
        {
            var parser = new BaZicParser();

            var inputCode = "FUNCTION a()";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(3, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("Invalid statement. New line expected.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserEndOfCodeNotExceptedFunctionNotEndedWithWhitespaces()
        {
            var parser = new BaZicParser();

            var inputCode = "FUNCTION a()   \t  ";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(3, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("Invalid statement. New line expected.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserEndOfCodeNotExceptedDoNoEnded()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Main(args[])
    DO
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(11, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("Invalid statement. Are you missing a keyword?", exception.Message);
        }

        [TestMethod]
        public void BaZicParserEndOfCodeNotExceptedDoAndFunctionNotEnded()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Main(args[])
    DO";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(9, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("A 'New line' is expected but a 'End code' has been found.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserEndOfCodeNotExceptedIfNotEnded()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Main(args[])
    IF
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(15, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("A valid expression is expected.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserEndOfCodeNotExceptedIfAndFunctionNoEnded()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Main(args[])
    IF";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(10, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("A valid expression is expected.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserEndOfCodeNotExceptedTyNotEnded()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Main(args[])
    TRY
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(4, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("A 'TRY' is expected but a 'FUNCTION' has been found.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserEndOfCodeNotExceptedTryAndFunctionNotEnded()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Main(args[])
    TRY";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(6, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("A 'New line' is expected but a 'End code' has been found.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserEndOfCodeNotExceptedTryCatchNotEnded()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Main(args[])
    TRY
    CATCH";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(6, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("A 'New line' is expected but a 'End code' has been found.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserEndOfCodeNotExceptedFunctionNotEndedAfterStatement()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Main(args[])
    THROW NEW System.Exception(""Hello"")";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(2, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("A 'END' is expected but a 'End code' has been found.", exception.Message);
        }
    }
}
