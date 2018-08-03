using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Code.AbstractSyntaxTree
{
    [TestClass]
    public class PrimitiveExpressionTest
    {
        [TestMethod]
        public void PrimitiveExpressionStringRepresentationNull()
        {
            var primitiveVal = new PrimitiveExpression();
            Assert.AreEqual("{null}", primitiveVal.ToString());
        }

        [TestMethod]
        public void PrimitiveExpressionStringRepresentationInteger()
        {
            var primitiveVal = new PrimitiveExpression(1);
            Assert.AreEqual("'1' (type:System.Int32)", primitiveVal.ToString());
        }

        [TestMethod]
        public void PrimitiveExpressionStringRepresentationString()
        {
            var primitiveVal = new PrimitiveExpression("Hello");
            Assert.AreEqual("'Hello' (type:System.String)", primitiveVal.ToString());
        }
    }
}
