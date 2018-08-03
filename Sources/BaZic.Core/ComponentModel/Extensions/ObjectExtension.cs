namespace BaZic.Core.ComponentModel.Extensions
{
    /// <summary>
    /// Provides a set of extensions for <see cref="object"/>.
    /// </summary>
    public static class ObjectExtension
    {
        #region Methods

        /// <summary>
        /// Check whether an object is exactly the specified class, not a derived one.
        /// </summary>
        /// <typeparam name="T">The expected type.</typeparam>
        /// <param name="obj">The object to check</param>
        /// <returns>Returns true if the object is exactly of the specified type.</returns>
        public static bool IsExactly<T>(this object obj) where T : class
        {
            return obj != null && obj.GetType() == typeof(T);
        }

        #endregion
    }
}
