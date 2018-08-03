using System;

namespace BaZic.Core.Exceptions
{
    internal sealed class IsTrueRequiredException : Exception
    {
        internal IsTrueRequiredException()
            : base("The value must be true")
        {
        }
    }
}
