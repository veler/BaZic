using System;

namespace BaZic.Core.Exceptions
{
    internal sealed class NotNullOrWhiteSpaceRequiredException : ArgumentException
    {
        internal NotNullOrWhiteSpaceRequiredException(string parameterName)
            : base(parameterName, "The value must not be null or white space")
        {
        }
    }
}
