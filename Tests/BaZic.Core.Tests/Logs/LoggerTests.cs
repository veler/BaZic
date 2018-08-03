using System;
using BaZic.Core.Logs;
using BaZic.Core.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Core.Tests.Logs
{
    [TestClass]
    public class LoggerTests
    {
        [TestMethod]
        public void Logs()
        {
            try
            {
                var instance = LogMock.Instance;
                Assert.Fail();
            }
            catch
            {
            }

            Logger.Initialize<LogMock>();
            Logger.Instance.SessionStarted();
            Assert.IsNotNull(Logger.Instance);
            Assert.IsInstanceOfType(Logger.Instance, typeof(LogMock));

            var logMock = (LogMock)Logger.Instance;
            var date1 = DateTime.Now.ToString("dd'/'MM'/'yyyy HH:mm:ss");

            Logger.Instance.Information("This is an info");
            Logger.Instance.Information("This is another info", "Hello");
            
            Logger.Instance.Debug("This is a debug");
            Logger.Instance.Warning("This is a warn");
            Logger.Instance.Error(new Exception("Error"));
            Logger.Instance.Information("Hello world");

            var date2 = DateTime.Now.ToString("dd'/'MM'/'yyyy HH:mm:ss");
            Assert.AreEqual(logMock.GetLogs().ToString(), "");
            Logger.Instance.Flush();
            Assert.AreNotEqual(logMock.GetLogs().ToString(), "");

            try
            {
                Logger.Instance.Fatal("This is a fatal error", new Exception("Error"));
                Assert.Fail();
            }
            catch
            {
            }

            try
            {
                Logger.Instance.Information("This is an info");
                Assert.Fail();
            }
            catch
            {
            }

            var logs = logMock.GetLogs().ToString();
#if DEBUG
            Assert.IsTrue(logs.Contains($"[Debug] [Logs] [{date1}] This is a debug"));
#endif

            Assert.IsTrue(logs.Contains($"[Information] [Logs] [{date1}] This is an info"));
            Assert.IsTrue(logs.Contains($"[Information] [Hello] [{date1}] This is another info"));
            Assert.IsTrue(logs.Contains($"[Warning] [Logs] [{date1}] This is a warn"));
            Assert.IsTrue(logs.Contains($"[Error] [Logs] [{date1}] Error"));
            Assert.IsTrue(logs.Contains($"[Information] [Logs] [{date1}] Hello world"));
            Assert.IsTrue(logs.Contains($"[Fatal] [Logs in LoggerTests.cs, line 46] [{date2}] This is a fatal error"));
            Assert.IsTrue(logs.Contains($"[Fatal] [Additional Information] [{date2}] Additional info..."));
        }
    }
}
