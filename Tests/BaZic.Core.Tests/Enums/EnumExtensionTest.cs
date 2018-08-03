using BaZic.Core.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Core.Tests.Enums
{
    [TestClass]
    public class EnumExtensionTest
    {
        enum EnumTest
        {
            [System.ComponentModel.Description("Foo description text")]
            Foo,
            Bar
        };

        [TestMethod]
        public void EnumExtensionGetDescription()
        {
            var enumeration = EnumTest.Foo;
            Assert.AreEqual("Foo description text", enumeration.GetDescription());
        }
    }
}
