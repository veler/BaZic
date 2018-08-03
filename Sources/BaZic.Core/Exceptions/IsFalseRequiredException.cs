using System;

namespace BaZic.Core.Exceptions
{
    internal sealed class IsFalseRequiredException : Exception
    {
        internal IsFalseRequiredException()
            : base("The value must be false")
        {
        }
    }
}
