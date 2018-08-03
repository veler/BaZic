using BaZicProgramReleaseMode;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Core.Tests.ComponentModel.Dictionary
{
    [TestClass]
    public class ObservableDictionaryTest
    {
        [TestMethod]
        public void ObservableDictionary()
        {
            var list = new ObservableDictionary() { "hello" };

            list.Add("world");
            list["yo"] = "heyy";
            list.Add("saturn");
            list.Add("pluto");

            Assert.IsTrue(list.ContainsKey(0));
            Assert.IsTrue(list.ContainsKey(1));
            Assert.IsTrue(list.ContainsKey("yo"));
            Assert.IsTrue(list.ContainsKey(3));
            Assert.IsTrue(list.ContainsKey(4));

            list.Remove(3);
            list.Remove(4);
            list.Add("hello there");

            Assert.AreEqual(4, list.Count);

            Assert.IsTrue(list.ContainsKey(0));
            Assert.IsTrue(list.ContainsKey(1));
            Assert.IsTrue(list.ContainsKey("yo"));
            Assert.IsTrue(list.ContainsKey(3));
        }
    }
}
