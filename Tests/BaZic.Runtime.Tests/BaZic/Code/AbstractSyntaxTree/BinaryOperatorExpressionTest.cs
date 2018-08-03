using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Code.AbstractSyntaxTree
{
    [TestClass]
    public class BinaryOperatorExpressionTest
    {
        [TestMethod]
        public void BinaryOperatorExpressions()
        {
            var binaryOperator = new BinaryOperatorExpression(new VariableReferenceExpression("Foo"), BinaryOperatorType.GreaterThanOrEqual, new VariableReferenceExpression("Bar"));
            Assert.AreEqual("Foo >= Bar", binaryOperator.ToString());
        }
    }
}
