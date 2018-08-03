using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Code.AbstractSyntaxTree
{
    [TestClass]
    public class VariableReferenceExpressionTest
    {
        [TestMethod]
        public void VariableReferenceExpressionNull()
        {
            var reference = new VariableReferenceExpression();
            Assert.IsNull(reference.Name);
        }
        [TestMethod]
        public void VariableReferenceExpressionNameGiven()
        {
            var reference = new VariableReferenceExpression("Foo");
            Assert.AreEqual("Foo", reference.ToString());
        }
        [TestMethod]
        public void VariableReferenceExpressionDeclarationGiven()
        {
            var reference = new VariableReferenceExpression(new VariableDeclaration("Boo"));
            Assert.AreEqual("Boo", reference.Name.ToString());
        }
    }
}
