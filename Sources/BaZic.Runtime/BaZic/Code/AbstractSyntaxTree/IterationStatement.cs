using System;
using System.Collections.Generic;
using System.Linq;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents an interation in an algorithm, typically represented by a for/while keyword.
    /// </summary>
    [Serializable]
    public sealed class IterationStatement : Statement
    {
        #region Properties

        /// <summary>
        /// Gets the statements in the iteration's body
        /// </summary>
        public IReadOnlyList<Statement> Statements { get; private set; }

        /// <summary>
        /// Gets the test expression of the iteration
        /// </summary>
        public Expression Condition { get; }

        /// <summary>
        /// Gets a value that define whether the test expression will be run before of after the execution of the iteration's body
        /// </summary>
        public bool ConditionAfterBody { get; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationStatement"/> class.
        /// </summary>
        public IterationStatement()
        {
            Statements = new List<Statement>().AsReadOnly();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationStatement"/> class.
        /// </summary>
        /// <param name="condition">The test expression of the iteration</param>
        /// <param name="conditionAfterBody">This value defines whether the test expression will be run before of after the execution of the iteration's body</param>
        public IterationStatement(Expression condition, bool conditionAfterBody = false)
            : this()
        {
            Condition = condition;
            ConditionAfterBody = conditionAfterBody;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set the body
        /// </summary>
        /// <param name="statements">The statements in the iteration's body</param>
        /// <returns>The current iteration statement</returns>
        public IterationStatement WithBody(params Statement[] statements)
        {
            Statements = new List<Statement>(statements).AsReadOnly();
            return this;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new IterationStatement(Condition, ConditionAfterBody)
            {
                Column = Column,
                Id = Id,
                Line = Line,
                StartOffset = StartOffset,
                NodeLength = NodeLength
            }
            .WithBody(Statements.ToArray());
        }

        #endregion
    }
}
