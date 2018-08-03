using System;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading;

namespace BaZic.Core.ComponentModel.Assemblies
{
    /// <summary>
    /// Provides a set of methods designed to manage the application domains.
    /// </summary>
    internal static class AppDomainManager
    {
        #region Methods

        /// <summary>
        /// Unloads the specified application domain.
        /// </summary>
        /// <param name="appDomain">An application domain to unload.</param>
        internal static void UnloadAppDomain(AppDomain appDomain)
        {
            Requires.NotNull(appDomain, nameof(appDomain));
            if (appDomain == AppDomain.CurrentDomain)
            {
                throw new CannotUnloadAppDomainException($"The AppDomain to unload cannot be the main AppDomain.");
            }

            try
            {
                GC.Collect();
                AppDomain.Unload(appDomain);
            }
            catch (CannotUnloadAppDomainException)
            {
                var i = 0;
                while (i < 10)
                {
                    Thread.Sleep(1000);
                    try
                    {
                        GC.Collect();
                        AppDomain.Unload(appDomain);
                    }
                    catch (Exception)
                    {
                        i++;
                        continue;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Creates a new AppDomain based on the parent AppDomains 
        /// Evidence and AppDomainSetup
        /// </summary>
        /// <returns>A newly created AppDomain</returns>
        internal static AppDomain CreateUniqueAppDomain()
        {
            return CreateUniqueAppDomain(AppDomain.CurrentDomain, Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Creates a new AppDomain based on the parent AppDomains 
        /// Evidence and AppDomainSetup
        /// </summary>
        /// <param name="parentDomain">The parent AppDomain</param>
        /// <param name="friendlyName">The friendly name to give to the AppDomain</param>
        /// <returns>A newly created AppDomain</returns>
        private static AppDomain CreateUniqueAppDomain(AppDomain parentDomain, string friendlyName)
        {
            Requires.NotNull(parentDomain, nameof(parentDomain));
            Requires.NotNullOrWhiteSpace(friendlyName, nameof(friendlyName));

            var evidence = new Evidence(parentDomain.Evidence);
            var setup = parentDomain.SetupInformation;
            setup.ShadowCopyFiles = bool.TrueString;
            var permissions = new PermissionSet(PermissionState.Unrestricted);
            return AppDomain.CreateDomain(friendlyName, evidence, setup, permissions);
        }

        #endregion
    }
}
