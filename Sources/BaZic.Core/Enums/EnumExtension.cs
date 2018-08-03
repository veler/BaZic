using System.ComponentModel;
using System.Linq;

namespace BaZic.Core.Enums
{
    /// <summary>
    /// Provides a set of extension for enumerations
    /// </summary>
    internal static class EnumExtension
    {
        #region Methods

        /// <summary>
        /// Retrieves the <see cref="DescriptionAttribute"/>'s value.
        /// </summary>
        /// <typeparam name="T">The targeted enumeration</typeparam>
        /// <param name="enumerationValue">The value</param>
        /// <returns>A string that corresponds to the description of the enumeration.</returns>
        internal static string GetDescription<T>(this T enumerationValue) where T : struct
        {
            var memberInfo = enumerationValue.GetType().GetMember(enumerationValue.ToString());
            if (memberInfo != null && memberInfo.Length > 0)
            {
                var attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false).ToArray();

                if (attrs != null && attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            return enumerationValue.ToString();
        }

        #endregion
    }
}
