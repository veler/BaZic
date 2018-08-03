using BaZic.Runtime.BaZic.Code.Lexer;
using BaZic.Runtime.BaZic.Code.Lexer.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Code.Lexer.Tokens
{
    [TestClass]
    public class IntegerTokenDefinitionTest
    {
        [TestMethod]
        public void IntegerTokenDefinitionMatch()
        {
            var previousTokenType = TokenType.NotDefined;
            var definition = new IntegerTokenDefinition();
            var lexer = new BaZicLexer();

            lexer.InputCode = "Identifier1112345";

            Assert.IsFalse(definition.Match(lexer, 0, previousTokenType).IsMatch);

            lexer.InputCode = "1112345Identifier";
            Assert.IsFalse(definition.Match(lexer, 0, previousTokenType).IsMatch);

            lexer.InputCode = "1111234 identifier";

            var match = definition.Match(lexer, 0, previousTokenType);
            Assert.IsTrue(match.IsMatch);
            Assert.AreEqual(TokenType.Integer, match.TokenType);
            Assert.AreEqual("1111234", match.Value);

            lexer.InputCode = "11 12345"; // The "11" is considered as the double.
            Assert.IsTrue(definition.Match(lexer, 0, previousTokenType).IsMatch);

            lexer.InputCode = "-1111234 identifier";
            Assert.AreEqual("-1111234", definition.Match(lexer, 0, previousTokenType).Value);

            lexer.InputCode = "+1111234 identifier";
            Assert.AreEqual("1111234", definition.Match(lexer, 0, previousTokenType).Value);

            lexer.InputCode = "1111234";
            Assert.AreEqual("1111234", definition.Match(lexer, 0, previousTokenType).Value);

            lexer.InputCode = "111,2345";
            Assert.AreEqual("111", definition.Match(lexer, 0, previousTokenType).Value);

            lexer.InputCode = "11123.45";
            Assert.AreEqual("11123", definition.Match(lexer, 0, previousTokenType).Value);
        }
    }
}
