using System;
using System.Threading.Tasks;

namespace BaZic.Core.ComponentModel.Assemblies
{
    /// <summary>
    /// Provides a marshaled object designed to do a proxy when we want to use a <see cref="Task"/> in a cross-<see cref="AppDomain"/> context.
    /// </summary>
    internal sealed class MarshaledResultSetter : MarshalByRefObject
    {
        #region Fields & Constants

        private TaskCompletionSource<object> _taskCompletionSource = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

        #endregion

        #region Properties

        /// <summary>
        /// Gets the task.
        /// </summary>
        internal Task Task => _taskCompletionSource.Task;

        #endregion

        #region Methods

        /// <summary>
        /// Notifies that the task ended.
        /// </summary>
        internal void NotifyEndTask()
        {
           _taskCompletionSource.TrySetResult(null);
        }

        #endregion
    }

    /// <summary>
    /// Provides a marshaled object designed to do a proxy when we want to use a <see cref="Task"/> in a cross-<see cref="AppDomain"/> context.
    /// </summary>
    /// <typeparam name="T">The type of the task result</typeparam>
    internal sealed class MarshaledResultSetter<T> : MarshalByRefObject
    {
        #region Fields & Constants

        private TaskCompletionSource<T> _taskCompletionSource = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

        #endregion

        #region Properties

        /// <summary>
        /// Gets the task.
        /// </summary>
        internal Task<T> Task => _taskCompletionSource.Task;

        #endregion

        #region Methods

        /// <summary>
        /// Sets the result of the task.
        /// </summary>
        /// <param name="result">The value.</param>
        internal void SetResult(T result)
        {
            _taskCompletionSource.TrySetResult(result);
        }

        #endregion
    }
}
