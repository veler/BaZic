using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Code.AbstractSyntaxTree
{
    [TestClass]
    public class ArrayIndexerExpressionTest
    {
        [TestMethod]
        public void ArrayIndexerExpressions()
        {
            var indexer = new ArrayIndexerExpression(new VariableReferenceExpression("Foo"), new Expression[] { new PrimitiveExpression(1) });
            Assert.AreEqual("Foo['1' (type:System.Int32)]", indexer.ToString());

            indexer = new ArrayIndexerExpression(new VariableReferenceExpression("Foo"), new Expression[] { new PrimitiveExpression("Key") });
            Assert.AreEqual("Foo['Key' (type:System.String)]", indexer.ToString());
        }
    }
}
