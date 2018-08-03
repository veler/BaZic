using BaZic.Runtime.Localization;
using System;
using System.Linq;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents a reference to a class in an algorithm
    /// </summary>
    [Serializable]
    public sealed class ClassReferenceExpression : ReferenceExpression
    {
        #region Properties

        /// <summary>
        /// Gets the full namespace path that contains the class
        /// </summary>
        public string Namespace { get; }

        /// <summary>
        /// Gets the name of the class
        /// </summary>
        public MemberIdentifier ClassName { get; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initliaze a new instance of the <see cref="ClassReferenceExpression"/> class.
        /// </summary>
        public ClassReferenceExpression()
        {
        }

        /// <summary>
        /// Initliaze a new instance of the <see cref="ClassReferenceExpression"/> class.
        /// </summary>
        /// <param name="@namespace">The full namespace path that contains the class</param>
        /// <param name="className">The name of the class</param>
        public ClassReferenceExpression(string @namespace, string className)
        {
            if (!IsValidNamespace(@namespace))
            {
                throw new ArgumentException(L.BaZic.AbstractSyntaxTree.InvalidNamespace, nameof(@namespace));
            }

            Namespace = @namespace;
            ClassName = new MemberIdentifier(className);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Verify if a namespace is valid (not null, no space, doesn't start with number and doesn't contains special character except the dot).
        /// </summary>
        /// <param name="@namespace">The namespace to check</param>
        /// <returns>Returns false if the namespace is invalid.</returns>
        private bool IsValidNamespace(string @namespace)
        {
            return !String.IsNullOrWhiteSpace(@namespace) && char.IsLetter(@namespace[0]) && @namespace.All(ch => char.IsLetterOrDigit(ch) || ch == '.');
        }

        /// <summary>
        /// Gets a string representation of the object 
        /// </summary>
        /// <returns>Returns the full path to the class (namespace + class name)</returns>
        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Namespace))
            {
                return ClassName.ToString();
            }
            return $"{Namespace}.{ClassName}";
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new ClassReferenceExpression(Namespace, ClassName.Identifier)
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
