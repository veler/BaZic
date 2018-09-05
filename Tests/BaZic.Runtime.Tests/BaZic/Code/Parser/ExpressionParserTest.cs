using BaZic.Runtime.BaZic.Code;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Code.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace BaZic.Runtime.Tests.BaZic.Code.Parser
{
    [TestClass]
    public class ExpressionParserTest
    {
        [TestInitialize]
        public void Initialize()
        {
            Runtime.TestUtilities.InitializeLogs();
        }

        [TestMethod]
        public void BaZicParserBadExpressionStartsWithOperator()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = + \"Hello\"";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(2, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("An expression cannot starts with an operator.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserBadExpressionBadStringSyntax()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = \"\"Hello\"";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(5, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("Invalid expression. 'New line', 'Start code', 'End code', 'Comment' is expected.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserBadExpressionBadInstanciateSyntax()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = NEW System.EventArgs[1.ToString())";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(5, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("A new expression requires () after type name.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserBadExpressionBadMethodCallSyntax()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = Method(1 @ 2)";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(11, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("Syntax error. Unexpected or missing character.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserBadExpressionStatementInExpression()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = Method(1 VARIABLE 2)";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(10, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("Invalid expression. ',', ']', ')' is expected.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserOperatorsExpression()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = 1 + 2";
            var program = parser.Parse(inputCode);
            var value = (BinaryOperatorExpression)program.Program.GlobalVariables.Single().DefaultValue;
            Assert.AreEqual(1, ((PrimitiveExpression)value.LeftExpression).Value);
            Assert.AreEqual(2, ((PrimitiveExpression)value.RightExpression).Value);
            Assert.AreEqual(BinaryOperatorType.Addition, value.Operator);
        }

        [TestMethod]
        public void BaZicParserOperatorsExpressionAnd()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = TRUE AND FALSE";
            var program = parser.Parse(inputCode);
            var value = (BinaryOperatorExpression)program.Program.GlobalVariables.Single().DefaultValue;
            Assert.AreEqual(BinaryOperatorType.LogicalAnd, value.Operator);
        }

        [TestMethod]
        public void BaZicParserOperatorsExpressionOr()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = TRUE OR FALSE";
            var program = parser.Parse(inputCode);
            var value = (BinaryOperatorExpression)program.Program.GlobalVariables.Single().DefaultValue;
            Assert.AreEqual(BinaryOperatorType.LogicalOr, value.Operator);
        }

        [TestMethod]
        public void BaZicParserOperatorsExpressionLessThan()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = 1 < 2";
            var program = parser.Parse(inputCode);
            var value = (BinaryOperatorExpression)program.Program.GlobalVariables.Single().DefaultValue;
            Assert.AreEqual(BinaryOperatorType.LessThan, value.Operator);
        }

        [TestMethod]
        public void BaZicParserOperatorsExpressionLessOrEqual()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = 1 <= 2";
            var program = parser.Parse(inputCode);
            var value = (BinaryOperatorExpression)program.Program.GlobalVariables.Single().DefaultValue;
            Assert.AreEqual(BinaryOperatorType.LessThanOrEqual, value.Operator);
        }

        [TestMethod]
        public void BaZicParserOperatorsExpressionConcatOr()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = TRUE OR (FALSE AND FALSE)";
            var program = parser.Parse(inputCode);
            var value = (BinaryOperatorExpression)program.Program.GlobalVariables.Single().DefaultValue;
            Assert.AreEqual(BinaryOperatorType.LogicalOr, value.Operator);
            value = (BinaryOperatorExpression)value.RightExpression;
            Assert.AreEqual(BinaryOperatorType.LogicalAnd, value.Operator);
        }

        [TestMethod]
        public void BaZicParserOperatorsExpressionConcatAnd()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = (TRUE OR FALSE) AND FALSE";
            var program = parser.Parse(inputCode);
            var value = (BinaryOperatorExpression)program.Program.GlobalVariables.Single().DefaultValue;
            Assert.AreEqual(BinaryOperatorType.LogicalAnd, value.Operator);
            value = (BinaryOperatorExpression)value.LeftExpression;
            Assert.AreEqual(BinaryOperatorType.LogicalOr, value.Operator);
        }

        [TestMethod]
        public void BaZicParserOperatorsExpressionNot()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = NOT 1 = 2";
            var program = parser.Parse(inputCode);
            var notValue = (NotOperatorExpression)program.Program.GlobalVariables.Single().DefaultValue;
            var value = (BinaryOperatorExpression)notValue.Expression;
            Assert.AreEqual(BinaryOperatorType.Equality, value.Operator);
        }

        [TestMethod]
        public void BaZicParserOperatorsExpressionNotNestedExpression()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = NOT (FALSE OR FALSE)";
            var program = parser.Parse(inputCode);
            var notValue = (NotOperatorExpression)program.Program.GlobalVariables.Single().DefaultValue;
            var value = (BinaryOperatorExpression)notValue.Expression;
            Assert.AreEqual(BinaryOperatorType.LogicalOr, value.Operator);
        }

        [TestMethod]
        public void BaZicParserOperatorsExpressionMath()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = 1 + 2 * (3 + 4 + 5)";
            var program = parser.Parse(inputCode);
            var value = (BinaryOperatorExpression)program.Program.GlobalVariables.Single().DefaultValue;
            Assert.AreEqual(BinaryOperatorType.Addition, value.Operator);
            value = (BinaryOperatorExpression)value.RightExpression;
            Assert.AreEqual(BinaryOperatorType.Multiply, value.Operator);
            value = (BinaryOperatorExpression)value.RightExpression;
            Assert.AreEqual(BinaryOperatorType.Addition, value.Operator);
            value = (BinaryOperatorExpression)value.LeftExpression;
            Assert.AreEqual(BinaryOperatorType.Addition, value.Operator);
        }

        [TestMethod]
        public void BaZicParserPrimaryExpressionBadReference()
        {
            var parser = new BaZicParser();

            var inputCode = @"
FUNCTION Method()
    VARIABLE var1 = This
END FUNCTION";
            var program = parser.Parse(inputCode);
            Assert.AreEqual("The name 'This' does not exist in the current context.", program.Issues.InnerException.Message);
        }

        [TestMethod]
        public void BaZicParserPrimaryExpressionBarReferenceOrNamespace()
        {
            var parser = new BaZicParser();

            var inputCode = @"
FUNCTION Method()
    VARIABLE var1 = var2.Property
END FUNCTION";
            var program = parser.Parse(inputCode);
            Assert.AreEqual("The name 'var2' does not look like a valid namespace or variable.", program.Issues.InnerException.Message);
        }

        [TestMethod]
        public void BaZicParserPrimaryExpressionPropertyReference()
        {
            var parser = new BaZicParser();

            var inputCode = @"
FUNCTION Method()
    VARIABLE var2 = NEW System.Core.MyClass()
    VARIABLE var1 = var2.Property
END FUNCTION";
            var program = parser.Parse(inputCode);
            var propertyRef = (PropertyReferenceExpression)((VariableDeclaration)program.Program.Methods.First().Statements.Last()).DefaultValue;
            var variableRef = (VariableReferenceExpression)propertyRef.TargetObject;
            Assert.AreEqual("Property", propertyRef.PropertyName.ToString());
            Assert.AreEqual("var2", variableRef.Name.ToString());
        }

        [TestMethod]
        public void BaZicParserPrimaryExpressionCoreMethodReference()
        {
            var parser = new BaZicParser();

            var inputCode = @"
FUNCTION Method()
    VARIABLE var2 = NEW System.Core.MyClass()
    VARIABLE var1 = var2.Method(1)
END FUNCTION";
            var program = parser.Parse(inputCode);
            var methodCoreRef = (InvokeCoreMethodExpression)((VariableDeclaration)program.Program.Methods.First().Statements.Last()).DefaultValue;
            var variableRef = (VariableReferenceExpression)methodCoreRef.TargetObject;
            Assert.AreEqual("Method", methodCoreRef.MethodName.ToString());
            Assert.IsFalse(methodCoreRef.Await);
            Assert.AreEqual(1, methodCoreRef.Arguments.Count);
            Assert.AreEqual("var2", variableRef.Name.ToString());
        }

        [TestMethod]
        public void BaZicParserPrimaryExpressionCoreInvoke()
        {
            var parser = new BaZicParser();

            var inputCode = @"
FUNCTION Method()
    VARIABLE var1 = 1.ToString()
END FUNCTION";
            var program = parser.Parse(inputCode);
            var methodCoreRef = (InvokeCoreMethodExpression)((VariableDeclaration)program.Program.Methods.First().Statements.Single()).DefaultValue;
            var primitiveExpr = (PrimitiveExpression)methodCoreRef.TargetObject;
            Assert.AreEqual("ToString", methodCoreRef.MethodName.ToString());
            Assert.AreEqual("1", primitiveExpr.Value.ToString());
        }

        [TestMethod]
        public void BaZicParserPrimaryExpressionInvokeAndUse()
        {
            var parser = new BaZicParser();

            var inputCode = @"
FUNCTION Method()
    VARIABLE var2 = NEW System.Core.MyClass()
    VARIABLE var1 = var2.Method(1)[0]
END FUNCTION";
            var program = parser.Parse(inputCode);
            var arrayIndexer = (ArrayIndexerExpression)((VariableDeclaration)program.Program.Methods.First().Statements.Last()).DefaultValue;
            var methodCoreRef = (InvokeCoreMethodExpression)arrayIndexer.TargetObject;
            var variableRef = (VariableReferenceExpression)methodCoreRef.TargetObject;
            Assert.AreEqual("Method", methodCoreRef.MethodName.ToString());
            Assert.IsFalse(methodCoreRef.Await);
            Assert.AreEqual(1, methodCoreRef.Arguments.Count);
            Assert.AreEqual("var2", variableRef.Name.ToString());
        }

        [TestMethod]
        public void BaZicParserPrimaryExpressionInvokeAndUseBadArrayIndexer()
        {
            var parser = new BaZicParser();

            var inputCode = @"
FUNCTION Method()
    VARIABLE var2 = NEW System.Core.MyClass()
    VARIABLE var1 = var2.Method(1)[]
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(2, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("Array index expected in the '[]'.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserPrimaryExpressionInstanciate()
        {
            var parser = new BaZicParser();

            var inputCode = @"
FUNCTION Method()
    VARIABLE var1 = NEW System.EventArgs(1, 2)
END FUNCTION";
            var program = parser.Parse(inputCode);
            var instanciation = (InstantiateExpression)((VariableDeclaration)program.Program.Methods.First().Statements.Single()).DefaultValue;
            Assert.AreEqual("System", instanciation.CreateType.Namespace);
            Assert.AreEqual("EventArgs", instanciation.CreateType.ClassName.Identifier);
            Assert.AreEqual("System.EventArgs", instanciation.CreateType.ToString());
            Assert.AreEqual(2, instanciation.Arguments.Count);
        }

        [TestMethod]
        public void BaZicParserPrimaryExpressionBadInstanciateSyntax1()
        {
            var parser = new BaZicParser();

            var inputCode = @"
FUNCTION Method()
    VARIABLE var1 = NEW System.EventArgs
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(2, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("A new expression requires () after type name.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserPrimaryExpressionBadInstanciateSyntax2()
        {
            var parser = new BaZicParser();

            var inputCode = @"
FUNCTION Method()
    VARIABLE var1 = NEW System.EventArgs[]
END FUNCTION";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(4, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("A new expression requires () after type name.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserPrimaryExpression()
        {
            var parser = new BaZicParser();

            var inputCode = @"
FUNCTION Method()
    VARIABLE var2[] = NEW []
    VARIABLE var1 = var2[0].Property.Method(NEW System.EventArgs(1, 2), 1)
END FUNCTION";
            var program = parser.Parse(inputCode);
            var methodCoreRef = (InvokeCoreMethodExpression)((VariableDeclaration)program.Program.Methods.First().Statements.Last()).DefaultValue;
            var propertyRef = (PropertyReferenceExpression)methodCoreRef.TargetObject;
            var arrayIndexer = (ArrayIndexerExpression)propertyRef.TargetObject;
            var variableRef = (VariableReferenceExpression)arrayIndexer.TargetObject;
            Assert.AreEqual("Method", methodCoreRef.MethodName.Identifier);
            Assert.AreEqual("Property", propertyRef.PropertyName.Identifier);
            Assert.AreEqual(1, arrayIndexer.Indexes.Length);
            Assert.AreEqual("var2", variableRef.Name.Identifier);
        }

        [TestMethod]
        public void BaZicParserAwaitCallExpressionTripleNestedConcat()
        {
            var parser = new BaZicParser();

            var inputCode = @"
EXTERN FUNCTION Main(args[])
    VARIABLE var1 = AWAIT Method1(args[AWAIT Method2()])
END FUNCTION";
            var program = parser.Parse(inputCode);
            var methodRef = (InvokeMethodExpression)((VariableDeclaration)program.Program.Methods.First().Statements.Single()).DefaultValue;
            Assert.AreEqual("Method1", methodRef.MethodName.ToString());
            Assert.IsTrue(methodRef.Await);
        }

        [TestMethod]
        public void BaZicParserAwaitCallExpressionDoubleNestedConcat()
        {
            var parser = new BaZicParser();

            var inputCode = @"
FUNCTION Method()
    VARIABLE var1 = AWAIT (AWAIT Method2()).Method1()
END FUNCTION";
            var program = parser.Parse(inputCode);
            var methodCoreRef = (InvokeCoreMethodExpression)((VariableDeclaration)program.Program.Methods.First().Statements.Single()).DefaultValue;
            Assert.AreEqual("Method1", methodCoreRef.MethodName.ToString());
            Assert.IsTrue(methodCoreRef.Await);
            var methodRef = (InvokeMethodExpression)methodCoreRef.TargetObject;
            Assert.AreEqual("Method2", methodRef.MethodName.ToString());
            Assert.IsTrue(methodRef.Await);
        }

        [TestMethod]
        public void BaZicParserAwaitCallExpressionNestedConcat()
        {
            var parser = new BaZicParser();

            var inputCode = @"
FUNCTION Method()
    VARIABLE var1 = (AWAIT Method2()).Method1()
END FUNCTION";
            var program = parser.Parse(inputCode);
            var methodCoreRef = (InvokeCoreMethodExpression)((VariableDeclaration)program.Program.Methods.First().Statements.Single()).DefaultValue;
            Assert.AreEqual("Method1", methodCoreRef.MethodName.ToString());
            Assert.IsFalse(methodCoreRef.Await);
            var methodRef = (InvokeMethodExpression)methodCoreRef.TargetObject;
            Assert.AreEqual("Method2", methodRef.MethodName.ToString());
            Assert.IsTrue(methodRef.Await);
        }

        [TestMethod]
        public void BaZicParserAwaitCallExpressionConcat()
        {
            var parser = new BaZicParser();

            var inputCode = @"
FUNCTION Method()
    VARIABLE var1 = AWAIT Method2().Method1()
END FUNCTION";
            var program = parser.Parse(inputCode);
            var methodCoreRef = (InvokeCoreMethodExpression)((VariableDeclaration)program.Program.Methods.First().Statements.Single()).DefaultValue;
            Assert.AreEqual("Method1", methodCoreRef.MethodName.ToString());
            Assert.IsTrue(methodCoreRef.Await);
            var methodRef = (InvokeMethodExpression)methodCoreRef.TargetObject;
            Assert.AreEqual("Method2", methodRef.MethodName.ToString());
            Assert.IsFalse(methodRef.Await);
        }

        [TestMethod]
        public void BaZicParserAwaitCallExpressionInvocationExpected()
        {
            var parser = new BaZicParser();

            var inputCode = @"
FUNCTION Method()
    VARIABLE var1 = AWAIT Identifier
END FUNCTION";
            try
            {
                var program = parser.Parse(inputCode);
            }
            catch (BaZicParserException exception)
            {
                Assert.AreEqual("Asynchronous method invocation expected.", exception.Message);
            }
        }

        [TestMethod]
        public void BaZicParserPrimitiveExpression()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = \"Hello\"";
            var program = parser.Parse(inputCode);
            Assert.AreEqual("Hello", ((PrimitiveExpression)program.Program.GlobalVariables.Single().DefaultValue).Value);
        }

        [TestMethod]
        public void BaZicParserPrimitiveExpressionStringSeveralLines()
        {
            var parser = new BaZicParser();

            var inputCode = @"VARIABLE var1 = ""Hello
World""";
            var program = parser.Parse(inputCode);
            Assert.AreEqual("Hello\r\nWorld", ((PrimitiveExpression)program.Program.GlobalVariables.Single().DefaultValue).Value);
        }

        [TestMethod]
        public void BaZicParserPrimitiveExpressionNestedQuotes()
        {
            var parser = new BaZicParser();

            var inputCode = @"VARIABLE var1 = ""Hel\""l\r\no""";
            var program = parser.Parse(inputCode);
            Assert.AreEqual(@"Hel""l
o", ((PrimitiveExpression)program.Program.GlobalVariables.Single().DefaultValue).Value);
        }

        [TestMethod]
        public void BaZicParserPrimitiveExpressionInt()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = 1";
            var program = parser.Parse(inputCode);
            Assert.AreEqual(1, ((PrimitiveExpression)program.Program.GlobalVariables.Single().DefaultValue).Value);
        }

        [TestMethod]
        public void BaZicParserPrimitiveExpressionStringInt()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = \"1\"";
            var program = parser.Parse(inputCode);
            Assert.AreEqual("1", ((PrimitiveExpression)program.Program.GlobalVariables.Single().DefaultValue).Value);
        }

        [TestMethod]
        public void BaZicParserPrimitiveExpressionDouble()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = 1.2";
            var program = parser.Parse(inputCode);
            Assert.AreEqual(1.2, ((PrimitiveExpression)program.Program.GlobalVariables.Single().DefaultValue).Value);
        }

        [TestMethod]
        public void BaZicParserPrimitiveExpressionTrue()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = TRUE";
            var program = parser.Parse(inputCode);
            Assert.AreEqual(true, ((PrimitiveExpression)program.Program.GlobalVariables.Single().DefaultValue).Value);
        }

        [TestMethod]
        public void BaZicParserPrimitiveExpressionFalse()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = False";
            var program = parser.Parse(inputCode);
            Assert.AreEqual(false, ((PrimitiveExpression)program.Program.GlobalVariables.Single().DefaultValue).Value);
        }

        [TestMethod]
        public void BaZicParserPrimitiveExpressionNull()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = NULL";
            var program = parser.Parse(inputCode);
            Assert.IsNull(((PrimitiveExpression)program.Program.GlobalVariables.Single().DefaultValue).Value);
        }

        [TestMethod]
        public void BaZicParserPrimitiveExpressionBadReference()
        {
            var parser = new BaZicParser();

            var inputCode = @"
FUNCTION Method()
    VARIABLE var1 = NULLFOO
END FUNCTION
";
            var program = parser.Parse(inputCode);
            Assert.AreEqual("The name 'NULLFOO' does not exist in the current context.", program.Issues.InnerException.Message);
        }

        [TestMethod]
        public void BaZicParserPrimitiveExpressionArray()
        {
            var parser = new BaZicParser();

            var inputCode = @"VARIABLE var1[] = NEW [""Hello"", NEW [""Foo"", ""Bar"", ""Buz""]]";
            var program = parser.Parse(inputCode);
            Assert.AreEqual(2, ((ArrayCreationExpression)program.Program.GlobalVariables.Single().DefaultValue).Values.Count);
        }

        [TestMethod]
        public void BaZicParserPrimitiveExpressionUnexpectedIntBeforeString()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = 1\"Hello\"";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(6, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("Syntax error. Unexpected or missing character.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserPrimitiveExpressionOperatorExpected()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = 1 \"Hello\"";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(5, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("Invalid expression. 'New line', 'Start code', 'End code', 'Comment' is expected.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserPrimitiveExpressionMethodNameExpected()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = 1()";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(2, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("Method name expected.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserExpressionGroupSeparator()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = (1)";
            var program = parser.Parse(inputCode);
            Assert.AreEqual(1, ((PrimitiveExpression)program.Program.GlobalVariables.Single().DefaultValue).Value);
        }

        [TestMethod]
        public void BaZicParserExpressionGroupSeparatorNested()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = ((1))";
            var program = parser.Parse(inputCode);
            Assert.AreEqual(1, ((PrimitiveExpression)program.Program.GlobalVariables.Single().DefaultValue).Value);
        }

        [TestMethod]
        public void BaZicParserExpressionGroupSeparatorMissingLeftParen()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = 1)";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(6, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("Invalid expression. 'New line', 'Start code', 'End code', 'Comment' is expected.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserExpressionGroupSeparatorMissingRightParen()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = (1";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(3, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("Invalid expression. ')' is expected.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserExpressionGroupSeparatorMissingRightParenInNested()
        {
            var parser = new BaZicParser();

            var inputCode = "VARIABLE var1 = ((1)";

            var program = parser.Parse(inputCode);
            Assert.AreEqual(3, program.Issues.InnerExceptions.Count);
            var exception = (BaZicParserException)program.Issues.InnerExceptions.First();
            Assert.AreEqual("Invalid expression. ')' is expected.", exception.Message);
        }

        [TestMethod]
        public void BaZicParserExpressionClassReference()
        {
            var parser = new BaZicParser();

            var inputCode = @"
EXTERN FUNCTION Main(args[])
    VARIABLE var1 = MyClass.MyProperty
END FUNCTION";
            var program = parser.Parse(inputCode);
            Assert.AreEqual(3, program.Issues.InnerExceptions.Count);
            Assert.AreEqual("The name 'MyClass' does not look like a valid namespace or variable.", program.Issues.InnerExceptions.First().Message);



            inputCode = @"
EXTERN FUNCTION Main(args[])
    VARIABLE var2 = System.MyClass.MyProperty
    VARIABLE var3 = System.Core.MyClass.MyProperty
    VARIABLE var4 = System.Core.MyClass.MyProperty[0]
    VARIABLE var5 = AWAIT System.Core.MyClass.MyMethod(var2)
    RETURN System.Core.MyClass.MyMethod(var2)
END FUNCTION";
            program = parser.Parse(inputCode);
            Assert.AreEqual(3, program.Issues.InnerExceptions.Count);

            var stmts = program.Program.Methods.First().Statements;

            var expr = (PropertyReferenceExpression)((VariableDeclaration)stmts[0]).DefaultValue;
            var classRef = (ClassReferenceExpression)expr.TargetObject;
            Assert.AreEqual("MyProperty", expr.PropertyName.Identifier);
            Assert.AreEqual("MyClass", classRef.ClassName.Identifier);
            Assert.AreEqual("System", classRef.Namespace);

            expr = (PropertyReferenceExpression)((VariableDeclaration)stmts[1]).DefaultValue;
            classRef = (ClassReferenceExpression)expr.TargetObject;
            Assert.AreEqual("MyProperty", expr.PropertyName.Identifier);
            Assert.AreEqual("MyClass", classRef.ClassName.Identifier);
            Assert.AreEqual("System.Core", classRef.Namespace);

            var arrayIndex = (ArrayIndexerExpression)((VariableDeclaration)stmts[2]).DefaultValue;
            expr = (PropertyReferenceExpression)arrayIndex.TargetObject;
            classRef = (ClassReferenceExpression)expr.TargetObject;
            Assert.AreEqual("MyProperty", expr.PropertyName.Identifier);
            Assert.AreEqual("MyClass", classRef.ClassName.Identifier);
            Assert.AreEqual("System.Core", classRef.Namespace);

            var methodInvoke = (InvokeCoreMethodExpression)((VariableDeclaration)stmts[3]).DefaultValue;
            classRef = (ClassReferenceExpression)methodInvoke.TargetObject;
            Assert.AreEqual("MyMethod", methodInvoke.MethodName.Identifier);
            Assert.AreEqual("MyClass", classRef.ClassName.Identifier);
            Assert.AreEqual("System.Core", classRef.Namespace);

            methodInvoke = (InvokeCoreMethodExpression)((ReturnStatement)stmts[4]).Expression;
            classRef = (ClassReferenceExpression)methodInvoke.TargetObject;
            Assert.AreEqual("MyMethod", methodInvoke.MethodName.Identifier);
            Assert.AreEqual("MyClass", classRef.ClassName.Identifier);
            Assert.AreEqual("System.Core", classRef.Namespace);
        }
    }
}
