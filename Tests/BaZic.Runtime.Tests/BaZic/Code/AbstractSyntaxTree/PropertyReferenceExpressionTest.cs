using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Code.AbstractSyntaxTree
{
    [TestClass]
    public class PropertyReferenceExpressionTest
    {
        [TestMethod]
        public void PropertyReferenceExpressionTests()
        {
            var reference = new PropertyReferenceExpression();
            Assert.IsNull(reference.PropertyName);

            reference = new PropertyReferenceExpression(new VariableReferenceExpression("Foo"), "Boo");
            Assert.AreEqual("Foo.Boo", reference.ToString());
        }
    }
}
