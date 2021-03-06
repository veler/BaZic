// Helper for CSharp generated code.

namespace BaZicProgramReleaseMode
{
    /// <summary>
    /// Provides a set of methods designed to help the generated program to run with the same behavior than with a BaZic code.
    /// </summary>
    public partial class ProgramHelper
    {
        #region Fields

        private readonly System.Collections.Generic.List<System.Threading.Tasks.Task> _unwaitedMethodInvocation = new System.Collections.Generic.List<System.Threading.Tasks.Task>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Dispatcher of the UI thread.
        /// </summary>
        public System.Windows.Threading.Dispatcher UIDispatcher { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Entry point of the entire application.
        /// </summary>
        /// <param name="args"></param>
        [System.STAThreadAttribute()]
        public static void Main(string[] args)
        {
            var instance = new Program();
            instance.Main(args);
        }

        /// <summary>
        /// Returns the result of a task. If the task does not return a result, this method will return null.
        /// </summary>
        /// <param name="task">The task to run.</param>
        /// <returns>Null if there is not result.</returns>
        internal dynamic RunTaskSynchronously(System.Threading.Tasks.Task task)
        {
            task.Wait();
            var type = task.GetType();
            if (!type.IsGenericType)
            {
                task.Dispose();
                return null;
            }
            else
            {
                dynamic result = type.GetProperty(nameof(System.Threading.Tasks.Task<System.Object>.Result)).GetValue(task);
                task.Dispose();
                return result;
            }
        }

        /// <summary>
        /// Await the given task and returns its value of null if the task is a void.
        /// </summary>
        /// <param name="task">The task to run.</param>
        /// <returns>Null if the task is a void.</returns>
        internal async System.Threading.Tasks.Task<dynamic> RunTask(System.Threading.Tasks.Task task)
        {
            await task;
            if (!task.GetType().IsGenericType)
            {
                task.Dispose();
                return null;
            }
            else
            {
                return task;
            }
        }

        /// <summary>
        /// Wait for all the unwaited tasks that have been detected during the program execution.
        /// </summary>
        internal async void WaitAllUnwaitedThreads()
        {
            var waitThreads = true;
            do
            {
                System.Threading.Tasks.Task[] threads = null;
                lock (_unwaitedMethodInvocation)
                {
                    threads = _unwaitedMethodInvocation.ToArray();
                }

                await System.Threading.Tasks.Task.WhenAll(threads);

                lock (_unwaitedMethodInvocation)
                {
                    waitThreads = System.Linq.Enumerable.Any(_unwaitedMethodInvocation, t => !t.IsCanceled && !t.IsCompleted && !t.IsFaulted);
                }
            } while (waitThreads);

            _unwaitedMethodInvocation.Clear();
        }

        /// <summary>
        /// Add an unwaited task to the list of task to wait to allow the program to consider itself as done.
        /// </summary>
        /// <param name="task">The task to add.</param>
        /// <returns>The added task.</returns>
        internal System.Threading.Tasks.Task AddUnwaitedThread(System.Threading.Tasks.Task task)
        {
            lock (_unwaitedMethodInvocation)
            {
                _unwaitedMethodInvocation.Add(task);
            }

            return task;
        }

        /// <summary>
        /// If the result of the specified function is a <see cref="Task"/>, adds an unwaited task to the list of task to wait to allow the program to consider itself as done.
        /// </summary>
        /// <param name="targetObject">The object that contains the method to invoke.</param>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="args">The arguments of the method.</param>
        /// <returns>The added task.</returns>
        internal dynamic AddUnwaitedThreadIfRequired(dynamic targetObject, string methodName, params dynamic[] args)
        {
            var type = (System.Type)targetObject.GetType();
            object result = null;

            if (targetObject is System.Windows.FrameworkElement)
            {
                UIDispatcher.Invoke(() =>
                {
                    result = type.InvokeMember(methodName, System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, null, targetObject, args);
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
            else
            {
                result = type.InvokeMember(methodName, System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, null, targetObject, args);
            }

            return AddUnwaitedThreadIfRequired(result);
        }

        /// <summary>
        /// If the result of the specified function is a <see cref="Task"/>, adds an unwaited task to the list of task to wait to allow the program to consider itself as done.
        /// </summary>
        /// <param name="targetType">The type that contains the static method to invoke.</param>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="args">The arguments of the method.</param>
        /// <returns>The added task.</returns>
        internal dynamic AddUnwaitedThreadIfRequired(System.Type targetType, string methodName, params dynamic[] args)
        {
            var result = targetType.InvokeMember(methodName, System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, null, null, args);

            return AddUnwaitedThreadIfRequired(result);
        }

        /// <summary>
        /// If the result of the specified function is a <see cref="Task"/>, adds an unwaited task to the list of task to wait to allow the program to consider itself as done.
        /// </summary>
        /// <param name="targetObject">The object that contains the method to invoke.</param>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="args">The arguments of the method.</param>
        /// <returns>The added task.</returns>
        private dynamic AddUnwaitedThreadIfRequired(dynamic result)
        {
            if (result != null && result is System.Threading.Tasks.Task)
            {
                var task = (System.Threading.Tasks.Task)result;
                lock (_unwaitedMethodInvocation)
                {
                    var taskType = task.GetType();
                    if (!taskType.IsGenericType)
                    {
                        _unwaitedMethodInvocation.Add(task);
                        result = null;
                    }
                    else
                    {
                        _unwaitedMethodInvocation.Add(task);
                        result = task;
                    }
                }
            }

            return result;
        }

        #endregion
    }
}