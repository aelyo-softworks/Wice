using System;
using System.Collections.Generic;
using System.Linq;
using ColorCode;
using ColorCode.Common;
using ColorCode.Compilation.Languages;
using DirectN;

namespace Wice.Samples.Gallery.Utilities
{
    // so we can add our own colors for Wice's classes
    public class WiceLanguage : ILanguage
    {
        private static readonly Lazy<WiceLanguage> _default = new Lazy<WiceLanguage>(() =>
        {
            var lang = new WiceLanguage();
            Languages.Load(lang);
            return lang;
        }, true);
        public static WiceLanguage Default => _default.Value;

        private readonly List<LanguageRule> _rules = new List<LanguageRule>();

        public WiceLanguage()
        {
            // re-use c# rules
            _rules.AddRange(new CSharp().Rules);

            var types = typeof(Visual).Assembly.GetTypes().Where(t => t.IsPublic && t.GenericTypeArguments.Length == 0).Select(t => t.Name).ToList();
            types.Add(nameof(_D3DCOLORVALUE));

            // hexa
            _rules.Add(new LanguageRule(@"\b0x[0-9A-F]{1,}\b", new Dictionary<int, string> { { 0, ScopeName.Number } }));

            // wice classes
            _rules.Add(new LanguageRule(@"\b(" + string.Join("|", types) + @")\b", new Dictionary<int, string> { { 1, ScopeName.ClassName } }));
        }

        public const string LanguageId = "wice";
        public string Id => LanguageId;
        public string Name => LanguageId;
        public string CssClassName => LanguageId;
        public bool HasAlias(string lang) => false;
        public string FirstLinePattern => null;
        public IList<LanguageRule> Rules => _rules;
    }
}
