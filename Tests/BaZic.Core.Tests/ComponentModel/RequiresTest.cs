using BaZic.Core.ComponentModel;
using BaZic.Core.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Core.Tests.ComponentModel
{
    [TestClass]
    public class RequiresTest
    {
        [TestMethod]
        [ExpectedException(typeof(NotNullRequiredException))]
        public void RequiresNotNull()
        {
            Requires.NotNull(null, "param1");
        }

        [TestMethod]
        [ExpectedException(typeof(NotNullOrEmptyRequiredException))]
        public void RequiresNotNullOrEmpty()
        {
            Requires.NotNullOrEmpty("", "param1");
        }

        [TestMethod]
        [ExpectedException(typeof(NotNullOrWhiteSpaceRequiredException))]
        public void RequiresNotNullOrWhiteSpace()
        {
            Requires.NotNullOrWhiteSpace(" ", "param1");
        }

        [TestMethod]
        [ExpectedException(typeof(IsFalseRequiredException))]
        public void RequiresIsFalse()
        {
            Requires.IsFalse(true);
        }

        [TestMethod]
        [ExpectedException(typeof(IsTrueRequiredException))]
        public void RequiresIsTrue()
        {
            Requires.IsTrue(false);
        }
    }
}
