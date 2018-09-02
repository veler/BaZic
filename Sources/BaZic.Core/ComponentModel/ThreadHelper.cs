using System;
using System.Threading;
using System.Windows;

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
            if (Application.Current != null)
            {
                var priority = System.Windows.Threading.DispatcherPriority.Normal;
                if (isBackground)
                {
                    priority = System.Windows.Threading.DispatcherPriority.Background;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    action();
                }, priority);
            }
            else
            {
                var thread = new Thread((ThreadStart)new SynchronizationCallback(action));
                thread.SetApartmentState(ApartmentState.STA);
                thread.IsBackground = isBackground;
                thread.CurrentCulture = Localization.LocalizationHelper.GetCurrentCulture();
                thread.Start();
                thread.Join();
            }
        }

        #endregion
    }

}
