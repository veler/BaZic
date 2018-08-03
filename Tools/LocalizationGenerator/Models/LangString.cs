using System.Collections.Generic;

namespace LocalizationGenerator.Models
{
    internal sealed class LangString
    {
        internal string XPath { get; set; }

        internal List<LangString> LangStrings { get; set; }

        internal List<SingleString> Strings { get; set; }

        internal LangString()
        {
            Strings = new List<SingleString>();
            LangStrings = new List<LangString>();
        }
    }
}
