using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using System;

namespace BaZic.Runtime.BaZic.Runtime.Debugger
{
    /// <summary>
    /// Provide information about an error during the execution of an algorithm.
    /// </summary>
    [Serializable]
    public sealed class Error
    {
        #region Properties

        /// <summary>
        /// Gets or sets the thrown exception.
        /// </summary>
        public BaZicInterpreterException Exception { get; private set; }

        /// <summary>
        /// Gets the related syntax tree part to the exception.
        /// </summary>
        public NodeObject SyntaxTreeObject { get; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        public Error()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="exception">The exception thrown.</param>
        public Error(BaZicInterpreterException exception)
        {
            Exception = exception;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="exception">The exception thrown.</param>
        /// <param name="syntaxTreeObject">The algorithm object where the problem comes.</param>
        public Error(BaZicInterpreterException exception, NodeObject syntaxTreeObject)
            : this(exception)
        {
            SyntaxTreeObject = syntaxTreeObject;
        }

        #endregion
    }
}
