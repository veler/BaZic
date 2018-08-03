using System;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents a reference to a variable in an algorithm
    /// </summary>
    [Serializable]
    public sealed class PropertyReferenceExpression : ReferenceExpression, IAssignable
    {
        #region Properties

        /// <summary>
        /// Gets or sets the class reference or variable that contains the property
        /// </summary>
        public ReferenceExpression TargetObject { get; set; }

        /// <summary>
        /// Gets the name of the variable
        /// </summary>
        public MemberIdentifier PropertyName { get; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyReferenceExpression"/> class.
        /// </summary>
        public PropertyReferenceExpression()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyReferenceExpression"/> class.
        /// </summary>
        /// <param name="targetObject">The class reference or variable that contains the property</param>
        /// <param name="name">The name of the variable we make reference</param>
        public PropertyReferenceExpression(ReferenceExpression targetObject, string name)
        {
            TargetObject = targetObject;
            PropertyName = new MemberIdentifier(name);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a string representation of the reference
        /// </summary>
        /// <returns>String that reprensents the reference</returns>
        public override string ToString()
        {
            return $"{TargetObject}.{PropertyName}";
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new PropertyReferenceExpression(TargetObject, PropertyName.Identifier)
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
