using System;
using System.Collections.Generic;
using System.Linq;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents an object creation, typically represtented as a new expression. If the goal is to create an object from the CLR, please use <see cref="InstantiateExpression"/>
    /// </summary>
    [Serializable]
    public sealed class InstantiateExpression : ReferenceExpression
    {
        #region Properties

        /// <summary>
        /// Gets or sets a reference to the class to instantiate
        /// </summary>
        public ClassReferenceExpression CreateType { get; set; }

        /// <summary>
        /// Gets the arguments to pass in the class's constructor
        /// </summary>
        public IReadOnlyList<Expression> Arguments { get; private set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of <see cref="InstantiateExpression"/>
        /// </summary>
        public InstantiateExpression()
        {
            Arguments = new List<Expression>().AsReadOnly();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="InstantiateExpression"/>
        /// </summary>
        /// <param name="createType">Reference to the class to instantiate</param>
        public InstantiateExpression(ClassReferenceExpression createType)
            : this()
        {
            CreateType = createType;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set instanciation's arguments.
        /// </summary>
        /// <param name="parameters">The arguments</param>
        /// <returns>The current instanciation</returns>
        public InstantiateExpression WithParameters(params Expression[] parameters)
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
            return $"new {CreateType}()";
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new InstantiateExpression(CreateType)
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
