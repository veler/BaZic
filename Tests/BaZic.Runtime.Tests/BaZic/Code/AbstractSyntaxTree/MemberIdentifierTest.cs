using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Code.AbstractSyntaxTree
{
    [TestClass]
    public class MemberIdentifierTest
    {
        [TestMethod]
        public void MemberIdentifierValidation()
        {
            new MemberIdentifier("Foo");
            new MemberIdentifier("Bar1");
            new MemberIdentifier("Bo2OUy8F");
        }

        [TestMethod]
        public void MemberIdentifierValidationStartsWithNumber()
        {
            try
            {
                new MemberIdentifier("1Foo");
                Assert.Fail();
            }
            catch { }
        }

        [TestMethod]
        public void MemberIdentifierValidationEndsWithSymbol()
        {
            try
            {
                new MemberIdentifier("Foo@");
                Assert.Fail();
            }
            catch { }
        }

        [TestMethod]
        public void MemberIdentifierValidationIsWhitespace()
        {
            try
            {
                new MemberIdentifier("  ");
                Assert.Fail();
            }
            catch { }
        }

        [TestMethod]
        public void MemberIdentifierValidationHasWhitespace()
        {
            try
            {
                new MemberIdentifier("Foo Bar");
                Assert.Fail();
            }
            catch { }
        }
    }
}
