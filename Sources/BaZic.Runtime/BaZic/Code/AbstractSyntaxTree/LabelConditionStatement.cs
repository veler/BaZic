using System;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents a special condition that go to a specific label if the condition is not validated. This must be use only in a optimization context.
    /// </summary>
    [Serializable]
    public sealed class LabelConditionStatement : Statement
    {
        #region Properties

        /// <summary>
        /// Gets the condition that must be tested.
        /// </summary>
        public NotOperatorExpression Condition { get; }

        /// <summary>
        /// Gets the name of the label to go when the condition is not validated.
        /// </summary>
        public MemberIdentifier LabelName { get; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of then <see cref="LabelConditionStatement"/> class.
        /// </summary>
        public LabelConditionStatement()
        {

        }

        /// <summary>
        /// Initializes a new instance of then <see cref="LabelConditionStatement"/> class.
        /// </summary>
        /// <param name="condition">The condition</param>
        /// <param name="labelName">The name of the label to go when the condition is not validated.</param>
        public LabelConditionStatement(Expression condition, string labelName)
        {
            Condition = new NotOperatorExpression(condition);
            LabelName = new MemberIdentifier(labelName);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new LabelConditionStatement(Condition, LabelName.Identifier)
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
