using System;
using System.Collections.Generic;

namespace BaZic.Core.ComponentModel.Comparers
{
    /// <summary>
    /// Defines methods compare two type by checking if one is assignable to the other.
    /// </summary>
    internal sealed class TypeAssignableComparer : IEqualityComparer<Type>
    {
        /// <inheritdoc/>
        public bool Equals(Type x, Type y)
        {
            return x.IsAssignableFrom(y);
        }

        /// <inheritdoc/>
        public int GetHashCode(Type obj)
        {
            return obj.FullName.GetHashCode();
        }
    }
}
