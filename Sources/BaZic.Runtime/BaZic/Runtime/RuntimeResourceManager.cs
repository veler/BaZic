using BaZic.Core.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BaZic.Runtime.BaZic.Runtime
{
    /// <summary>
    /// Provides a static helper designed to bind imported resources in a BaZic program with UI, like images.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class RuntimeResourceManager
    {
        #region Properties

        /// <summary>
        /// Gets the resources at runtime.
        /// </summary>
        public static Dictionary<string, Freezable> Resources { get; private set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeResourceManager"/> class.
        /// </summary>
        static RuntimeResourceManager()
        {
            Resources = new Dictionary<string, Freezable>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a file to the resources.
        /// </summary>
        /// <param name="interpreterInstanceIdentifier">The identifier of the BaZic interpreter instance related to this resource.</param>
        /// <param name="filePath">A file path of type JPG, JPEG, PNG or GIF.</param>
        /// <returns>Returns the generated resource name for the XAML code which is composed of <c>"{interpreterInstanceIdentifier}_{fileNameInLowerCase}"</c>.</returns>
        internal static string AddOrReplaceResource(string interpreterInstanceIdentifier, string filePath)
        {
            var resourceName = Path.GetFileName(filePath).ToLowerInvariant();

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Resource file not found for the BaZic's resource manager.", filePath);
            }

            if (Consts.ImageResourcesType.Any(ext => resourceName.EndsWith(ext)))
            {
                if (!string.IsNullOrWhiteSpace(interpreterInstanceIdentifier) && !resourceName.StartsWith(interpreterInstanceIdentifier))
                {
                    resourceName = interpreterInstanceIdentifier + resourceName;
                }

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.UriSource = new Uri(filePath, UriKind.Absolute);
                bitmapImage.EndInit();
                Resources[resourceName] = bitmapImage;

                return resourceName;
            }
            else
            {
                throw new NotSupportedException($"A resource of type '{Path.GetExtension(filePath)}' is not supported in a BaZic program.");
            }
        }

        /// <summary>
        /// Deletes all the resources for the specified interpreter instance identifier.
        /// </summary>
        /// <param name="interpreterInstanceIdentifier">The identifier of the interpreter instance related to this resource.</param>
        internal static void DeleteResources(string interpreterInstanceIdentifier)
        {
            if (string.IsNullOrWhiteSpace(interpreterInstanceIdentifier))
            {
                Resources.Clear();
            }
            else
            {
                var resourceKeys = Resources.Keys.Where(key => key.StartsWith(interpreterInstanceIdentifier)).ToList();
                foreach (var key in resourceKeys)
                {
                    Resources.Remove(key);
                }
            }
        }

        #endregion
    }
}
