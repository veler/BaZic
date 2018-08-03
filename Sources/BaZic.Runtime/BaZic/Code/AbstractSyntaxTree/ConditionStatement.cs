using System;
using System.Collections.Generic;
using System.Linq;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents a conditional statement in an algorithm, typically represented as an if statement
    /// </summary>
    [Serializable]
    public sealed class ConditionStatement : Statement
    {
        #region Properties

        /// <summary>
        /// Gets the expression to evaluate true or false
        /// </summary>
        public Expression Condition { get; }

        /// <summary>
        /// Gets a collection of statements to run when the condition is true
        /// </summary>
        public IReadOnlyList<Statement> TrueStatements { get; private set; }

        /// <summary>
        /// Gets a collection of statements to run when the condition is false
        /// </summary>
        public IReadOnlyList<Statement> FalseStatements { get; private set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of then <see cref="ConditionStatement"/> class.
        /// </summary>
        public ConditionStatement()
        {
            TrueStatements = new List<Statement>().AsReadOnly();
            FalseStatements = new List<Statement>().AsReadOnly();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionStatement"/> class.
        /// </summary>
        /// <param name="condition">The expression to evaluate true or false</param>
        public ConditionStatement(Expression condition)
            : this()
        {
            Condition = condition;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set condition's true statements.
        /// </summary>
        /// <param name="statements">The collection of statements to run when the condition is true</param>
        /// <returns>The current condition</returns>
        public ConditionStatement WithThenBody(params Statement[] statements)
        {
            TrueStatements = new List<Statement>(statements).AsReadOnly();
            return this;
        }

        /// <summary>
        /// Set condition's false statements.
        /// </summary>
        /// <param name="statements">The collection of statements to run when the condition is false</param>
        /// <returns>The current condition</returns>
        public ConditionStatement WithElseBody(params Statement[] statements)
        {
            FalseStatements = new List<Statement>(statements).AsReadOnly();
            return this;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new ConditionStatement(Condition)
            {
                Column = Column,
                Id = Id,
                Line = Line,
                StartOffset = StartOffset,
                NodeLength = NodeLength
            }
            .WithThenBody(TrueStatements.ToArray())
            .WithElseBody(FalseStatements.ToArray());
        }

        #endregion
    }
}
