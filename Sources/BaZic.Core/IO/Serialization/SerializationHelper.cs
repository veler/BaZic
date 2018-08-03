using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace BaZic.Core.IO.Serialization
{
    /// <summary>
    /// Provides a set of methods to serialize and deserialize a data.
    /// </summary>
    internal static class SerializationHelper
    {
        /// <summary>
        /// Serialize the given object to a byte array representation.
        /// </summary>
        /// <param name="source">The object to serialize</param>
        /// <returns>Returns a byte array representation of the given value.</returns>
        internal static byte[] ConvertToBinary(object source)
        {
            if (source == null)
            {
                return null;
            }

            if (!source.GetType().IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", nameof(source));
            }

            var formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Deserialize the given byte array representation to an object.
        /// </summary>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <param name="xml">The byte array representation of an object.</param>
        /// <returns>An object resulting from the deserialization of a byte array.</returns>
        internal static T ConvertFromBinary<T>(byte[] binary) where T : class, new()
        {
            if (binary == null)
            {
                return default(T);
            }

            var formatter = new BinaryFormatter();
            var stream = new MemoryStream(binary);
            using (stream)
            {
                stream.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(stream) as T;
            }
        }
    }
}
