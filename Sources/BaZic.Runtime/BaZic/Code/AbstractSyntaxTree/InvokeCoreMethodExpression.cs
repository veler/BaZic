using System;
using System.Linq;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents a call to method from the CLR (i.e : System.IO.File.ReadAllText). If the goal is to call a method represented by the syntax tree, please use <see cref="InvokeMethodExpression"/>
    /// </summary>
    [Serializable]
    public sealed class InvokeCoreMethodExpression : InvokeMethodExpression
    {
        #region Properties

        /// <summary>
        /// Gets or sets the class reference that contains the method
        /// </summary>
        public ReferenceExpression TargetObject { get; set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InvokeCoreMethodExpression"/> class.
        /// </summary>
        public InvokeCoreMethodExpression()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvokeCoreMethodExpression"/> class.
        /// </summary>
        /// <param name="targetObject">A reference to a variable or to a class</param>
        /// <param name="methodName">The method name to call</param>
        /// <param name="await">Defines whether a call to a asynchronous method should be done synchronously or not</param>
        public InvokeCoreMethodExpression(ReferenceExpression targetObject, string methodName, bool await)
            : base(methodName, await)
        {
            TargetObject = targetObject;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a string representation of the reference
        /// </summary>
        /// <returns>String that reprensents the reference</returns>
        public override string ToString()
        {
            return $"{TargetObject}.{MethodName}()";
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new InvokeCoreMethodExpression(TargetObject, MethodName.Identifier, Await)
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
