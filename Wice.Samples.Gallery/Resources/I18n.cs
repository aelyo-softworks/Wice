using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using DirectN;
using Wice.Utilities;

namespace Wice.Samples.Gallery.Resources
{
    // this class uses a syntax similar to Javascript's i18next Framework, based on xml files (one per language)
    public static class I18n
    {
        public const string DefaultTwoLetterISOLanguageCode = "en";

        private static readonly Regex _nestingRegex = new Regex(@"(\$t\((?<e>[^)]+)\))+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Lazy<ConcurrentDictionary<string, ConcurrentDictionary<string, string>>> _texts = new Lazy<ConcurrentDictionary<string, ConcurrentDictionary<string, string>>>(LoadTexts, true);

        public static string T(string key, object context = null) => T(null, key, context);
        public static string T(string twoLetterISOLanguageCode, string key, object context = null)
        {
            if (key == null)
                return null;

            twoLetterISOLanguageCode = twoLetterISOLanguageCode ?? Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
            if (!_texts.Value.TryGetValue(twoLetterISOLanguageCode, out var langDic))
            {
                if (twoLetterISOLanguageCode.EqualsIgnoreCase(DefaultTwoLetterISOLanguageCode))
                    return Format(twoLetterISOLanguageCode, key, context);

                return T(DefaultTwoLetterISOLanguageCode, key, context);
            }

            if (!langDic.TryGetValue(key, out var localized))
            {
                if (twoLetterISOLanguageCode.EqualsIgnoreCase(DefaultTwoLetterISOLanguageCode))
                    return Format(twoLetterISOLanguageCode, key, context);

                return T(DefaultTwoLetterISOLanguageCode, key, context);
            }

            return Format(twoLetterISOLanguageCode, localized, context);
        }

        private static string Format(string twoLetterISOLanguageCode, string text, object context = null)
        {
            if (text == null)
                return null;

            if (context == null)
                return text;

            return text.FormatWith(context, (c, e, t, p) =>
            {
                var eval = StringFormatter.EvaluateExpression(c, e, t, p);
                return ResolveNesting(twoLetterISOLanguageCode, eval);
            });
        }

        private static ConcurrentDictionary<string, ConcurrentDictionary<string, string>> LoadTexts()
        {
            var dic = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            var nested = new List<Tuple<string, string>>(); // lang + key

            // load from file (for possible extensions)
            var path = System.IO.Path.Combine(Program.StorageDirectoryPath, "i18n");
            if (IOUtilities.DirectoryExists(path))
            {
                foreach (var file in Directory.EnumerateFiles(path, "*.xml"))
                {
                    var doc = new XmlDocument();
                    try
                    {
                        doc.Load(file);
                    }
                    catch
                    {
                        continue;
                    }
                    parse(doc);
                }
            }

            // load from embedded resources (overwrites files)
            foreach (var name in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                if (!name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    continue;

                var doc = new XmlDocument();
                try
                {
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
                    {
                        if (stream != null)
                        {
                            doc.Load(stream);
                        }
                    }
                }
                catch
                {
                    continue;
                }
                parse(doc);
            }

            // resolve all nested
            foreach (var n in nested)
            {
                var langDic = dic[n.Item1];
                var text = langDic[n.Item2];
                string resolved = _nestingRegex.Replace(text, (match) =>
                {
                    var sb = new StringBuilder();
                    var group = match.Groups["e"];
                    if (langDic.TryGetValue(group.Value, out var replacement))
                        return replacement;

                    return group.Value;
                });

                if (resolved != text)
                {
                    langDic[n.Item2] = resolved;
                }
            }
            return dic;

            void parse(XmlDocument document)
            {
                foreach (var langNode in document.SelectNodes("/lang").OfType<XmlElement>())
                {
                    var lang = langNode.GetAttribute("n").Nullify();
                    foreach (var stringNode in langNode.SelectNodes("s").OfType<XmlElement>())
                    {
                        var name = stringNode.GetAttribute("n").Nullify();
                        if (name == null)
                            continue;

                        var text = stringNode.InnerText;

                        // default for i18next is double braces
                        // FormatWith only support single braces
                        text = text.Replace("{{", "{").Replace("}}", "}");

                        if (!dic.TryGetValue(lang, out var langDic))
                        {
                            langDic = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                            dic.AddOrUpdate(lang, langDic, (k, o) => o);
                        }

                        langDic[name] = text;

                        if (text.IndexOf("$t(") >= 0)
                        {
                            nested.Add(new Tuple<string, string>(lang, name));
                        }
                    }
                }
            }
        }

        public static string ResolveNesting(string text) => ResolveNesting(null, text);
        public static string ResolveNesting(string twoLetterISOLanguageCode, string text)
        {
            if (text == null)
                return null;

            twoLetterISOLanguageCode = twoLetterISOLanguageCode ?? Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
            return _nestingRegex.Replace(text, (match) =>
            {
                var sb = new StringBuilder();
                var group = match.Groups["e"];
                if (!_texts.Value.TryGetValue(twoLetterISOLanguageCode, out var langDic))
                    return group.Value;

                if (langDic.TryGetValue(group.Value, out var replacement))
                    return replacement;

                return group.Value;
            });
        }
    }
}
