using BaZic.Core.ComponentModel;
using System;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents the entry point method of a program. It is a method called 'Main' with a single argument of type Array.
    /// </summary>
    [Serializable]
    public sealed class EntryPointMethod : MethodDeclaration
    {
        #region Properties

        /// <summary>
        /// Gets if the method is asynchronous or not.
        /// </summary>
        public new static bool IsAsync { get; private set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntryPointMethod"/> class.
        /// </summary>
        public EntryPointMethod()
            : base(Consts.EntryPointMethodName, false)
        {
            WithParameters(new ParameterDeclaration("args", true));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new EntryPointMethod()
            {
                Column = Column,
                Id = Id,
                Line = Line,
                StartOffset = StartOffset,
                NodeLength = NodeLength
            };
        }

        #endregion
    }
}
