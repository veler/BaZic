using BaZic.Runtime.BaZic.Code.Lexer;
using BaZic.Runtime.BaZic.Code.Lexer.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Code.Lexer.Tokens
{
    [TestClass]
    public class StringTokenDefinitionTest
    {
        [TestMethod]
        public void StringTokenDefinitionMatch()
        {
            var previousTokenType = TokenType.NotDefined;
            var definition = new StringTokenDefinition();
            var lexer = new BaZicLexer();

            lexer.InputCode = "Identifier \"Hello\n" +
                        "World\"";

            Assert.IsFalse(definition.Match(lexer, 0, previousTokenType).IsMatch);

            lexer.InputCode = "\"Hello\n" +
                        "World";
            Assert.IsFalse(definition.Match(lexer, 0, previousTokenType).IsMatch);

            lexer.InputCode = "\"Hello World";
            Assert.IsFalse(definition.Match(lexer, 0, previousTokenType).IsMatch);

            lexer.InputCode = "\"Hello\n" +
                       "World\" Identifier";

            var match = definition.Match(lexer, 0, previousTokenType);
            Assert.IsTrue(match.IsMatch);
            Assert.AreEqual(TokenType.String, match.TokenType);
            Assert.AreEqual("Hello\nWorld", match.Value);

            lexer.InputCode = "\"Hello \\n World\"";

            match = definition.Match(lexer, 0, previousTokenType);
            Assert.IsTrue(match.IsMatch);
            Assert.AreEqual("Hello \\n World", match.Value);

            lexer.InputCode = "\"Hello \\\" World\"";
            Assert.AreEqual("Hello \\\" World", definition.Match(lexer, 0, previousTokenType).Value);

            lexer.InputCode = "\"Hello\"\"World\"";
            Assert.AreEqual("Hello", definition.Match(lexer, 0, previousTokenType).Value);

            lexer.InputCode = "\"Hello\"Identifier\"World\"";
            Assert.AreEqual("Hello", definition.Match(lexer, 0, previousTokenType).Value);
        }
    }
}
