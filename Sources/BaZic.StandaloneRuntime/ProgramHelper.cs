using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;

namespace BaZic.StandaloneRuntime
{
    /// <summary>
    /// Provides a set of methods designed to help the generated program to run with the same behavior than with a BaZic code.
    /// </summary>
    public class ProgramHelper
    {
        #region Fields

        private readonly List<Task> _unwaitedMethodInvocation = new List<Task>();

        private readonly string _xamlCode;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Dispatcher of the UI thread.
        /// </summary>
        public Dispatcher UIDispatcher { get; private set; }

        /// <summary>
        /// Gets user interface.
        /// </summary>
        public FrameworkElement UserInterface { get; private set; }

        /// <summary>
        /// Sets the result of the user interface when the window is closing.
        /// </summary>
        public dynamic UiResult { private get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Raised when the Idle state can be set in the BaZicInterpreter.
        /// </summary>
        public event EventHandler IdleStateOccured;

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramHelper"/> class.
        /// </summary>
        /// <param name="xamlCode">The optional XAML code for the UI.</param>
        public ProgramHelper(string xamlCode)
        {
            _xamlCode = xamlCode;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the result of a task. If the task does not return a result, this method will return null.
        /// </summary>
        /// <param name="task">The task to run.</param>
        /// <returns>Null if there is not result.</returns>
        public dynamic RunTaskSynchronously(Task task)
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
                dynamic result = type.GetProperty(nameof(Task<object>.Result)).GetValue(task);
                task.Dispose();
                return result;
            }
        }

        /// <summary>
        /// Await the given task and returns its value of null if the task is a void.
        /// </summary>
        /// <param name="task">The task to run.</param>
        /// <returns>Null if the task is a void.</returns>
        public async Task<dynamic> RunTask(Task task)
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
        public async void WaitAllUnwaitedThreads()
        {
            var waitThreads = true;
            do
            {
                Task[] threads = null;
                lock (_unwaitedMethodInvocation)
                {
                    threads = _unwaitedMethodInvocation.ToArray();
                }

                await Task.WhenAll(threads);

                lock (_unwaitedMethodInvocation)
                {
                    waitThreads = _unwaitedMethodInvocation.Any(t => !t.IsCanceled && !t.IsCompleted && !t.IsFaulted);
                }
            } while (waitThreads);

            _unwaitedMethodInvocation.Clear();
        }

        /// <summary>
        /// Add an unwaited task to the list of task to wait to allow the program to consider itself as done.
        /// </summary>
        /// <param name="task">The task to add.</param>
        /// <returns>The added task.</returns>
        public Task AddUnwaitedThread(Task task)
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
        public dynamic AddUnwaitedThreadIfRequired(dynamic targetObject, string methodName, params dynamic[] args)
        {
            var type = (Type)targetObject.GetType();
            object result = null;

            if (targetObject is FrameworkElement)
            {
                UIDispatcher.Invoke(() =>
                {
                    result = type.InvokeMember(methodName, BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, targetObject, args);
                }, DispatcherPriority.Background);
            }
            else
            {
                result = type.InvokeMember(methodName, BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, targetObject, args);
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
        public dynamic AddUnwaitedThreadIfRequired(Type targetType, string methodName, params dynamic[] args)
        {
            var result = targetType.InvokeMember(methodName, BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, args);

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
            if (result != null && result is Task)
            {
                var task = (Task)result;
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

        /// <summary>
        /// Close the UI.
        /// </summary>
        public void CloseUserInterface()
        {
            try
            {
                UIDispatcher?.Invoke(() =>
                {
                    if (UserInterface is Window window)
                    {
                        window?.Close();
                    }
                    Dispatcher.CurrentDispatcher?.InvokeShutdown();
                }, DispatcherPriority.Send);
            }
            catch { }
        }

        /// <summary>
        /// Load the user interface in memory.
        /// </summary>
        public void LoadUserInterface()
        {
            ProgramResourceManager.LoadResources();
            UserInterface = XamlReader.Parse(_xamlCode) as FrameworkElement;
            UIDispatcher = UserInterface.Dispatcher;
        }

        /// <summary>
        /// Show the user interface of the program.
        /// </summary>
        /// <returns>Returns the result of the Window.Closed event from the user interface.</returns>
        public dynamic ShowUserInterface()
        {
            Exception eventException = null;

            var window = UserInterface as Window;

            if (window != null)
            {
                window.Closed += (sender, e) =>
                {
                    UIDispatcher?.InvokeShutdown();
                };

                window.Loaded += (sender, e) =>
                {
                    IdleStateOccured?.Invoke(this, e);
                };
            }
            else
            {
                IdleStateOccured?.Invoke(this, new EventArgs());
            }

            try
            {
                window?.Show();
                Dispatcher.Run();
            }
            catch (Exception exception)
            {
                eventException = exception;
            }
            finally
            {
                try
                {
                    window?.Close();
                }
                catch { }
                window = null;
                UserInterface = null;
            }

            if (eventException != null)
            {
                throw eventException;
            }

            return UiResult;
        }

        /// <summary>
        /// Gets the specified control from the user interface.
        /// </summary>
        /// <param name="controlName">The name of the control to retrieves.</param>
        /// <returns>Returns null if the control does not exist.</returns>
        public dynamic GetControl(string controlName)
        {
            dynamic dynamic = UserInterface.FindName(controlName);
            return dynamic;
        }

        #endregion
    }
}