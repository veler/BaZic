using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Code.AbstractSyntaxTree
{
    [TestClass]
    public class MethodDeclarationTest
    {
        [TestMethod]
        public void MethodDeclarations()
        {
            var method = new MethodDeclaration("Foo", false, false)
                        .WithParameters(new ParameterDeclaration("Bar"))
                        .WithBody(
                            new VariableDeclaration("Var"),
                            new ReturnStatement()
                        );

            Assert.AreEqual(1, method.Arguments.Count);
            Assert.AreEqual(2, method.Statements.Count);
        }
    }
}
