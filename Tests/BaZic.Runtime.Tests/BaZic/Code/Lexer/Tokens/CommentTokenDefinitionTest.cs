using BaZic.Runtime.BaZic.Code.Lexer;
using BaZic.Runtime.BaZic.Code.Lexer.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Code.Lexer.Tokens
{
    [TestClass]
    public class CommentTokenDefinitionTest
    {
        [TestMethod]
        public void CommentTokenDefinitionMatch()
        {
            var previousTokenType = TokenType.NotDefined;
            var definition = new CommentTokenDefinition();
            var lexer = new BaZicLexer();

            lexer.InputCode = "Identifier # This is a comment\n" +
                        "Another line of code";

            Assert.IsFalse(definition.Match(lexer, 0, previousTokenType).IsMatch);

            lexer.InputCode = "# This is a comment\n" +
                        "Another line of code";

            var match = definition.Match(lexer, 0, previousTokenType);
            Assert.IsTrue(match.IsMatch);
            Assert.AreEqual(TokenType.Comment, match.TokenType);
            Assert.AreEqual("# This is a comment", match.Value);

            lexer.InputCode = "# This is a comment";

            match = definition.Match(lexer, 0, previousTokenType);
            Assert.IsTrue(match.IsMatch);
            Assert.AreEqual("# This is a comment", match.Value);
        }
    }
}
