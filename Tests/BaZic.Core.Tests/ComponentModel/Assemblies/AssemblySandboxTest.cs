using BaZic.Core.ComponentModel.Assemblies;
using BaZic.Core.Logs;
using BaZic.Core.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace BaZic.Core.Tests.ComponentModel.Assemblies
{
    [TestClass]
    public class AssemblySandboxTest
    {
        [TestInitialize]
        public void Initialize()
        {
            Logger.Initialize<LogMock>();
            Logger.Instance.SessionStarted();
        }

        [TestMethod]
        public void AssemblySandboxLoadFromFullName()
        {
            var sandbox1 = new AssemblySandbox();
            var sandbox2 = new AssemblySandbox();

            Assert.AreEqual(0, sandbox1.GetAssemblies().Count);
            Assert.AreEqual(0, sandbox2.GetAssemblies().Count);

            sandbox1.LoadAssembly("PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            Assert.AreEqual("PresentationCore", sandbox1.GetAssemblies().Single().Name);
            Assert.AreEqual(0, sandbox2.GetAssemblies().Count);

            sandbox1.LoadAssembly("PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            Assert.AreEqual("PresentationCore", sandbox1.GetAssemblies().Single().Name);
            Assert.AreEqual(0, sandbox2.GetAssemblies().Count);

            sandbox1.Dispose();
            sandbox2.Dispose();
        }

        [TestMethod]
        public void AssemblySandboxLoadFromPath()
        {
            var sandbox1 = new AssemblySandbox();
            var sandbox2 = new AssemblySandbox();

            Assert.AreEqual(0, sandbox1.GetAssemblies().Count);
            Assert.AreEqual(0, sandbox2.GetAssemblies().Count);

            sandbox1.LoadAssembly(@"C:\WINDOWS\Microsoft.Net\assembly\GAC_32\PresentationCore\v4.0_4.0.0.0__31bf3856ad364e35\PresentationCore.dll");

            Assert.AreEqual("PresentationCore", sandbox1.GetAssemblies().Single().Name);
            Assert.AreEqual(0, sandbox2.GetAssemblies().Count);

            sandbox1.LoadAssembly("PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            Assert.AreEqual("PresentationCore", sandbox1.GetAssemblies().Single().Name);
            Assert.AreEqual(0, sandbox2.GetAssemblies().Count);

            sandbox1.Dispose();
            sandbox2.Dispose();
        }

        [TestMethod]
        public void AssemblySandboxLoadType()
        {
            var sandbox1 = new AssemblySandbox();

            try
            {
                Type.GetType("System.Windows.UIElement", true, false);
                Assert.Fail();
            }
            catch (TypeLoadException)
            {
            }
            catch
            {
                Assert.Fail();
            }

            try
            {
                sandbox1.GetTypeRef("System.Windows.UIElement");
                Assert.Fail();
            }
            catch (TypeLoadException)
            {
            }
            catch
            {
                Assert.Fail();
            }

            sandbox1.LoadAssembly("PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            try
            {
                Type.GetType("System.Windows.UIElement", true, false);
                Assert.Fail();
            }
            catch (TypeLoadException)
            {
            }
            catch
            {
                Assert.Fail();
            }

            Assert.AreEqual(1, sandbox1.GetAssemblies().Count);
            Assert.IsNotNull(sandbox1.GetTypeRef("System.Windows.UIElement", "PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
            Assert.AreEqual(1, sandbox1.GetAssemblies().Count);
            Assert.IsNotNull(sandbox1.GetTypeRef("System.Windows.UIElement"));
            Assert.AreEqual(6, sandbox1.GetAssemblies().Count);

            sandbox1.Dispose();
        }
    }
}
