using BaZic.Runtime.BaZic.Code.Lexer;
using BaZic.Runtime.BaZic.Code.Lexer.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Code.Lexer.Tokens
{
    [TestClass]
    public class DoubleTokenDefinitionTest
    {
        [TestMethod]
        public void DoubleTokenDefinitionMatch()
        {
            var previousTokenType = TokenType.NotDefined;
            var definition = new DoubleTokenDefinition();
            var lexer = new BaZicLexer();

            lexer.InputCode = "Identifier111.2345";

            Assert.IsFalse(definition.Match(lexer, 0, previousTokenType).IsMatch);

            lexer.InputCode = "111.2345Identifier";
            Assert.IsFalse(definition.Match(lexer, 0, previousTokenType).IsMatch);

            lexer.InputCode = "11 123.45"; // The "11" is NOT considered as the double.
            Assert.IsFalse(definition.Match(lexer, 0, previousTokenType).IsMatch);

            lexer.InputCode = "111,2345";
            Assert.IsFalse(definition.Match(lexer, 0, previousTokenType).IsMatch);

            lexer.InputCode = "11,123.45";
            Assert.IsFalse(definition.Match(lexer, 0, previousTokenType).IsMatch);

            lexer.InputCode = "100";
            Assert.IsFalse(definition.Match(lexer, 0, previousTokenType).IsMatch);

            lexer.InputCode = "10.0";
            Assert.IsTrue(definition.Match(lexer, 0, previousTokenType).IsMatch);

            lexer.InputCode = "111.1234 identifier";

            var match = definition.Match(lexer, 0, previousTokenType);
            Assert.IsTrue(match.IsMatch);
            Assert.AreEqual(TokenType.Double, match.TokenType);
            Assert.AreEqual("111.1234", match.Value);

            lexer.InputCode = "-111.1234 identifier";
            Assert.AreEqual("-111.1234", definition.Match(lexer, 0, previousTokenType).Value);

            lexer.InputCode = "+111.1234 identifier";
            Assert.AreEqual("111.1234", definition.Match(lexer, 0, previousTokenType).Value);

            lexer.InputCode = "111.1234";
            Assert.AreEqual("111.1234", definition.Match(lexer, 0, previousTokenType).Value);

            lexer.InputCode = ".45";
            Assert.AreEqual("0.45", definition.Match(lexer, 0, previousTokenType).Value);

            lexer.InputCode = "-.45";
            Assert.AreEqual("-0.45", definition.Match(lexer, 0, previousTokenType).Value);
        }
    }
}
