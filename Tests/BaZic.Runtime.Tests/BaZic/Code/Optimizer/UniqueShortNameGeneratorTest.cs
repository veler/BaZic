using BaZic.Runtime.BaZic.Code.Optimizer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Code.Optimizer
{
    [TestClass]
    public class UniqueShortNameGeneratorTest
    {
        [TestMethod]
        public void UniqueShortNameGeneratorTests()
        {
            var generator = new UniqueShortNameGenerator();

            Assert.AreEqual("A", generator.GetNextName());
            Assert.AreEqual("B", generator.GetNextName());

            for (int i = 0; i < 30; i++)
            {
                generator.GetNextName();
            }

            Assert.AreEqual("AG", generator.GetNextName());
        }
    }
}
