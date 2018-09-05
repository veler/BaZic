using BaZic.Runtime.BaZic.Code;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Code.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace BaZic.Runtime.Tests.BaZic.Code.Parser
{
    [TestClass]
    public class StatementParserTest
    {
        [TestInitialize]
        public void Initialize()
        {
            Runtime.TestUtilities.InitializeLogs();
        }

        [TestMethod]
        public void BaZicParserVariableDeclaration1()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 \n\n  \n  \n\n";

            var result = parser.Parse(inputCode);
            Assert.AreEqual(1, result.Program.GlobalVariables.Count);
            Assert.AreEqual("var1", result.Program.GlobalVariables.First().Name.ToString());
            Assert.IsNull(result.Program.GlobalVariables.First().DefaultValue);
            Assert.IsFalse(result.Program.GlobalVariables.First().IsArray);
        }

        [TestMethod]
        public void BaZicParserVariableDeclaration2()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = 1 # Comment\n"
          + "VARIABLE Var1 # Comment";

            var result = parser.Parse(inputCode);
            Assert.AreEqual(2, result.Program.GlobalVariables.Count);
            Assert.AreEqual("var1", result.Program.GlobalVariables.First().Name.ToString());
            Assert.AreEqual("Var1", result.Program.GlobalVariables.Last().Name.ToString());
        }

        [TestMethod]
        public void BaZicParserVariableDeclaration3()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 FUNCTION Method1()";

            var result = parser.Parse(inputCode);
            Assert.AreEqual(6, result.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)result.Issues.InnerExceptions.First();
            Assert.AreEqual(1, exception.Line);
            Assert.AreEqual(15, exception.Column);
            Assert.AreEqual("Invalid statement. Assignment or new line expected.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserVariableDeclaration4()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1  [] # Comment";

            var result = parser.Parse(inputCode);
            Assert.AreEqual(1, result.Program.GlobalVariables.Count);
            Assert.IsTrue(result.Program.GlobalVariables.First().IsArray);
        }

        [TestMethod]
        public void BaZicParserVariableDeclaration5()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE variable_name";

            var result = parser.Parse(inputCode);
            Assert.AreEqual("variable_name", result.Program.GlobalVariables.Single().Name.Identifier);
        }

        [TestMethod]
        public void BaZicParserVariableDeclaration6()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1[] = NEW []";

            var result = parser.Parse(inputCode);
            Assert.IsTrue(result.Program.GlobalVariables.Single().IsArray);
            Assert.AreEqual(typeof(ArrayCreationExpression), result.Program.GlobalVariables.Single().DefaultValue.GetType());
        }

        [TestMethod]
        public void BaZicParserVariableDeclaration7()
        {
            var parser = new BaZicParser();

            var inputCode = @"
FUNCTION Method()
    VARIABLE var1[] = MethodThatReturnsAnArray()
END FUNCTION
";

            var result = parser.Parse(inputCode);
            Assert.IsTrue(((VariableDeclaration)result.Program.Methods.First().Statements.Single()).IsArray);
            Assert.AreEqual(typeof(InvokeMethodExpression), ((VariableDeclaration)result.Program.Methods.First().Statements.Single()).DefaultValue.GetType());
        }

        [TestMethod]
        public void BaZicParserVariableDeclaration8()
        {
            var parser = new BaZicParser();

            var inputCode = @"VARIABLE var1[] = MethodThatReturnsAnArray()";

            var result = parser.Parse(inputCode);
            Assert.AreEqual(3, result.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)result.Issues.InnerExceptions[1];
            Assert.AreEqual(1, exception.Line);
            Assert.AreEqual(19, exception.Column);
            Assert.AreEqual("The default value of a global variable can only be made of primitive values, not any reference, instanciation or invocation.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserVariableDeclaration9()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = \n"
                      + "VARIABLE var2";

            var result = parser.Parse(inputCode);
            Assert.AreEqual(5, result.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)result.Issues.InnerExceptions.First();
            Assert.AreEqual(1, exception.Line);
            Assert.AreEqual(17, exception.Column);
            Assert.AreEqual("A valid expression is expected.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserVariableDeclaration10()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = \"Hello\n" +
                        "World\" Identifier";

            var result = parser.Parse(inputCode);
            Assert.AreEqual(5, result.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)result.Issues.InnerExceptions.First();
            Assert.AreEqual(2, exception.Line);
            Assert.AreEqual(7, exception.Column);
            Assert.AreEqual("Invalid expression. 'New line', 'Start code', 'End code', 'Comment' is expected.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserVariableDeclaration11()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = Method()";

            var result = parser.Parse(inputCode);
            Assert.AreEqual(3, result.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)result.Issues.InnerExceptions[1];
            Assert.AreEqual("The default value of a global variable can only be made of primitive values, not any reference, instanciation or invocation.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserVariableDeclaration12()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE variable-name";

            var result = parser.Parse(inputCode);
            Assert.AreEqual(6, result.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)result.Issues.InnerExceptions.First();
            Assert.AreEqual("Invalid statement. Assignment or new line expected.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserVariableDeclaration13()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1[] = # Comment";

            var result = parser.Parse(inputCode);
            Assert.AreEqual(4, result.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)result.Issues.InnerExceptions.First();
            Assert.AreEqual(1, exception.Line);
            Assert.AreEqual(19, exception.Column);
            Assert.AreEqual("A valid expression is expected.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserVariableDeclaration14()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1[] = 1";

            var result = parser.Parse(inputCode);
            Assert.AreEqual(2, result.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)result.Issues.InnerExceptions.First();
            Assert.AreEqual("The variable 'var1' is declared as an array. The default value is not an array.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserVariableDeclaration15()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = NEW []";

            var result = parser.Parse(inputCode);
            Assert.AreEqual(2, result.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)result.Issues.InnerExceptions.First();
            Assert.AreEqual("The variable 'var1' is not declared as an array but the default value is an array.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserVariableDeclaration16()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE 1 #Comment";

            var result = parser.Parse(inputCode);
            Assert.AreEqual(2, result.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)result.Issues.InnerExceptions.First();
            Assert.AreEqual("An identifier is expected.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserVariableDeclaration17()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE identi@fier";

            var result = parser.Parse(inputCode);
            Assert.AreEqual(2, result.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)result.Issues.InnerExceptions.First();
            Assert.AreEqual("An identifier can only contains alphanumeric characters.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserVariableDeclaration18()
        {
            var parser = new BaZicParser();

            var inputCode = "var1 VARIABLE";

            var result = parser.Parse(inputCode);
            Assert.AreEqual(2, result.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)result.Issues.InnerExceptions.First();
            Assert.AreEqual("The name 'var1' does not exist in the current context.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserVariableDeclaration19()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE VARIABLE = 1";

            var result = parser.Parse(inputCode);
            Assert.AreEqual(2, result.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)result.Issues.InnerExceptions.First();
            Assert.AreEqual("The identifier 'VARIABLE' is a private BaZic keyword and cannot be use as an identifier.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserVariableDeclaration20()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = 1.2 + \n" +
            "3.45";

            var result = parser.Parse(inputCode);
            Assert.AreEqual(3, result.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)result.Issues.InnerExceptions.First();
            Assert.AreEqual("Unexpected end of expression. An expression must be wrote on a unique line.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserVariableDeclaration21()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1[] = NEW [ 1\n" +
            ", 3.45]";

            var result = parser.Parse(inputCode);
            Assert.AreEqual(10, result.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)result.Issues.InnerExceptions.First();
            Assert.AreEqual("Invalid expression. ',', ']', ')' is expected.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserVariableDeclaration22()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1[] = NEW [ 1,\n" +
            "3.45]";

            var result = parser.Parse(inputCode);
            Assert.AreEqual(7, result.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)result.Issues.InnerExceptions.First();
            Assert.AreEqual("A ']' is expected but a 'New line' has been found.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserVariableDeclaration23()
        {
            var parser = new BaZicParser();

            var inputCode = @"
FUNCTION Method()
    VARIABLE x = x
END FUNCTION";

            var result = parser.Parse(inputCode);
            Assert.AreEqual(3, result.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)result.Issues.InnerExceptions.First();
            Assert.AreEqual("The name 'x' does not exist in the current context.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserVariableDeclaration24()
        {
            var parser = new BaZicParser();

            var inputCode = @"
VARIABLE Button1";

            var xamlCode = @"
<Window xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
    <Grid>
        <Button Name=""Button1""/>
    </Grid>
</Window>";

            var result = parser.Parse(inputCode, xamlCode);
            Assert.AreEqual(2, result.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)result.Issues.InnerExceptions.First();
            Assert.AreEqual("A control accessor 'Button1' is already declared line 0.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserVariableDeclaration25()
        {
            var parser = new BaZicParser();

            var inputCode = @"VARIABLE var1[] = NEW [ ""Hello
World"" ]";
            var result = parser.Parse(inputCode);
            Assert.AreEqual(typeof(ArrayCreationExpression), result.Program.GlobalVariables.Single().DefaultValue.GetType());
        }

        [TestMethod]
        public void BaZicParserControlAccessorUse()
        {
            var parser = new BaZicParser();

            var xamlCode = @"
<Window xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
    <Grid>
        <ListBox Name=""ListBox1""/>
    </Grid>
</Window>";

            var inputCode = "";

            var result = parser.Parse(inputCode, xamlCode);
            var uiProgram = (BaZicUiProgram)result.Program;
            Assert.AreEqual(0, uiProgram.GlobalVariables.Count);
            Assert.AreEqual(0, uiProgram.UiControlAccessors.Count);
        }

        [TestMethod]
        public void BaZicParserControlAccessorUse2()
        {
            var parser = new BaZicParser();

            var xamlCode = @"
<Window xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
    <Grid>
        <ListBox Name=""ListBox1""/>
    </Grid>
</Window>";

            var inputCode = "VARIABLE test";

            var result = parser.Parse(inputCode, xamlCode);
            var uiProgram = (BaZicUiProgram)result.Program;
            Assert.AreEqual(1, uiProgram.GlobalVariables.Count);
            Assert.AreEqual(1, uiProgram.UiControlAccessors.Count);
            Assert.AreEqual("ListBox1", uiProgram.UiControlAccessors.First().ControlName);
            Assert.AreEqual("ListBox1", uiProgram.UiControlAccessors.First().Variable.Name.ToString());
            Assert.IsNull(uiProgram.UiControlAccessors.First().Variable.DefaultValue);
            Assert.IsFalse(uiProgram.UiControlAccessors.First().Variable.IsArray);
        }

        [TestMethod]
        public void BaZicParserThrowStatement()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    THROW NEW System.Exception()
END FUNCTION";

            var program = parser.Parse(inputCode);
            var returnStatement = (ThrowStatement)program.Program.Methods.First().Statements.Single();
            Assert.IsNotNull(returnStatement.Expression);
        }

        [TestMethod]
        public void BaZicParserThrowStatementMissingExpression()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    THROW
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(3, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("A valid expression is expected.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserReturnStatement1()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    RETURN
END FUNCTION";

            var program = parser.Parse(inputCode);
            var returnStatement = (ReturnStatement)program.Program.Methods.First().Statements.Single();
            Assert.IsNull(returnStatement.Expression);
        }

        [TestMethod]
        public void BaZicParserReturnStatement2()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    RETURN 1
END FUNCTION";

            var program = parser.Parse(inputCode);
            var returnStatement = (ReturnStatement)program.Program.Methods.First().Statements.Single();
            Assert.IsNotNull(returnStatement.Expression);
        }

        [TestMethod]
        public void BaZicParserBreakStatement()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    DO
        BREAK
    LOOP WHILE TRUE
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.IsInstanceOfType(program.Program.Methods.First().Statements.Single(), typeof(IterationStatement));
        }

        [TestMethod]
        public void BaZicParserBreakStatementWithExpression()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    DO
        BREAK 1
    LOOP WHILE TRUE
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(2, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("Invalid statement. New line expected.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserBreakStatementNotInLoop()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    BREAK
END FUNCTION";

            var program = parser.Parse(inputCode);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.Single();
            Assert.AreEqual("The 'BREAK' statement can only be use in a DO LOOP block.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserMethodDeclaration1()
        {
            var parser = new BaZicParser();

            var inputCode =
@"ASYNC FUNCTION Method1(var1, var2[])
    VARIABLE var3 = var1
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.IsNotInstanceOfType(program.Program.Methods.First(), typeof(EntryPointMethod));
            Assert.IsTrue(program.Program.Methods.First().IsAsync);
            Assert.AreEqual("Method1", program.Program.Methods.First().Name.Identifier);
            Assert.AreEqual("var1", program.Program.Methods.First().Arguments.First().Name.Identifier);
            Assert.IsTrue(program.Program.Methods.First().Arguments.Last().IsArray);
            Assert.AreEqual("var3", ((VariableDeclaration)program.Program.Methods.First().Statements.Single()).Name.Identifier);
        }

        [TestMethod]
        public void BaZicParserMethodDeclaration2()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method()
END FUNCTION

FUNCTION MEthod()
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual("Method", program.Program.Methods.First().Name.Identifier);
            Assert.AreEqual("MEthod", program.Program.Methods[1].Name.Identifier);
        }

        [TestMethod]
        public void BaZicParserMethodDeclaration3()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method(arg, ARG)
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual("arg", program.Program.Methods.First().Arguments.First().Name.Identifier);
            Assert.AreEqual("ARG", program.Program.Methods.First().Arguments.Last().Name.Identifier);
        }

        [TestMethod]
        public void BaZicParserMethodDeclaration4()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args[]) # Program's entry poin
    # Content

END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.IsInstanceOfType(program.Program.Methods.First(), typeof(EntryPointMethod));
        }

        [TestMethod]
        public void BaZicParserMethodDeclaration5()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1(var1, var2[])
    FUNCTION Method2()
        VARIABLE var3 = var1
    END FUNCTION
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(2, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("A method cannot be declared in this context.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserMethodDeclaration6()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1(var1, var2[])
     VARIABLE var3 = var1";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(3, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions[1];
            Assert.AreEqual("A 'END' is expected but a 'End code' has been found.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserMethodDeclaration7()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1(var1 | var2[])
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(10, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("Syntax error. Unexpected or missing character.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserMethodDeclaration8()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
END IF";

            var program = parser.Parse(inputCode);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.Single();
            Assert.AreEqual("A 'FUNCTION' is expected but a 'IF' has been found.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserMethodDeclaration9()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1     [ ]
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(3, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("A '(' is expected but a '[' has been found.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserMethodDeclaration10()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1() END FUNCTION";

            var program = parser.Parse(inputCode);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.Single();
            Assert.AreEqual("Invalid statement. New line expected.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserMethodDeclaration11()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION ASYNC Method1()
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(8, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("The identifier 'ASYNC' is a private BaZic keyword and cannot be use as an identifier.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserMethodDeclaration12()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EVENT ASYNC FUNCTION Method()
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(11, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("An event function cannot be marked as asynchronous.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserMethodDeclaration13()
        {
            var parser = new BaZicParser();

            var inputCode =
@"ASYNC EVENT FUNCTION Method()
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(10, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("An event function cannot be marked as asynchronous.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserMethodDeclaration14()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EVENT FUNCTION Method(arg1)
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(3, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("In an EVENT FUNCTION statement, the identifier must strickly have the syntax 'ControlName_PropertyName'.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserMethodDeclaration15()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EVENT FUNCTION Button1_OnClick(arg1)
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(2, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("An event function must not have parameter.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserMethodDeclaration16()
        {
            var parser = new BaZicParser();

            var inputCode =
@"Extern Async FUNCTION Main(args[])
END FUNCTION";

            var program = parser.Parse(inputCode);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.Single();
            Assert.AreEqual("The program's entry point can't be asynchronous.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserMethodDeclaration17()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main()
END FUNCTION";

            var program = parser.Parse(inputCode);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.Single();
            Assert.AreEqual("The program's entry point must have one and only one argument.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserMethodDeclaration18()
        {
            var parser = new BaZicParser();

            var inputCode =
@"EXTERN FUNCTION Main(args)
END FUNCTION";

            var program = parser.Parse(inputCode);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.Single();
            Assert.AreEqual("The program's entry point's argument must be an array.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserMethodDeclaration19()
        {
            var parser = new BaZicParser();

            var inputCode =
@"ASYNC Extern FUNCTION Main(args)
END FUNCTION";

            var program = parser.Parse(inputCode);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("A 'FUNCTION' is expected but a 'EXTERN' has been found.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserIterationStatement1()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    DO
        DO WHILE FALSE
            VARIABLE var1
        LOOP
    LOOP WHILE TRUE
END FUNCTION";

            var program = parser.Parse(inputCode);
            var loop = (IterationStatement)program.Program.Methods.First().Statements.Single();
            Assert.IsTrue(loop.ConditionAfterBody);
            Assert.IsInstanceOfType(loop.Condition, typeof(PrimitiveExpression));
            Assert.IsInstanceOfType(loop.Statements.Single(), typeof(IterationStatement));
        }

        [TestMethod]
        public void BaZicParserIterationStatement2()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    DO WHILE TRUE
        VARIABLE var1 = 1
    LOOP
END FUNCTION";

            var program = parser.Parse(inputCode);
            var loop = (IterationStatement)program.Program.Methods.First().Statements.Single();
            Assert.IsFalse(loop.ConditionAfterBody);
            Assert.IsInstanceOfType(loop.Condition, typeof(PrimitiveExpression));
            Assert.IsInstanceOfType(loop.Statements.Single(), typeof(VariableDeclaration));
        }

        [TestMethod]
        public void BaZicParserIterationStatement3()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    DO WHILE TRUE
    LOOP WHILE TRUE
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(4, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("Invalid statement. New line expected.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserIterationStatement4()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    WHILE DO TRUE
    LOOP
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(7, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("Invalid statement. Are you missing a keyword?", exception.Message);
        }

        [TestMethod]
        public void BaZicParserIterationStatement5()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    DO WHILE TRUE
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(11, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("Invalid statement. Are you missing a keyword?", exception.Message);
        }

        [TestMethod]
        public void BaZicParserConditionStatement1()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    IF TRUE THEN
        VARIABLE var1 = 1
    ELSE
        VARIABLE var2 = 2
        VARIABLE var3 = 3
    END IF
END FUNCTION";

            var program = parser.Parse(inputCode);
            var condition = (ConditionStatement)program.Program.Methods.First().Statements.Single();
            Assert.AreEqual(1, condition.TrueStatements.Count);
            Assert.AreEqual(2, condition.FalseStatements.Count);
        }

        [TestMethod]
        public void BaZicParserConditionStatement2()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    IF TRUE THEN
        VARIABLE var1 = 1
    END IF
END FUNCTION";

            var program = parser.Parse(inputCode);
            var condition = (ConditionStatement)program.Program.Methods.First().Statements.Single();
            Assert.AreEqual(1, condition.TrueStatements.Count);
            Assert.AreEqual(0, condition.FalseStatements.Count);
        }

        [TestMethod]
        public void BaZicParserConditionStatement3()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    IF THEN
        VARIABLE var1 = 1
    END IF
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(2, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("A valid expression is expected.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserConditionStatement4()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    IF TRUE
        VARIABLE var1 = 1
    END IF
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(6, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("Invalid expression. 'THEN' is expected.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserConditionStatement5()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    IF true ELSE
        VARIABLE var1 = 1
    END LOOP
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(4, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("Invalid expression. 'THEN' is expected.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserTryCatchStatement1()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    TRY
        VARIABLE var1 = 1
    CATCH
        VARIABLE var2 = 2
        VARIABLE var3 = 3
    END TRY
END FUNCTION";

            var program = parser.Parse(inputCode);
            var tryCatch = (TryCatchStatement)program.Program.Methods.First().Statements.Single();
            Assert.AreEqual(1, tryCatch.TryStatements.Count);
            Assert.AreEqual(2, tryCatch.CatchStatements.Count);
        }

        [TestMethod]
        public void BaZicParserTryCatchStatement2()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    TRY
        VARIABLE var1 = 1
    END TRY
END FUNCTION";

            var program = parser.Parse(inputCode);
            var tryCatch = (TryCatchStatement)program.Program.Methods.First().Statements.Single();
            Assert.AreEqual(1, tryCatch.TryStatements.Count);
            Assert.AreEqual(0, tryCatch.CatchStatements.Count);
        }

        [TestMethod]
        public void BaZicParserTryCatchStatement3()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    TRY
        VARIABLE var1 = 1
    CATCH
        RETURN Exception
    END TRY
END FUNCTION";

            var program = parser.Parse(inputCode);
            var tryCatch = (TryCatchStatement)program.Program.Methods.First().Statements.Single();
            Assert.AreEqual(1, tryCatch.TryStatements.Count);
            Assert.AreEqual(1, tryCatch.CatchStatements.Count);
        }

        [TestMethod]
        public void BaZicParserTryCatchStatement4()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    TRY TRUE
        VARIABLE var1 = 1
    END TRY
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(3, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("A 'New line' is expected but a 'TRUE' has been found.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserTryCatchStatement5()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    TRY
        VARIABLE var1 = 1
    CATCH TRUE
    END TRY
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(3, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions[1];
            Assert.AreEqual("A 'New line' is expected but a 'TRUE' has been found.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserTryCatchStatement6()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    TRY
        RETURN Exception
    CATCH
    END TRY
END FUNCTION";

            var program = parser.Parse(inputCode);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.Single();
            Assert.AreEqual("The 'EXCEPTION' keyword can only be use in a CATCH block.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserAssignAndExpressionStatement1()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    VARIABLE var1 = 1
    var1 = 2 + 3
END FUNCTION";

            var program = parser.Parse(inputCode);
            var assign = (AssignStatement)program.Program.Methods.First().Statements.Last();
            Assert.IsInstanceOfType(assign.LeftExpression, typeof(VariableReferenceExpression));
            Assert.IsInstanceOfType(assign.RightExpression, typeof(BinaryOperatorExpression));
        }

        [TestMethod]
        public void BaZicParserAssignAndExpressionStatement2()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    Method1()
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.IsInstanceOfType(program.Program.Methods.First().Statements.Single(), typeof(ExpressionStatement));
        }

        [TestMethod]
        public void BaZicParserAssignAndExpressionStatement3()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    var1
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(2, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("The name 'var1' does not exist in the current context.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserAssignAndExpressionStatement4()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    Method1() = 2
END FUNCTION";

            var program = parser.Parse(inputCode);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.Single();
            Assert.AreEqual("Invalid statement. The left expression is not assignable.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserAssignAndExpressionStatement5()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    VARIABLE var1
    var1 = 
END FUNCTION";

            var program = parser.Parse(inputCode);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.Single();
            Assert.AreEqual("Unexpected end of expression. An expression must be wrote on a unique line.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserAssignAndExpressionStatement6()
        {
            var parser = new BaZicParser();

            var inputCode =
@"FUNCTION Method1()
    1 + 2
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(2, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("Only assignment and call can be use as a statement here.", exception.Message);
        }
    }
}
