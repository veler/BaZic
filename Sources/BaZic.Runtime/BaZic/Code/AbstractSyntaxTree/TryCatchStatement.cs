using System;
using System.Collections.Generic;
using System.Linq;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents a try block with an optional catch clauses.
    /// </summary>
    [Serializable]
    public sealed class TryCatchStatement : Statement
    {
        #region Properties

        /// <summary>
        /// Gets a collection of statements to evaluate
        /// </summary>
        public IReadOnlyList<Statement> TryStatements { get; private set; }

        /// <summary>
        /// Gets a collection of statements to run in case of exception thrown
        /// </summary>
        public IReadOnlyList<Statement> CatchStatements { get; private set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of then <see cref="TryCatchStatement"/> class.
        /// </summary>
        public TryCatchStatement()
        {
            TryStatements = new List<Statement>().AsReadOnly();
            CatchStatements = new List<Statement>().AsReadOnly();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set try clause's statements.
        /// </summary>
        /// <param name="statements">The collection of statements to try to run</param>
        /// <returns>The current try catch statement</returns>
        public TryCatchStatement WithTryBody(params Statement[] statements)
        {
            TryStatements = new List<Statement>(statements).AsReadOnly();
            return this;
        }

        /// <summary>
        /// Set catch clause's statements.
        /// </summary>
        /// <param name="statements">The collection of statements to run chen an exception is thrown</param>
        /// <returns>The current try catch statement</returns>
        public TryCatchStatement WithCatchBody(params Statement[] statements)
        {
            CatchStatements = new List<Statement>(statements).AsReadOnly();
            return this;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new TryCatchStatement()
            {
                Column = Column,
                Id = Id,
                Line = Line,
                StartOffset = StartOffset,
                NodeLength = NodeLength
            }
            .WithTryBody(TryStatements.ToArray())
            .WithCatchBody(CatchStatements.ToArray());
        }

        #endregion
    }
}
