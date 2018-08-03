using BaZic.Core.ComponentModel;
using BaZic.Core.ComponentModel.Extensions;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BaZic.Runtime.BaZic.Code.Optimizer
{
    /// <summary>
    /// Provides an optimizer designed to inline a BaZic code and improve its performances for the interpreter.
    /// </summary>
    public sealed class BaZicOptimizer
    {
        #region Fields & Constants

        private UniqueShortNameGenerator _nameGenerator;
        private BaZicProgram _program;
        private List<MethodInformation> _methodInformations;
        private Stack<LabelDeclaration> _iterationEndLabels;

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BaZicOptimizer"/> class.
        /// </summary>
        public BaZicOptimizer()
        {
            _nameGenerator = new UniqueShortNameGenerator();
            _methodInformations = new List<MethodInformation>();
            _iterationEndLabels = new Stack<LabelDeclaration>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Optimize a BaZic syntax tree.
        /// </summary>
        /// <param name="program">The syntax tree to optimize.</param>
        /// <returns>Returns a new syntax tree that is optimized.</returns>
        public BaZicProgram Optimize(BaZicProgram program)
        {
            _nameGenerator.Reset();
            _program = program;
            _methodInformations.Clear();
            _iterationEndLabels.Clear();

            foreach (var method in program.Methods)
            {
                method.WithBody(OptimizeStatementBlock(method.Statements).ToArray());
            }

            if (program is BaZicUiProgram uiProgram)
            {
                return new BaZicUiProgram(true)
                {
                    Xaml = uiProgram.Xaml
                }
                .WithUiEvents(uiProgram.UiEvents.ToArray())
                .WithUiBindings(uiProgram.UiBindings.ToArray())
                .WithAssemblies(program.Assemblies.ToArray())
                .WithVariables(program.GlobalVariables.ToArray())
                .WithMethods(program.Methods.ToArray());
            }

            return new BaZicProgram(true)
                .WithAssemblies(program.Assemblies.ToArray())
                .WithVariables(program.GlobalVariables.ToArray())
                .WithMethods(program.Methods.ToArray());
        }

        /// <summary>
        /// Optimize and inline a block of statements.
        /// </summary>
        /// <param name="statements">The statements</param>
        /// <returns>A list of optimized statements.</returns>
        private List<Statement> OptimizeStatementBlock(IReadOnlyCollection<Statement> statements)
        {
            var newStatements = new List<Statement>();

            foreach (var statement in statements)
            {
                switch (statement.Clone())
                {
                    case BreakStatement breakStatement:
                        newStatements.Add(new GoToLabelStatement(_iterationEndLabels.Peek().Name.Identifier));
                        break;

                    case AssignStatement assign:
                        newStatements.AddRange(InlineAssign(assign));
                        break;

                    case ConditionStatement condition:
                        newStatements.AddRange(InlineCondition(condition));
                        break;

                    case ReturnStatement @return:
                        newStatements.AddRange(OptimizeReturn(@return));
                        break;

                    case ThrowStatement @throw:
                        @throw.Expression = OptimizeExpression(@throw.Expression);
                        newStatements.Add(@throw);
                        break;

                    case TryCatchStatement tryCatch:
                        tryCatch.WithTryBody(OptimizeStatementBlock(tryCatch.TryStatements).ToArray());
                        tryCatch.WithCatchBody(OptimizeStatementBlock(tryCatch.CatchStatements).ToArray());
                        newStatements.Add(tryCatch);
                        break;

                    case VariableDeclaration variable:
                        variable = FixVariableDeclaration(variable);
                        if (variable.DefaultValue == null || !variable.DefaultValue.IsExactly<InvokeMethodExpression>())
                        {
                            newStatements.Add(variable);
                        }
                        else
                        {
                            var defaultValue = variable.DefaultValue;
                            variable.WithDefaultValue(null);
                            newStatements.Add(variable);
                            newStatements.AddRange(InlineAssign(new AssignStatement(new VariableReferenceExpression(variable)
                            {
                                VariableDeclarationID = variable.Id
                            }, OptimizeExpression(defaultValue))));
                        }
                        break;

                    case ExpressionStatement expression:
                        if (expression.Expression.IsExactly<InvokeMethodExpression>())
                        {
                            var stmts = InlineMethodInvocation((InvokeMethodExpression)expression.Expression, null);
                            if (stmts != null)
                            {
                                newStatements.AddRange(stmts);
                                break;
                            }
                        }

                        expression.Expression = OptimizeExpression(expression.Expression);
                        newStatements.Add(expression);
                        break;

                    case IterationStatement iteration:
                        newStatements.AddRange(InlineIteration(iteration));
                        break;

                    default:
                        newStatements.Add(statement);
                        break;
                }
            }

            return newStatements;
        }

        /// <summary>
        /// Optimize an expression for the interpreter (mainly do the link between the variable declaration and its references).
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>An optimized expression.</returns>
        private Expression OptimizeExpression(Expression expression)
        {
            if (expression == null)
            {
                return null;
            }

            switch (expression.Clone())
            {
                case VariableReferenceExpression variableReference:
                    variableReference = FixVariableReferenceExpression(variableReference);
                    return variableReference;

                case PropertyReferenceExpression propertyReference:
                    propertyReference.TargetObject = (ReferenceExpression)OptimizeExpression(propertyReference.TargetObject);
                    return propertyReference;

                case NotOperatorExpression notOperator:
                    notOperator.Expression = OptimizeExpression(notOperator.Expression);
                    return notOperator;

                case InvokeCoreMethodExpression invokeCoreMethod:
                    var argsCore = new List<Expression>();
                    foreach (var argument in invokeCoreMethod.Arguments)
                    {
                        argsCore.Add(OptimizeExpression(argument));
                    }
                    invokeCoreMethod.TargetObject = (ReferenceExpression)OptimizeExpression(invokeCoreMethod.TargetObject);
                    invokeCoreMethod.WithParameters(argsCore.ToArray());
                    return invokeCoreMethod;

                case InvokeMethodExpression invokeMethod:
                    var args = new List<Expression>();
                    foreach (var argument in invokeMethod.Arguments)
                    {
                        args.Add(OptimizeExpression(argument));
                    }
                    invokeMethod.WithParameters(args.ToArray());
                    return invokeMethod;

                case InstantiateExpression instantiate:
                    var argsInstance = new List<Expression>();
                    foreach (var argument in instantiate.Arguments)
                    {
                        argsInstance.Add(OptimizeExpression(argument));
                    }
                    instantiate.WithParameters(argsInstance.ToArray());
                    instantiate.CreateType = (ClassReferenceExpression)OptimizeExpression(instantiate.CreateType);
                    return instantiate;

                case BinaryOperatorExpression binaryOperator:
                    binaryOperator.LeftExpression = OptimizeExpression(binaryOperator.LeftExpression);
                    binaryOperator.RightExpression = OptimizeExpression(binaryOperator.RightExpression);
                    return binaryOperator;

                case ArrayIndexerExpression arrayIndexer:
                    arrayIndexer.TargetObject = (ReferenceExpression)OptimizeExpression(arrayIndexer.TargetObject);
                    for (int i = 0; i < arrayIndexer.Indexes.Length; i++)
                    {
                        arrayIndexer.Indexes[i] = OptimizeExpression(arrayIndexer.Indexes[i]);
                    }
                    return arrayIndexer;

                case ArrayCreationExpression arrayCreation:
                    var values = new List<Expression>();
                    foreach (var val in arrayCreation.Values)
                    {
                        values.Add(OptimizeExpression(val));
                    }
                    arrayCreation.WithValues(values.ToArray());
                    return arrayCreation;

                default:
                    return expression;
            }
        }

        /// <summary>
        /// Inline a condition statement.
        /// </summary>
        /// <param name="conditionStatement">The condition to inline.</param>
        /// <returns>A set of statements that represents a condition.</returns>
        private List<Statement> InlineCondition(ConditionStatement conditionStatement)
        {
            var newStatements = new List<Statement>();

            if (conditionStatement.FalseStatements.Count > 0)
            {
                var elseLabel = CreateNewLabel();
                var endIfLabel = CreateNewLabel();
                newStatements.Add(new LabelConditionStatement(OptimizeExpression(conditionStatement.Condition), elseLabel.Name.Identifier));
                newStatements.AddRange(OptimizeStatementBlock(conditionStatement.TrueStatements));
                newStatements.Add(new GoToLabelStatement(endIfLabel.Name.Identifier));
                newStatements.Add(elseLabel);
                newStatements.AddRange(OptimizeStatementBlock(conditionStatement.FalseStatements));
                newStatements.Add(endIfLabel);
            }
            else
            {
                var endIfLabel = CreateNewLabel();
                newStatements.Add(new LabelConditionStatement(OptimizeExpression(conditionStatement.Condition), endIfLabel.Name.Identifier));
                newStatements.AddRange(OptimizeStatementBlock(conditionStatement.TrueStatements));
                newStatements.Add(endIfLabel);
            }

            return newStatements;
        }

        /// <summary>
        /// Inline an iteration statement.
        /// </summary>
        /// <param name="iterationStatement">The iteration to inline.</param>
        /// <returns>A set of statements that represents an iteration.</returns>
        private List<Statement> InlineIteration(IterationStatement iterationStatement)
        {
            var newStatements = new List<Statement>();

            var startIteration = CreateNewLabel();
            var endIteration = CreateNewLabel();

            _iterationEndLabels.Push(endIteration);

            newStatements.Add(startIteration);
            if (!iterationStatement.ConditionAfterBody)
            {
                newStatements.Add(new LabelConditionStatement(OptimizeExpression(iterationStatement.Condition), endIteration.Name.Identifier));
            }
            newStatements.AddRange(OptimizeStatementBlock(iterationStatement.Statements));
            if (iterationStatement.ConditionAfterBody)
            {
                newStatements.Add(new LabelConditionStatement(OptimizeExpression(iterationStatement.Condition), endIteration.Name.Identifier));
            }
            newStatements.Add(new GoToLabelStatement(startIteration.Name.Identifier));
            newStatements.Add(endIteration);

            _iterationEndLabels.Pop();

            return newStatements;
        }

        /// <summary>
        /// Inline, if possible a local method invocation.
        /// </summary>
        /// <param name="invokeMethod">The method invocation expression.</param>
        /// <param name="returnedValueAssignation">The expression that must receive the invocation result.</param>
        /// <returns>A set of statements that represents the inlined method.</returns>
        private List<Statement> InlineMethodInvocation(InvokeMethodExpression invokeMethod, Expression returnedValueAssignation)
        {
            var methodDeclaration = _program.Methods.SingleOrDefault(method => string.Compare(method.Name.Identifier, invokeMethod.MethodName.Identifier, StringComparison.Ordinal) == 0);

            if (methodDeclaration == null)
            {
                throw new BaZicParserException(invokeMethod.Line, invokeMethod.Column, invokeMethod.StartOffset, invokeMethod.NodeLength, L.BaZic.Optimizer.FormattedUndeclaredName(invokeMethod.MethodName));
            }
            else if (invokeMethod.Arguments.Count != methodDeclaration.Arguments.Count)
            {
                throw new BaZicParserException(invokeMethod.Line, invokeMethod.Column, invokeMethod.StartOffset, invokeMethod.NodeLength, L.BaZic.Optimizer.FormattedMethodNoMatchArguments(invokeMethod.MethodName, invokeMethod.Arguments.Count));
            }
            else if (methodDeclaration.IsAsync)
            {
                return null;
            }
            else if (_methodInformations.Count(method => method.MethodDeclaration.Id == methodDeclaration.Id) >= Consts.RecursivityLimit)
            {
                // After a certain number of recursive call, we stop to perform a recursive call.
                return null;
            }


            var newStatements = new List<Statement>();

            var startMethod = CreateNewLabel();
            var endMethod = CreateNewLabel();

            var arguments = new Dictionary<Guid, VariableDeclaration>();
            for (var i = 0; i < methodDeclaration.Arguments.Count; i++)
            {
                var argument = methodDeclaration.Arguments[i];
                arguments.Add(argument.Id, new VariableDeclaration(argument.Name.Identifier, argument.IsArray).WithDefaultValue(OptimizeExpression(invokeMethod.Arguments[i])));
            }

            var methodInfo = new MethodInformation()
            {
                MethodDeclaration = methodDeclaration,
                SubstituteVariableDeclarations = arguments,
                StartLabel = startMethod,
                EndLabel = endMethod,
                ReturnValueRecepter = new VariableDeclaration($"RET{startMethod.Name}", false)
            };

            _methodInformations.Insert(0, methodInfo);

            foreach (var substituteArgument in methodInfo.SubstituteVariableDeclarations)
            {
                newStatements.Add(substituteArgument.Value);
            }

            newStatements.Add(methodInfo.ReturnValueRecepter);
            newStatements.Add(startMethod);
            newStatements.AddRange(OptimizeStatementBlock(methodDeclaration.Statements));
            newStatements.Add(endMethod);

            if (returnedValueAssignation != null)
            {
                newStatements.Add(new AssignStatement(OptimizeExpression(returnedValueAssignation), new VariableReferenceExpression(methodInfo.ReturnValueRecepter.Name.Identifier)
                {
                    VariableDeclarationID = methodInfo.ReturnValueRecepter.Id
                }));
            }

            _methodInformations.RemoveAt(0);

            return newStatements;
        }

        /// <summary>
        /// Inline an assignment statement in the case of where the right expression is a method invocation.
        /// </summary>
        /// <param name="assignStatement">The assign statement.</param>
        /// <returns>A list of statement that represents the assignment.</returns>
        private List<Statement> InlineAssign(AssignStatement assignStatement)
        {
            var newStatements = new List<Statement>();


            if (assignStatement.RightExpression.IsExactly<InvokeMethodExpression>())
            {
                var stmts = InlineMethodInvocation((InvokeMethodExpression)assignStatement.RightExpression, assignStatement.LeftExpression);
                if (stmts != null)
                {
                    newStatements.AddRange(stmts);
                    return newStatements;
                }
            }

            assignStatement.LeftExpression = OptimizeExpression(assignStatement.LeftExpression);
            assignStatement.RightExpression = OptimizeExpression(assignStatement.RightExpression);
            newStatements.Add(assignStatement);

            return newStatements;
        }

        /// <summary>
        /// Inline a return statement in the case of where the right expression is a method invocation and exit the method.
        /// </summary>
        /// <param name="returnStatement">The return statement.</param>
        /// <returns>A list of statement that represents the return statement.</returns>
        private List<Statement> OptimizeReturn(ReturnStatement returnStatement)
        {
            var newStatements = new List<Statement>();

            if (_methodInformations.Count > 0)
            {
                var methodInfo = _methodInformations.First();
                if (returnStatement.Expression != null)
                {
                    newStatements.AddRange(InlineAssign(new AssignStatement(new VariableReferenceExpression(methodInfo.ReturnValueRecepter.Name.Identifier)
                    {
                        VariableDeclarationID = methodInfo.ReturnValueRecepter.Id
                    }, OptimizeExpression(returnStatement.Expression))));
                }
                newStatements.Add(new GoToLabelStatement(methodInfo.EndLabel.Name.Identifier));
            }
            else
            {
                if (returnStatement.Expression != null)
                {
                    if (returnStatement.Expression.IsExactly<InvokeMethodExpression>())
                    {
                        var returnVariableName = $"RET{CreateNewLabel().Name}";
                        var variableDecl = new VariableDeclaration(returnVariableName, false);
                        var variableRef = new VariableReferenceExpression(variableDecl)
                        {
                            VariableDeclarationID = variableDecl.Id
                        };
                        newStatements.Add(variableDecl);
                        newStatements.AddRange(InlineAssign(new AssignStatement(variableRef, OptimizeExpression(returnStatement.Expression))));
                        newStatements.Add(new ReturnStatement(variableRef));
                        return newStatements;
                    }
                    else
                    {
                        returnStatement.Expression = OptimizeExpression(returnStatement.Expression);
                    }
                }
                newStatements.Add(returnStatement);
            }

            return newStatements;
        }

        /// <summary>
        /// Replace a variable declaration by a new one where we are sure it has a unique ID and use it to make good links with the variable reference even if in the case of a code merge/concatenation, 2 varibles has the same name.
        /// </summary>
        /// <param name="variableDeclaration">The variable declaration.</param>
        /// <returns>A new variable declaration.</returns>
        private VariableDeclaration FixVariableDeclaration(VariableDeclaration variableDeclaration)
        {
            variableDeclaration.WithDefaultValue(OptimizeExpression(variableDeclaration.DefaultValue));

            if (_methodInformations.Count > 0)
            {
                var methodInfo = _methodInformations.First();
                var oldId = variableDeclaration.Id;
                variableDeclaration = new VariableDeclaration(variableDeclaration.Name.Identifier, variableDeclaration.IsArray).WithDefaultValue(variableDeclaration.DefaultValue);
                methodInfo.SubstituteVariableDeclarations.Add(oldId, variableDeclaration);
            }

            return variableDeclaration;
        }

        /// <summary>
        /// Replace a variable reference by a new one where we are sure it has a unique ID and use it to make good links with the variable declaration even if in the case of a code merge/concatenation, 2 varibles has the same name.
        /// </summary>
        /// <param name="variableReference">The variable reference.</param>
        /// <returns>A new variable reference.</returns>
        private VariableReferenceExpression FixVariableReferenceExpression(VariableReferenceExpression variableReference)
        {
            if (_methodInformations.Count > 0)
            {
                var methodInfo = _methodInformations.First();

                methodInfo.SubstituteVariableDeclarations.TryGetValue(variableReference.VariableDeclarationID, out VariableDeclaration substituteVariableDeclaration);

                if (substituteVariableDeclaration != null)
                {
                    return new VariableReferenceExpression(variableReference.Name.Identifier)
                    {
                        VariableDeclarationID = substituteVariableDeclaration.Id
                    };
                }
            }

            return variableReference;
        }

        /// <summary>
        /// Generates a new unique label declaration.
        /// </summary>
        /// <returns>A unique label declaration.</returns>
        private LabelDeclaration CreateNewLabel()
        {
            return new LabelDeclaration($"_{_nameGenerator.GetNextName()}");
        }

        #endregion
    }
}
