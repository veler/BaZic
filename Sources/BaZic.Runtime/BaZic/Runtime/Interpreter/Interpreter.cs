using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Debugger;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using BaZic.Runtime.BaZic.Runtime.Interpreter.Expression;
using BaZic.Runtime.BaZic.Runtime.Memory;
using BaZic.Runtime.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BaZic.Runtime.BaZic.Runtime.Interpreter
{
    /// <summary>
    /// Provides the bases of a program, method or block interpreter.
    /// </summary>
    internal abstract class Interpreter
    {
        #region Fields & Constants

        private readonly List<Variable> _variables = new List<Variable>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether the interpreter should stop its work as soon as possible.
        /// </summary>
        internal bool IsAborted
        {
            get
            {
                return BaZicInterpreter.State == BaZicInterpreterState.Stopped || BaZicInterpreter.State == BaZicInterpreterState.StoppedWithError;
            }
        }

        /// <summary>
        /// Gets the parent interpreter.
        /// </summary>
        internal Interpreter ParentInterpreter { get; }

        /// <summary>
        /// Gets the parent BaZic interpreter.
        /// </summary>
        protected BaZicInterpreterCore BaZicInterpreter { get; }

        /// <summary>
        /// Gets the list of variable in memory.
        /// </summary>
        internal IReadOnlyList<Variable> Variables => _variables.AsReadOnly();

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Interpreter"/> class.
        /// </summary>
        /// <param name="baZicInterpreter">The main interpreter.</param>
        /// <param name="parentInterpreter">The parent interpreter.</param>
        protected Interpreter(BaZicInterpreterCore baZicInterpreter, Interpreter parentInterpreter)
        {
            BaZicInterpreter = baZicInterpreter;
            ParentInterpreter = parentInterpreter;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new variable in memory.
        /// </summary>
        /// <param name="variableDeclaration">The variable declaration to use.</param>
        /// <param name="searchInParents">Defines whether finding a variable can be performed in the parent interpreters.</param>
        internal void AddVariable(VariableDeclaration variableDeclaration, bool searchInParents = true)
        {
            var existingVariable = GetVariable(variableDeclaration.Id, variableDeclaration.Name.Identifier, false, searchInParents);
            if (existingVariable != null)
            {
                if (!BaZicInterpreter.ProgramIsOptimized)
                {
                    BaZicInterpreter.ChangeState(this, new InternalException(L.BaZic.Runtime.Interpreters.Interpreter.FormattedDuplicatedVariableId(variableDeclaration.Id)), variableDeclaration);
                    return;
                }
                else
                {
                    var variableRef = new VariableReferenceExpression(existingVariable.Name)
                    {
                        Line = variableDeclaration.Line,
                        Column = variableDeclaration.Column,
                        StartOffset = variableDeclaration.StartOffset,
                        NodeLength = variableDeclaration.NodeLength,
                        VariableDeclarationID = existingVariable.Id
                    };
                    SetVariable(variableRef, RunExpression(variableDeclaration.DefaultValue));
                    return;
                }
            }

            if (IsAborted)
            {
                return;
            }

            var defaultValue = RunExpression(variableDeclaration.DefaultValue);
            var defaultValueInfo = ValueInfo.GetValueInfo(defaultValue);
            var variable = new Variable(variableDeclaration);

            AddVariable(variableDeclaration, variable, defaultValue, defaultValueInfo, true);
        }

        /// <summary>
        /// Creates a new binded variable in memory.
        /// </summary>
        /// <param name="bindingDeclaration">The binding declaration to use.</param>
        internal void AddVariable(BindingDeclaration bindingDeclaration)
        {
            if (GetType() != typeof(ProgramInterpreter))
            {
                BaZicInterpreter.ChangeState(this, new IncoherentStatementException(L.BaZic.Runtime.Interpreters.Interpreter.ForbiddenBinding), bindingDeclaration);
                return;
            }

            var existingVariable = GetVariable(bindingDeclaration.Id, bindingDeclaration.Variable.Name.Identifier, false, true);
            if (existingVariable != null)
            {
                BaZicInterpreter.ChangeState(this, new InternalException(L.BaZic.Runtime.Interpreters.Interpreter.FormattedDuplicatedVariableId(bindingDeclaration.Id)), bindingDeclaration);
                return;
            }

            var defaultValue = RunExpression(bindingDeclaration.Variable.DefaultValue);
            var defaultValueInfo = ValueInfo.GetValueInfo(defaultValue);

            if (IsAborted)
            {
                return;
            }

            var targetControl = ((ProgramInterpreter)this).UserInterface.FindName(bindingDeclaration.ControlName);

            if (targetControl == null)
            {
                BaZicInterpreter.ChangeState(this, new UiException(L.BaZic.Runtime.Interpreters.Interpreter.FormattedUiControlNotFound(bindingDeclaration.ControlName)));
                return;
            }

            var binding = new Binding(bindingDeclaration, targetControl, BaZicInterpreter);

            AddVariable(bindingDeclaration.Variable, binding, defaultValue, defaultValueInfo, false);
        }

        /// <summary>
        /// Adds a new variable to the memory.
        /// </summary>
        /// <param name="variableDeclaration">The variable declaration.</param>
        /// <param name="variable">The variable created from the <paramref name="variableDeclaration"/>.</param>
        /// <param name="defaultValue">The default value retrieved from the <paramref name="variableDeclaration"/>.</param>
        /// <param name="defaultValueInfo">The information about the <paramref name="defaultValue"/>.</param>
        /// <param name="setValueIfNull">Defines whether the value of the variable must be set if the given default value is null.</param>
        private void AddVariable(VariableDeclaration variableDeclaration, Variable variable, object defaultValue, ValueInfo defaultValueInfo, bool setValueIfNull)
        {
            if (IsAborted)
            {
                return;
            }

            if (!defaultValueInfo.IsNull)
            {
                if (variable.IsArray && !defaultValueInfo.IsArray)
                {
                    BaZicInterpreter.ChangeState(this, new NotAssignableException(L.BaZic.Runtime.Interpreters.Interpreter.FormattedNotArrayExpected(variableDeclaration.Name)), variableDeclaration.DefaultValue);
                    return;
                }
                else if (!variable.IsArray && defaultValueInfo.IsArray)
                {
                    BaZicInterpreter.ChangeState(this, new NotAssignableException(L.BaZic.Runtime.Interpreters.Interpreter.FormattedArrayExpected(variableDeclaration.Name)), variableDeclaration.DefaultValue);
                    return;
                }
            }

            if (defaultValueInfo.IsNull)
            {
                if (setValueIfNull)
                {
                    variable.SetValue(defaultValue, defaultValueInfo);
                }
            }
            else
            {
                variable.SetValue(defaultValue, defaultValueInfo);
            }

            _variables.Add(variable);

            if (GetType() != typeof(ProgramInterpreter))
            {
                GetParentMethodInterpreter(includeThis: true).DebugCallInfo.Variables.Add(variable);
            }

            if (BaZicInterpreter.Verbose)
            {
                VerboseLog(L.BaZic.Runtime.Interpreters.Interpreter.FormattedVariableDeclared(variable.Name, variable));
            }
        }

        /// <summary>
        /// Set the value of the specified variable currently in memory based on its unique ID.
        /// </summary>
        /// <param name="variableRef">The reference to the variable to set.</param>
        /// <param name="value">The value to give to the variable</param>
        internal void SetVariable(VariableReferenceExpression variableRef, object value)
        {
            var variable = GetVariable(variableRef.VariableDeclarationID, variableRef.Name.Identifier, true);

            if (IsAborted)
            {
                return;
            }

            var valueInfo = ValueInfo.GetValueInfo(value);

            if (!valueInfo.IsNull)
            {
                if (variable.IsArray && !valueInfo.IsArray)
                {
                    BaZicInterpreter.ChangeState(this, new NotAssignableException(L.BaZic.Runtime.Interpreters.Interpreter.FormattedArrayExpected(variableRef.Name)), variableRef);
                    return;
                }
                else if (!variable.IsArray && valueInfo.IsArray)
                {
                    BaZicInterpreter.ChangeState(this, new NotAssignableException(L.BaZic.Runtime.Interpreters.Interpreter.FormattedNotArrayExpected(variableRef.Name)), variableRef);
                    return;
                }
            }

            variable.SetValue(value, valueInfo);

            if (BaZicInterpreter.Verbose)
            {
                VerboseLog(L.BaZic.Runtime.Interpreters.Interpreter.FormattedVariableSetted(variable.Name, variable));
            }
        }

        /// <summary>
        /// Look for the specified variable currently in memory based on its unique ID.
        /// </summary>
        /// <param name="variableId">The unique ID that represents the variable.</param>
        /// <param name="variableName">The friendly name of the variable.</param>
        /// <param name="throwIfNotFound">(optional) Indicates whether an exception must be thrown if the variable is not found. By default, this argument is True.</param>
        /// <param name="searchInParents">Defines whether finding a variable can be performed in the parent interpreters.</param>
        /// <returns>Returns the variable if it has been found, otherwise, null or throws an exception.</returns>
        internal Variable GetVariable(Guid variableId, string variableName, bool throwIfNotFound = true, bool searchInParents = true)
        {
            var interpreter = this;

            do
            {
                var variable = interpreter.Variables.FirstOrDefault(v => v.Id == variableId);

                if (variable != null)
                {
                    return variable;
                }

                if (searchInParents)
                {
                    interpreter = interpreter.ParentInterpreter;
                }
            } while (searchInParents && interpreter != null);

            if (throwIfNotFound)
            {
                BaZicInterpreter.ChangeState(this, new VariableNotFoundException(variableName));
            }

            return null;
        }

        /// <summary>
        /// Retrieves the parent interpreter of the current executed method.
        /// </summary>
        /// <param name="includeThis">(optional) Defines whether the search must include the current interpreter.</param>
        /// <param name="throwIfNotFound">(optional) Defines whether an error must be thrown if the parent interpreter is not found.</param>
        /// <returns>The parent <see cref="MethodInterpreter"/></returns>
        internal MethodInterpreter GetParentMethodInterpreter(bool includeThis = false, bool throwIfNotFound = true)
        {
            if (includeThis && this is MethodInterpreter thisMethodInterpreter)
            {
                return thisMethodInterpreter;
            }

            var parentInterpreter = ParentInterpreter;

            while (parentInterpreter != null)
            {
                if (parentInterpreter is MethodInterpreter methodInterpreter)
                {
                    return methodInterpreter;
                }

                parentInterpreter = parentInterpreter.ParentInterpreter;
            }

            if (throwIfNotFound)
            {
                BaZicInterpreter.ChangeState(this, new InternalException(L.BaZic.Runtime.Interpreters.Interpreter.ParentMethodNotFound));
            }
            return null;
        }

        /// <summary>
        /// Execute an expression
        /// </summary>
        /// <param name="expression">The expression to interpret</param>
        /// <returns>Returns the returned value of the expression</returns>
        internal object RunExpression(Code.AbstractSyntaxTree.Expression expression)
        {
            if (expression == null)
            {
                return null;
            }

            if (BaZicInterpreter.Verbose)
            {
                VerboseLog(L.BaZic.Runtime.Interpreters.Interpreter.FormattedExecutingExpression(expression.GetType().Name));
            }

            object expressionResult = null;
            switch (expression)
            {
                case ArrayCreationExpression arrayCreation:
                    expressionResult = new ArrayCreationInterpreter(BaZicInterpreter, this, arrayCreation).Run();
                    break;

                case ArrayIndexerExpression arrayIndexer:
                    expressionResult = new ArrayIndexerInterpreter(BaZicInterpreter, this, arrayIndexer).Run();
                    break;

                case BinaryOperatorExpression binaryOperator:
                    expressionResult = new BinaryOperatorInterpreter(BaZicInterpreter, this, binaryOperator).Run();
                    break;

                case ClassReferenceExpression classReference:
                    expressionResult = new ClassReferenceInterpreter(BaZicInterpreter, this, classReference).Run();
                    break;

                case ExceptionReferenceExpression exceptionReference:
                    expressionResult = new ExceptionInterpreter(BaZicInterpreter, this, exceptionReference).Run();
                    break;

                case InstantiateExpression instantiate:
                    expressionResult = new InstantiateInterpreter(BaZicInterpreter, this, instantiate).Run();
                    break;

                case InvokeCoreMethodExpression invokeCoreMethod:
                    expressionResult = new InvokeCoreMethodInterpreter(BaZicInterpreter, this, invokeCoreMethod).Run();
                    break;

                case InvokeMethodExpression invokeMethod:
                    expressionResult = new InvokeMethodInterpreter(BaZicInterpreter, this, invokeMethod).Run();
                    break;

                case NotOperatorExpression notOperator:
                    expressionResult = new NotOperatorInterpreter(BaZicInterpreter, this, notOperator).Run();
                    break;

                case PrimitiveExpression primitive:
                    expressionResult = new PrimitiveInterpreter(BaZicInterpreter, this, primitive).Run();
                    break;

                case PropertyReferenceExpression propertyReference:
                    expressionResult = new PropertyReferenceInterpreter(BaZicInterpreter, this, propertyReference).Run();
                    break;

                case VariableReferenceExpression variableReference:
                    expressionResult = new VariableReferenceInterpreter(BaZicInterpreter, this, variableReference).Run();
                    break;

                default:
                    BaZicInterpreter.ChangeState(this, new InternalException(L.BaZic.Runtime.Interpreters.Interpreter.FormattedInterpreterNotFound(expression.GetType().Name)), expression);
                    break;
            }

            if (IsAborted)
            {
                return null;
            }

            if (BaZicInterpreter.Verbose)
            {
                VerboseLog(L.BaZic.Runtime.Interpreters.Interpreter.FormattedExpressionReturnedValue(expressionResult, ValueInfo.GetValueInfo(expressionResult)));
            }

            return expressionResult;
        }

        /// <summary>
        /// Add a new log
        /// </summary>
        /// <param name="source">The source from where we changed the state (an interpreter usually)</param>
        /// <param name="format">the message format</param>
        /// <param name="args">the message arguments</param>
        internal void VerboseLog(string message)
        {
            BaZicInterpreter.ChangeState(this, new BaZicInterpreterStateChangeEventArgs(message));
        }

        #endregion
    }
}
