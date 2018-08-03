using BaZic.Runtime.Localization;
using BaZicProgramReleaseMode;
using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace BaZic.Runtime.BaZic.Runtime.Debugger
{
    /// <summary>
    /// Represents info about a type of data.
    /// </summary>
    [Serializable]
    public sealed class ValueInfo
    {
        #region Properties

        /// <summary>
        /// Gets whether the data is an <see cref="ICollection{T}"/>, <see cref="IEnumerable{T}"/> or <see cref="IList{T}"/>
        /// </summary>
        internal bool IsArray { get; private set; }

        /// <summary>
        /// Gets whether the data is a primitive value (<see cref="ValueType"/>, <see cref="string"/>, <see cref="Array"/>).
        /// </summary>
        internal bool IsPrimitive { get; private set; }

        /// <summary>
        /// Gets whether the data is null.
        /// </summary>
        internal bool IsNull { get; private set; }

        /// <summary>
        /// Gets the type of the data.
        /// </summary>
        internal Type Type { get; private set; }

        /// <summary>
        /// Gets, in the case of a string or array, the length of the data.
        /// </summary>
        internal int Length { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Generates a new <see cref="ValueInfo"/> object that provides informations about the passed value.
        /// </summary>
        /// <param name="value">The value to analyze.</param>
        /// <returns>A <see cref="ValueInfo"/> that contains the information about the value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ValueInfo GetValueInfo(object value)
        {
            var isArray = false;
            var isPrimitive = false;
            var isNull = false;
            Type type = null;
            var length = 0;

            if (value == null)
            {
                isNull = true;
            }
            else
            {
                type = value.GetType();

                if (type.IsValueType)
                {
                    isPrimitive = true;
                }
                else if (type == typeof(string))
                {
                    isPrimitive = true;
                    length = ((string)value).Length;
                }
                else if (type == typeof(ObservableDictionary))
                {
                    isArray = true;
                    length = ((ObservableDictionary)value).Count;
                }
                else if (type.IsArray || type == typeof(Array))
                {
                    var elementType = type.GetElementType();
                    if (elementType.IsValueType || elementType == typeof(string))
                    {
                        isPrimitive = true;
                    }

                    isArray = true;
                    length = ((Array)value).Length;
                }
                else if (typeof(ICollection).IsAssignableFrom(type))
                {
                    isArray = true;
                    length = ((ICollection)value).Count;
                }
            }

            return new ValueInfo()
            {
                IsArray = isArray,
                IsPrimitive = isPrimitive,
                IsNull = isNull,
                Type = type,
                Length = length
            };
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (IsNull)
            {
                return L.BaZic.Runtime.Debugger.ValueInfo.Null;
            }

            if (IsArray)
            {
                return L.BaZic.Runtime.Debugger.ValueInfo.FormattedArrayInfo(Type.FullName, Length);
            }

            return Type.FullName;
        }

        #endregion
    }
}
