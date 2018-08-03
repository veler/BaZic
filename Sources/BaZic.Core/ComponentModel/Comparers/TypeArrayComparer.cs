using System;
using System.Collections.Generic;
using System.Reflection;

namespace BaZic.Core.ComponentModel.Comparers
{
    /// <summary>
    /// Defines methods to support the comparison of Type array for equality.
    /// </summary>
    internal sealed class TypeArrayComparer : IEqualityComparer<Type[]>
    {
        /// <inheritdoc/>
        public bool Equals(Type[] x, Type[] y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }

            for (var i = 0; i < x.Length; i++)
            {
                var typeLeft = x[i];
                var typeRight = y[i];

                if (!new TypeDelegator(typeLeft).Equals(typeRight))
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public int GetHashCode(Type[] obj)
        {
            var result = 17;
            for (var i = 0; i < obj.Length; i++)
            {
                unchecked
                {
                    result = result * 23 + obj[i].FullName.GetHashCode();
                }
            }
            return result;
        }
    }
}
