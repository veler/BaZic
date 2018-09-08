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
@"namespace BaZicProgramReleaseMode
{
    [System.Serializable]
    public class Program
    {
        private readonly ProgramHelper _programHelperInstance = new ProgramHelper();
        public ProgramHelper ProgramHelperInstance => _programHelperInstance;
        private dynamic Foo = null;

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
            _programHelperInstance.WaitAllUnwaitedThreads();
            }
            return null;
        }
    }
}";

            Assert.IsTrue(code.Contains(expected));
        }

        [TestMethod]
        public void CSharpCodeGeneratorUi()
        {
            var inputCodeUi =
@"
VARIABLE var1

EXTERN FUNCTION Main(args[])
END FUNCTION

EVENT FUNCTION Window1_Loaded()
    ListBox1.ItemsSource = NEW [""Value 1"", ""Value 2""]
    TextBox1.Text = ""Value to add""
END FUNCTION

EVENT FUNCTION Button1_Click()
    ListBox1.ItemsSource.Add(TextBox1_Text)
END FUNCTION

# The XAML will be provided separatly";

            var xamlCode = @"
<Window xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Name=""Window1"">
    <StackPanel>
        <TextBox Name=""TextBox1""/>
        <Button Name=""Button1"" Content=""Add a value""/>
        <ListBox Name=""ListBox1""/>
    </StackPanel>
</Window>";

            var program = new BaZicParser().Parse(inputCodeUi, xamlCode, optimize: true).Program;

            var code = new CSharpCodeGenerator().Generate(program);

            var expected =
@"namespace BaZicProgramReleaseMode
{
    [System.Serializable]
    public class Program
    {
        private readonly ProgramHelper _programHelperInstance = new ProgramHelper();
        public ProgramHelper ProgramHelperInstance => _programHelperInstance;
        private dynamic var1 = null;

        private dynamic Window1
        { 
            get {
                dynamic result = _programHelperInstance.GetControl(nameof(Window1));
                return result;
            }
        }
        
        private dynamic TextBox1
        { 
            get {
                dynamic result = _programHelperInstance.GetControl(nameof(TextBox1));
                return result;
            }
        }
        
        private dynamic Button1
        { 
            get {
                dynamic result = _programHelperInstance.GetControl(nameof(Button1));
                return result;
            }
        }
        
        private dynamic ListBox1
        { 
            get {
                dynamic result = _programHelperInstance.GetControl(nameof(ListBox1));
                return result;
            }
        }
        

        internal dynamic Window1_Loaded()
        {
            _programHelperInstance.UIDispatcher.Invoke(() => { ListBox1.ItemsSource = new BaZicProgramReleaseMode.ObservableDictionary() { ""Value 1"", ""Value 2"" }; }, System.Windows.Threading.DispatcherPriority.Background);
            _programHelperInstance.UIDispatcher.Invoke(() => { TextBox1.Text = ""Value to add""; }, System.Windows.Threading.DispatcherPriority.Background);
            return null;
        }

        internal dynamic Button1_Click()
        {
            _programHelperInstance.UIDispatcher.Invoke(() => { _programHelperInstance.AddUnwaitedThreadIfRequired(ListBox1.ItemsSource, ""Add""); }, System.Windows.Threading.DispatcherPriority.Background);
            return null;
        }

        public dynamic Main(dynamic args)
        {
            try {

            _programHelperInstance.LoadUserInterface();
            ((System.Windows.Window)_programHelperInstance.GetControl(""Window1"")).Loaded += (sender, e) => { Window1_Loaded(); };
            ((System.Windows.Controls.Button)_programHelperInstance.GetControl(""Button1"")).Click += (sender, e) => { Button1_Click(); };
            return _programHelperInstance.ShowUserInterface();
            } finally {
            _programHelperInstance.WaitAllUnwaitedThreads();
            }
            return null;
        }
    }
}

// Helper for CSharp generated code.";

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
@"namespace BaZicProgramReleaseMode
{
    [System.Serializable]
    public class Program
    {
        private readonly ProgramHelper _programHelperInstance = new ProgramHelper();
        public ProgramHelper ProgramHelperInstance => _programHelperInstance;
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
            _programHelperInstance.WaitAllUnwaitedThreads();
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
@"namespace BaZicProgramReleaseMode
{
    [System.Serializable]
    public class Program
    {
        private readonly ProgramHelper _programHelperInstance = new ProgramHelper();
        public ProgramHelper ProgramHelperInstance => _programHelperInstance;
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
            _programHelperInstance.WaitAllUnwaitedThreads();
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
@"namespace BaZicProgramReleaseMode
{
    [System.Serializable]
    public class Program
    {
        private readonly ProgramHelper _programHelperInstance = new ProgramHelper();
        public ProgramHelper ProgramHelperInstance => _programHelperInstance;
        public dynamic Main(dynamic args)
        {
            try {
            dynamic Foo = null;
            return Foo[0];
            } finally {
            _programHelperInstance.WaitAllUnwaitedThreads();
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
@"namespace BaZicProgramReleaseMode
{
    [System.Serializable]
    public class Program
    {
        private readonly ProgramHelper _programHelperInstance = new ProgramHelper();
        public ProgramHelper ProgramHelperInstance => _programHelperInstance;
        public dynamic Main(dynamic args)
        {
            try {
            dynamic Baz = new System.Array();
            dynamic Boo = new System.String();
            } finally {
            _programHelperInstance.WaitAllUnwaitedThreads();
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
@"namespace BaZicProgramReleaseMode
{
    [System.Serializable]
    public class Program
    {
        private readonly ProgramHelper _programHelperInstance = new ProgramHelper();
        public ProgramHelper ProgramHelperInstance => _programHelperInstance;
        public dynamic Main(dynamic args)
        {
            try {
            Foo(1, 2 - 1).GetAwaiter().GetResult();
            dynamic integer = 1;
            return _programHelperInstance.AddUnwaitedThreadIfRequired(integer, ""ToString"", ""X"");
            } finally {
            _programHelperInstance.WaitAllUnwaitedThreads();
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
@"namespace BaZicProgramReleaseMode
{
    [System.Serializable]
    public class Program
    {
        private readonly ProgramHelper _programHelperInstance = new ProgramHelper();
        public ProgramHelper ProgramHelperInstance => _programHelperInstance;
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
            _programHelperInstance.WaitAllUnwaitedThreads();
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
@"namespace BaZicProgramReleaseMode
{
    [System.Serializable]
    public class Program
    {
        private readonly ProgramHelper _programHelperInstance = new ProgramHelper();
        public ProgramHelper ProgramHelperInstance => _programHelperInstance;
        public dynamic Main(dynamic args)
        {
            try {
            dynamic foo = ""Hello"";
            return foo.Length;
            } finally {
            _programHelperInstance.WaitAllUnwaitedThreads();
            }
            return null;
        }
    }
}";

            Assert.IsTrue(code.Contains(expected));
        }
    }
}
