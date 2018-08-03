using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Code.AbstractSyntaxTree
{
    [TestClass]
    public class ParameterDeclarationTest
    {
        [TestMethod]
        public void ParameterDeclarations()
        {
            var variable = new ParameterDeclaration("Foo");

            Assert.AreEqual("Foo", variable.Name.ToString());
            Assert.IsFalse(variable.IsArray);
        }
    }
}
