using BaZic.Core.ComponentModel;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using System;

namespace BaZic.Runtime.BaZic.Code.Parser
{
    /// <summary>
    /// Represents the result of a BaZic source code parsing.
    /// </summary>
    public sealed class ParserResult
    {
        #region Properties

        /// <summary>
        /// Gets the parsed program.
        /// </summary>
        public BaZicProgram Program { get; }

        /// <summary>
        /// Get the issues related to the parsing.
        /// </summary>
        public AggregateException Issues { get; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserResult"/> class.
        /// </summary>
        /// <param name="program">The program</param>
        /// <param name="issues">The issues</param>
        public ParserResult(BaZicProgram program, AggregateException issues)
        {
            Requires.NotNull(issues, nameof(issues));

            Program = program;
            Issues = issues;
        }

        #endregion
    }
}
