using System.ComponentModel;

namespace BaZic.Runtime.BaZic.Code.Parser
{
    /// <summary>
    /// Defines explicitely if a right parenthesis or right bracket is expected after a left one.
    /// </summary>
    internal enum ExpressionGroupSeparator
    {
        [Description("[")]
        RightBracket,

        [Description("(")]
        RightParenth
    }
}
