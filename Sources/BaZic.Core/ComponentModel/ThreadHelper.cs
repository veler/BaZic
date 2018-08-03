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
        public static void RunOnStaThread(Action action)
        {
            var thread = new Thread((ThreadStart)new SynchronizationCallback(action));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        #endregion
    }

}
