using System.ComponentModel;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Defines identifiers for supported binary operators
    /// </summary>
    public enum BinaryOperatorType
    {
        /// <summary>
        /// Identity equal operator
        /// </summary>
        [Description("==")]
        Equality = 0,
        /// <summary>
        /// Bitwise or operator
        /// </summary>
        [Description("|")]
        BitwiseOr = 1,
        /// <summary>
        /// Bitwise and operator
        /// </summary>
        [Description("&")]
        BitwiseAnd = 2,
        /// <summary>
        /// Boolean or operator. This represents a short circuiting operator. A short circuiting
        /// operator will evaluate only as many expressions as necessary before returning
        /// a correct value.
        /// </summary>
        [Description("||")]
        LogicalOr = 3,
        /// <summary>
        /// Boolean and operator. This represents a short circuiting operator. A short circuiting
        /// operator will evaluate only as many expressions as necessary before returning
        /// a correct value.
        /// </summary>
        [Description("&&")]
        LogicalAnd = 4,
        /// <summary>
        /// Less than operator
        /// </summary>
        [Description("<")]
        LessThan = 5,
        /// <summary>
        /// Less than or equal operator
        /// </summary>
        [Description("<=")]
        LessThanOrEqual = 6,
        /// <summary>
        /// Greater than operator
        /// </summary>
        [Description(">")]
        GreaterThan = 7,
        /// <summary>
        /// Greater than or equal operator
        /// </summary>
        [Description(">=")]
        GreaterThanOrEqual = 8,
        /// <summary>
        /// Addition operator
        /// </summary>
        [Description("+")]
        Addition = 9,
        /// <summary>
        /// Subtraction operator
        /// </summary>
        [Description("-")]
        Subtraction = 10,
        /// <summary>
        /// Multiplication operator
        /// </summary>
        [Description("*")]
        Multiply = 11,
        /// <summary>
        /// Division operator
        /// </summary>
        [Description("/")]
        Division = 12,
        /// <summary>
        /// Modulus operator
        /// </summary>
        [Description("%")]
        Modulus = 13
    }
}
