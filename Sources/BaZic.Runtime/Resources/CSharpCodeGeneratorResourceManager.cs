// Helper for CSharp generated code.

namespace BaZicProgramReleaseMode
{

    /// <summary>
    /// Provides a static helper designed to bind imported resources in a BaZic program with UI, like images.
    /// </summary>
    public static class ProgramResourceManager
    {
        #region Fields & Constants

        private static readonly string[] ImageResourcesType = new string[] { ".jpg", ".jpeg", ".png", ".gif" };

        #endregion

        #region Properties

        /// <summary>
        /// Gets the resources at runtime.
        /// </summary>
        public static System.Collections.Generic.Dictionary<string, System.Windows.Freezable> Resources { get; private set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramResourceManager"/> class.
        /// </summary>
        static ProgramResourceManager()
        {
            Resources = new System.Collections.Generic.Dictionary<string, System.Windows.Freezable>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads all the resources.
        /// </summary>
        internal static void LoadResources()
        {
            var currentAssembly = typeof(ProgramResourceManager).Assembly;
            var resourceNames = currentAssembly.GetManifestResourceNames();

            foreach (var resourceName in resourceNames)
            {
                if (System.Linq.Enumerable.Any(ImageResourcesType, ext => resourceName.EndsWith(ext)))
                {
                    var bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
                    using (var stream = currentAssembly.GetManifestResourceStream(resourceName))
                    {
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = stream;
                        bitmapImage.EndInit();
                    }
                    Resources[resourceName] = bitmapImage;
                }
                else
                {
                    throw new System.NotSupportedException($"A resource of type '{resourceName}' is not supported in a BaZic program.");
                }
            }
        }

        #endregion
    }
}
