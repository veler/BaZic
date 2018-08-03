using LocalizationGenerator.Models;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace LocalizationGenerator
{
    internal sealed class XmlLocalizationAnalyzer
    {
        #region Fields & Constants

        private string _currentFileNameNoExtension;

        #endregion

        #region Properties

        internal List<string> AvailableLanguages { get; }

        internal List<LangString> LangStrings { get; }

        #endregion

        #region Constructors & Destructors

        internal XmlLocalizationAnalyzer()
        {
            AvailableLanguages = new List<string>();
            LangStrings = new List<LangString>();
        }

        #endregion

        #region Methods

        internal void Load(string filePath)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(filePath);

            var fileNamePart = filePath.Split('\\').Last().Split('.');
            var languageId = fileNamePart.First().Split('_').Last().ToLowerInvariant();
            _currentFileNameNoExtension = fileNamePart.First().Split('_').First();
            if (!AvailableLanguages.Contains(languageId))
            {
                AvailableLanguages.Add(languageId);
            }

            var rootNode = xmlDocument.SelectSingleNode("Language");
            foreach (XmlNode childNode in rootNode.ChildNodes)
            {
                ReadXmlNode(childNode, languageId, "Language", LangStrings);
            }
        }

        private void ReadXmlNode(XmlNode node, string languageId, string xPath, List<LangString> parentLangStringStrings)
        {
            xPath += $"/{node.Name}";
            if (node.ChildNodes.Count == 1 && node.FirstChild is XmlText text)
            {
                var langString = GetLangString(xPath, parentLangStringStrings);
                AddString(langString, languageId, node.InnerText);
            }
            else
            {
                var langString = GetLangString(xPath, parentLangStringStrings);
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    ReadXmlNode(childNode, languageId, xPath, langString.LangStrings);
                }
            }
        }

        private LangString GetLangString(string xpath, List<LangString> parentLangStringStrings)
        {
            xpath = xpath.Trim('/');
            var langString = parentLangStringStrings.SingleOrDefault(s => s.XPath == xpath);

            if (langString == null)
            {
                langString = new LangString() { XPath = xpath };
                parentLangStringStrings.Add(langString);
            }

            return langString;
        }

        private void AddString(LangString langString, string languageId, string value)
        {
            if (langString.Strings.Any(s => s.LanguageId == languageId))
            {
                throw new System.Exception($"There is already a value for the language '{languageId}' in '{langString.XPath}'.");
            }

            langString.Strings.Add(new SingleString()
            {
                LanguageId = languageId,
                Value = value,
                FileNameNoExtension = _currentFileNameNoExtension
            });
        }

        #endregion
    }
}
