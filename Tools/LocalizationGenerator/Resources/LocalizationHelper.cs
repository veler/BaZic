    public static partial class LocalizationHelper
    {
        #region Fields & Constants

        private static string defaultLanguage = "en";
        private static ReadOnlyCollection<CultureInfo> availableCultures;
        private static Dictionary<string, XmlDocument> loadedResources = new Dictionary<string, XmlDocument>();
        private static bool isDesignMode = System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime;

        #endregion

        #region Events

        public static event PropertyChangedEventHandler StaticPropertyChanged;

        #endregion

        #region Constructors

        static LocalizationHelper()
        {
            var defaultCulture = GetAvailableCultures().Single(c => c.Name == defaultLanguage);
            SetCurrentCulture(defaultCulture);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a string from the international resources.
        /// </summary>
        /// <param name="resourceFilePath">The assembly name and namespace to access to the resource file.</param>
        /// <param name="xpath">The path to the value in the resource file.</param>
        internal static string GetString(string resourceFilePath, string xpath)
        {
            return GetString(resourceFilePath, xpath, GetCurrentCulture().Name.ToLowerInvariant());
        }

        /// <summary>
        /// Gets a string from the international resources.
        /// </summary>
        /// <param name="resourceFilePath">The assembly name and namespace to access to the resource file.</param>
        /// <param name="xpath">The path to the value in the resource file.</param>
        /// <param name="cultureName">The name of the culture</param>
        private static string GetString(string resourceFilePath, string xpath, string cultureName)
        {
            var assemblyResourceFilePath = $"{resourceFilePath}_{cultureName}.xml";
            XmlDocument resourceDocument;
            loadedResources.TryGetValue(assemblyResourceFilePath, out resourceDocument);

            if (resourceDocument == null)
            {
                var currentAssembly = Assembly.GetExecutingAssembly();
                resourceDocument = new XmlDocument();

                if (!isDesignMode)
                {
                    resourceDocument.Load(new StreamReader(currentAssembly.GetManifestResourceStream($"{currentAssembly.GetName().Name}.{assemblyResourceFilePath}")));
                }
                else
                {
                    return xpath;
                }

                loadedResources.Add(assemblyResourceFilePath, resourceDocument);
            }

            var node = resourceDocument.SelectSingleNode(xpath);

            if (node != null)
            {
                return node.InnerText;
            }

            if (cultureName == defaultLanguage)
            {
                return $"[NO TEXT FOUND IN THE DEFAULT LANGUAGE]";
            }

            return GetString(resourceFilePath, xpath, defaultLanguage);
        }

        /// <summary>
        /// Retrieves the current culture.
        /// </summary>
        internal static CultureInfo GetCurrentCulture()
        {
            return Thread.CurrentThread.CurrentCulture;
        }

        /// <summary>
        /// Change the current culture of the application. Restarting the application is probably required.
        /// </summary>
        /// <param name="culture">The culture to set.</param>
        /// <param name="refreshUi">Defines whether the UI must be refresh. Put this parameter to false if the change should not affect the current UI. Keeping this parameter to True may affect performances.</param>
        public static void SetCurrentCulture(CultureInfo culture, bool refreshUi = true)
        {
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            loadedResources.Clear();

            if (refreshUi)
            {
                RaisePropertyChanged();
                [RaiseAllPropertyChanged]
            }
        }

        /// <summary>
        /// Retrieves the list of available culture for this application
        /// </summary>
        /// <returns>A list of available <see cref="CultureInfo"/></returns>
        internal static ReadOnlyCollection<CultureInfo> GetAvailableCultures()
        {
            if (availableCultures != null)
            {
                return availableCultures;
            }

            var result = new List<CultureInfo>();
            var builtInSupportedCultures = [Namespace].[Class].AvailableCultureIds;
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

            foreach (var culture in cultures)
            {
                if (culture.Equals(CultureInfo.InvariantCulture))
                {
                    continue;
                }

                if (builtInSupportedCultures.Any(c => c.ToLowerInvariant() == culture.Name))
                {
                    result.Add(culture);
                }
            }

            availableCultures = result.AsReadOnly();
            return availableCultures;
        }

        public static void RaisePropertyChanged()
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(string.Empty));
        }

        #endregion
    }
