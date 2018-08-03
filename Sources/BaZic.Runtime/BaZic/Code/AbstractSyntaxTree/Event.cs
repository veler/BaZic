using System;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents binding between a control event from the user interface and a method declaration in a BaZic program.
    /// </summary>
    [Serializable]
    public sealed class Event : NodeObject
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name of the control in the user interface.
        /// </summary>
        public string ControlName { get; set; }

        /// <summary>
        /// Gets or sets the name of the event of the control.
        /// </summary>
        public string ControlEventName { get; set; }

        /// <summary>
        /// Gets or sets the id to the method that must be invoked when the control event is fired.
        /// </summary>
        public Guid MethodId { get; set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Event"/> class.
        /// </summary>
        public Event()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Event"/> class.
        /// </summary>
        /// <param name="controlName">The name of the control in the user interface.</param>
        /// <param name="controlEventName">The name of the event of the control.</param>
        /// <param name="methodId">The id to the method that must be invoked when the control event is fired.</param>
        public Event(string controlName, string controlEventName, Guid methodId)
        {
            ControlName = controlName?.Trim();
            ControlEventName = controlEventName;
            MethodId = methodId;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new Event(ControlName, ControlEventName, MethodId)
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
