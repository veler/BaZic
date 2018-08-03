using System;

namespace BaZic.Core.Exceptions
{
    internal sealed class NotNullRequiredException : ArgumentNullException
    {
        internal NotNullRequiredException(string parameterName)
            : base(parameterName, "The value must not be null")
        {
        }
    }
}
