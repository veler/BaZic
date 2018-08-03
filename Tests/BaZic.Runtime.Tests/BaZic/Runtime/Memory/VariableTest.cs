using System;
using System.Collections.Generic;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using BaZic.Runtime.BaZic.Runtime.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Runtime.Memory
{
    [TestClass]
    public class VariableTest
    {
        [TestMethod]
        public void MemoryVariable()
        {
            var variable = new Variable(new VariableDeclaration("var1", true));

            variable.SetValue(new int[] { 1, 2, 3 });
            variable.SetValue(new Exception[3] { null, null, null });
            variable.SetValue(new List<string> { "1", "2", "hello" });
            variable.SetValue(new Dictionary<int, string>());

            try
            {
                variable.SetValue(0);
                Assert.Fail();
            }
            catch (NotAssignableException)
            {
            }
            catch (Exception)
            {
                Assert.Fail();
            }



            variable = new Variable(new VariableDeclaration("var1", false));

            variable.SetValue(1);
            variable.SetValue(new Exception());
            variable.SetValue("hello");

            try
            {
                variable.SetValue(new List<string> { "1", "2", "hello" });
                Assert.Fail();
            }
            catch (NotAssignableException)
            {
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }
    }
}
