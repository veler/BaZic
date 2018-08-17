using System;
using System.Threading;

namespace BaZic.Core.ComponentModel
{
    /// <summary>
    /// Provides a set of functions designed to help to manage threads.
    /// </summary>
    public static class ThreadHelper
    {
        #region Methods

        /// <summary>
        /// Runs an action on STA thread.
        /// </summary>
        /// <param name="action">The action to run.</param>
        /// <param name="isBackground">Defines whether the thread must run in background or not.</param>
        public static void RunOnStaThread(Action action, bool isBackground = false)
        {
            var thread = new Thread((ThreadStart)new SynchronizationCallback(action));
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = isBackground;
            thread.CurrentCulture = Localization.LocalizationHelper.GetCurrentCulture();
            thread.Start();
            thread.Join();
        }

        #endregion
    }

}
