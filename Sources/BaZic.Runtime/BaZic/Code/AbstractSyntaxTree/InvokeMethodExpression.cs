using System;
using System.Collections.Generic;
using System.Linq;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents a call to a method in an algorithm. If the goal is to call a method from the CLR, please use <see cref="InvokeCoreMethodExpression"/>
    /// </summary>
    [Serializable]
    public class InvokeMethodExpression : ReferenceExpression
    {
        #region Properties

        /// <summary>
        /// Gets the name of the method to call
        /// </summary>
        public MemberIdentifier MethodName { get; }

        /// <summary>
        /// Gets an array of arguments to pass to the call
        /// </summary>
        public IReadOnlyList<Expression> Arguments { get; private set; }

        /// <summary>
        /// Gets or sets whether a call to a asynchronous method should be done synchronously or not
        /// </summary>
        public bool Await { get; set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InvokeMethodExpression"/> class.
        /// </summary>
        public InvokeMethodExpression()
        {
            Arguments = new List<Expression>().AsReadOnly();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvokeMethodExpression"/> class.
        /// </summary>
        /// <param name="methodName">The method name to call</param>
        /// <param name="await">Defines whether a call to a asynchronous method should be done synchronously or not</param>
        public InvokeMethodExpression(string methodName, bool await)
            : this()
        {
            MethodName = new MemberIdentifier(methodName);
            Await = await;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set call's arguments.
        /// </summary>
        /// <param name="parameters">The arguments</param>
        /// <returns>The current expression</returns>
        public InvokeMethodExpression WithParameters(params Expression[] parameters)
        {
            Arguments = new List<Expression>(parameters).AsReadOnly();
            return this;
        }

        /// <summary>
        /// Gets a string representation of the reference
        /// </summary>
        /// <returns>String that reprensents the reference</returns>
        public override string ToString()
        {
            return $"{MethodName.Identifier}()";
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new InvokeMethodExpression(MethodName.Identifier, Await)
            {
                Column = Column,
                Id = Id,
                Line = Line,
                StartOffset = StartOffset,
                NodeLength = NodeLength
            }
            .WithParameters(Arguments.ToArray());
        }

        #endregion
    }
}
