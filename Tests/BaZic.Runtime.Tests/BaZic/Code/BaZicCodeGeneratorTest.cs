using BaZic.Runtime.BaZic.Code;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaZic.Runtime.Tests.BaZic.Code
{
    [TestClass]
    public class BaZicCodeGeneratorTest
    {
        [TestMethod]
        public void BaZicCodeGeneratorIndent()
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

            var code = new BaZicCodeGenerator().Generate(program);

            var expected =
@"# BaZic code generated automatically

VARIABLE Foo

EXTERN FUNCTION Main(args[])
    VARIABLE Bar[]
    DO WHILE Foo = Bar
        IF TRUE THEN
            # If true
        ELSE
            # If not true
        END IF
    LOOP
END FUNCTION";

            Assert.AreEqual(expected, code);
        }

        [TestMethod]
        public void BaZicCodeGeneratorUi()
        {
            var program = new BaZicUiProgram()
                          .WithControlAccessors(new ControlAccessorDeclaration("Button1"))
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

            var code = new BaZicCodeGenerator().Generate(program);

            var expected =
@"# BaZic code generated automatically

VARIABLE Foo

EXTERN FUNCTION Main(args[])
    VARIABLE Bar[]
    DO WHILE Foo = Bar
        IF TRUE THEN
            # If true
        ELSE
            # If not true
        END IF
    LOOP
END FUNCTION";

            Assert.AreEqual(expected, code);
        }

        [TestMethod]
        public void BaZicCodeGeneratorCondition()
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

            var code = new BaZicCodeGenerator().Generate(program);

            var expected =
@"# BaZic code generated automatically

EXTERN FUNCTION Main(args[])
    IF ((1 < 2) AND (3 < 4)) OR (NOT FALSE) THEN
        # If true
    ELSE
        IF 1 < 2 THEN
            IF NOT (1 < 2) THEN
                # If true
            ELSE
                # If not true
            END IF
        ELSE
            # If not true
        END IF
    END IF
END FUNCTION";

            Assert.AreEqual(expected, code);
        }

        [TestMethod]
        public void BaZicCodeGeneratorTryCatch()
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

            var code = new BaZicCodeGenerator().Generate(program);

            var expected =
@"# BaZic code generated automatically

EXTERN FUNCTION Main(args[])
    TRY
        # Evaluation
    CATCH
        THROW EXCEPTION
    END TRY
END FUNCTION";

            Assert.AreEqual(expected, code);
        }

        [TestMethod]
        public void BaZicCodeGeneratorArrayIndexer()
        {
            var program = new BaZicProgram()
                          .WithMethods(
                              new EntryPointMethod()
                              .WithBody(
                                  new VariableDeclaration("Foo", true),
                                  new ReturnStatement(new ArrayIndexerExpression(new VariableReferenceExpression("Foo"), new Expression[] { new PrimitiveExpression(0) }))
                              )
                          );

            var code = new BaZicCodeGenerator().Generate(program);

            var expected =
@"# BaZic code generated automatically

EXTERN FUNCTION Main(args[])
    VARIABLE Foo[]
    RETURN Foo[0]
END FUNCTION";

            Assert.AreEqual(expected, code);
        }

        [TestMethod]
        public void BaZicCodeGeneratorClassReference()
        {
            var program = new BaZicProgram()
                          .WithMethods(
                              new EntryPointMethod()
                              .WithBody(
                                  new VariableDeclaration("Baz", true).WithDefaultValue(new InstantiateExpression(new ClassReferenceExpression("System", "Array"))),
                                  new VariableDeclaration("Boo", true).WithDefaultValue(new InstantiateExpression(new ClassReferenceExpression("System", "String")))
                              )
                          );

            var code = new BaZicCodeGenerator().Generate(program);

            var expected =
@"# BaZic code generated automatically

EXTERN FUNCTION Main(args[])
    VARIABLE Baz[] = NEW System.Array()
    VARIABLE Boo[] = NEW System.String()
END FUNCTION";

            Assert.AreEqual(expected, code);
        }

        [TestMethod]
        public void BaZicCodeGeneratorInvoke()
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

            var code = new BaZicCodeGenerator().Generate(program);

            var expected =
@"# BaZic code generated automatically

EXTERN FUNCTION Main(args[])
    AWAIT Foo(1, 2 - 1)
    VARIABLE integer = 1
    RETURN integer.ToString(""X"")
END FUNCTION

EXTERN ASYNC FUNCTION Foo(arg1[], arg2)

END FUNCTION";

            Assert.AreEqual(expected, code);
        }

        [TestMethod]
        public void BaZicCodeGeneratorPrimitiveValues()
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

            var code = new BaZicCodeGenerator().Generate(program);

            var expected =
@"# BaZic code generated automatically

EXTERN FUNCTION Main(args[])
    VARIABLE foo
    foo = 1
    foo = 1.1234
    foo = -1.5467
    foo = ""Hel\""l\r\no""
    foo = TRUE
    foo = FALSE
    foo = NULL
    VARIABLE bar[] = NEW [""Hello"", NEW [""Foo"", ""Bar"", ""Buz""]]
    bar = NEW [1, 1.1234, ""Hello""]
END FUNCTION";

            Assert.AreEqual(expected, code);
        }

        [TestMethod]
        public void BaZicCodeGeneratorProperty()
        {
            var program = new BaZicProgram()
                          .WithMethods(
                              new EntryPointMethod()
                              .WithBody(
                                  new VariableDeclaration("foo").WithDefaultValue(new PrimitiveExpression("Hello")),
                                  new ReturnStatement(new PropertyReferenceExpression(new VariableReferenceExpression("foo"), "Length"))
                              )
                          );

            var code = new BaZicCodeGenerator().Generate(program);

            var expected =
@"# BaZic code generated automatically

EXTERN FUNCTION Main(args[])
    VARIABLE foo = ""Hello""
    RETURN foo.Length
END FUNCTION";

            Assert.AreEqual(expected, code);
        }
    }
}
