using BaZic.Core.ComponentModel;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Runtime.Debugger;
using BaZic.Runtime.BaZic.Runtime.Debugger.Exceptions;
using BaZic.Runtime.Localization;
using System;
using System.Runtime.CompilerServices;

namespace BaZic.Runtime.BaZic.Runtime.Memory
{
    /// <summary>
    /// Represents a variable at runtime.
    /// </summary>
    [Serializable]
    public class Variable : Core.ComponentModel.IDisposable
    {
        #region Properties

        /// <summary>
        /// Gets the unique ID of the variable, used to make a reference to it in the interpreter.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets whether the variable is of type <see cref="object"/> or <see cref="Collection{T}"/> of <see cref="object"/>.
        /// </summary>
        public bool IsArray { get; }

        /// <summary>
        /// Gets or sets the value of the variable.
        /// </summary>
        public virtual object Value { get; protected set; }

        /// <summary>
        /// Gets the information about the value of the variable.
        /// </summary>
        public ValueInfo Info { get; private set; }

        /// <inheritdoc/>
        public bool IsDisposed { get; private set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Variable"/> class.
        /// </summary>
        /// <param name="variableDeclaration">The <see cref="VariableDeclaration"/> used to create the new variable in memory at runtime.</param>
        public Variable(VariableDeclaration variableDeclaration)
        {
            Requires.NotNull(variableDeclaration, nameof(variableDeclaration));
            Id = variableDeclaration.Id;
            IsArray = variableDeclaration.IsArray;
            Name = variableDeclaration.Name.Identifier;
        }

        /// <summary>
        /// Finalizes the instance of the class.
        /// </summary>
        ~Variable()
        {
            OnDispose(false);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the value of the variable.
        /// </summary>
        /// <param name="value">The value to give.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetValue(object value)
        {
            SetValue(value, ValueInfo.GetValueInfo(value));
        }

        /// <summary>
        /// Sets the value of the variable.
        /// </summary>
        /// <param name="value">The value to give.</param>
        /// <param name="info">The info related to the value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetValue(object value, ValueInfo info)
        {
            Requires.NotNull(info, nameof(info));

            lock (this)
            {
                Info = info;

                if (!Info.IsNull && IsArray != Info.IsArray)
                {
                    throw new NotAssignableException(L.BaZic.Runtime.Memory.VariableNoArray);
                }

                Value = value;
            }
        }

        public override string ToString()
        {
            if (Value == null)
            {
                return L.BaZic.Runtime.Debugger.ValueInfo.Null;
            }

            return $"{Value} ({Info})";
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            OnDispose(true);
        }

        /// <summary>
        /// Should be called when the object is being disposed.
        /// </summary>
        /// <param name="disposing">Was Dispose() called or did we get here from the finalizer?</param>
        private void OnDispose(bool disposing)
        {
            if (disposing)
            {
                if (!IsDisposed)
                {
                    Value = null; // Once this is set to null and that that the destructor of this class is called, the garbage collector will automatically Dispose the resource if it is disposable.
                    Info = null;
                }
            }

            IsDisposed = true;
        }

        #endregion
    }
}
