using BaZic.Runtime.BaZic.Runtime.Debugger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace BaZic.Runtime.Tests.BaZic.Runtime.Debugger
{
    [TestClass]
    public class ValueInfoTest
    {
        [TestInitialize]
        public void Initialize()
        {
            TestUtilities.InitializeLogs();
        }

        [TestMethod]
        public void ValueInfoNull()
        {
            var info = ValueInfo.GetValueInfo(null);
            Assert.IsTrue(info.IsNull);
            Assert.IsFalse(info.IsArray);
            Assert.IsFalse(info.IsPrimitive);
            Assert.AreEqual(0, info.Length);
            Assert.IsNull(info.Type);
        }

        [TestMethod]
        public void ValueInfoPrimitive1()
        {
            var info = ValueInfo.GetValueInfo(1);
            Assert.IsFalse(info.IsNull);
            Assert.IsFalse(info.IsArray);
            Assert.IsTrue(info.IsPrimitive);
            Assert.AreEqual(0, info.Length);
            Assert.AreEqual(typeof(int), info.Type);
        }

        [TestMethod]
        public void ValueInfoPrimitive2()
        {
            var info = ValueInfo.GetValueInfo(1.234);
            Assert.IsFalse(info.IsNull);
            Assert.IsFalse(info.IsArray);
            Assert.IsTrue(info.IsPrimitive);
            Assert.AreEqual(0, info.Length);
            Assert.AreEqual(typeof(double), info.Type);
        }

        [TestMethod]
        public void ValueInfoPrimitive3()
        {
            var info = ValueInfo.GetValueInfo(true);
            Assert.IsFalse(info.IsNull);
            Assert.IsFalse(info.IsArray);
            Assert.IsTrue(info.IsPrimitive);
            Assert.AreEqual(0, info.Length);
            Assert.AreEqual(typeof(bool), info.Type);
        }

        [TestMethod]
        public void ValueInfoPrimitive4()
        {
            var info = ValueInfo.GetValueInfo('c');
            Assert.IsFalse(info.IsNull);
            Assert.IsFalse(info.IsArray);
            Assert.IsTrue(info.IsPrimitive);
            Assert.AreEqual(0, info.Length);
            Assert.AreEqual(typeof(char), info.Type);
        }

        [TestMethod]
        public void ValueInfoPrimitive5()
        {
            var info = ValueInfo.GetValueInfo("Hello");
            Assert.IsFalse(info.IsNull);
            Assert.IsFalse(info.IsArray);
            Assert.IsTrue(info.IsPrimitive);
            Assert.AreEqual(5, info.Length);
            Assert.AreEqual(typeof(string), info.Type);
        }

        [TestMethod]
        public void ValueInfoPrimitive6()
        {
            var info = ValueInfo.GetValueInfo(new Exception("hello"));
            Assert.IsFalse(info.IsNull);
            Assert.IsFalse(info.IsArray);
            Assert.IsFalse(info.IsPrimitive);
            Assert.AreEqual(0, info.Length);
            Assert.AreEqual(typeof(Exception), info.Type);
        }

        [TestMethod]
        public void ValueInfoPrimitive7()
        {
            var info = ValueInfo.GetValueInfo(new int[1]);
            Assert.IsFalse(info.IsNull);
            Assert.IsTrue(info.IsArray);
            Assert.IsTrue(info.IsPrimitive);
            Assert.AreEqual(1, info.Length);
            Assert.AreEqual(typeof(int[]), info.Type);
        }

        [TestMethod]
        public void ValueInfoArray1()
        {
            var info = ValueInfo.GetValueInfo(new object[] { 1, 2, "hello" });
            Assert.IsFalse(info.IsNull);
            Assert.IsTrue(info.IsArray);
            Assert.IsFalse(info.IsPrimitive);
            Assert.AreEqual(3, info.Length);
            Assert.AreEqual(typeof(object[]), info.Type);
        }

        [TestMethod]
        public void ValueInfoArray2()
        {
            var info = ValueInfo.GetValueInfo(new int[3] { 1, 2, 3 });
            Assert.IsFalse(info.IsNull);
            Assert.IsTrue(info.IsArray);
            Assert.IsTrue(info.IsPrimitive);
            Assert.AreEqual(3, info.Length);
            Assert.AreEqual(typeof(int[]), info.Type);
        }

        [TestMethod]
        public void ValueInfoArray3()
        {
            var info = ValueInfo.GetValueInfo(new Exception[3] { null, null, null });
            Assert.IsFalse(info.IsNull);
            Assert.IsTrue(info.IsArray);
            Assert.IsFalse(info.IsPrimitive);
            Assert.AreEqual(3, info.Length);
            Assert.AreEqual(typeof(Exception[]), info.Type);
        }

        [TestMethod]
        public void ValueInfoArray4()
        {
            var info = ValueInfo.GetValueInfo(new List<string> { "1", "2", "hello" });
            Assert.IsFalse(info.IsNull);
            Assert.IsTrue(info.IsArray);
            Assert.IsFalse(info.IsPrimitive);
            Assert.AreEqual(3, info.Length);
            Assert.AreEqual(typeof(List<string>), info.Type);
        }
    }
}
