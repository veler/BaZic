using BaZic.Core.ComponentModel;
using BaZic.Core.Enums;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BaZic.Runtime.BaZic.Code
{
    /// <summary>
    /// Provides a BaZic code generator
    /// </summary>
    public class BaZicCodeGenerator
    {
        #region Fields & Constants

        private int _indentSpaceCount;

        #endregion

        #region Methods

        /// <summary>
        /// Generates a BaZic code from a syntax tree.
        /// </summary>
        /// <param name="syntaxTree">The syntax tree that represents the algorithm</param>
        /// <returns>A BaZic code</returns>
        public string Generate(BaZicProgram syntaxTree)
        {
            if (syntaxTree is BaZicUiProgram)
            {
                return Generate((BaZicUiProgram)syntaxTree);
            }

            _indentSpaceCount = 0;

            var globalVariables = new List<string>();
            foreach (var variable in syntaxTree.GlobalVariables)
            {
                globalVariables.Add(GenerateVariableDeclaration(variable));
            }

            var globalVariablesString = string.Join(Environment.NewLine, globalVariables);
            if (!string.IsNullOrWhiteSpace(globalVariablesString))
            {
                globalVariablesString += Environment.NewLine + Environment.NewLine;
            }

            var methods = new List<string>();
            foreach (var method in syntaxTree.Methods)
            {
                methods.Add(GenerateMethodDeclaration(method));
            }

            var methodsString = string.Join(Environment.NewLine + Environment.NewLine, methods);

            return $"# BaZic code generated automatically" + Environment.NewLine + Environment.NewLine +
                   $"{globalVariablesString}" +
                   $"{methodsString}";
        }

        /// <summary>
        /// Generates a BaZic code with UI from a syntax tree.
        /// </summary>
        /// <param name="syntaxTree">The syntax tree that represents the algorithm</param>
        /// <returns>A BaZic code</returns>
        public string Generate(BaZicUiProgram syntaxTree)
        {
            _indentSpaceCount = 0;

            var globalVariables = new List<string>();
            foreach (var variable in syntaxTree.GlobalVariables)
            {
                globalVariables.Add(GenerateVariableDeclaration(variable));
            }

            var globalVariablesString = string.Join(Environment.NewLine, globalVariables);
            if (!string.IsNullOrWhiteSpace(globalVariablesString))
            {
                globalVariablesString += Environment.NewLine + Environment.NewLine;
            }

            var bindings = new List<string>();
            foreach (var binding in syntaxTree.UiBindings)
            {
                bindings.Add(GenerateBindingDeclaration(binding));
            }

            var bindingsString = string.Join(Environment.NewLine, bindings);
            if (!string.IsNullOrWhiteSpace(bindingsString))
            {
                bindingsString += Environment.NewLine + Environment.NewLine;
            }

            var methods = new List<string>();
            foreach (var method in syntaxTree.Methods)
            {
                methods.Add(GenerateMethodDeclaration(method, syntaxTree.UiEvents.SingleOrDefault(e => e.MethodId == method.Id)));
            }

            var methodsString = string.Join(Environment.NewLine + Environment.NewLine, methods);

            return $"# BaZic code generated automatically" + Environment.NewLine + Environment.NewLine +
                   $"{globalVariablesString}" +
                   $"{bindingsString}" +
                   $"{methodsString}";
        }

        /// <summary>
        /// Generates the code for an <see cref="Expression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A BaZic code</returns>
        private string GenerateExpression(Expression expression)
        {
            if (expression == null)
            {
                return string.Empty;
            }

            switch (expression)
            {
                case ReferenceExpression reference:
                    return GenerateReferenceExpression(reference);

                case BinaryOperatorExpression binaryOperator:
                    return GenerateBinaryOperatorExpression(binaryOperator);

                case NotOperatorExpression notOperator:
                    return GenerateNotOperatorExpression(notOperator);

                default:
                    throw new NotImplementedException(L.BaZic.BaZicCodeGenerator.FormattedNoGeneratorForExpressionImplemented(expression.GetType().FullName));
            }
        }

        /// <summary>
        /// Generates the code for a <see cref="ReferenceExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A BaZic code</returns>
        private string GenerateReferenceExpression(ReferenceExpression expression)
        {
            switch (expression)
            {
                case ArrayCreationExpression arrayCreation:
                    return GenerateArrayCreationExpression(arrayCreation);

                case ArrayIndexerExpression arrayIndexer:
                    return GenerateArrayIndexerExpression(arrayIndexer);

                case ClassReferenceExpression classReference:
                    return GenerateClassReferenceExpression(classReference);

                case PropertyReferenceExpression propertyReference:
                    return GeneratePropertyReferenceExpression(propertyReference);

                case VariableReferenceExpression variableReference:
                    return GenerateVariableReferenceExpression(variableReference);

                case ExceptionReferenceExpression exception:
                    return GenerateExceptionReferenceExpression(exception);

                case InstantiateExpression instantiate:
                    return GenerateInstantiateExpression(instantiate);

                case InvokeCoreMethodExpression invokeCoreMethod:
                    return GenerateInvokeCoreMethodExpression(invokeCoreMethod);

                case InvokeMethodExpression invokeMethod:
                    return GenerateInvokeMethodExpression(invokeMethod);

                case PrimitiveExpression primitive:
                    return GeneratePrimitiveExpression(primitive);

                default:
                    throw new NotImplementedException(L.BaZic.BaZicCodeGenerator.FormattedNoGeneratorForExpressionImplemented(expression.GetType().FullName));
            }
        }

        /// <summary>
        /// Generates the code for a <see cref="Statement"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A BaZic code</returns>
        private string GenerateStatement(Statement statement)
        {
            switch (statement)
            {
                case CommentStatement comment:
                    return GenerateCommentStatement(comment);

                case LabelDeclaration label:
                    return GenerateLabelDeclaration(label);

                case GoToLabelStatement gotoLabel:
                    return GenerateGoToLabelStatement(gotoLabel);

                case LabelConditionStatement labelCondition:
                    return GenerateLabelConditionStatement(labelCondition);

                case AssignStatement assign:
                    return GenerateAssignStatement(assign);

                case ConditionStatement condition:
                    return GenerateConditionStatement(condition);

                case IterationStatement iteration:
                    return GenerateIterationStatement(iteration);

                case TryCatchStatement tryCatch:
                    return GenerateTryCatchStatement(tryCatch);

                case VariableDeclaration variable:
                    return GenerateVariableDeclaration(variable);

                case ReturnStatement @return:
                    return GenerateReturnStatement(@return);

                case ThrowStatement @throw:
                    return GenerateThrowStatement(@throw);

                case BreakStatement @break:
                    return GenerateBreakStatement();

                case BreakpointStatement breakpoint:
                    return GenerateBreakpointStatement();

                case ExpressionStatement expression:
                    return GenerateExpressionStatement(expression);

                default:
                    throw new NotImplementedException(L.BaZic.BaZicCodeGenerator.FormattedNoGeneratorForStatementImplemented(statement.GetType().FullName));
            }
        }

        /// <summary>
        /// Generates the code for an <see cref="ArrayIndexerExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A BaZic code</returns>
        private string GenerateArrayIndexerExpression(ArrayIndexerExpression expression)
        {
            Requires.NotNull(expression.TargetObject, nameof(expression.TargetObject));
            Requires.NotNull(expression.Indexes, nameof(expression.Indexes));

            var targetObject = GenerateReferenceExpression(expression.TargetObject);
            var indexes = new List<string>();
            foreach (var index in expression.Indexes)
            {
                indexes.Add(GenerateExpression(index));
            }

            return $"{targetObject}[{string.Join(", ", indexes)}]";
        }

        /// <summary>
        /// Generates the code for an <see cref="ClassReferenceExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A BaZic code</returns>
        private string GenerateClassReferenceExpression(ClassReferenceExpression expression)
        {
            return $"{expression.Namespace}.{expression.ClassName}";
        }

        /// <summary>
        /// Generates the code for an <see cref="PropertyReferenceExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A BaZic code</returns>
        private string GeneratePropertyReferenceExpression(PropertyReferenceExpression expression)
        {
            Requires.NotNull(expression.TargetObject, nameof(expression.TargetObject));

            var targetObject = GenerateReferenceExpression(expression.TargetObject);

            return $"{targetObject}.{expression.PropertyName}";
        }

        /// <summary>
        /// Generates the code for an <see cref="VariableReferenceExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A BaZic code</returns>
        private string GenerateVariableReferenceExpression(VariableReferenceExpression expression)
        {
            Requires.NotNull(expression.Name, nameof(expression.Name));

            return expression.Name.ToString();
        }

        /// <summary>
        /// Generates the code for an <see cref="ExceptionReferenceExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A BaZic code</returns>
        private string GenerateExceptionReferenceExpression(ExceptionReferenceExpression expression)
        {
            return "EXCEPTION";
        }

        /// <summary>
        /// Generates the code for an <see cref="BinaryOperatorExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A BaZic code</returns>
        private string GenerateBinaryOperatorExpression(BinaryOperatorExpression expression)
        {
            Requires.NotNull(expression.LeftExpression, nameof(expression.LeftExpression));
            Requires.NotNull(expression.RightExpression, nameof(expression.RightExpression));

            var leftExpression = GenerateExpression(expression.LeftExpression);
            if (expression.LeftExpression is BinaryOperatorExpression || expression.LeftExpression is NotOperatorExpression)
            {
                leftExpression = $"({leftExpression})";
            }

            var rightExpression = GenerateExpression(expression.RightExpression);
            if (expression.RightExpression is BinaryOperatorExpression || expression.RightExpression is NotOperatorExpression)
            {
                rightExpression = $"({rightExpression})";
            }

            var @operator = string.Empty;
            switch (expression.Operator)
            {
                case BinaryOperatorType.Equality:
                    @operator = "=";
                    break;

                case BinaryOperatorType.BitwiseOr:
                    @operator = "OR";
                    break;

                case BinaryOperatorType.BitwiseAnd:
                    @operator = "AND";
                    break;

                case BinaryOperatorType.LogicalOr:
                    @operator = "OR";
                    break;

                case BinaryOperatorType.LogicalAnd:
                    @operator = "AND";
                    break;

                case BinaryOperatorType.LessThan:
                    @operator = "<";
                    break;

                case BinaryOperatorType.LessThanOrEqual:
                    @operator = "<=";
                    break;

                case BinaryOperatorType.GreaterThan:
                    @operator = ">";
                    break;

                case BinaryOperatorType.GreaterThanOrEqual:
                    @operator = ">=";
                    break;

                case BinaryOperatorType.Addition:
                    @operator = "+";
                    break;

                case BinaryOperatorType.Subtraction:
                    @operator = "-";
                    break;

                case BinaryOperatorType.Multiply:
                    @operator = "*";
                    break;

                case BinaryOperatorType.Division:
                    @operator = "/";
                    break;

                case BinaryOperatorType.Modulus:
                    @operator = "%";
                    break;

                default:
                    throw new NotImplementedException(L.BaZic.BaZicCodeGenerator.FormattedNoGeneratorForOperatorImplemented(expression.Operator.GetDescription()));
            }

            return $"{leftExpression} {@operator} {rightExpression}";
        }

        /// <summary>
        /// Generates the code for an <see cref="NotOperatorExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A BaZic code</returns>
        private string GenerateNotOperatorExpression(NotOperatorExpression expression)
        {
            Requires.NotNull(expression.Expression, nameof(expression.Expression));

            var expressionString = GenerateExpression(expression.Expression);
            if (expression.Expression is BinaryOperatorExpression || expression.Expression is NotOperatorExpression)
            {
                return $"NOT ({expressionString})";
            }

            return $"NOT {expressionString}";
        }

        /// <summary>
        /// Generates the code for an <see cref="InstantiateExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A BaZic code</returns>
        private string GenerateInstantiateExpression(InstantiateExpression expression)
        {
            Requires.NotNull(expression.CreateType, nameof(expression.CreateType));

            var type = GenerateClassReferenceExpression(expression.CreateType);
            var arguments = new List<string>();

            foreach (var argument in expression.Arguments)
            {
                arguments.Add(GenerateExpression(argument));
            }

            return $"NEW {type}({string.Join(", ", arguments)})";
        }

        /// <summary>
        /// Generates the code for an <see cref="PrimitiveExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A BaZic code</returns>
        private string GeneratePrimitiveExpression(PrimitiveExpression expression)
        {
            return GenerateObjectStringRepresentation(expression.Value);
        }

        /// <summary>
        /// Generates the code for an <see cref="ArrayCreationExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A BaZic code</returns>
        private string GenerateArrayCreationExpression(ArrayCreationExpression expression)
        {
            var values = new List<string>();
            foreach (var item in expression.Values)
            {
                values.Add(GenerateExpression(item));
            }

            return $"NEW [{string.Join(", ", values)}]";
        }

        /// <summary>
        /// Generates a <see cref="String"/> representation of an <see cref="Object"/>
        /// </summary>
        /// <param name="value">The value to convert to string</param>
        /// <returns>A string representation of the value</returns>
        private string GenerateObjectStringRepresentation(object value)
        {
            if (value == null)
            {
                return "NULL";
            }

            switch (value)
            {
                case bool typedValue:
                    return typedValue.ToString().ToUpperInvariant();

                case string typedValue:
                    return $"\"{FormatLiteralString(typedValue)}\"";

                case Array typedValue:
                    var values = new List<string>();
                    foreach (var item in typedValue)
                    {
                        values.Add(GenerateObjectStringRepresentation(item));
                    }

                    return $"NEW [{string.Join(", ", values)}]";

                default:
                    return value.ToString();
            }
        }

        /// <summary>
        /// Generates the code for an <see cref="InvokeMethodExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A BaZic code</returns>
        private string GenerateInvokeMethodExpression(InvokeMethodExpression expression)
        {
            Requires.NotNull(expression.MethodName, nameof(expression.MethodName));

            var wait = string.Empty;
            var arguments = new List<string>();

            foreach (var argument in expression.Arguments)
            {
                arguments.Add(GenerateExpression(argument));
            }

            if (expression.Await)
            {
                wait = "AWAIT ";
            }

            return $"{wait}{expression.MethodName}({string.Join(", ", arguments)})";
        }

        /// <summary>
        /// Generates the code for an <see cref="InvokeCoreMethodExpression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>A BaZic code</returns>
        private string GenerateInvokeCoreMethodExpression(InvokeCoreMethodExpression expression)
        {
            Requires.NotNull(expression.TargetObject, nameof(expression.TargetObject));
            Requires.NotNull(expression.MethodName, nameof(expression.MethodName));

            var targetObject = GenerateReferenceExpression(expression.TargetObject);

            var arguments = new List<string>();
            foreach (var argument in expression.Arguments)
            {
                arguments.Add(GenerateExpression(argument));
            }

            var wait = string.Empty;
            if (expression.Await)
            {
                wait = "AWAIT ";
            }

            return $"{wait}{targetObject}.{expression.MethodName}({string.Join(", ", arguments)})";
        }

        /// <summary>
        /// Generates the code for a <see cref="CommentStatement"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A BaZic code</returns>
        private string GenerateCommentStatement(CommentStatement statement)
        {
            return $"# {statement.Comment}";
        }

        /// <summary>
        /// Generates the code for a <see cref="LabelDeclaration"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A BaZic code</returns>
        private string GenerateLabelDeclaration(LabelDeclaration statement)
        {
            return $"{statement.Name}:";
        }

        /// <summary>
        /// Generates the code for a <see cref="GoToLabelStatement"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A BaZic code</returns>
        private string GenerateGoToLabelStatement(GoToLabelStatement statement)
        {
            return $"GOTO {statement.Name}";
        }

        /// <summary>
        /// Generates the code for a <see cref="LabelConditionStatement"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A BaZic code</returns>
        private string GenerateLabelConditionStatement(LabelConditionStatement statement)
        {
            Requires.NotNull(statement.Condition, nameof(statement.Condition));

            var condition = GenerateExpression(statement.Condition);
            return $"IF {condition} GOTO {statement.LabelName}";
        }

        /// <summary>
        /// Generates the code for a <see cref="AssignStatement"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A BaZic code</returns>
        private string GenerateAssignStatement(AssignStatement statement)
        {
            Requires.NotNull(statement.LeftExpression, nameof(statement.LeftExpression));
            Requires.NotNull(statement.RightExpression, nameof(statement.RightExpression));

            var leftExpression = GenerateExpression(statement.LeftExpression);
            var rightExpression = GenerateExpression(statement.RightExpression);

            return $"{leftExpression} = {rightExpression}";
        }

        /// <summary>
        /// Generates the code for a <see cref="ConditionStatement"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A BaZic code</returns>
        private string GenerateConditionStatement(ConditionStatement statement)
        {
            Requires.NotNull(statement.Condition, nameof(statement.Condition));

            var condition = GenerateExpression(statement.Condition);

            var indent = IncreaseIndent();

            var trueStatements = new List<string>();
            foreach (var stmt in statement.TrueStatements)
            {
                trueStatements.Add($"{indent}{GenerateStatement(stmt)}");
            }

            var trueStatementsString = string.Join(Environment.NewLine, trueStatements);

            if (statement.FalseStatements.Count > 0)
            {
                var falseStatements = new List<string>();
                foreach (var stmt in statement.FalseStatements)
                {
                    falseStatements.Add($"{indent}{GenerateStatement(stmt)}");
                }

                var falseStatementsString = string.Join(Environment.NewLine, falseStatements);

                indent = DecreaseIndent();

                return $"IF {condition} THEN" + Environment.NewLine +
                       $"{trueStatementsString}" + Environment.NewLine +
                       $"{indent}ELSE" + Environment.NewLine +
                       $"{falseStatementsString}" + Environment.NewLine +
                       $"{indent}END IF";
            }

            indent = DecreaseIndent();

            return $"IF {condition} THEN" + Environment.NewLine +
                   $"{trueStatementsString}" + Environment.NewLine +
                   $"{indent}END IF";
        }

        /// <summary>
        /// Generates the code for a <see cref="IterationStatement"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A BaZic code</returns>
        private string GenerateIterationStatement(IterationStatement statement)
        {
            Requires.NotNull(statement.Condition, nameof(statement.Condition));

            var condition = GenerateExpression(statement.Condition);

            var indent = IncreaseIndent();

            var statements = new List<string>();
            foreach (var stmt in statement.Statements)
            {
                statements.Add($"{indent}{GenerateStatement(stmt)}");
            }

            var statementsString = string.Join(Environment.NewLine, statements);

            indent = DecreaseIndent();

            if (statement.ConditionAfterBody)
            {
                return $"DO" + Environment.NewLine +
                       $"{statementsString}" + Environment.NewLine +
                       $"{indent}LOOP WHILE {condition}";
            }
            else
            {
                return $"DO WHILE {condition}" + Environment.NewLine +
                       $"{statementsString}" + Environment.NewLine +
                       $"{indent}LOOP";
            }
        }

        /// <summary>
        /// Generates the code for a <see cref="TryCatchStatement"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A BaZic code</returns>
        private string GenerateTryCatchStatement(TryCatchStatement statement)
        {
            var indent = IncreaseIndent();

            var tryStatements = new List<string>();
            foreach (var stmt in statement.TryStatements)
            {
                tryStatements.Add($"{indent}{GenerateStatement(stmt)}");
            }

            var tryStatementsString = string.Join(Environment.NewLine, tryStatements);

            if (statement.CatchStatements.Count > 0)
            {
                var catchStatements = new List<string>();
                foreach (var stmt in statement.CatchStatements)
                {
                    catchStatements.Add($"{indent}{GenerateStatement(stmt)}");
                }

                var catchStatementsString = string.Join(Environment.NewLine, catchStatements);

                indent = DecreaseIndent();

                return $"TRY" + Environment.NewLine +
                       $"{tryStatementsString}" + Environment.NewLine +
                       $"{indent}CATCH" + Environment.NewLine +
                       $"{catchStatementsString}" + Environment.NewLine +
                       $"{indent}END TRY";
            }

            indent = DecreaseIndent();

            return $"TRY" + Environment.NewLine +
                   $"{tryStatementsString}" + Environment.NewLine +
                   $"{indent}END TRY";
        }

        /// <summary>
        /// Generates the code for a <see cref="VariableDeclaration"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A BaZic code</returns>
        private string GenerateVariableDeclaration(VariableDeclaration statement)
        {
            var arrayMarkup = statement.IsArray ? "[]" : string.Empty;

            if (statement.DefaultValue == null)
            {
                return $"VARIABLE {statement.Name}{arrayMarkup}";
            }

            var defaultValue = GenerateExpression(statement.DefaultValue);
            return $"VARIABLE {statement.Name}{arrayMarkup} = {defaultValue}";
        }

        /// <summary>
        /// Generates the code for a <see cref="BindingDeclaration"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A BaZic code</returns>
        private string GenerateBindingDeclaration(BindingDeclaration statement)
        {
            var arrayMarkup = statement.Variable.IsArray ? "[]" : string.Empty;

            if (statement.Variable.DefaultValue == null)
            {
                return $"BIND {statement.ControlName}_{statement.ControlPropertyName}{arrayMarkup}";
            }

            var defaultValue = GenerateExpression(statement.Variable.DefaultValue);
            return $"BIND {statement.ControlName}_{statement.ControlPropertyName}{arrayMarkup} = {defaultValue}";
        }

        /// <summary>
        /// Generates the code for a <see cref="ReturnStatement"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A BaZic code</returns>
        private string GenerateReturnStatement(ReturnStatement statement)
        {
            return $"RETURN {GenerateExpressionStatement(statement)}";
        }

        /// <summary>
        /// Generates the code for a <see cref="ThrowStatement"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A BaZic code</returns>
        private string GenerateThrowStatement(ThrowStatement statement)
        {
            return $"THROW {GenerateExpressionStatement(statement)}";
        }

        /// <summary>
        /// Generates the code for a <see cref="BreakStatement"/>.
        /// </summary>
        /// <returns>A BaZic code</returns>
        private string GenerateBreakStatement()
        {
            return "BREAK";
        }

        /// <summary>
        /// Generates the code for a <see cref="BreakpointStatement"/>.
        /// </summary>
        /// <returns>A BaZic code</returns>
        private string GenerateBreakpointStatement()
        {
            return "BREAKPOINT";
        }

        /// <summary>
        /// Generates the code for a <see cref="ExpressionStatement"/>.
        /// </summary>
        /// <param name="statement">The statement</param>
        /// <returns>A BaZic code</returns>
        private string GenerateExpressionStatement(ExpressionStatement statement)
        {
            return GenerateExpression(statement.Expression);
        }

        /// <summary>
        /// Generates the code for a <see cref="MethodDeclaration"/>.
        /// </summary>
        /// <param name="method">The method declaration</param>
        /// <param name="uiEvent">(optional) The UI event linked to the method.</param>
        /// <returns>A BaZic code</returns>
        private string GenerateMethodDeclaration(MethodDeclaration method, Event uiEvent = null)
        {
            Requires.NotNull(method.Name, nameof(method.Name));

            var indent = IncreaseIndent();

            var statements = new List<string>();
            foreach (var stmt in method.Statements)
            {
                statements.Add($"{indent}{GenerateStatement(stmt)}");
            }

            DecreaseIndent();

            var statementsString = string.Join(Environment.NewLine, statements);

            var arguments = new List<string>();
            foreach (var argument in method.Arguments)
            {
                arguments.Add(GenerateParameterDeclaration(argument));
            }

            var accessor = string.Empty;
            if (method.IsExtern)
            {
                accessor = "EXTERN ";
            }

            var eventAccessor = string.Empty;
            if (uiEvent != null)
            {
                eventAccessor = "EVENT ";
            }

            var asyncAccessor = string.Empty;
            if (method.IsAsync)
            {
                asyncAccessor = "ASYNC ";
            }

            return $"{accessor}{eventAccessor}{asyncAccessor}FUNCTION {method.Name}({string.Join(", ", arguments)})" + Environment.NewLine +
                   $"{statementsString}" + Environment.NewLine +
                   $"END FUNCTION";
        }

        /// <summary>
        /// Generates the code for a <see cref="ParameterDeclaration"/>.
        /// </summary>
        /// <param name="parameter">The parameter declaration</param>
        /// <returns>A BaZic code</returns>
        private string GenerateParameterDeclaration(ParameterDeclaration parameter)
        {
            Requires.NotNull(parameter.Name, nameof(parameter.Name));

            var arrayMarkup = parameter.IsArray ? "[]" : string.Empty;

            return $"{parameter.Name}{arrayMarkup}";
        }

        /// <summary>
        /// Transform a verbatim string to a literal string.
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>A verbatim string.</returns>
        private string FormatLiteralString(string input)
        {
            return input.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
        }

        /// <summary>
        /// Increase the indentation of 4 white spaces.
        /// </summary>
        /// <returns>Returns a string full of white spaces that correspond to the new indentation.</returns>
        private string IncreaseIndent()
        {
            _indentSpaceCount += 4;
            return new string(' ', _indentSpaceCount);
        }

        /// <summary>
        /// Decrease the indentation of 4 white spaces.
        /// </summary>
        /// <returns>Returns a string full of white spaces that correspond to the new indentation.</returns>
        private string DecreaseIndent()
        {
            _indentSpaceCount -= 4;
            return new string(' ', _indentSpaceCount);
        }

        #endregion
    }
}
