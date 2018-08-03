using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Code.AbstractSyntaxTree
{
    [TestClass]
    public class VariableDeclarationTest
    {
        [TestMethod]
        public void VariableDeclarations()
        {
            var variable = new VariableDeclaration("Foo");

            Assert.AreEqual("Foo", variable.Name.ToString());
            Assert.IsFalse(variable.IsArray);
            Assert.IsNull(variable.DefaultValue);

            variable.WithDefaultValue(new PrimitiveExpression(10));

            Assert.IsNotNull(variable.DefaultValue);

            try
            {
                variable.WithDefaultValue(null);
                Assert.Fail();
            }
            catch { }
        }
    }
}
