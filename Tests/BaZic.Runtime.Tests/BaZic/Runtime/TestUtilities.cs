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
            results.Add(await RunDebug(inputBaZicCode, xamlCode, args));
            results.Add(await RunDebugOptimized(inputBaZicCode, xamlCode, args));
            results.Add(await RunDebugVerbose(inputBaZicCode, xamlCode, args));
            results.Add(await RunDebugOptimizedVerbose(inputBaZicCode, xamlCode, args));
            results.Add(await RunRelease(inputBaZicCode, xamlCode, args));

            foreach (var item in results)
            {
                Assert.AreEqual(expectedProgramResult, item);
            }
        }

        internal static async Task<string> RunDebug(string inputBaZicCode, string xamlCode, params object[] args)
        {
            using (var interpreter = new BaZicInterpreter(inputBaZicCode, xamlCode, false))
            {
                interpreter.SetDependencies("PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                await interpreter.StartDebugAsync(false, args);

                if (interpreter.Error != null)
                {
                    throw interpreter.Error.Exception;
                }

                return interpreter.ProgramResult?.ToString();
            }
        }

        internal static async Task<string> RunDebugOptimized(string inputBaZicCode, string xamlCode, params object[] args)
        {
            using (var interpreter = new BaZicInterpreter(inputBaZicCode, xamlCode, true))
            {
                interpreter.SetDependencies("PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                await interpreter.StartDebugAsync(false, args);

                if (interpreter.Error != null)
                {
                    throw interpreter.Error.Exception;
                }

                return interpreter.ProgramResult?.ToString();
            }
        }

        internal static async Task<string> RunDebugVerbose(string inputBaZicCode, string xamlCode, params object[] args)
        {
            using (var interpreter = new BaZicInterpreter(inputBaZicCode, xamlCode, false))
            {
                interpreter.SetDependencies("PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                await interpreter.StartDebugAsync(true, args);

                if (interpreter.Error != null)
                {
                    throw interpreter.Error.Exception;
                }

                return interpreter.ProgramResult?.ToString();
            }
        }

        internal static async Task<string> RunDebugOptimizedVerbose(string inputBaZicCode, string xamlCode, params object[] args)
        {
            using (var interpreter = new BaZicInterpreter(inputBaZicCode, xamlCode, true))
            {
                interpreter.SetDependencies("PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                await interpreter.StartDebugAsync(true, args);

                if (interpreter.Error != null)
                {
                    throw interpreter.Error.Exception;
                }

                return interpreter.ProgramResult?.ToString();
            }
        }

        internal static async Task<string> RunRelease(string inputBaZicCode, string xamlCode, params object[] args)
        {
            using (var interpreter = new BaZicInterpreter(inputBaZicCode, xamlCode, false))
            {
                interpreter.SetDependencies("PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                await interpreter.StartReleaseAsync(false, args);

                if (interpreter.Error != null)
                {
                    throw interpreter.Error.Exception;
                }

                return interpreter.ProgramResult?.ToString();
            }
        }
    }
}
