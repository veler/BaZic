using BaZic.Runtime.BaZic.Code.Lexer;
using BaZic.Runtime.BaZic.Code.Lexer.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Code.Lexer.Tokens
{
    [TestClass]
    public class TokenDefinitionTest
    {
        [TestMethod]
        public void KeywordTokenDefinitionMatch()
        {
            var previousTokenType = TokenType.NotDefined;
            var definition = new TokenDefinition(TokenType.Async, "ASYNC");
            var lexer = new BaZicLexer();

            lexer.InputCode = "Identifier Async";

            Assert.IsFalse(definition.Match(lexer, 0, previousTokenType).IsMatch);

            lexer.InputCode = "Async2 Function2";
            Assert.IsFalse(definition.Match(lexer, 0, previousTokenType).IsMatch);

            lexer.InputCode = "ASYNC Identifier";

            var match = definition.Match(lexer, 0, previousTokenType);
            Assert.IsTrue(match.IsMatch);
            Assert.AreEqual(TokenType.Async, match.TokenType);
            Assert.AreEqual("ASYNC", match.Value);

            lexer.InputCode = "Async function";

            match = definition.Match(lexer, 0, previousTokenType);
            Assert.IsTrue(match.IsMatch);
            Assert.AreEqual("ASYNC", match.Value);
        }

        [TestMethod]
        public void SpecialCharacterTokenDefinitionMatch()
        {
            var previousTokenType = TokenType.NotDefined;
            var definition = new TokenDefinition(TokenType.Comma, ",", false);
            var lexer = new BaZicLexer();

            lexer.InputCode = "Identifier, Id2";

            Assert.IsFalse(definition.Match(lexer, 0, previousTokenType).IsMatch);

            lexer.InputCode = ", Identifier";

            var match = definition.Match(lexer, 0, previousTokenType);
            Assert.IsTrue(match.IsMatch);
            Assert.AreEqual(TokenType.Comma, match.TokenType);
            Assert.AreEqual(",", match.Value);

            lexer.InputCode = ",Identifier";

            match = definition.Match(lexer, 0, previousTokenType);
            Assert.IsTrue(match.IsMatch);
            Assert.AreEqual(",", match.Value);
        }
    }
}
