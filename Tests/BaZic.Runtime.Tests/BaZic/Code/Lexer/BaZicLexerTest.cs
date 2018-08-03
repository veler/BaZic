using BaZic.Runtime.BaZic.Code.Lexer;
using BaZic.Runtime.BaZic.Code.Lexer.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Code.Lexer
{
    [TestClass]
    public class BaZicLexerTest
    {
        [TestMethod]
        public void TokenizeBasicTest()
        {
            var lexer = new BaZicLexer();

            var test1 = " VARIABLE  var1";
            var result = lexer.Tokenize(test1);

            Assert.AreEqual(4, result.Count);

            Assert.AreEqual(TokenType.StartCode, result[0].TokenType);

            Assert.AreEqual(TokenType.Variable, result[1].TokenType);
            Assert.AreEqual("VARIABLE", result[1].Value);

            Assert.AreEqual(TokenType.Identifier, result[2].TokenType);
            Assert.AreEqual("var1", result[2].Value);

            Assert.AreEqual(TokenType.EndCode, result[3].TokenType);




            result = lexer.Tokenize(test1, keepWhitespaces: true);

            Assert.AreEqual(7, result.Count);

            Assert.AreEqual(TokenType.StartCode, result[0].TokenType);

            Assert.AreEqual(TokenType.Variable, result[2].TokenType);
            Assert.AreEqual("VARIABLE", result[2].Value);

            Assert.AreEqual(TokenType.Identifier, result[5].TokenType);
            Assert.AreEqual("var1", result[5].Value);

            Assert.AreEqual(TokenType.EndCode, result[6].TokenType);
        }

        [TestMethod]
        public void TokenizeInvalid1()
        {
            var lexer = new BaZicLexer();

            var test1 = "VARIABLE { @var1";
            var result = lexer.Tokenize(test1);

            Assert.AreEqual(5, result.Count);

            Assert.AreEqual(TokenType.Variable, result[1].TokenType);
            Assert.AreEqual("VARIABLE", result[1].Value);

            Assert.AreEqual(TokenType.NotDefined, result[2].TokenType);
            Assert.AreEqual("{", result[2].Value);

            Assert.AreEqual(TokenType.NotDefined, result[3].TokenType);
            Assert.AreEqual("@var1", result[3].Value);

            Assert.AreEqual(TokenType.EndCode, result[4].TokenType);

            test1 = "VARIABLE var1 = 1;";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(6, result.Count);
            Assert.AreEqual(TokenType.NotDefined, result[4].TokenType);
        }

        [TestMethod]
        public void TokenizeInvalid2()
        {
            var lexer = new BaZicLexer();
            
            var test1 = "VARIABLE var1 = 1;";
            var result = lexer.Tokenize(test1);

            Assert.AreEqual(6, result.Count);
            Assert.AreEqual(TokenType.NotDefined, result[4].TokenType);
        }

        [TestMethod]
        public void TokenizeVariableDefaultValue()
        {
            var lexer = new BaZicLexer();

            var test1 = "VARIABLE var1 = true";
            var result = lexer.Tokenize(test1);

            Assert.AreEqual(6, result.Count);
            Assert.AreEqual(TokenType.Variable, result[1].TokenType);
            Assert.AreEqual(TokenType.Identifier, result[2].TokenType);
            Assert.AreEqual(TokenType.Equal, result[3].TokenType);
            Assert.AreEqual(TokenType.True, result[4].TokenType);
            Assert.AreEqual(TokenType.EndCode, result[5].TokenType);
        }

        [TestMethod]
        public void TokenizeVariableArray()
        {
            var lexer = new BaZicLexer();
            
            var test1 = "VARIABLE var1[]";
            var result = lexer.Tokenize(test1);

            Assert.AreEqual(6, result.Count);
            Assert.AreEqual(TokenType.Variable, result[1].TokenType);
            Assert.AreEqual(TokenType.Identifier, result[2].TokenType);
            Assert.AreEqual(TokenType.LeftBracket, result[3].TokenType);
            Assert.AreEqual(TokenType.RightBracket, result[4].TokenType);
            Assert.AreEqual(TokenType.EndCode, result[5].TokenType);
        }

        [TestMethod]
        public void TokenizeVariableArrayWithValues()
        {
            var lexer = new BaZicLexer();
            
            var test1 = "VARIABLE var1[] = [\"Hello\", \"World\"]";
            var result = lexer.Tokenize(test1);

            Assert.AreEqual(12, result.Count);
            Assert.AreEqual(TokenType.Variable, result[1].TokenType);
            Assert.AreEqual(TokenType.Identifier, result[2].TokenType);
            Assert.AreEqual(TokenType.LeftBracket, result[3].TokenType);
            Assert.AreEqual(TokenType.RightBracket, result[4].TokenType);
            Assert.AreEqual(TokenType.Equal, result[5].TokenType);
            Assert.AreEqual(TokenType.LeftBracket, result[6].TokenType);
            Assert.AreEqual(TokenType.String, result[7].TokenType);
            Assert.AreEqual(TokenType.Comma, result[8].TokenType);
            Assert.AreEqual(TokenType.String, result[9].TokenType);
            Assert.AreEqual(TokenType.RightBracket, result[10].TokenType);
            Assert.AreEqual(TokenType.EndCode, result[11].TokenType);
        }

        [TestMethod]
        public void TokenizeString()
        {
            var lexer = new BaZicLexer();

            var test1 = "VARIABLE var1 = \"Hello\"";
            var result = lexer.Tokenize(test1);

            Assert.AreEqual(6, result.Count);
            Assert.AreEqual(TokenType.String, result[4].TokenType);
            Assert.AreEqual("Hello", result[4].Value);

            test1 = "VARIABLE var1 = \"Hello\" \"World\"";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(7, result.Count);
            Assert.AreEqual("Hello", result[4].Value);
            Assert.AreEqual("World", result[5].Value);

            test1 = "VARIABLE var1 = \"Hello\"+\"World\"";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(8, result.Count);
            Assert.AreEqual("Hello", result[4].Value);
            Assert.AreEqual(TokenType.Plus, result[5].TokenType);
            Assert.AreEqual("World", result[6].Value);

            test1 = "VARIABLE var1 = \"Hello\\\"World\\\"\"";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(6, result.Count);
            Assert.AreEqual(TokenType.String, result[4].TokenType);
            Assert.AreEqual("Hello\\\"World\\\"", result[4].Value);
        }

        [TestMethod]
        public void TokenizeIntegers()
        {
            var lexer = new BaZicLexer();

            var test1 = "var1 = 123";
            var result = lexer.Tokenize(test1);

            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(TokenType.Integer, result[3].TokenType);
            Assert.AreEqual("123", result[3].Value);

            test1 = "var1 = -123";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(TokenType.Integer, result[3].TokenType);
            Assert.AreEqual("-123", result[3].Value);

            test1 = "var1 = +123";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(TokenType.Integer, result[3].TokenType);
            Assert.AreEqual("123", result[3].Value);

            test1 = "var1 = -(-123 + 2)";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(10, result.Count);
            Assert.AreEqual(TokenType.Minus, result[3].TokenType);
            Assert.AreEqual(TokenType.LeftParenth, result[4].TokenType);
            Assert.AreEqual(TokenType.Integer, result[5].TokenType);
            Assert.AreEqual(TokenType.Plus, result[6].TokenType);
            Assert.AreEqual(TokenType.Integer, result[7].TokenType);
            Assert.AreEqual(TokenType.RightParenth, result[8].TokenType);
            Assert.AreEqual("-123", result[5].Value);
            Assert.AreEqual("2", result[7].Value);

            test1 = "-1 + +4";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(TokenType.Integer, result[1].TokenType);
            Assert.AreEqual("-1", result[1].Value);
            Assert.AreEqual(TokenType.Plus, result[2].TokenType);
            Assert.AreEqual(TokenType.Integer, result[3].TokenType);
            Assert.AreEqual("4", result[3].Value);

            test1 = "1+-1";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(TokenType.Integer, result[1].TokenType);
            Assert.AreEqual("1", result[1].Value);
            Assert.AreEqual(TokenType.Plus, result[2].TokenType);
            Assert.AreEqual(TokenType.Integer, result[3].TokenType);
            Assert.AreEqual("-1", result[3].Value);

            test1 = "1-+1";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(TokenType.Integer, result[1].TokenType);
            Assert.AreEqual("1", result[1].Value);
            Assert.AreEqual(TokenType.Minus, result[2].TokenType);
            Assert.AreEqual(TokenType.Integer, result[3].TokenType);
            Assert.AreEqual("1", result[3].Value);

            test1 = "1--1";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(TokenType.Integer, result[1].TokenType);
            Assert.AreEqual("1", result[1].Value);
            Assert.AreEqual(TokenType.Minus, result[2].TokenType);
            Assert.AreEqual(TokenType.Integer, result[3].TokenType);
            Assert.AreEqual("-1", result[3].Value);

            test1 = "1++1";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(TokenType.Integer, result[1].TokenType);
            Assert.AreEqual("1", result[1].Value);
            Assert.AreEqual(TokenType.Plus, result[2].TokenType);
            Assert.AreEqual(TokenType.Integer, result[3].TokenType);
            Assert.AreEqual("1", result[3].Value);

            test1 = "1+1";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(TokenType.Integer, result[1].TokenType);
            Assert.AreEqual("1", result[1].Value);
            Assert.AreEqual(TokenType.Plus, result[2].TokenType);
            Assert.AreEqual(TokenType.Integer, result[3].TokenType);
            Assert.AreEqual("1", result[3].Value);

            test1 = "1-1";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(TokenType.Integer, result[1].TokenType);
            Assert.AreEqual("1", result[1].Value);
            Assert.AreEqual(TokenType.Minus, result[2].TokenType);
            Assert.AreEqual(TokenType.Integer, result[3].TokenType);
            Assert.AreEqual("1", result[3].Value);
        }

        [TestMethod]
        public void TokenizeDouble()
        {
            var lexer = new BaZicLexer();

            var test1 = "var1 = 123.45";
            var result = lexer.Tokenize(test1);

            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(TokenType.Double, result[3].TokenType);
            Assert.AreEqual("123.45", result[3].Value);

            test1 = "var1 = -123.45";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(TokenType.Double, result[3].TokenType);
            Assert.AreEqual("-123.45", result[3].Value);

            test1 = "var1 = -.45";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(TokenType.Double, result[3].TokenType);
            Assert.AreEqual("-0.45", result[3].Value);

            test1 = "var1 = +123.45";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(TokenType.Double, result[3].TokenType);
            Assert.AreEqual("123.45", result[3].Value);

            test1 = "var1 = -(-123.45 + 2.45)";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(10, result.Count);
            Assert.AreEqual(TokenType.Minus, result[3].TokenType);
            Assert.AreEqual(TokenType.LeftParenth, result[4].TokenType);
            Assert.AreEqual(TokenType.Double, result[5].TokenType);
            Assert.AreEqual(TokenType.Plus, result[6].TokenType);
            Assert.AreEqual(TokenType.Double, result[7].TokenType);
            Assert.AreEqual(TokenType.RightParenth, result[8].TokenType);
            Assert.AreEqual("-123.45", result[5].Value);
            Assert.AreEqual("2.45", result[7].Value);

            test1 = "-1.45 + +4.45";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(TokenType.Double, result[1].TokenType);
            Assert.AreEqual("-1.45", result[1].Value);
            Assert.AreEqual(TokenType.Plus, result[2].TokenType);
            Assert.AreEqual(TokenType.Double, result[3].TokenType);
            Assert.AreEqual("4.45", result[3].Value);

            test1 = "var1 = -123.45f";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(7, result.Count);
            Assert.AreEqual(TokenType.Integer, result[3].TokenType);
            Assert.AreEqual("-123", result[3].Value);
            Assert.AreEqual(TokenType.Dot, result[4].TokenType);
            Assert.AreEqual(TokenType.NotDefined, result[5].TokenType);

            test1 = "var1 = -123.45.6";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(7, result.Count);
            Assert.AreEqual(TokenType.Integer, result[3].TokenType);
            Assert.AreEqual("-123", result[3].Value);

            test1 = "var1 = -123,45";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(7, result.Count);
            Assert.AreEqual(TokenType.Integer, result[3].TokenType);
            Assert.AreEqual("-123", result[3].Value);

            test1 = "var1 = -123 45";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(6, result.Count);
            Assert.AreEqual(TokenType.Integer, result[3].TokenType);
            Assert.AreEqual("-123", result[3].Value);

            test1 = "1.2+1.2";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(TokenType.Double, result[1].TokenType);
            Assert.AreEqual("1.2", result[1].Value);
            Assert.AreEqual(TokenType.Plus, result[2].TokenType);
            Assert.AreEqual(TokenType.Double, result[3].TokenType);
            Assert.AreEqual("1.2", result[3].Value);

            test1 = "1.2-1.2";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(TokenType.Double, result[1].TokenType);
            Assert.AreEqual("1.2", result[1].Value);
            Assert.AreEqual(TokenType.Minus, result[2].TokenType);
            Assert.AreEqual(TokenType.Double, result[3].TokenType);
            Assert.AreEqual("1.2", result[3].Value);
        }

        [TestMethod]
        public void TokenizeIdentifiers()
        {
            var lexer = new BaZicLexer();

            var test1 = "VARIABLE Variable = 1";
            var result = lexer.Tokenize(test1);

            Assert.AreEqual(TokenType.Variable, result[2].TokenType);

            test1 = "VARIABLE Variables";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(TokenType.Identifier, result[2].TokenType);
            Assert.AreEqual("Variables", result[2].Value);

            test1 = "VARIABLE Variable_Name";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(TokenType.Identifier, result[2].TokenType);
            Assert.AreEqual("Variable_Name", result[2].Value);

            test1 = "VARIABLE Variable-Name";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(TokenType.Identifier, result[2].TokenType);
            Assert.AreEqual("Variable", result[2].Value);

            test1 = "VARIABLE v";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(TokenType.Identifier, result[2].TokenType);
            Assert.AreEqual("v", result[2].Value);

            test1 = "VARIABLE 1v";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(TokenType.NotDefined, result[2].TokenType);

            test1 = "VARIABLE v1";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(TokenType.Identifier, result[2].TokenType);
            Assert.AreEqual("v1", result[2].Value);

            test1 = "VARIABLE v.ToString()";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(TokenType.Identifier, result[2].TokenType);
            Assert.AreEqual("v", result[2].Value);

            test1 = "VARIABLE v@";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(TokenType.Identifier, result[2].TokenType);
            Assert.AreEqual("v@", result[2].Value);

            test1 = "VARIABLE v@[]";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(TokenType.Identifier, result[2].TokenType);
            Assert.AreEqual("v@", result[2].Value);

            test1 = "VARIABLE ANDHello";
            result = lexer.Tokenize(test1);

            Assert.AreEqual(TokenType.Identifier, result[2].TokenType);
            Assert.AreEqual("ANDHello", result[2].Value);
        }

        [TestMethod]
        public void TokenizeCode()
        {
            var lexer = new BaZicLexer();

            var test1 = @"VARIABLE var1 = 1

FUNCTION Main(arg1, arg2)
    IF NOT arg1 <= arg2 THEN
        System.Console.WriteLine(""Hello ""+var1+1)
    END IF
END FUNCTION";

            var result = lexer.Tokenize(test1);

            Assert.AreEqual(42, result.Count);

            var expectedToken = new TokenType[] {
                TokenType.StartCode,
                TokenType.Variable,
                TokenType.Identifier,
                TokenType.Equal,
                TokenType.Integer,
                TokenType.NewLine,
                TokenType.NewLine,
                TokenType.Function,
                TokenType.Identifier,
                TokenType.LeftParenth,
                TokenType.Identifier,
                TokenType.Comma,
                TokenType.Identifier,
                TokenType.RightParenth,
                TokenType.NewLine,
                TokenType.If,
                TokenType.Not,
                TokenType.Identifier,
                TokenType.LesserThan,
                TokenType.Equal,
                TokenType.Identifier,
                TokenType.Then,
                TokenType.NewLine,
                TokenType.Identifier,
                TokenType.Dot,
                TokenType.Identifier,
                TokenType.Dot,
                TokenType.Identifier,
                TokenType.LeftParenth,
                TokenType.String,
                TokenType.Plus,
                TokenType.Identifier,
                TokenType.Plus,
                TokenType.Integer,
                TokenType.RightParenth,
                TokenType.NewLine,
                TokenType.End,
                TokenType.If,
                TokenType.NewLine,
                TokenType.End,
                TokenType.Function,
                TokenType.EndCode,
            };

            for (var i = 0; i < result.Count; i++)
            {
                Assert.AreEqual(expectedToken[i], result[i].TokenType);
            }
        }
    }
}
