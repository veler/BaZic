using BaZic.Runtime.BaZic.Code;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Code.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Code
{
    [TestClass]
    public class CSharpCodeGeneratorTest
    {
        [TestMethod]
        public void CSharpCodeGeneratorIndent()
        {
            var program = new BaZicProgram()
                          .WithVariables(
                                new VariableDeclaration("Foo")
                          ).WithMethods(
                                new EntryPointMethod()
                                .WithBody(
                                    new VariableDeclaration("Bar", true),
                                    new IterationStatement(new BinaryOperatorExpression(new VariableReferenceExpression("Foo"), BinaryOperatorType.Equality, new VariableReferenceExpression("Bar")))
                                    .WithBody(
                                        new ConditionStatement(new PrimitiveExpression(true))
                                        .WithThenBody(
                                            new CommentStatement("If true")
                                        ).WithElseBody(
                                            new CommentStatement("If not true")
                                        )
                                    )
                                )
                          );

            var code = new CSharpCodeGenerator().Generate(program);

            var expected =
@"// CSharp code generated automatically

namespace BaZicProgramReleaseMode
{
    [System.Serializable]
    public sealed class Program : System.MarshalByRefObject
    {
        dynamic Foo = null;

        [System.LoaderOptimization(System.LoaderOptimization.MultiDomainHost)]
        public dynamic Main(dynamic args)
        {
            try {
            dynamic Bar = null;
            while (Foo == Bar)
            {
                if (true)
                {
                    // If true
                }
                else
                {
                    // If not true
                }
            }
            } finally {
            ProgramHelper.WaitAllUnwaitedThreads();
            }
            return null;
        }
    }
}

// Helper for CSharp generated code.

namespace BaZicProgramReleaseMode
{
    /// <summary>
    /// Provides a set of methods designed to help the generated program to run with the same behavior than with a BaZic code.
    /// </summary>
    internal static class ProgramHelper
    {
        #region Fields

        private readonly static System.Collections.Generic.List<System.Threading.Tasks.Task> _unwaitedMethodInvocation = new System.Collections.Generic.List<System.Threading.Tasks.Task>();

        #endregion

        #region Methods

        /// <summary>
        /// Returns the result of a task. If the task does not return a result, this method will return null.
        /// </summary>
        /// <param name=""task"">The task to run.</param>
        /// <returns>Null if there is not result.</returns>
        internal static dynamic RunTaskSynchronously(System.Threading.Tasks.Task task)
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
        /// <param name=""task"">The task to run.</param>
        /// <returns>Null if the task is a void.</returns>
        internal static async System.Threading.Tasks.Task<dynamic> RunTask(System.Threading.Tasks.Task task)
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
        /// Runs an action on STA thread.
        /// </summary>
        /// <param name=""func"">The function to run.</param>
        internal static dynamic RunOnStaThread(System.Func<dynamic> func)
        {
            dynamic result = null;
            var thread = new System.Threading.Thread(new System.Threading.ThreadStart(() =>
            {
                result = func();
            }));
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
            thread.Join();
            return result;
        }

        /// <summary>
        /// Wait for all the unwaited tasks that have been detected during the program execution.
        /// </summary>
        internal static async void WaitAllUnwaitedThreads()
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
        /// <param name=""task"">The task to add.</param>
        /// <returns>The added task.</returns>
        internal static System.Threading.Tasks.Task AddUnwaitedThread(System.Threading.Tasks.Task task)
        {
            lock (_unwaitedMethodInvocation)
            {
                _unwaitedMethodInvocation.Add(task);
            }

            return task;
        }

        /// <summary>
        /// If the result of the specified function is a <see cref=""Task""/>, adds an unwaited task to the list of task to wait to allow the program to consider itself as done.
        /// </summary>
        /// <param name=""targetObject"">The object that contains the method to invoke.</param>
        /// <param name=""methodName"">The name of the method to invoke.</param>
        /// <param name=""args"">The arguments of the method.</param>
        /// <returns>The added task.</returns>
        internal static dynamic AddUnwaitedThreadIfRequired(dynamic targetObject, string methodName, params dynamic[] args)
        {
            var type = (System.Type)targetObject.GetType();
            var result = type.InvokeMember(methodName, System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, null, targetObject, args);

            return AddUnwaitedThreadIfRequired(result);
        }

        /// <summary>
        /// If the result of the specified function is a <see cref=""Task""/>, adds an unwaited task to the list of task to wait to allow the program to consider itself as done.
        /// </summary>
        /// <param name=""targetType"">The type that contains the static method to invoke.</param>
        /// <param name=""methodName"">The name of the method to invoke.</param>
        /// <param name=""args"">The arguments of the method.</param>
        /// <returns>The added task.</returns>
        internal static dynamic AddUnwaitedThreadIfRequired(System.Type targetType, string methodName, params dynamic[] args)
        {
            var result = targetType.InvokeMember(methodName, System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, null, null, args);

            return AddUnwaitedThreadIfRequired(result);
        }

        /// <summary>
        /// If the result of the specified function is a <see cref=""Task""/>, adds an unwaited task to the list of task to wait to allow the program to consider itself as done.
        /// </summary>
        /// <param name=""targetObject"">The object that contains the method to invoke.</param>
        /// <param name=""methodName"">The name of the method to invoke.</param>
        /// <param name=""args"">The arguments of the method.</param>
        /// <returns>The added task.</returns>
        private static dynamic AddUnwaitedThreadIfRequired(dynamic result)
        {
            if (result is System.Threading.Tasks.Task task)
            {
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
}";

            Assert.IsTrue(code.Contains(expected));
        }

        [TestMethod]
        public void CSharpCodeGeneratorUi()
        {
            var inputCodeUi =
@"
BIND ListBox1_ItemsSource[] = NEW [""Value 1"", ""Value 2""]
BIND TextBox1_Text = ""Value to add""
VARIABLE var1

EXTERN FUNCTION Main(args[])
END FUNCTION

EVENT FUNCTION Button1_Click()
    ListBox1_ItemsSource.Add(TextBox1_Text)
END FUNCTION

# The XAML will be provided separatly";

            var xamlCode = @"
<Window xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
    <StackPanel>
        <TextBox Name=""TextBox1""/>
        <Button Name=""Button1"" Content=""Add a value""/>
        <ListBox Name=""ListBox1""/>
    </StackPanel>
</Window>";

            var program = new BaZicParser().Parse(inputCodeUi, xamlCode, true).Program;

            var code = new CSharpCodeGenerator().Generate(program);

            var expected =
@"// CSharp code generated automatically

namespace BaZicProgramReleaseMode
{
    [System.Serializable]
    public sealed class Program : System.MarshalByRefObject
    {
        private dynamic var1 = null;

        private dynamic ListBox1_ItemsSource { get { dynamic result = ProgramUiHelper.Instance.GetControl(""ListBox1"").ItemsSource; return result; } set { ProgramUiHelper.Instance.GetControl(""ListBox1"").ItemsSource = value; } }
        private dynamic TextBox1_Text { get { dynamic result = ProgramUiHelper.Instance.GetControl(""TextBox1"").Text; return result; } set { ProgramUiHelper.Instance.GetControl(""TextBox1"").Text = value; } }

        public Program()
        {
            ProgramUiHelper.CreateNewInstance();
        }

        internal dynamic Button1_Click()
        {
            ProgramHelper.AddUnwaitedThreadIfRequired(ListBox1_ItemsSource, ""Add"", TextBox1_Text);
            return null;
        }

        [System.LoaderOptimization(System.LoaderOptimization.MultiDomainHost)]
        public dynamic Main(dynamic args)
        {
            try {

            return ProgramHelper.RunOnStaThread(() => {
            ProgramUiHelper.Instance.LoadWindow();
            ListBox1_ItemsSource = new BaZicProgramReleaseMode.ObservableDictionary() { ""Value 1"", ""Value 2"" };
            TextBox1_Text = ""Value to add"";
            ((System.Windows.Controls.Button)ProgramUiHelper.Instance.GetControl(""Button1"")).Click += (sender, e) => { Button1_Click(); };
            return ProgramUiHelper.Instance.ShowWindow();
            });
            } finally {
            ProgramHelper.WaitAllUnwaitedThreads();
            }
            return null;
        }
    }
}

// Helper for CSharp generated code.

namespace BaZicProgramReleaseMode
{
    /// <summary>
    /// Provides a set of methods designed to help the generated program to run with the same behavior than with a BaZic code.
    /// </summary>
    internal static class ProgramHelper
    {
        #region Fields

        private readonly static System.Collections.Generic.List<System.Threading.Tasks.Task> _unwaitedMethodInvocation = new System.Collections.Generic.List<System.Threading.Tasks.Task>();

        #endregion

        #region Methods

        /// <summary>
        /// Returns the result of a task. If the task does not return a result, this method will return null.
        /// </summary>
        /// <param name=""task"">The task to run.</param>
        /// <returns>Null if there is not result.</returns>
        internal static dynamic RunTaskSynchronously(System.Threading.Tasks.Task task)
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
        /// <param name=""task"">The task to run.</param>
        /// <returns>Null if the task is a void.</returns>
        internal static async System.Threading.Tasks.Task<dynamic> RunTask(System.Threading.Tasks.Task task)
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
        /// Runs an action on STA thread.
        /// </summary>
        /// <param name=""func"">The function to run.</param>
        internal static dynamic RunOnStaThread(System.Func<dynamic> func)
        {
            dynamic result = null;
            var thread = new System.Threading.Thread(new System.Threading.ThreadStart(() =>
            {
                result = func();
            }));
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
            thread.Join();
            return result;
        }

        /// <summary>
        /// Wait for all the unwaited tasks that have been detected during the program execution.
        /// </summary>
        internal static async void WaitAllUnwaitedThreads()
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
        /// <param name=""task"">The task to add.</param>
        /// <returns>The added task.</returns>
        internal static System.Threading.Tasks.Task AddUnwaitedThread(System.Threading.Tasks.Task task)
        {
            lock (_unwaitedMethodInvocation)
            {
                _unwaitedMethodInvocation.Add(task);
            }

            return task;
        }

        /// <summary>
        /// If the result of the specified function is a <see cref=""Task""/>, adds an unwaited task to the list of task to wait to allow the program to consider itself as done.
        /// </summary>
        /// <param name=""targetObject"">The object that contains the method to invoke.</param>
        /// <param name=""methodName"">The name of the method to invoke.</param>
        /// <param name=""args"">The arguments of the method.</param>
        /// <returns>The added task.</returns>
        internal static dynamic AddUnwaitedThreadIfRequired(dynamic targetObject, string methodName, params dynamic[] args)
        {
            var type = (System.Type)targetObject.GetType();
            var result = type.InvokeMember(methodName, System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, null, targetObject, args);

            return AddUnwaitedThreadIfRequired(result);
        }

        /// <summary>
        /// If the result of the specified function is a <see cref=""Task""/>, adds an unwaited task to the list of task to wait to allow the program to consider itself as done.
        /// </summary>
        /// <param name=""targetType"">The type that contains the static method to invoke.</param>
        /// <param name=""methodName"">The name of the method to invoke.</param>
        /// <param name=""args"">The arguments of the method.</param>
        /// <returns>The added task.</returns>
        internal static dynamic AddUnwaitedThreadIfRequired(System.Type targetType, string methodName, params dynamic[] args)
        {
            var result = targetType.InvokeMember(methodName, System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, null, null, args);

            return AddUnwaitedThreadIfRequired(result);
        }

        /// <summary>
        /// If the result of the specified function is a <see cref=""Task""/>, adds an unwaited task to the list of task to wait to allow the program to consider itself as done.
        /// </summary>
        /// <param name=""targetObject"">The object that contains the method to invoke.</param>
        /// <param name=""methodName"">The name of the method to invoke.</param>
        /// <param name=""args"">The arguments of the method.</param>
        /// <returns>The added task.</returns>
        private static dynamic AddUnwaitedThreadIfRequired(dynamic result)
        {
            if (result is System.Threading.Tasks.Task task)
            {
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

// Helper for CSharp generated code.

namespace BaZicProgramReleaseMode
{
    /// <summary>
    /// Provides a set of methods designed to help the generated program to run with the same behavior than with a BaZic code.
    /// </summary>
    internal sealed class ProgramUiHelper
    {
        #region Fields & Constants

        private string _xamlCode = ""\r\n<Window xmlns=\""http://schemas.microsoft.com/winfx/2006/xaml/presentation\"">\r\n    <StackPanel>\r\n        <TextBox Name=\""TextBox1\""/>\r\n        <Button Name=\""Button1\"" Content=\""Add a value\""/>\r\n        <ListBox Name=\""ListBox1\""/>\r\n    </StackPanel>\r\n</Window>"";
        private System.Windows.Window _userInterface;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current instance of the helper.
        /// </summary>
        internal static ProgramUiHelper Instance { get; private set; }

        /// <summary>
        /// Sets the result of the user interface when the window is closing.
        /// </summary>
        internal dynamic UiResult { private get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new static instance of <see cref=""ProgramUiHelper""/>.
        /// </summary>
        internal static void CreateNewInstance()
        {
            Instance = new ProgramUiHelper();
        }

        /// <summary>
        /// Load the user interface in memory.
        /// </summary>
        internal void LoadWindow()
        {
            _userInterface = System.Windows.Markup.XamlReader.Parse(_xamlCode) as System.Windows.Window;
        }

        /// <summary>
        /// Show the user interface of the program.
        /// </summary>
        /// <returns>Returns the result of the Window.Closed event from the user interface.</returns>
        internal dynamic ShowWindow()
        {
            System.Exception eventException = null;

            _userInterface.Closed += (sender, e) =>
            {
                _userInterface?.Dispatcher?.InvokeShutdown();
            };

            try
            {
                _userInterface.Show();
                System.Windows.Threading.Dispatcher.Run();
            }
            catch (System.Exception exception)
            {
                eventException = exception;
            }
            finally
            {
                try
                {
                    _userInterface.Close();
                }
                catch { }
                _userInterface = null;
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
        /// <param name=""controlName"">The name of the control to retrieves.</param>
        /// <returns>Returns null if the control does not exist.</returns>
        internal dynamic GetControl(System.String controlName)
        {
            dynamic dynamic = _userInterface.FindName(controlName);
            return dynamic;
        }

        #endregion
    }
}";

            Assert.IsTrue(code.Contains(expected));
        }

        [TestMethod]
        public void CSharpCodeGeneratorCondition()
        {
            var program = new BaZicProgram()
                          .WithMethods(
                              new EntryPointMethod()
                              .WithBody(
                                  new ConditionStatement(new BinaryOperatorExpression(new BinaryOperatorExpression(new BinaryOperatorExpression(new PrimitiveExpression(1), BinaryOperatorType.LessThan, new PrimitiveExpression(2)), BinaryOperatorType.LogicalAnd, new BinaryOperatorExpression(new PrimitiveExpression(3), BinaryOperatorType.LessThan, new PrimitiveExpression(4))), BinaryOperatorType.LogicalOr, new NotOperatorExpression(new PrimitiveExpression(false))))
                                  .WithThenBody(
                                      new CommentStatement("If true")
                                  ).WithElseBody(
                                      new ConditionStatement(new BinaryOperatorExpression(new PrimitiveExpression(1), BinaryOperatorType.LessThan, new PrimitiveExpression(2)))
                                      .WithThenBody(
                                          new ConditionStatement(new NotOperatorExpression(new BinaryOperatorExpression(new PrimitiveExpression(1), BinaryOperatorType.LessThan, new PrimitiveExpression(2))))
                                          .WithThenBody(
                                              new CommentStatement("If true")
                                          ).WithElseBody(
                                              new CommentStatement("If not true")
                                          )
                                      ).WithElseBody(
                                          new CommentStatement("If not true")
                                      )
                                  )
                              )
                          );

            var code = new CSharpCodeGenerator().Generate(program);

            var expected =
@"// CSharp code generated automatically

namespace BaZicProgramReleaseMode
{
    [System.Serializable]
    public sealed class Program : System.MarshalByRefObject
    {
        [System.LoaderOptimization(System.LoaderOptimization.MultiDomainHost)]
        public dynamic Main(dynamic args)
        {
            try {
            if (((1 < 2) && (3 < 4)) || (!false))
            {
                // If true
            }
            else
            {
                if (1 < 2)
                {
                    if (!(1 < 2))
                    {
                        // If true
                    }
                    else
                    {
                        // If not true
                    }
                }
                else
                {
                    // If not true
                }
            }
            } finally {
            ProgramHelper.WaitAllUnwaitedThreads();
            }
            return null;
        }
    }
}";

            Assert.IsTrue(code.Contains(expected));
        }

        [TestMethod]
        public void CSharpCodeGeneratorTryCatch()
        {
            var program = new BaZicProgram()
                          .WithMethods(
                              new EntryPointMethod()
                              .WithBody(
                                  new TryCatchStatement()
                                  .WithTryBody(
                                      new CommentStatement("Evaluation")
                                  ).WithCatchBody(
                                      new ThrowStatement(new ExceptionReferenceExpression())
                                  )
                              )
                          );

            var code = new CSharpCodeGenerator().Generate(program);

            var expected =
@"// CSharp code generated automatically

namespace BaZicProgramReleaseMode
{
    [System.Serializable]
    public sealed class Program : System.MarshalByRefObject
    {
        [System.LoaderOptimization(System.LoaderOptimization.MultiDomainHost)]
        public dynamic Main(dynamic args)
        {
            try {
            try
            {
                // Evaluation
            }
            catch (System.Exception EXCEPTION)
            {
                throw EXCEPTION;
            }
            } finally {
            ProgramHelper.WaitAllUnwaitedThreads();
            }
            return null;
        }
    }
}";

            Assert.IsTrue(code.Contains(expected));
        }

        [TestMethod]
        public void CSharpCodeGeneratorArrayIndexer()
        {
            var program = new BaZicProgram()
                          .WithMethods(
                              new EntryPointMethod()
                              .WithBody(
                                  new VariableDeclaration("Foo", true),
                                  new ReturnStatement(new ArrayIndexerExpression(new VariableReferenceExpression("Foo"), new Expression[] { new PrimitiveExpression(0) }))
                              )
                          );

            var code = new CSharpCodeGenerator().Generate(program);

            var expected =
@"// CSharp code generated automatically

namespace BaZicProgramReleaseMode
{
    [System.Serializable]
    public sealed class Program : System.MarshalByRefObject
    {
        [System.LoaderOptimization(System.LoaderOptimization.MultiDomainHost)]
        public dynamic Main(dynamic args)
        {
            try {
            dynamic Foo = null;
            return Foo[0];
            } finally {
            ProgramHelper.WaitAllUnwaitedThreads();
            }
            return null;
        }
    }
}";

            Assert.IsTrue(code.Contains(expected));
        }

        [TestMethod]
        public void CSharpCodeGeneratorClassReference()
        {
            var program = new BaZicProgram()
                          .WithMethods(
                              new EntryPointMethod()
                              .WithBody(
                                  new VariableDeclaration("Baz", true).WithDefaultValue(new InstantiateExpression(new ClassReferenceExpression("System", "Array"))),
                                  new VariableDeclaration("Boo", true).WithDefaultValue(new InstantiateExpression(new ClassReferenceExpression("System", "String")))
                              )
                          );

            var code = new CSharpCodeGenerator().Generate(program);

            var expected =
@"// CSharp code generated automatically

namespace BaZicProgramReleaseMode
{
    [System.Serializable]
    public sealed class Program : System.MarshalByRefObject
    {
        [System.LoaderOptimization(System.LoaderOptimization.MultiDomainHost)]
        public dynamic Main(dynamic args)
        {
            try {
            dynamic Baz = new System.Array();
            dynamic Boo = new System.String();
            } finally {
            ProgramHelper.WaitAllUnwaitedThreads();
            }
            return null;
        }
    }
}";

            Assert.IsTrue(code.Contains(expected));
        }

        [TestMethod]
        public void CSharpCodeGeneratorInvoke()
        {
            var program = new BaZicProgram()
                          .WithMethods(
                              new EntryPointMethod()
                              .WithBody(
                                  new ExpressionStatement(new InvokeMethodExpression("Foo", true).WithParameters(new PrimitiveExpression(1), new BinaryOperatorExpression(new PrimitiveExpression(2), BinaryOperatorType.Subtraction, new PrimitiveExpression(1)))),
                                  new VariableDeclaration("integer").WithDefaultValue(new PrimitiveExpression(1)),
                                  new ReturnStatement(new InvokeCoreMethodExpression(new VariableReferenceExpression("integer"), "ToString", false).WithParameters(new PrimitiveExpression("X")))
                              ),
                              new MethodDeclaration("Foo", true, true)
                              .WithParameters(new ParameterDeclaration("arg1", true), new ParameterDeclaration("arg2"))
                          );

            var code = new CSharpCodeGenerator().Generate(program);

            var expected =
@"// CSharp code generated automatically

namespace BaZicProgramReleaseMode
{
    [System.Serializable]
    public sealed class Program : System.MarshalByRefObject
    {
        [System.LoaderOptimization(System.LoaderOptimization.MultiDomainHost)]
        public dynamic Main(dynamic args)
        {
            try {
            Foo(1, 2 - 1).GetAwaiter().GetResult();
            dynamic integer = 1;
            return ProgramHelper.AddUnwaitedThreadIfRequired(integer, ""ToString"", ""X"");
            } finally {
            ProgramHelper.WaitAllUnwaitedThreads();
            }
            return null;
        }

        public async System.Threading.Tasks.Task<dynamic> Foo(dynamic arg1, dynamic arg2)
        {

            return await System.Threading.Tasks.Task.FromResult<object>(null);
        }
    }
}";

            Assert.IsTrue(code.Contains(expected));
        }

        [TestMethod]
        public void CSharpCodeGeneratorPrimitiveValues()
        {
            var program = new BaZicProgram()
                          .WithMethods(
                              new EntryPointMethod()
                              .WithBody(
                                  new VariableDeclaration("foo"),
                                  new AssignStatement(new VariableReferenceExpression("foo"), new PrimitiveExpression(1)),
                                  new AssignStatement(new VariableReferenceExpression("foo"), new PrimitiveExpression(1.1234)),
                                  new AssignStatement(new VariableReferenceExpression("foo"), new PrimitiveExpression(-1.5467f)),
                                  new AssignStatement(new VariableReferenceExpression("foo"), new PrimitiveExpression("Hel\"l\r\no")),
                                  new AssignStatement(new VariableReferenceExpression("foo"), new PrimitiveExpression(true)),
                                  new AssignStatement(new VariableReferenceExpression("foo"), new PrimitiveExpression(false)),
                                  new AssignStatement(new VariableReferenceExpression("foo"), new PrimitiveExpression()),
                                  new VariableDeclaration("bar", true).WithDefaultValue(new PrimitiveExpression(new object[] { "Hello", new object[] { "Foo", "Bar", "Buz" } })),
                                  new AssignStatement(new VariableReferenceExpression("bar"), new ArrayCreationExpression().WithValues(new PrimitiveExpression(1), new PrimitiveExpression(1.1234), new PrimitiveExpression("Hello")))
                              )
                          );

            var code = new CSharpCodeGenerator().Generate(program);

            var expected =
@"// CSharp code generated automatically

namespace BaZicProgramReleaseMode
{
    [System.Serializable]
    public sealed class Program : System.MarshalByRefObject
    {
        [System.LoaderOptimization(System.LoaderOptimization.MultiDomainHost)]
        public dynamic Main(dynamic args)
        {
            try {
            dynamic foo = null;
            foo = 1;
            foo = 1.1234;
            foo = -1.5467;
            foo = ""Hel\""l\r\no"";
            foo = true;
            foo = false;
            foo = null;
            dynamic bar = new BaZicProgramReleaseMode.ObservableDictionary() { ""Hello"", new BaZicProgramReleaseMode.ObservableDictionary() { ""Foo"", ""Bar"", ""Buz"" } };
            bar = new BaZicProgramReleaseMode.ObservableDictionary() { 1, 1.1234, ""Hello"" };
            } finally {
            ProgramHelper.WaitAllUnwaitedThreads();
            }
            return null;
        }
    }
}";

            Assert.IsTrue(code.Contains(expected));
        }

        [TestMethod]
        public void CSharpCodeGeneratorProperty()
        {
            var program = new BaZicProgram()
                          .WithMethods(
                              new EntryPointMethod()
                              .WithBody(
                                  new VariableDeclaration("foo").WithDefaultValue(new PrimitiveExpression("Hello")),
                                  new ReturnStatement(new PropertyReferenceExpression(new VariableReferenceExpression("foo"), "Length"))
                              )
                          );

            var code = new CSharpCodeGenerator().Generate(program);

            var expected =
@"// CSharp code generated automatically

namespace BaZicProgramReleaseMode
{
    [System.Serializable]
    public sealed class Program : System.MarshalByRefObject
    {
        [System.LoaderOptimization(System.LoaderOptimization.MultiDomainHost)]
        public dynamic Main(dynamic args)
        {
            try {
            dynamic foo = ""Hello"";
            return foo.Length;
            } finally {
            ProgramHelper.WaitAllUnwaitedThreads();
            }
            return null;
        }
    }
}";

            Assert.IsTrue(code.Contains(expected));
        }
    }
}
