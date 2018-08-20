using BaZic.Core.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BaZic.Runtime.BaZic.Runtime.Debugger
{
    /// <summary>
    /// Helps to manage the state of the interpreter.
    /// </summary>
    internal sealed class RunningStateManager
    {
        #region Fields & Constants

        private readonly Dictionary<Guid, List<Task>> _unwaitedMethodInvocation = new Dictionary<Guid, List<Task>>();
        private readonly BaZicInterpreterCore _baZicInterpreterCore;

        private bool _isRunningMainFunction;
        private int _externMethodRunningCount;

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BaZicInterpreterCore"/> class.
        /// </summary>
        /// <param name="baZicInterpreterCore">The parent <see cref="BaZicInterpreterCore"/></param>
        internal RunningStateManager(BaZicInterpreterCore baZicInterpreterCore)
        {
            _baZicInterpreterCore = baZicInterpreterCore;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Defines whether the main method is running or not.
        /// </summary>
        /// <param name="isRunning">True if the main method is running.</param>
        internal void SetIsRunningMainFunction(bool isRunning)
        {
            _isRunningMainFunction = isRunning;
        }

        /// <summary>
        /// Notifies that an extern method called by the user at a point started.
        /// </summary>
        internal void StartsExternMethod()
        {
            Interlocked.Increment(ref _externMethodRunningCount);
        }
        /// <summary>
        /// Notifies that an extern method called by the user at a point stopped.
        /// </summary>
        internal void StopsExternMethod()
        {
            Interlocked.Decrement(ref _externMethodRunningCount);
        }

        /// <summary>
        /// Add an unwaited task to the list of task to wait to allow the program to consider itself as done.
        /// </summary>
        internal Guid AddCallStackForUnwaitedMethodInvocation()
        {
            Guid guid;

            lock (_unwaitedMethodInvocation)
            {
                do
                {
                    guid = Guid.NewGuid();
                } while (_unwaitedMethodInvocation.ContainsKey(guid));

                _unwaitedMethodInvocation.Add(guid, new List<Task>());
            }

            return guid;
        }

        /// <summary>
        /// Add an unwaited task to the list of task to wait to allow the program to consider itself as done.
        /// </summary>
        /// <param name="executionFlowId">A GUID that defines in which callstack does this task must be added.</param>
        /// <param name="task">The task to add.</param>
        internal void AddUnwaitedMethodInvocation(Guid executionFlowId, Task task)
        {
            Requires.NotNull(task, nameof(task));
            var tasks = _unwaitedMethodInvocation[executionFlowId];
            lock (tasks)
            {
                tasks.Add(task);
            }
        }

        /// <summary>
        /// Waits for all the unwaited tasks.
        /// </summary>
        /// <param name="executionFlowId">A GUID that defines which callstack must be waited.<param>
        internal void WaitUnwaitedMethodInvocation(Guid executionFlowId)
        {
            var tasks = _unwaitedMethodInvocation[executionFlowId];

            if (tasks == null)
            {
                return;
            }

            var waitThreads = false;
            do
            {
                waitThreads = false;

                Task[] tasksToWait;
                lock (tasks)
                {
                    tasksToWait = new Task[tasks.Count];
                    tasks.CopyTo(tasksToWait);
                }

                Task.WhenAll(tasksToWait).ConfigureAwait(false).GetAwaiter().GetResult();

                var i = tasksToWait.Length;
                while (i < tasks.Count && !waitThreads)
                {
                    var t = tasks[i];
                    waitThreads = !t.IsCanceled && !t.IsCompleted && !t.IsFaulted;
                    i++;
                }
            } while (waitThreads);

            lock (tasks)
            {
                foreach (var task in tasks)
                {
                    task.Dispose();
                }
                tasks.Clear();
            }
        }

        /// <summary>
        /// Waits for all the unwaited tasks.
        /// </summary>
        /// <param name="executionFlowId">A GUID that defines which callstack must be waited.<param>
        internal void WaitAllUnwaitedMethodInvocation()
        {
            foreach (var executionFlow in _unwaitedMethodInvocation)
            {
                WaitUnwaitedMethodInvocation(executionFlow.Key);
            }
        }

        /// <summary>
        /// Determines whether the interpreter must goes in Idle or Running state and set it if possible.
        /// </summary>
        internal void UpdateState()
        {
            if (_baZicInterpreterCore.State == BaZicInterpreterState.Stopped || _baZicInterpreterCore.State == BaZicInterpreterState.StoppedWithError)
            {
                return;
            }

            var newState = BaZicInterpreterState.Running;
            lock (_unwaitedMethodInvocation)
            {
                if (!_isRunningMainFunction && _externMethodRunningCount == 0 && _unwaitedMethodInvocation.All(tasks => tasks.Value.Count == 0))
                {
                    newState = BaZicInterpreterState.Idle;
                }
            }

            _baZicInterpreterCore.ChangeState(this, new BaZicInterpreterStateChangeEventArgs(newState));
        }

        #endregion
    }
}
