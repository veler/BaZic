using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Memory;
using System;
using System.Collections.Generic;

namespace BaZic.Runtime.BaZic.Runtime.Debugger
{
    /// <summary>
    /// Represents a call in a call stack.
    /// </summary>
    [Serializable]
    public sealed class Call
    {
        #region Properties

        /// <summary>
        /// Gets the invocation expression.
        /// </summary>
        public InvokeMethodExpression InvokeMethodExpression { get; }

        /// <summary>
        /// Gets the list of accessible variables.
        /// </summary>
        public List<Variable> Variables { get; set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Call"/> class.
        /// </summary>
        /// <param name="invokeMethodExpression">The reference to the called method.</param>
        public Call(InvokeMethodExpression invokeMethodExpression)
        {
            InvokeMethodExpression = invokeMethodExpression;
            Variables = new List<Variable>();
        }

        #endregion
    }
}
