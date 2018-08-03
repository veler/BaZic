using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Code.AbstractSyntaxTree
{
    [TestClass]
    public class ClassReferenceExpressionTest
    {
        [TestMethod]
        public void ClassReferenceExpressionClassNameValid()
        {
            // Check class name
            var reference = new ClassReferenceExpression("namespace", "Foo");
            new ClassReferenceExpression("namespace", "Bar1");
            new ClassReferenceExpression("namespace", "Bo2OUy8F");

            Assert.AreEqual("namespace.Foo", reference.ToString());
        }

        [TestMethod]
        public void ClassReferenceExpressionClassNameStartWithNumber()
        {
            try
            {
                new ClassReferenceExpression("namespace", "1Foo");
                Assert.Fail();
            }
            catch { }
        }

        [TestMethod]
        public void ClassReferenceExpressionClassNameEndsWithSymbol()
        {
            try
            {
                new ClassReferenceExpression("namespace", "Foo@");
                Assert.Fail();
            }
            catch { }
        }

        [TestMethod]
        public void ClassReferenceExpressionClassNameIsWhitespace()
        {
            try
            {
                new ClassReferenceExpression("namespace", "  ");
                Assert.Fail();
            }
            catch { }
        }

        [TestMethod]
        public void ClassReferenceExpressionClassNameHasWhitespace()
        {
            try
            {
                new ClassReferenceExpression("namespace", "Foo Bar");
                Assert.Fail();
            }
            catch { }
        }

        [TestMethod]
        public void ClassReferenceExpressionNameSpace()
        {
            // Check namespace name
            var reference = new ClassReferenceExpression("Boo.Bee", "Foo");
            new ClassReferenceExpression("Foo.Boo.Bee1", "Foo");
            new ClassReferenceExpression("Foo.Be3UYT76567g", "Foo");

            Assert.AreEqual("Boo.Bee.Foo", reference.ToString());
        }

        [TestMethod]
        public void ClassReferenceExpressionNameSpaceStartsWithNumber()
        {
            try
            {
                new ClassReferenceExpression("1Foo", "Foo");
                Assert.Fail();
            }
            catch { }
        }

        [TestMethod]
        public void ClassReferenceExpressionNameSpaceEndsWithSymbol()
        {
            try
            {
                new ClassReferenceExpression("Foo@", "Foo");
                Assert.Fail();
            }
            catch { }
        }

        [TestMethod]
        public void ClassReferenceExpressionNameSpaceIsWhitespace()
        {
            try
            {
                new ClassReferenceExpression("  ", "Foo");
                Assert.Fail();
            }
            catch { }
        }

        [TestMethod]
        public void ClassReferenceExpressionNameSpaceHasWhitespace()
        {
            try
            {
                new ClassReferenceExpression("Foo Bar", "Foo");
                Assert.Fail();
            }
            catch { }
        }
    }
}
