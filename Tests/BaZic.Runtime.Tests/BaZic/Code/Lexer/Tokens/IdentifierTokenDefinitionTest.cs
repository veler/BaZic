using BaZic.Runtime.BaZic.Code.Lexer;
using BaZic.Runtime.BaZic.Code.Lexer.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Code.Lexer.Tokens
{
    [TestClass]
    public class IdentifierTokenDefinitionTest
    {
        [TestMethod]
        public void IdentifierTokenDefinitionMatch()
        {
            var previousTokenType = TokenType.NotDefined;
            var definition = new IdentifierTokenDefinition();
            var lexer = new BaZicLexer();

            lexer.InputCode = " Identifier1112345";

            Assert.IsFalse(definition.Match(lexer, 0, previousTokenType).IsMatch);

            lexer.InputCode = "+Identifier1112345";
            Assert.IsFalse(definition.Match(lexer, 0, previousTokenType).IsMatch);

            lexer.InputCode = "@dentifier";
            Assert.IsFalse(definition.Match(lexer, 0, previousTokenType).IsMatch);

            lexer.InputCode = "1dentifier";
            Assert.IsFalse(definition.Match(lexer, 0, previousTokenType).IsMatch);

            lexer.InputCode = "Identifier1[";

            var match = definition.Match(lexer, 0, previousTokenType);
            Assert.IsTrue(match.IsMatch);
            Assert.AreEqual(TokenType.Identifier, match.TokenType);
            Assert.AreEqual("Identifier1", match.Value);

            lexer.InputCode = "Identifier";
            Assert.AreEqual("Identifier", definition.Match(lexer, 0, previousTokenType).Value);

            lexer.InputCode = "édentifier";
            Assert.AreEqual("édentifier", definition.Match(lexer, 0, previousTokenType).Value);

            lexer.InputCode = "éde@ntifier";
            Assert.AreEqual("éde@ntifier", definition.Match(lexer, 0, previousTokenType).Value);

            lexer.InputCode = "éde+ntifier";
            Assert.AreEqual("éde", definition.Match(lexer, 0, previousTokenType).Value);
        }
    }
}
