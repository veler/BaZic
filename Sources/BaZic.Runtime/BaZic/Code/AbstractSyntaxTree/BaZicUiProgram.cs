using System;
using System.Collections.Generic;
using System.Linq;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents the root of a BaZic program with a unique user interface
    /// </summary>
    [Serializable]
    public sealed class BaZicUiProgram : BaZicProgram
    {
        #region Properties

        /// <summary>
        /// Gets or sets the XAML code that represents the user interface.
        /// </summary>
        public string Xaml { get; set; }

        /// <summary>
        /// Gets the list of controls accessors in the UI.
        /// </summary>
        public IReadOnlyList<ControlAccessorDeclaration> UiControlAccessors { get; private set; }

        /// <summary>
        /// Gets the list of binding to a UI component's behavior.
        /// </summary>
        public IReadOnlyList<Event> UiEvents { get; private set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BaZicUiProgram"/> class.
        /// </summary>
        public BaZicUiProgram()
            : base()
        {
            UiControlAccessors = new List<ControlAccessorDeclaration>().AsReadOnly();
            UiEvents = new List<Event>().AsReadOnly();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaZicUiProgram"/> class.
        /// </summary>
        /// <param name="isOptimized">Defines whether the program has been optimized.</param>
        public BaZicUiProgram(bool isOptimized)
            : base(isOptimized)
        {
            UiControlAccessors = new List<ControlAccessorDeclaration>().AsReadOnly();
            UiEvents = new List<Event>().AsReadOnly();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set program bindings.
        /// </summary>
        /// <param name="bindings">The bindings</param>
        /// <returns>The current program</returns>
        public BaZicUiProgram WithControlAccessors(params ControlAccessorDeclaration[] bindings)
        {
            UiControlAccessors = new List<ControlAccessorDeclaration>(bindings).AsReadOnly();
            return this;
        }

        /// <summary>
        /// Set program events.
        /// </summary>
        /// <param name="events">The events</param>
        /// <returns>The current program</returns>
        public BaZicUiProgram WithUiEvents(params Event[] events)
        {
            UiEvents = new List<Event>(events).AsReadOnly();
            return this;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new BaZicUiProgram(IsOptimized)
            {
                Column = Column,
                Id = Id,
                Line = Line,
                Xaml = Xaml,
                StartOffset = StartOffset,
                NodeLength = NodeLength
            }
            .WithControlAccessors(UiControlAccessors.ToArray())
            .WithUiEvents(UiEvents.ToArray())
            .WithMethods(Methods.ToArray())
            .WithVariables(GlobalVariables.ToArray())
            .WithAssemblies(Assemblies.ToArray());
        }

        #endregion
    }
}
