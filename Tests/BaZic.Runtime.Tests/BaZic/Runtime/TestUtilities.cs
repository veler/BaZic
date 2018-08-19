using BaZic.Core.Logs;
using BaZic.Core.Tests.Mocks;
using BaZic.Runtime.BaZic.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BaZic.Runtime.Tests.BaZic.Runtime
{
    internal static class TestUtilities
    {
        internal static void InitializeLogs()
        {
            Logger.Initialize<LogMock>();
            Logger.Instance.SessionStarted();
            Localization.LocalizationHelper.SetCurrentCulture(new System.Globalization.CultureInfo("en"));
        }

        internal static async Task TestAllRunningMode(string expectedProgramResult, string inputBaZicCode, string xamlCode = "", params object[] args)
        {
            Localization.LocalizationHelper.SetCurrentCulture(new System.Globalization.CultureInfo("en"));

            var results = new List<string>();

            var tasks = new Task[]
            {
                RunDebug(results, inputBaZicCode, xamlCode, args),
                RunDebugOptimized(results, inputBaZicCode, xamlCode, args),
                RunDebugVerbose(results, inputBaZicCode, xamlCode, args),
                RunDebugOptimizedVerbose(results, inputBaZicCode, xamlCode, args),
                RunRelease(results, inputBaZicCode, xamlCode, args)
            };

            await Task.WhenAll(tasks);

            if (results.Count == 0)
            {
                Assert.Fail();
            }

            foreach (var item in results)
            {
                Assert.AreEqual(expectedProgramResult, item);
            }
        }

        internal static async Task RunDebug(List<string> resultReceiver, string inputBaZicCode, string xamlCode, params object[] args)
        {
            using (var interpreter = new BaZicInterpreter(inputBaZicCode, xamlCode, false))
            {
                interpreter.SetDependencies("PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                await interpreter.StartDebugAsync(false, args);

                if (interpreter.Error != null)
                {
                    throw interpreter.Error.Exception;
                }

                resultReceiver.Add(interpreter.ProgramResult?.ToString());
            }
        }

        internal static async Task RunDebugOptimized(List<string> resultReceiver, string inputBaZicCode, string xamlCode, params object[] args)
        {
            using (var interpreter = new BaZicInterpreter(inputBaZicCode, xamlCode, true))
            {
                interpreter.SetDependencies("PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                await interpreter.StartDebugAsync(false, args);

                if (interpreter.Error != null)
                {
                    throw interpreter.Error.Exception;
                }

                resultReceiver.Add(interpreter.ProgramResult?.ToString());
            }
        }

        internal static async Task RunDebugVerbose(List<string> resultReceiver, string inputBaZicCode, string xamlCode, params object[] args)
        {
            using (var interpreter = new BaZicInterpreter(inputBaZicCode, xamlCode, false))
            {
                interpreter.SetDependencies("PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                await interpreter.StartDebugAsync(true, args);

                if (interpreter.Error != null)
                {
                    throw interpreter.Error.Exception;
                }

                resultReceiver.Add(interpreter.ProgramResult?.ToString());
            }
        }

        internal static async Task RunDebugOptimizedVerbose(List<string> resultReceiver, string inputBaZicCode, string xamlCode, params object[] args)
        {
            using (var interpreter = new BaZicInterpreter(inputBaZicCode, xamlCode, true))
            {
                interpreter.SetDependencies("PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                await interpreter.StartDebugAsync(true, args);

                if (interpreter.Error != null)
                {
                    throw interpreter.Error.Exception;
                }

                resultReceiver.Add(interpreter.ProgramResult?.ToString());
            }
        }

        internal static async Task RunRelease(List<string> resultReceiver, string inputBaZicCode, string xamlCode, params object[] args)
        {
            using (var interpreter = new BaZicInterpreter(inputBaZicCode, xamlCode, false))
            {
                interpreter.SetDependencies("PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                await interpreter.StartReleaseAsync(false, args);

                if (interpreter.Error != null)
                {
                    throw interpreter.Error.Exception;
                }

                resultReceiver.Add(interpreter.ProgramResult?.ToString());
            }
        }
    }
}
