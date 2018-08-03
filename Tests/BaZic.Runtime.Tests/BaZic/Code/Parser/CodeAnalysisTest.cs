using BaZic.Runtime.BaZic.Code;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Code.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace BaZic.Runtime.Tests.BaZic.Code.Parser
{
    [TestClass]
    public class CodeAnalysisTest
    {
        [TestInitialize]
        public void Initialize()
        {
            Runtime.TestUtilities.InitializeLogs();
        }

        [TestMethod]
        public void BaZicCodeAnalysisMethodDeclaration()
        {
            var parser = new BaZicParser();
            var inputCode =
@"FUNCTION Method1()
END FUNCTION
FUNCTION Method1()
END FUNCTION";

            var result = parser.Parse(inputCode);
            var baZicEx = (BaZicParserException)result.Issues.InnerExceptions.Single();
            Assert.AreEqual("A method 'Method1' is already declared line 1.", baZicEx.Message);
            Assert.AreEqual(3, baZicEx.Line);
            Assert.AreEqual(9, baZicEx.Column);
        }

        [TestMethod]
        public void BaZicCodeAnalysisGlobalVariableDeclarationDuplicated()
        {
            var parser = new BaZicParser();
            var inputCode =
@"
VARIABLE x

FUNCTION Method1()
    RETURN x
END FUNCTION

VARIABLE x";

            var result = parser.Parse(inputCode);
            var baZicEx = (BaZicParserException)result.Issues.InnerExceptions.First();
            Assert.AreEqual("A variable or binding 'x' is already declared line 2.", baZicEx.Message);
            Assert.AreEqual(8, baZicEx.Line);
            Assert.AreEqual(9, baZicEx.Column);
        }

        [TestMethod]
        public void BaZicCodeAnalysisGlobalVariableDeclarationDuplicatedInSeveralScopes()
        {
            var parser = new BaZicParser();
            var inputCode =
@"
VARIABLE x

FUNCTION Method1()
    VARIABLE x
END FUNCTION";

            var result = parser.Parse(inputCode);
            var baZicEx = (BaZicParserException)result.Issues.InnerExceptions.First();
            Assert.AreEqual("A variable or binding 'x' is already declared line 2.", baZicEx.Message);
            Assert.AreEqual(5, baZicEx.Line);
            Assert.AreEqual(13, baZicEx.Column);
        }

        [TestMethod]
        public void BaZicCodeAnalysisGlobalVariableDeclarationDeclaredButNotused()
        {
            var parser = new BaZicParser();
            var inputCode =
@"
VARIABLE x

FUNCTION Method1()
END FUNCTION";

            var result = parser.Parse(inputCode);
            var baZicEx = (BaZicParserException)result.Issues.InnerExceptions.Single();
            Assert.AreEqual("The variable 'x' is declared but never used.", baZicEx.Message);
        }

        [TestMethod]
        public void BaZicCodeAnalysisVariableDeclaration()
        {
            var parser = new BaZicParser();
            var inputCode =
@"

FUNCTION x()
    DO WHILE TRUE
        VARIABLE x
        x = 1
    LOOP
    VARIABLE x
    RETURN x
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(0, program.Issues.InnerExceptions.Count);
        }

        [TestMethod]
        public void BaZicCodeAnalysisVariableDeclarationDifferentScopes()
        {
            var parser = new BaZicParser();
            var inputCode =
@"

FUNCTION x()
    IF TRUE THEN
        VARIABLE x
        RETURN x
    ELSE
        VARIABLE x
        RETURN x
    END IF
END FUNCTION";

            var program = parser.Parse(inputCode);
            var variableDecl = (VariableDeclaration)((ConditionStatement)program.Program.Methods.Single().Statements.First()).TrueStatements.First();
            Assert.AreEqual("x", variableDecl.Name.Identifier);
        }

        [TestMethod]
        public void BaZicCodeAnalysisVariableDeclarationSameVariableInAccessibleScope()
        {
            var parser = new BaZicParser();
            var inputCode =
@"
FUNCTION Method1()
    VARIABLE x
    DO WHILE TRUE
        IF TRUE THEN
            VARIABLE x
        END IF
    LOOP
END FUNCTION";

            var result = parser.Parse(inputCode);
            var baZicEx = (BaZicParserException)result.Issues.InnerExceptions.First();
            Assert.AreEqual("A variable or binding 'x' is already declared line 3.", baZicEx.Message);
            Assert.AreEqual(6, baZicEx.Line);
            Assert.AreEqual(21, baZicEx.Column);
        }

        [TestMethod]
        public void BaZicCodeAnalysisMethodInvocationNotExist()
        {
            var parser = new BaZicParser();
            var inputCode =
@"FUNCTION Method1()
END FUNCTION

FUNCTION Main(args[])
    Method2()
END FUNCTION";

            var result = parser.Parse(inputCode);
            var baZicEx = (BaZicParserException)result.Issues.InnerExceptions.Single();
            Assert.AreEqual("The name 'Method2' does not exist in the current context.", baZicEx.Message);
        }

        [TestMethod]
        public void BaZicCodeAnalysisMethodInvocationBadArgumentCount()
        {
            var parser = new BaZicParser();
            var inputCode =
@"FUNCTION Method1()
END FUNCTION

FUNCTION Main(args[])
    Method1(1)
END FUNCTION";

            var result = parser.Parse(inputCode);
            var baZicEx = (BaZicParserException)result.Issues.InnerExceptions.Single();
            Assert.AreEqual("There is no method 'Method1' that takes 1 argument(s).", baZicEx.Message);
        }

        [TestMethod]
        public void BaZicCodeAnalysisMethodInvocationAwaitNonAsync()
        {
            var parser = new BaZicParser();
            var inputCode =
@"FUNCTION Main(args[])
    AWAIT Method1()
    Method1()
END FUNCTION

FUNCTION Method1()
    Method1()
END FUNCTION";

            var result = parser.Parse(inputCode);
            var baZicEx = (BaZicParserException)result.Issues.InnerExceptions.Single();
            Assert.AreEqual("Cannot await a synchronous method.", baZicEx.Message);

        }

        [TestMethod]
        public void BaZicCodeAnalysisMethodInvocation()
        {
            var parser = new BaZicParser();
            var inputCode =
@"FUNCTION Main(args[])
    Method1(NULL)
END FUNCTION

FUNCTION Method1(arg)
    Method1(Method2(arg))
END FUNCTION

FUNCTION Method2(arg)
    RETURN arg
END FUNCTION";

            var result = parser.Parse(inputCode);

            Assert.AreEqual(0, result.Issues.InnerExceptions.Count);
        }

        [TestMethod]
        public void BaZicCodeAnalysisArrayIndexerOneDimension()
        {
            var parser = new BaZicParser();
            var inputCode =
@"
FUNCTION Main(args[])
    VARIABLE x[] = NEW [1, 2, 3]
    VARIABLE y = x[0, 1]
    RETURN y
END FUNCTION";

            var result = parser.Parse(inputCode);
            var baZicEx = (BaZicParserException)result.Issues.InnerExceptions.Single();
            Assert.AreEqual("The BaZic declared variable or parameter 'x' is an array of one dimension. Only one index must be specified here.", baZicEx.Message);
        }

        [TestMethod]
        public void BaZicCodeAnalysisArrayIndexerNotArray()
        {
            var parser = new BaZicParser();
            var inputCode =
@"
FUNCTION Main(args[])
    VARIABLE x = 1
    VARIABLE y = x[0]
    RETURN y
END FUNCTION";

            var result = parser.Parse(inputCode);
            var baZicEx = (BaZicParserException)result.Issues.InnerExceptions.Single();
            Assert.AreEqual("The variable 'x' is not an array and cannot be use in this context.", baZicEx.Message);
        }

        [TestMethod]
        public void BaZicCodeAnalysisVariableReferenceArray()
        {
            var parser = new BaZicParser();
            var inputCode =
@"
FUNCTION Main(args[])
    VARIABLE array[] = NEW [1, 2, 3]
    VARIABLE nonArray = array
    RETURN nonArray
END FUNCTION";

            var result = parser.Parse(inputCode);
            var baZicEx = (BaZicParserException)result.Issues.InnerExceptions.Single();
            Assert.AreEqual("The variable 'array' is an array and cannot be use in this context.", baZicEx.Message);
        }

        [TestMethod]
        public void BaZicCodeAnalysisVariableReferenceNonArray()
        {
            var parser = new BaZicParser();
            var inputCode =
@"
FUNCTION Main(args[])
    VARIABLE nonArray = 1
    VARIABLE array[] = nonArray
    RETURN array
END FUNCTION";

            var result = parser.Parse(inputCode);
            var baZicEx = (BaZicParserException)result.Issues.InnerExceptions.Single();
            Assert.AreEqual("The variable 'nonArray' is not an array and cannot be use in this context.", baZicEx.Message);
        }

        [TestMethod]
        public void BaZicCodeAnalysisVariableReferenceNotFound()
        {
            var parser = new BaZicParser();
            var inputCode =
@"
FUNCTION Main(args[])
    VARIABLE y = x
END FUNCTION";

            var result = parser.Parse(inputCode);
            Assert.AreEqual(3, result.Issues.InnerExceptions.Count);
            var baZicEx = (BaZicParserException)result.Issues.InnerExceptions.First();
            Assert.AreEqual("The name 'x' does not exist in the current context.", baZicEx.Message);
        }

        [TestMethod]
        public void BaZicCodeAnalysisVariableReferenceArrayWithNotArray()
        {
            var parser = new BaZicParser();
            var inputCode =
@"
FUNCTION Main(args[])
    VARIABLE x
    VARIABLE y[]
    y = x
    RETURN y
END FUNCTION";

            var result = parser.Parse(inputCode);
            Assert.AreEqual(1, result.Issues.InnerExceptions.Count);
            var baZicEx = (BaZicParserException)result.Issues.InnerExceptions.First();
            Assert.AreEqual("The value is an array on a side and not on the other side of the assignment.", baZicEx.Message);
        }

        [TestMethod]
        public void BaZicCodeAnalysisVariableReferenceArrayOnSide()
        {
            var parser = new BaZicParser();
            var inputCode =
@"
FUNCTION Main(args[])
    VARIABLE x
    VARIABLE y[]
    y = x
    RETURN y
END FUNCTION";

            var result = parser.Parse(inputCode);
            var baZicEx = (BaZicParserException)result.Issues.InnerExceptions.Single();
            Assert.AreEqual("The value is an array on a side and not on the other side of the assignment.", baZicEx.Message);
        }

        [TestMethod]
        public void BaZicCodeAnalysisAssignStatement()
        {
            var parser = new BaZicParser();
            var inputCode =
@"
FUNCTION Method1()
    VARIABLE x = 1
    x = NEW [1, 2, 3]
END FUNCTION";

            var result = parser.Parse(inputCode);
            var baZicEx = (BaZicParserException)result.Issues.InnerExceptions.Single();
            Assert.AreEqual("Cannot assign an array to a non-array variable or parameter.", baZicEx.Message);
        }
    }
}
