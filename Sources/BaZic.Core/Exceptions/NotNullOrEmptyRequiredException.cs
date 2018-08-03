using System;

namespace BaZic.Core.Exceptions
{
    internal sealed class NotNullOrEmptyRequiredException : ArgumentException
    {
        internal NotNullOrEmptyRequiredException(string parameterName)
            : base(parameterName, "The value must not be null or empty")
        {
        }
    }
}
