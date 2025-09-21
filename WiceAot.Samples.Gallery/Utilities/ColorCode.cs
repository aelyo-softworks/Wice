//
// ColorCode Universal
//
// amalgamified using https://github.com/smourier/CSharpMerge from https://github.com/CommunityToolkit/ColorCode-Universal
//
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

#pragma warning disable CA2211 // Non-constant fields should not be visible

namespace ColorCode
{
    /// <summary>
    /// Creates a <see cref="CodeColorizerBase"/>, for creating Syntax Highlighted code.
    /// </summary>
    /// <param name="Style">The Custom styles to Apply to the formatted Code.</param>
    /// <param name="languageParser">The language parser that the <see cref="CodeColorizerBase"/> instance will use for its lifetime.</param>
    public abstract class CodeColorizerBase(StyleDictionary? styles, ILanguageParser? languageParser)
    {
        /// <summary>
        /// Writes the parsed source code to the ouput using the specified style sheet.
        /// </summary>
        /// <param name="parsedSourceCode">The parsed source code to format and write to the output.</param>
        /// <param name="scopes">The captured scopes for the parsed source code.</param>
        protected abstract void Write(string parsedSourceCode, IList<Scope> scopes);

        /// <summary>
        /// The language parser that the <see cref="CodeColorizerBase"/> instance will use for its lifetime.
        /// </summary>
        protected readonly ILanguageParser languageParser = languageParser
                ?? new LanguageParser(new LanguageCompiler(Languages.CompiledLanguages, Languages.CompileLock), Languages.LanguageRepository);

        /// <summary>
        /// The styles to Apply to the formatted Code.
        /// </summary>
        public readonly StyleDictionary Styles = styles ?? StyleDictionary.DefaultLight;
    }

    /// <summary>
    /// Defines how ColorCode will parse the source code of a given language.
    /// </summary>
    public interface ILanguage
    {
        /// <summary>
        /// Gets the identifier of the language (e.g., csharp).
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the first line pattern (regex) to use when determining if the language matches a source text.
        /// </summary>
        string? FirstLinePattern { get; }

        /// <summary>
        /// Gets the "friendly" name of the language (e.g., C#).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the collection of language rules in the language.
        /// </summary>
        IList<LanguageRule> Rules { get; }

        /// <summary>
        /// Get the CSS class name to use for a language
        /// </summary>
        string CssClassName { get; }

        /// <summary>
        /// Returns true if the specified string is an alias for the language
        /// </summary>
        bool HasAlias(string lang);
    }

    /// <summary>
    /// Defines a single rule for a language. For instance a language rule might define string literals for a given language.
    /// </summary>
    public class LanguageRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageRule"/> class.
        /// </summary>
        /// <param name="regex">The regular expression that defines what the language rule matches and captures.</param>
        /// <param name="captures">The scope indices and names of the regular expression's captures.</param>
        public LanguageRule(string regex, IDictionary<int, string?> captures)
        {
            Guard.ArgNotNullAndNotEmpty(regex, nameof(regex));
            Guard.EnsureParameterIsNotNullAndNotEmpty(captures, "captures");

            Regex = regex;
            Captures = captures;
        }

        /// <summary>
        /// Gets the regular expression that defines what the language rule matches and captures.
        /// </summary>
        /// <value>The regular expression that defines what the language rule matches and captures.</value>
        public string Regex { get; private set; }

        /// <summary>
        /// Gets the scope indices and names of the regular expression's captures.
        /// </summary>
        /// <value>The scope indices and names of the regular expression's captures.</value>
        public IDictionary<int, string?> Captures { get; private set; }
    }

    /// <summary>
    /// Provides easy access to ColorCode's built-in languages, as well as methods to load and find custom languages.
    /// </summary>
    public static class Languages
    {
        internal static readonly LanguageRepository LanguageRepository;
        internal static readonly Dictionary<string, ILanguage> LoadedLanguages;
        internal static Dictionary<string, CompiledLanguage> CompiledLanguages;
        internal static ReaderWriterLockSlim CompileLock;

        static Languages()
        {
            LoadedLanguages = [];
            CompiledLanguages = [];
            LanguageRepository = new LanguageRepository(LoadedLanguages);
            CompileLock = new ReaderWriterLockSlim();

            Load<JavaScript>();
            Load<Html>();
            Load<CSharp>();
            Load<VbDotNet>();
            Load<Ashx>();
            Load<Asax>();
            Load<Aspx>();
            Load<AspxCs>();
            Load<AspxVb>();
            Load<Sql>();
            Load<Xml>();
            Load<Php>();
            Load<Css>();
            Load<Cpp>();
            Load<Java>();
            Load<PowerShell>();
            Load<Typescript>();
            Load<FSharp>();
            Load<Koka>();
            Load<Haskell>();
            Load<Markdown>();
            Load<Fortran>();
        }

        /// <summary>
        /// Gets an enumerable list of all loaded languages.
        /// </summary>
        public static IEnumerable<ILanguage> All => LanguageRepository.All;

        /// <summary>
        /// Language support for ASP.NET HTTP Handlers (.ashx files).
        /// </summary>
        /// <value>Language support for ASP.NET HTTP Handlers (.ashx files).</value>
        public static ILanguage Ashx => LanguageRepository.FindById(LanguageId.Ashx);

        /// <summary>
        /// Language support for ASP.NET application files (.asax files).
        /// </summary>
        /// <value>Language support for ASP.NET application files (.asax files).</value>
        public static ILanguage Asax => LanguageRepository.FindById(LanguageId.Asax);

        /// <summary>
        /// Language support for ASP.NET pages (.aspx files).
        /// </summary>
        /// <value>Language support for ASP.NET pages (.aspx files).</value>
        public static ILanguage Aspx => LanguageRepository.FindById(LanguageId.Aspx);

        /// <summary>
        /// Language support for ASP.NET C# code-behind files (.aspx.cs files).
        /// </summary>
        /// <value>Language support for ASP.NET C# code-behind files (.aspx.cs files).</value>
        public static ILanguage AspxCs => LanguageRepository.FindById(LanguageId.AspxCs);

        /// <summary>
        /// Language support for ASP.NET Visual Basic.NET code-behind files (.aspx.vb files).
        /// </summary>
        /// <value>Language support for ASP.NET Visual Basic.NET code-behind files (.aspx.vb files).</value>
        public static ILanguage AspxVb => LanguageRepository.FindById(LanguageId.AspxVb);

        /// <summary>
        /// Language support for C#.
        /// </summary>
        /// <value>Language support for C#.</value>
        public static ILanguage CSharp => LanguageRepository.FindById(LanguageId.CSharp);

        /// <summary>
        /// Language support for HTML.
        /// </summary>
        /// <value>Language support for HTML.</value>
        public static ILanguage Html => LanguageRepository.FindById(LanguageId.Html);

        /// <summary>
        /// Language support for Java.
        /// </summary>
        /// <value>Language support for Java.</value>
        public static ILanguage Java => LanguageRepository.FindById(LanguageId.Java);

        /// <summary>
        /// Language support for JavaScript.
        /// </summary>
        /// <value>Language support for JavaScript.</value>
        public static ILanguage JavaScript => LanguageRepository.FindById(LanguageId.JavaScript);

        /// <summary>
        /// Language support for PowerShell
        /// </summary>
        /// <value>Language support for PowerShell.</value>
        public static ILanguage PowerShell => LanguageRepository.FindById(LanguageId.PowerShell);

        /// <summary>
        /// Language support for SQL.
        /// </summary>
        /// <value>Language support for SQL.</value>
        public static ILanguage Sql => LanguageRepository.FindById(LanguageId.Sql);

        /// <summary>
        /// Language support for Visual Basic.NET.
        /// </summary>
        /// <value>Language support for Visual Basic.NET.</value>
        public static ILanguage VbDotNet => LanguageRepository.FindById(LanguageId.VbDotNet);

        /// <summary>
        /// Language support for XML.
        /// </summary>
        /// <value>Language support for XML.</value>
        public static ILanguage Xml => LanguageRepository.FindById(LanguageId.Xml);

        /// <summary>
        /// Language support for PHP.
        /// </summary>
        /// <value>Language support for PHP.</value>
        public static ILanguage Php => LanguageRepository.FindById(LanguageId.Php);

        /// <summary>
        /// Language support for CSS.
        /// </summary>
        /// <value>Language support for CSS.</value>
        public static ILanguage Css => LanguageRepository.FindById(LanguageId.Css);

        /// <summary>
        /// Language support for C++.
        /// </summary>
        /// <value>Language support for C++.</value>
        public static ILanguage Cpp => LanguageRepository.FindById(LanguageId.Cpp);

        /// <summary>
        /// Language support for Typescript.
        /// </summary>
        /// <value>Language support for typescript.</value>
        public static ILanguage Typescript => LanguageRepository.FindById(LanguageId.TypeScript);

        /// <summary>
        /// Language support for F#.
        /// </summary>
        /// <value>Language support for F#.</value>
        public static ILanguage FSharp => LanguageRepository.FindById(LanguageId.FSharp);

        /// <summary>
        /// Language support for Koka.
        /// </summary>
        /// <value>Language support for Koka.</value>
        public static ILanguage Koka => LanguageRepository.FindById(LanguageId.Koka);

        /// <summary>
        /// Language support for Haskell.
        /// </summary>
        /// <value>Language support for Haskell.</value>
        public static ILanguage Haskell => LanguageRepository.FindById(LanguageId.Haskell);

        /// <summary>
        /// Language support for Markdown.
        /// </summary>
        /// <value>Language support for Markdown.</value>
        public static ILanguage Markdown => LanguageRepository.FindById(LanguageId.Markdown);

        /// <summary>
        /// Language support for Fortran.
        /// </summary>
        /// <value>Language support for Fortran.</value>
        public static ILanguage Fortran => LanguageRepository.FindById(LanguageId.Fortran);

        /// <summary>
        /// Finds a loaded language by the specified identifier.
        /// </summary>
        /// <param name="id">The identifier of the language to find.</param>
        /// <returns>An <see cref="ILanguage" /> instance if the specified identifier matches a loaded language; otherwise, null.</returns>
        public static ILanguage FindById(string id) => LanguageRepository.FindById(id);

        private static void Load<T>()
            where T : ILanguage, new() => Load(new T());

        /// <summary>
        /// Loads the specified language.
        /// </summary>
        /// <param name="language">The language to load.</param>
        public static void Load(ILanguage language) => LanguageRepository.Load(language);
    }

    public static class ExtensionMethods
    {
        public static void SortStable<T>(this IList<T> list,
                                         Comparison<T> comparison)
        {
            Guard.ArgNotNull(list, nameof(list));

            int count = list.Count;

            for (int j = 1; j < count; j++)
            {
                T key = list[j];

                int i = j - 1;
                for (; i >= 0 && comparison(list[i], key) > 0; i--)
                {
                    list[i + 1] = list[i];
                }

                list[i + 1] = key;
            }
        }
    }

    public static class Guard
    {
        public static void ArgNotNull(object arg, string paramName)
        {
            if (arg == null)
                throw new ArgumentNullException(paramName);
        }

        public static void ArgNotNullAndNotEmpty(string arg, string paramName)
        {
            if (arg == null)
                throw new ArgumentNullException(paramName);

            if (string.IsNullOrEmpty(arg))
                throw new ArgumentException(string.Format("The {0} argument value must not be empty.", paramName), paramName);
        }

        public static void EnsureParameterIsNotNullAndNotEmpty<TKey, TValue>(IDictionary<TKey, TValue> parameter, string parameterName)
        {
            if (parameter == null || parameter.Count == 0)
                throw new ArgumentNullException(parameterName);
        }

        public static void ArgNotNullAndNotEmpty<T>(IList<T> arg, string paramName)
        {
            if (arg == null)
                throw new ArgumentNullException(paramName);

            if (arg.Count == 0)
                throw new ArgumentException(string.Format("The {0} argument value must not be empty.", paramName), paramName);
        }
    }

    public interface ILanguageRepository
    {
        IEnumerable<ILanguage> All { get; }
        ILanguage FindById(string languageId);
        void Load(ILanguage language);
    }

    public static class LanguageId
    {
        public const string Asax = "asax";
        public const string Ashx = "ashx";
        public const string Aspx = "aspx";
        public const string AspxCs = "aspx(c#)";
        public const string AspxVb = "aspx(vb.net)";
        public const string CSharp = "c#";
        public const string Cpp = "cpp";
        public const string Css = "css";
        public const string FSharp = "f#";
        public const string Html = "html";
        public const string Java = "java";
        public const string JavaScript = "javascript";
        public const string TypeScript = "typescript";
        public const string Php = "php";
        public const string PowerShell = "powershell";
        public const string Sql = "sql";
        public const string VbDotNet = "vb.net";
        public const string Xml = "xml";
        public const string Koka = "koka";
        public const string Haskell = "haskell";
        public const string Markdown = "markdown";
        public const string Fortran = "fortran";
    }

    public class LanguageRepository(Dictionary<string, ILanguage> loadedLanguages) : ILanguageRepository
    {
        private readonly Dictionary<string, ILanguage> loadedLanguages = loadedLanguages;
        private readonly ReaderWriterLockSlim loadLock = new();

        public IEnumerable<ILanguage> All => loadedLanguages.Values;

        public ILanguage FindById(string languageId)
        {
            Guard.ArgNotNullAndNotEmpty(languageId, nameof(languageId));

            ILanguage? language = null;

            loadLock.EnterReadLock();

            try
            {
                // If we have a matching name for the language then use it
                // otherwise check if any languages have that string as an
                // alias. For example: "js" is an alias for Javascript.
                language = loadedLanguages.FirstOrDefault(x => (x.Key.Equals(languageId, StringComparison.CurrentCultureIgnoreCase)) ||
                                                               (x.Value.HasAlias(languageId))).Value;
            }
            finally
            {
                loadLock.ExitReadLock();
            }

            return language;
        }

        public void Load(ILanguage language)
        {
            Guard.ArgNotNull(language, nameof(language));

            if (string.IsNullOrEmpty(language.Id))
                throw new ArgumentException("The language identifier must not be null or empty.", nameof(language));

            loadLock.EnterWriteLock();

            try
            {
                loadedLanguages[language.Id] = language;
            }
            finally
            {
                loadLock.ExitWriteLock();
            }
        }
    }

    public class ScopeName
    {
        public const string ClassName = "Class Name";
        public const string Comment = "Comment";
        public const string CssPropertyName = "CSS Property Name";
        public const string CssPropertyValue = "CSS Property Value";
        public const string CssSelector = "CSS Selector";
        public const string HtmlAttributeName = "HTML Attribute ScopeName";
        public const string HtmlAttributeValue = "HTML Attribute Value";
        public const string HtmlComment = "HTML Comment";
        public const string HtmlElementName = "HTML Element ScopeName";
        public const string HtmlEntity = "HTML Entity";
        public const string HtmlOperator = "HTML Operator";
        public const string HtmlServerSideScript = "HTML Server-Side Script";
        public const string HtmlTagDelimiter = "Html Tag Delimiter";
        public const string Keyword = "Keyword";
        public const string LanguagePrefix = "&";
        public const string PlainText = "Plain Text";
        public const string PowerShellAttribute = "PowerShell PowerShellAttribute";
        public const string PowerShellOperator = "PowerShell Operator";
        public const string PowerShellType = "PowerShell Type";
        public const string PowerShellVariable = "PowerShell Variable";
        public const string PreprocessorKeyword = "Preprocessor Keyword";
        public const string SqlSystemFunction = "SQL System Function";
        public const string String = "String";
        public const string StringCSharpVerbatim = "String (C# @ Verbatim)";
        public const string XmlAttribute = "XML Attribute";
        public const string XmlAttributeQuotes = "XML Attribute Quotes";
        public const string XmlAttributeValue = "XML Attribute Value";
        public const string XmlCDataSection = "XML CData Section";
        public const string XmlComment = "XML Comment";
        public const string XmlDelimiter = "XML Delimiter";
        public const string XmlDocComment = "XML Doc Comment";
        public const string XmlDocTag = "XML Doc Tag";
        public const string XmlName = "XML Name";
        public const string Type = "Type";
        public const string TypeVariable = "Type Variable";
        public const string NameSpace = "Name Space";
        public const string Constructor = "Constructor";
        public const string Predefined = "Predefined";
        public const string PseudoKeyword = "Pseudo Keyword";
        public const string StringEscape = "String Escape";
        public const string ControlKeyword = "Control Keyword";
        public const string Number = "Number";
        public const string Operator = "Operator";
        public const string Delimiter = "Delimiter";
        public const string MarkdownHeader = "Markdown Header";
        public const string MarkdownCode = "Markdown Code";
        public const string MarkdownListItem = "Markdown List Item";
        public const string MarkdownEmph = "Markdown Emphasized";
        public const string MarkdownBold = "Markdown Bold";
        public const string BuiltinFunction = "Built In Function";
        public const string BuiltinValue = "Built In Value";
        public const string Attribute = "Attribute";
        public const string SpecialCharacter = "Special Character";
        public const string Intrinsic = "Intrinsic";
        public const string Brackets = "Brackets";
        public const string Continuation = "Continuation";
    }

    public class TextInsertion
    {
        public virtual int Index { get; set; }
        public virtual string? Text { get; set; }
        public virtual Scope? Scope { get; set; }
    }

    public class CompiledLanguage
    {
        public CompiledLanguage(string id,
                                string name,
                                Regex regex,
                                IList<string?> captures)
        {
            Guard.ArgNotNullAndNotEmpty(id, nameof(id));
            Guard.ArgNotNullAndNotEmpty(name, nameof(name));
            Guard.ArgNotNull(regex, nameof(regex));
            Guard.ArgNotNullAndNotEmpty(captures, nameof(captures));

            Id = id;
            Name = name;
            Regex = regex;
            Captures = captures;
        }

        public IList<string?> Captures { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public Regex Regex { get; set; }

        public override string ToString() => Name;
    }

    public interface ILanguageCompiler
    {
        CompiledLanguage Compile(ILanguage language);
    }

    public class LanguageCompiler(Dictionary<string, CompiledLanguage> compiledLanguages, ReaderWriterLockSlim compileLock) : ILanguageCompiler
    {
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
        private static readonly Regex numberOfCapturesRegex = new(@"(?x)(?<!(\\|(?!\\)\(\?))\((?!\?)", RegexOptions.Compiled);
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
        private readonly Dictionary<string, CompiledLanguage> compiledLanguages = compiledLanguages;
        private readonly ReaderWriterLockSlim compileLock = compileLock;

        public CompiledLanguage Compile(ILanguage language)
        {
            Guard.ArgNotNull(language, nameof(language));

            if (string.IsNullOrEmpty(language.Id))
                throw new ArgumentException("The language identifier must not be null.", nameof(language));

            CompiledLanguage compiledLanguage;

            compileLock.EnterReadLock();
            try
            {
                // for performance reasons we should first try with
                // only a read lock since the majority of the time
                // it'll be created already and upgradeable lock blocks
                if (compiledLanguages.TryGetValue(language.Id, out CompiledLanguage? value))
                    return value;
            }
            finally
            {
                compileLock.ExitReadLock();
            }

            compileLock.EnterUpgradeableReadLock();
            try
            {
                if (compiledLanguages.TryGetValue(language.Id, out CompiledLanguage? value))
                    compiledLanguage = value;
                else
                {
                    compileLock.EnterWriteLock();

                    try
                    {
                        if (string.IsNullOrEmpty(language.Name))
                            throw new ArgumentException("The language name must not be null or empty.", nameof(language));

                        if (language.Rules == null || language.Rules.Count == 0)
                            throw new ArgumentException("The language rules collection must not be empty.", nameof(language));

                        compiledLanguage = CompileLanguage(language);

                        compiledLanguages.Add(compiledLanguage.Id, compiledLanguage);
                    }
                    finally
                    {
                        compileLock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                compileLock.ExitUpgradeableReadLock();
            }

            return compiledLanguage;
        }

        private static CompiledLanguage CompileLanguage(ILanguage language)
        {
            string id = language.Id;
            string name = language.Name;

            CompileRules(language.Rules, out Regex regex, out var captures);

            return new CompiledLanguage(id, name, regex, captures);
        }

        private static void CompileRules(IList<LanguageRule> rules, out Regex regex, out IList<string?> captures)
        {
            StringBuilder regexBuilder = new();
            captures = [];

            regexBuilder.AppendLine("(?x)");
            captures.Add(null);

            CompileRule(rules[0], regexBuilder, captures, true);

            for (int i = 1; i < rules.Count; i++)
                CompileRule(rules[i], regexBuilder, captures, false);

            regex = new Regex(regexBuilder.ToString());
        }

        private static void CompileRule(LanguageRule languageRule, StringBuilder regex, ICollection<string?> captures, bool isFirstRule)
        {
            if (!isFirstRule)
            {
                regex.AppendLine();
                regex.AppendLine();
                regex.AppendLine("|");
                regex.AppendLine();
            }

            regex.AppendFormat("(?-xis)(?m)({0})(?x)", languageRule.Regex);

            int numberOfCaptures = GetNumberOfCaptures(languageRule.Regex);

            for (int i = 0; i <= numberOfCaptures; i++)
            {
                string? scope = null;

                foreach (int captureIndex in languageRule.Captures.Keys)
                {
                    if (i == captureIndex)
                    {
                        scope = languageRule.Captures[captureIndex];
                        break;
                    }
                }

                captures.Add(scope);
            }
        }

        private static int GetNumberOfCaptures(string regex) => numberOfCapturesRegex.Matches(regex).Count;
    }

    public static class RuleCaptures
    {
        public static IDictionary<int, string?> JavaScript;
        public static IDictionary<int, string?> CSharpScript;
        public static IDictionary<int, string?> VbDotNetScript;

        static RuleCaptures()
        {
            JavaScript = BuildCaptures(LanguageId.JavaScript);
            CSharpScript = BuildCaptures(LanguageId.CSharp);
            VbDotNetScript = BuildCaptures(LanguageId.VbDotNet);
        }

        private static Dictionary<int, string?> BuildCaptures(string languageId) => new()
        {
                           {1, ScopeName.HtmlTagDelimiter},
                           {2, ScopeName.HtmlElementName},
                           {3, ScopeName.HtmlAttributeName},
                           {4, ScopeName.HtmlOperator},
                           {5, ScopeName.HtmlAttributeValue},
                           {6, ScopeName.HtmlAttributeName},
                           {7, ScopeName.HtmlOperator},
                           {8, ScopeName.HtmlAttributeValue},
                           {9, ScopeName.HtmlAttributeName},
                           {10, ScopeName.HtmlOperator},
                           {11, ScopeName.HtmlAttributeValue},
                           {12, ScopeName.HtmlAttributeName},
                           {13, ScopeName.HtmlAttributeName},
                           {14, ScopeName.HtmlOperator},
                           {15, ScopeName.HtmlAttributeValue},
                           {16, ScopeName.HtmlAttributeName},
                           {17, ScopeName.HtmlOperator},
                           {18, ScopeName.HtmlAttributeValue},
                           {19, ScopeName.HtmlAttributeName},
                           {20, ScopeName.HtmlOperator},
                           {21, ScopeName.HtmlAttributeValue},
                           {22, ScopeName.HtmlAttributeName},
                           {23, ScopeName.HtmlOperator},
                           {24, ScopeName.HtmlAttributeValue},
                           {25, ScopeName.HtmlAttributeName},
                           {26, ScopeName.HtmlTagDelimiter},
                           {27, string.Format("{0}{1}", ScopeName.LanguagePrefix, languageId)},
                           {28, ScopeName.HtmlTagDelimiter},
                           {29, ScopeName.HtmlElementName},
                           {30, ScopeName.HtmlTagDelimiter}
                       };
    }

    public static class RuleFormats
    {
        public static string JavaScript;
        public static string ServerScript;

        static RuleFormats()
        {
            const string script = @"(?xs)(<)(script)
                                        {0}[\s\n]+({1})[\s\n]*(=)[\s\n]*(""{2}""){0}[\s\n]*(>)
                                        (.*?)
                                        (</)(script)(>)";

            const string attributes = @"(?:[\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*([^\s\n""']+?)
                                           |[\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*(""[^\n]+?"")
                                           |[\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*('[^\n]+?')
                                           |[\s\n]+([a-z0-9-_]+) )*";

            JavaScript = string.Format(script, attributes, "type|language", "[^\n]*javascript");
            ServerScript = string.Format(script, attributes, "runat", "server");
        }
    }

    public class Asax : ILanguage
    {
        public string Id => LanguageId.Asax;
        public string Name => "ASAX";
        public string CssClassName => "asax";
        public string? FirstLinePattern => null;

        public IList<LanguageRule> Rules => [
                               new(
                                   @"(<%)(--.*?--)(%>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.HtmlServerSideScript },
                                           { 2, ScopeName.HtmlComment },
                                           { 3, ScopeName.HtmlServerSideScript }
                                       }),
                               new(
                                   @"(?is)(?<=<%@.+?language=""c\#"".*?%>.*?<script.*?runat=""server"">)(.*)(?=</script>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, string.Format("{0}{1}", ScopeName.LanguagePrefix, LanguageId.CSharp) }
                                       }),
                               new(
                                   @"(?is)(?<=<%@.+?language=""vb"".*?%>.*?<script.*?runat=""server"">)(.*)(?=</script>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, string.Format("{0}{1}", ScopeName.LanguagePrefix, LanguageId.VbDotNet) }
                                       }),
                               new(
                                   @"(?xi)(</?)
                                         (?: ([a-z][a-z0-9-]*)(:) )*
                                         ([a-z][a-z0-9-_]*)
                                         (?:
                                            [\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*([^\s\n""']+?)
                                           |[\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*(""[^\n]+?"")
                                           |[\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*('[^\n]+?')
                                           |[\s\n]+([a-z0-9-_]+) )*
                                         [\s\n]*
                                         (/?>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.HtmlTagDelimiter },
                                           { 2, ScopeName.HtmlElementName },
                                           { 3, ScopeName.HtmlTagDelimiter },
                                           { 4, ScopeName.HtmlElementName },
                                           { 5, ScopeName.HtmlAttributeName },
                                           { 6, ScopeName.HtmlOperator },
                                           { 7, ScopeName.HtmlAttributeValue },
                                           { 8, ScopeName.HtmlAttributeName },
                                           { 9, ScopeName.HtmlOperator },
                                           { 10, ScopeName.HtmlAttributeValue },
                                           { 11, ScopeName.HtmlAttributeName },
                                           { 12, ScopeName.HtmlOperator },
                                           { 13, ScopeName.HtmlAttributeValue },
                                           { 14, ScopeName.HtmlAttributeName },
                                           { 15, ScopeName.HtmlTagDelimiter }
                                       }),
                               new(
                                   @"(<%)(@)(?:\s+([a-zA-Z0-9]+))*(?:\s+([a-zA-Z0-9]+)(=)(""[^\n]*?""))*\s*?(%>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.HtmlServerSideScript },
                                           { 2, ScopeName.HtmlTagDelimiter },
                                           { 3, ScopeName.HtmlElementName },
                                           { 4, ScopeName.HtmlAttributeName },
                                           { 5, ScopeName.HtmlOperator },
                                           { 6, ScopeName.HtmlAttributeValue },
                                           { 7, ScopeName.HtmlServerSideScript }
                                       })
                           ];

        public bool HasAlias(string lang) => false;

        public override string ToString() => Name;
    }

    public class Ashx : ILanguage
    {
        public string Id => LanguageId.Ashx;
        public string Name => "ASHX";
        public string CssClassName => "ashx";
        public string? FirstLinePattern => null;
        public IList<LanguageRule> Rules => [
                               new(
                                   @"(<%)(--.*?--)(%>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.HtmlServerSideScript },
                                           { 2, ScopeName.HtmlComment },
                                           { 3, ScopeName.HtmlServerSideScript }
                                       }),
                               new(
                                   @"(?is)(?<=<%@.+?language=""c\#"".*?%>)(.*)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, string.Format("{0}{1}", ScopeName.LanguagePrefix, LanguageId.CSharp) }
                                       }),
                               new(
                                   @"(?is)(?<=<%@.+?language=""vb"".*?%>)(.*)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, string.Format("{0}{1}", ScopeName.LanguagePrefix, LanguageId.VbDotNet) }
                                       }),
                               new(
                                   @"(<%)(@)(?:\s+([a-zA-Z0-9]+))*(?:\s+([a-zA-Z0-9]+)(=)(""[^\n]*?""))*\s*?(%>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.HtmlServerSideScript },
                                           { 2, ScopeName.HtmlTagDelimiter },
                                           { 3, ScopeName.HtmlElementName },
                                           { 4, ScopeName.HtmlAttributeName },
                                           { 5, ScopeName.HtmlOperator },
                                           { 6, ScopeName.HtmlAttributeValue },
                                           { 7, ScopeName.HtmlServerSideScript }
                                       })
                           ];

        public bool HasAlias(string lang) => false;

        public override string ToString() => Name;
    }

    public class Aspx : ILanguage
    {
        public string Id => LanguageId.Aspx;

        public string Name => "ASPX";

        public string CssClassName => "aspx";

        public string? FirstLinePattern => null;

        public IList<LanguageRule> Rules => [
                               new(
                                   @"(?s)(<%)(--.*?--)(%>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.HtmlServerSideScript },
                                           { 2, ScopeName.HtmlComment },
                                           { 3, ScopeName.HtmlServerSideScript }
                                       }),
                               new(
                                   @"(?s)<!--.*?-->",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.HtmlComment }
                                       }),
                               new(
                                   @"(?i)(<%)(@)(?:\s+([a-z0-9]+))*(?:\s+([a-z0-9]+)(=)(""[^\n]*?""))*\s*?(%>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.HtmlServerSideScript },
                                           { 2, ScopeName.HtmlTagDelimiter },
                                           { 3, ScopeName.HtmlElementName },
                                           { 4, ScopeName.HtmlAttributeName },
                                           { 5, ScopeName.HtmlOperator },
                                           { 6, ScopeName.HtmlAttributeValue },
                                           { 7, ScopeName.HtmlServerSideScript }
                                       }),
                               new(
                                   @"(?s)(?:(<%=|<%)(?!=|@|--))(?:.*?)(%>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.HtmlServerSideScript },
                                           { 2, ScopeName.HtmlServerSideScript }
                                       }),
                               new(
                                   @"(?is)(<!)(DOCTYPE)(?:\s+([a-z0-9]+))*(?:\s+(""[^""]*?""))*(>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.HtmlTagDelimiter },
                                           { 2, ScopeName.HtmlElementName },
                                           { 3, ScopeName.HtmlAttributeName },
                                           { 4, ScopeName.HtmlAttributeValue },
                                           { 5, ScopeName.HtmlTagDelimiter }
                                       }),
                               new(RuleFormats.JavaScript, RuleCaptures.JavaScript),
                               new(
                                   @"(?xi)(</?)
                                         (?: ([a-z][a-z0-9-]*)(:) )*
                                         ([a-z][a-z0-9-_]*)
                                         (?:
                                            [\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*([^\s\n""']+?)
                                           |[\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*(""[^\n]+?"")
                                           |[\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*('[^\n]+?')
                                           |[\s\n]+([a-z0-9-_]+) )*
                                         [\s\n]*
                                         (/?>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.HtmlTagDelimiter },
                                           { 2, ScopeName.HtmlElementName },
                                           { 3, ScopeName.HtmlTagDelimiter },
                                           { 4, ScopeName.HtmlElementName },
                                           { 5, ScopeName.HtmlAttributeName },
                                           { 6, ScopeName.HtmlOperator },
                                           { 7, ScopeName.HtmlAttributeValue },
                                           { 8, ScopeName.HtmlAttributeName },
                                           { 9, ScopeName.HtmlOperator },
                                           { 10, ScopeName.HtmlAttributeValue },
                                           { 11, ScopeName.HtmlAttributeName },
                                           { 12, ScopeName.HtmlOperator },
                                           { 13, ScopeName.HtmlAttributeValue },
                                           { 14, ScopeName.HtmlAttributeName },
                                           { 15, ScopeName.HtmlTagDelimiter }
                                       }),
                               new(
                                   @"(?i)&[a-z0-9]+?;",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.HtmlEntity }
                                       })
                           ];

        public bool HasAlias(string lang) => false;

        public override string ToString() => Name;
    }

    public class AspxCs : ILanguage
    {
        public string Id => LanguageId.AspxCs;

        public string Name => "ASPX (C#)";

        public string CssClassName => "aspx-cs";

        public string? FirstLinePattern => @"(?xims)<%@\s*?(?:page|control|master|servicehost|webservice).*?(?:language=""c\#""|src="".+?.cs"").*?%>";

        public IList<LanguageRule> Rules => [
                               new(
                                   @"(?s)(<%)(--.*?--)(%>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.HtmlServerSideScript },
                                           { 2, ScopeName.HtmlComment },
                                           { 3, ScopeName.HtmlServerSideScript }
                                       }),
                               new(
                                   @"(?s)<!--.*-->",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.HtmlComment },
                                       }),
                               new(
                                   @"(?i)(<%)(@)(?:\s+([a-z0-9]+))*(?:\s+([a-z0-9]+)\s*(=)\s*(""[^\n]*?""))*\s*?(%>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.HtmlServerSideScript },
                                           { 2, ScopeName.HtmlTagDelimiter },
                                           { 3, ScopeName.HtmlElementName },
                                           { 4, ScopeName.HtmlAttributeName },
                                           { 5, ScopeName.HtmlOperator },
                                           { 6, ScopeName.HtmlAttributeValue },
                                           { 7, ScopeName.HtmlServerSideScript }
                                       }),
                               new(
                                   @"(?s)(?:(<%=|<%)(?!=|@|--))(.*?)(%>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.HtmlServerSideScript },
                                           { 2, string.Format("{0}{1}", ScopeName.LanguagePrefix, LanguageId.CSharp) },
                                           { 3, ScopeName.HtmlServerSideScript }
                                       }),
                               new(RuleFormats.ServerScript, RuleCaptures.CSharpScript),
                               new(
                                   @"(?i)(<!)(DOCTYPE)(?:\s+([a-zA-Z0-9]+))*(?:\s+(""[^""]*?""))*(>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.HtmlTagDelimiter },
                                           { 2, ScopeName.HtmlElementName },
                                           { 3, ScopeName.HtmlAttributeName },
                                           { 4, ScopeName.HtmlAttributeValue },
                                           { 5, ScopeName.HtmlTagDelimiter }
                                       }),
                               new(RuleFormats.JavaScript, RuleCaptures.JavaScript),
                               new(
                                   @"(?xis)(</?)
                                          (?: ([a-z][a-z0-9-]*)(:) )*
                                          ([a-z][a-z0-9-_]*)
                                          (?:
                                             [\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*([^\s\n""']+?)
                                            |[\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*(""[^\n]+?"")
                                            |[\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*('[^\n]+?')
                                            |[\s\n]+([a-z0-9-_]+) )*
                                          [\s\n]*
                                          (/?>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.HtmlTagDelimiter },
                                           { 2, ScopeName.HtmlElementName },
                                           { 3, ScopeName.HtmlTagDelimiter },
                                           { 4, ScopeName.HtmlElementName },
                                           { 5, ScopeName.HtmlAttributeName },
                                           { 6, ScopeName.HtmlOperator },
                                           { 7, ScopeName.HtmlAttributeValue },
                                           { 8, ScopeName.HtmlAttributeName },
                                           { 9, ScopeName.HtmlOperator },
                                           { 10, ScopeName.HtmlAttributeValue },
                                           { 11, ScopeName.HtmlAttributeName },
                                           { 12, ScopeName.HtmlOperator },
                                           { 13, ScopeName.HtmlAttributeValue },
                                           { 14, ScopeName.HtmlAttributeName },
                                           { 15, ScopeName.HtmlTagDelimiter }
                                       }),
                               new(
                                   @"(?i)&\#?[a-z0-9]+?;",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.HtmlEntity }
                                       }),
                           ];

        public bool HasAlias(string lang) => lang.ToLower() switch
        {
            "aspx-cs" or "aspx (cs)" or "aspx(cs)" => true,
            _ => false,
        };

        public override string ToString() => Name;
    }

    public class AspxVb : ILanguage
    {
        public string Id => LanguageId.AspxVb;

        public string Name => "ASPX (VB.NET)";

        public string CssClassName => "aspx-vb";

        public string? FirstLinePattern => @"(?xims)<%@\s*?(?:page|control|master|webhandler|servicehost|webservice).*?language=""vb"".*?%>";

        public IList<LanguageRule> Rules => [
                               new(
                                   @"(?s)(<%)(--.*?--)(%>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.HtmlServerSideScript },
                                           { 2, ScopeName.HtmlComment },
                                           { 3, ScopeName.HtmlServerSideScript }
                                       }),
                               new(
                                   @"(?s)<!--.*-->",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.HtmlComment },
                                       }),
                               new(
                                   @"(?i)(<%)(@)(?:\s+([a-z0-9]+))*(?:\s+([a-z0-9]+)\s*(=)\s*(""[^\n]*?""))*\s*?(%>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.HtmlServerSideScript },
                                           { 2, ScopeName.HtmlTagDelimiter },
                                           { 3, ScopeName.HtmlElementName },
                                           { 4, ScopeName.HtmlAttributeName },
                                           { 5, ScopeName.HtmlOperator },
                                           { 6, ScopeName.HtmlAttributeValue },
                                           { 7, ScopeName.HtmlServerSideScript }
                                       }),
                               new(
                                   @"(?s)(?:(<%=|<%)(?!=|@|--))(.*?)(%>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.HtmlServerSideScript },
                                           { 2, string.Format("{0}{1}", ScopeName.LanguagePrefix, LanguageId.VbDotNet) },
                                           { 3, ScopeName.HtmlServerSideScript }
                                       }),
                               new(RuleFormats.ServerScript, RuleCaptures.VbDotNetScript),
                               new(
                                   @"(?i)(<!)(DOCTYPE)(?:\s+([a-zA-Z0-9]+))*(?:\s+(""[^""]*?""))*(>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.HtmlTagDelimiter },
                                           { 2, ScopeName.HtmlElementName },
                                           { 3, ScopeName.HtmlAttributeName },
                                           { 4, ScopeName.HtmlAttributeValue },
                                           { 5, ScopeName.HtmlTagDelimiter }
                                       }),
                               new(RuleFormats.JavaScript, RuleCaptures.JavaScript),
                               new(
                                   @"(?xis)(</?)
                                          (?: ([a-z][a-z0-9-]*)(:) )*
                                          ([a-z][a-z0-9-_]*)
                                          (?:
                                             [\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*(?:'(<%\#)(.+?)(%>)')
                                            |[\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*(?:""(<%\#)(.+?)(%>)"")
                                            |[\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*([^\s\n""']+?)
                                            |[\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*(""[^\n]+?"")
                                            |[\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*('[^\n]+?')
                                            |[\s\n]+([a-z0-9-_]+) )*
                                          [\s\n]*
                                          (/?>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.HtmlTagDelimiter },
                                           { 2, ScopeName.HtmlElementName },
                                           { 3, ScopeName.HtmlTagDelimiter },
                                           { 4, ScopeName.HtmlElementName },
                                           { 5, ScopeName.HtmlAttributeName },
                                           { 6, ScopeName.HtmlOperator },
                                           { 7, ScopeName.HtmlServerSideScript },
                                           { 8, string.Format("{0}{1}", ScopeName.LanguagePrefix, LanguageId.VbDotNet) },
                                           { 9, ScopeName.HtmlServerSideScript },
                                           { 10, ScopeName.HtmlAttributeName },
                                           { 11, ScopeName.HtmlOperator },
                                           { 12, ScopeName.HtmlServerSideScript },
                                           { 13, string.Format("{0}{1}", ScopeName.LanguagePrefix, LanguageId.VbDotNet) },
                                           { 14, ScopeName.HtmlServerSideScript },
                                           { 15, ScopeName.HtmlAttributeName },
                                           { 16, ScopeName.HtmlOperator },
                                           { 17, ScopeName.HtmlAttributeValue },
                                           { 18, ScopeName.HtmlAttributeName },
                                           { 19, ScopeName.HtmlOperator },
                                           { 20, ScopeName.HtmlAttributeValue },
                                           { 21, ScopeName.HtmlAttributeName },
                                           { 22, ScopeName.HtmlOperator },
                                           { 23, ScopeName.HtmlAttributeValue },
                                           { 24, ScopeName.HtmlAttributeName },
                                           { 25, ScopeName.HtmlTagDelimiter }
                                       }),
                               new(
                                   @"(?i)&\#?[a-z0-9]+?;",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.HtmlEntity }
                                       }),
                           ];

        public bool HasAlias(string lang) => lang.ToLower() switch
        {
            "aspx-vb" or "aspx (vb.net)" or "aspx(vb.net)" => true,
            _ => false,
        };

        public override string ToString() => Name;
    }

    public class Cpp : ILanguage
    {
        public string Id => LanguageId.Cpp;

        public string Name => "C++";

        public string CssClassName => "cplusplus";

        public string? FirstLinePattern => null;

        public IList<LanguageRule> Rules => [
                               new(
                                   @"/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.Comment },
                                       }),
                               new(
                                   @"(//.*?)\r?$",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.Comment }
                                       }),
                               new(
                                   @"(?s)(""[^\n]*?(?<!\\)"")",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.String }
                                       }),
                               new(
                                   @"\b(abstract|array|auto|bool|break|case|catch|char|ref class|class|const|const_cast|continue|default|delegate|delete|deprecated|dllexport|dllimport|do|double|dynamic_cast|each|else|enum|event|explicit|export|extern|false|float|for|friend|friend_as|gcnew|generic|goto|if|in|initonly|inline|int|interface|literal|long|mutable|naked|namespace|new|noinline|noreturn|nothrow|novtable|nullptr|operator|private|property|protected|public|register|reinterpret_cast|return|safecast|sealed|selectany|short|signed|sizeof|static|static_cast|ref struct|struct|switch|template|this|thread|throw|true|try|typedef|typeid|typename|union|unsigned|using|uuid|value|virtual|void|volatile|wchar_t|while)\b",
                                   new Dictionary<int, string?>
                                       {
                                           {0, ScopeName.Keyword},
                                       }),
                           ];

        public bool HasAlias(string lang) => lang.ToLower() switch
        {
            "c++" or "c" => true,
            _ => false,
        };

        public override string ToString() => Name;
    }

    public class CSharp : ILanguage
    {
        public string Id => LanguageId.CSharp;

        public string Name => "C#";

        public string CssClassName => "csharp";

        public string? FirstLinePattern => null;

        public IList<LanguageRule> Rules => [
                               new(
                                   @"/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.Comment },
                                       }),
                               new(
                                   @"(///)(?:\s*?(<[/a-zA-Z0-9\s""=]+>))*([^\r\n]*)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.XmlDocTag },
                                           { 2, ScopeName.XmlDocTag },
                                           { 3, ScopeName.XmlDocComment },
                                       }),
                               new(
                                   @"(//.*?)\r?$",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.Comment }
                                       }),
                               new(
                                   @"'[^\n]*?(?<!\\)'",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.String }
                                       }),
                               new(
                                   @"(?s)@""(?:""""|.)*?""(?!"")",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.StringCSharpVerbatim }
                                       }),
                               new(
                                   @"(?s)(""[^\n]*?(?<!\\)"")",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.String }
                                       }),
                               new(
                                   @"\[(assembly|module|type|return|param|method|field|property|event):[^\]""]*(""[^\n]*?(?<!\\)"")?[^\]]*\]",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.Keyword },
                                           { 2, ScopeName.String }
                                       }),
                               new(
                                   @"^\s*(\#define|\#elif|\#else|\#endif|\#endregion|\#error|\#if|\#line|\#pragma|\#region|\#undef|\#warning).*?$",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.PreprocessorKeyword }
                                       }),
                               new(
                                   @"\b(abstract|as|ascending|base|bool|break|by|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|descending|do|double|dynamic|else|enum|equals|event|explicit|extern|false|finally|fixed|float|for|foreach|from|get|goto|group|if|implicit|in|int|into|interface|internal|is|join|let|lock|long|namespace|new|null|object|on|operator|orderby|out|override|params|partial|private|protected|public|readonly|ref|return|sbyte|sealed|select|set|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|var|virtual|void|volatile|where|while|yield|async|await|warning|disable)\b",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.Keyword }
                                       }),
                                   new(
                                   @"\b[0-9]{1,}\b",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.Number }
                                       }),
                                   /* WIP
                                   new LanguageRule(
                                       @"\b((?=<modifiers>public|protected|internal|private|abstract)?(?(?=<modifiers>) |[^]))[a-zA-Z][a-zA-Z0-9.]{1,})){1,}",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.ClassName }
                                       }), */
                           ];

        public bool HasAlias(string lang) => lang.ToLower() switch
        {
            "cs" or "c#" or "csharp" or "cake" => true,
            _ => false,
        };

        public override string ToString() => Name;
    }

    public class Css : ILanguage
    {
        public string Id => LanguageId.Css;

        public string Name => "CSS";

        public string CssClassName => "css";

        public string? FirstLinePattern => null;

        public IList<LanguageRule> Rules => [
                               new(
                                   @"(?msi)(?:(\s*/\*.*?\*/)|(([a-z0-9#. \[\]=\"":_-]+)\s*(?:,\s*|{))+(?:(\s*/\*.*?\*/)|(?:\s*([a-z0-9 -]+\s*):\s*([a-z0-9#,<>\?%. \(\)\\\/\*\{\}:'\""!_=-]+);?))*\s*})",
                                   new Dictionary<int, string?>
                                       {
                                           { 3, ScopeName.CssSelector },
                                           { 5, ScopeName.CssPropertyName },
                                           { 6, ScopeName.CssPropertyValue },
                                           { 4, ScopeName.Comment },
                                           { 1, ScopeName.Comment },
                                       }),
                           ];

        public bool HasAlias(string lang) => false;

        public override string ToString() => Name;
    }

    public class Fortran : ILanguage
    {
        public string Id => LanguageId.Fortran;

        public string Name => "Fortran";

        public string CssClassName => "fortran";

        public string? FirstLinePattern => null;

        public IList<LanguageRule> Rules => [
                               // Comments
                                new(
                                   @"!.*",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.Comment },
                                       }),
                                // String type 1
                                new(
                                   @""".*?""|'.*?'",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.String },
                                       }),
                                // Program keywords
                                new(
                                   @"(?i)\b(?:program|endprogram)\b",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.Keyword },
                                       }),
                                // Module keywords
                                new(
                                   @"(?i)\b(?:module|endmodule|contains)\b",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.Keyword },
                                       }),
                                // Type keywords
                                new(
                                   @"(?i)\b(?:type|endtype|abstract)\b",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.Keyword },
                                       }),
                                // Interface definition keywords
                                new(
                                   @"(?i)\b(?:interface|endinterface|operator|assignment)\b",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.Keyword },
                                       }),
                                // Procedure definition keywords
                                new(
                                   @"(?i)\b(?:function|endfunction|subroutine|endsubroutine|elemental|recursive|pure)\b",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.Keyword },
                                       }),
                               // Variable keywords
                                new(
                                   @"(?i)INTEGER|REAL|DOUBLE\s*PRECISION|COMPLEX|CHARACTER|LOGICAL|TYPE",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.Keyword },
                                       }),
                                // Attribute keywords
                                new(
                                   @"(?i)\b(?:parameter|allocatable|optional|pointer|save|dimension|target)\b",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.Keyword },
                                       }),
                                // Intent keywords
                                new(
                                   @"(?i)\b(intent)\s*(\()\s*(in|out|inout)\s*(\))",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.Keyword },
                                           { 2, ScopeName.Brackets },
                                           { 3, ScopeName.Keyword },
                                           { 4, ScopeName.Brackets },
                                       }),
                                // Namelist
                                new(
                                   @"(?i)\b(namelist)(/)\w+(/)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.Keyword },
                                           { 2, ScopeName.Brackets },
                                           { 3, ScopeName.Brackets },
                                       }),
                                // Intrinsic functions
                                new(
                                   @"(?i)\b(PRESENT" +
                                    "|INT|REAL|DBLE|CMPLX|AIMAG|CONJG|AINT|ANINT|NINT|ABS|MOD|SIGN|DIM|DPROD|MODULO|FLOOR|CEILING|MAX|MIN" +
                                    "|SQRT|EXP|LOG|LOG10|SIN|COS|TAN|ASIN|ACOS|ATAN|ATAN2|SINH|COSH|TANH" +
                                    "|ICHAR|CHAR|LGE|LGT|LLE|LLT|IACHAR|ACHAR|INDEX|VERIFY|ADJUSTL|ADJUSTR|SCAN|LEN_TRIM|REPEAT|TRIM" +
                                    "|KIND|SELECTED_INT_KIND|SELECTED_REAL_KIND" +
                                    "|LOGICAL" +
                                    "|IOR|IAND|NOT|IEOR| ISHFT|ISHFTC|BTEST|IBSET|IBCLR|BITSIZE" +
                                    "|TRANSFER" +
                                    "|RADIX|DIGITS|MINEXPONENT|MAXEXPONENT|PRECISION|RANGE|HUGE|TINY|EPSILON" +
                                    "|EXPONENT|SCALE|NEAREST|FRACTION|SET_EXPONENT|SPACING|RRSPACING" +
                                    "|LBOUND|SIZE|UBOUND" +
                                    "|MASK" +
                                    "|MATMUL|DOT_PRODUCT" +
                                    "|SUM|PRODUCT|MAXVAL|MINVAL|COUNT|ANY|ALL" +
                                    "|ALLOCATED|SIZE|SHAPE|LBOUND|UBOUND" +
                                    "|MERGE|SPREAD|PACK|UNPACK" +
                                    "|RESHAPE" +
                                    "|TRANSPOSE|EOSHIFT|CSHIFT" +
                                    "|MAXLOC|MINLOC" +
                                   @"|ASSOCIATED)\b(\()"
                                    ,
                                   new Dictionary<int, string?>
                                       {
                                           {1, ScopeName.Intrinsic },
                                           {2, ScopeName.Brackets },
                                       }),
                                // Intrinsic functions
                                new(
                                   @"(?i)(call)\s+(" +
                                   "DATE_AND_TIME|SYSTEM_CLOCK" +
                                   "|RANDOM_NUMBER|RANDOM_SEED" +
                                   "|MVBITS" +
                                   ")\b"
                                    ,
                                   new Dictionary<int, string?>
                                       {
                                           {1, ScopeName.Keyword },
                                           {2, ScopeName.Intrinsic },
                                       }),
                                // Special Character
                                new(
                                   @"\=|\*|\+|\\|\-|\.\w+\.|>|<|::|%|,|;|\?|\$",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.SpecialCharacter },
                                       }),
                                // Line Continuation
                                new(
                                   @"&",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.Continuation },
                                       }),
                                // Number
                                new(
                                   @"\b[0-9.]+(_\w+)?\b",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.Number },
                                       }),
                                // Brackets
                                new(
                                   @"[\(\)\[\]]",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.Brackets },
                                       }),
                                // Preprocessor keywords
                                new(
                                   @"(?i)(?:#define|#elif|#elifdef|#elifndef|#else|#endif|#error|#if|#ifdef|#ifndef|#include|#line|#pragma|#undef)\b",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.PreprocessorKeyword },
                                       }),
                                // General keywords
                                new(
                                   @"(?i)\b(?:end|use|do|enddo|select|endselect|case|endcase|if|then|else|endif|implicit|none"
                                        + @"|do\s+while|call|public|private|protected|where|go\s*to|file|block|data|blockdata"
                                        + @"|end\s*blockdata|default|procedure|include|go\s*to|allocate|deallocate|open|close|write|stop|return)\b",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.Keyword },
                                       })
                           ];

        public bool HasAlias(string lang) => lang.ToLower() switch
        {
            "fortran" => true,
            _ => false,
        };

        public override string ToString() => Name;
    }

    public class FSharp : ILanguage
    {
        public string Id => LanguageId.FSharp;

        public string Name => "F#";

        public string CssClassName => "FSharp";

        public string? FirstLinePattern => null;

        public IList<LanguageRule> Rules => [
                               new(
                                   @"\(\*([^*]|[\r\n]|(\*+([^*)]|[\r\n])))*\*+\)",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.Comment },
                                       }),
                               new(
                                   @"(///)(?:\s*?(<[/a-zA-Z0-9\s""=]+>))*([^\r\n]*)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.XmlDocTag },
                                           { 2, ScopeName.XmlDocTag },
                                           { 3, ScopeName.XmlDocComment },
                                       }),
                               new(
                                   @"(//.*?)\r?$",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.Comment }
                                       }),
                               new(
                                   @"'[^\n]*?(?<!\\)'",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.String }
                                       }),
                               new(
                                      @"(?s)@""(?:""""|.)*?""(?!"")",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.StringCSharpVerbatim }
                                       }),
                               new(
                                      @"(?s)""""""(?:""""|.)*?""""""(?!"")",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.StringCSharpVerbatim }
                                       }),
                               new(
                                   @"(?s)(""[^\n]*?(?<!\\)"")",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.String }
                                       }),
                               new(
                                   @"^\s*(\#else|\#endif|\#if).*?$",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.PreprocessorKeyword }
                                       }),
                               new(
                                   @"\b(let!|use!|do!|yield!|return!)\s",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.Keyword }
                                       }),
                               new(
                                   @"\b(abstract|and|as|assert|base|begin|class|default|delegate|do|done|downcast|downto|elif|else|end|exception|extern|false|finally|for|fun|function|global|if|in|inherit|inline|interface|internal|lazy|let|match|member|module|mutable|namespace|new|null|of|open|or|override|private|public|rec|return|sig|static|struct|then|to|true|try|type|upcast|use|val|void|when|while|with|yield|atomic|break|checked|component|const|constraint|constructor|continue|eager|fixed|fori|functor|include|measure|method|mixin|object|parallel|params|process|protected|pure|recursive|sealed|tailcall|trait|virtual|volatile|async|let!|use!|do!)\b",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.Keyword }
                                       }),
                               new(
                                   @"(\w|\s)(->)(\w|\s)",
                                   new Dictionary<int, string?>
                                       {
                                           { 2, ScopeName.Keyword }
                                       }),
                           ];

        public bool HasAlias(string lang) => lang.ToLower() switch
        {
            "fs" or "f#" or "fsharp" => true,
            _ => false,
        };

        public override string ToString() => Name;
    }

    public class Haskell : ILanguage
    {
        public string Id => LanguageId.Haskell;

        public string Name => "Haskell";

        public string CssClassName => "haskell";

        public string? FirstLinePattern => null;

        private const string nonnestComment = @"((?:--.*\r?\n|{-(?:[^-]|-(?!})|[\r\n])*-}))";

        private const string incomment = @"([^-{}]|{(?!-)|-(?!})|(?<!-)})*";
        private const string keywords = @"case|class|data|default|deriving|do|else|foreign|if|import|in|infix|infixl|infixr|instance|let|module|newtype|of|then|type|where";
        private const string opKeywords = @"\.\.|:|::|=|\\|\||<-|->|@|~|=>";

        private const string vsymbol = @"[\!\#\$%\&\⋆\+\./<=>\?@\\\^\-~\|]";
        private const string symbol = @"(?:" + vsymbol + "|:)";

        private const string varop = vsymbol + "(?:" + symbol + @")*";
        private const string conop = ":(?:" + symbol + @")*";

        private const string conid = @"(?:[A-Z][\w']*|\(" + conop + @"\))";
        private const string varid = @"(?:[a-z][\w']*|\(" + varop + @"\))";

        private const string qconid = @"((?:[A-Z][\w']*\.)*)" + conid;
        private const string qvarid = @"((?:[A-Z][\w']*\.)*)" + varid;
        private const string qconidop = @"((?:[A-Z][\w']*\.)*)(?:" + conid + "|" + conop + ")";

        private const string intype = @"(\bforall\b|=>)|" + qconidop + @"|(?!deriving|where|data|type|newtype|instance|class)([a-z][\w']*)|\->|[ \t!\#]|\r?\n[ \t]+(?=[\(\)\[\]]|->|=>|[A-Z])";
        private const string toptype = "(?:" + intype + "|::)";
        private const string nestedtype = @"(?:" + intype + ")";

        //private const string datatype = "(?:" + intype + @"|[,]|\r?\n[ \t]+|::|(?<!" + symbol + @"|^)([=\|])\s*(" + conid + ")|" + nonnestComment + ")";

        private const string inexports = @"(?:[\[\],\s]|(" + conid + ")|" + varid
                                          + "|" + nonnestComment
                                          + @"|\((?:[,\.\s]|(" + conid + ")|" + varid + @")*\)"
                                          + ")*";

        public IList<LanguageRule> Rules => [
                    // Nested block comments: note does not match no unclosed block comments.
                    new(
                        // Handle nested block comments using named balanced groups
                        @"{-+" + incomment +
                        @"(?>" +
                        @"(?>(?<comment>{-)" + incomment + ")+" +
                        @"(?>(?<-comment>-})" + incomment + ")+" +
                        @")*" +
                        @"(-+})",
                        new Dictionary<int, string?>
                            {
                                { 0, ScopeName.Comment },
                            }),
           
           
                    // Line comments
                    new(
                        @"(--.*?)\r?$",
                        new Dictionary<int, string?>
                            {
                                { 1, ScopeName.Comment }
                            }),    
        
                    // Types
                    new(
                        // Type highlighting using named balanced groups to balance parenthesized sub types
                        // 'toptype' and 'nestedtype' capture three groups: type keywords, namespaces, and type variables 
                        @"(?:" + @"\b(class|instance|deriving)\b"
                                + @"|::(?!" + symbol + ")"
                                + @"|\b(type)\s+" + toptype + @"*\s*(=)"
                                + @"|\b(data|newtype)\s+" + toptype + @"*\s*(=)\s*(" + conid + ")"
                                + @"|\s+(\|)\s*(" + conid + ")"
                          + ")" + toptype + "*" +
                        @"(?:" +
                            @"(?:(?<type>[\(\[<])(?:" + nestedtype + @"|[,]" + @")*)+" +
                            @"(?:(?<-type>[\)\]>])(?:" + nestedtype + @"|(?(type)[,])" + @")*)+" +
                        @")*",
                        new Dictionary<int,string?> {
                            { 0, ScopeName.Type },

                            { 1, ScopeName.Keyword },   // class instance etc

                            { 2, ScopeName.Keyword},        // type
                            { 3, ScopeName.Keyword},
                            { 4, ScopeName.NameSpace },
                            { 5, ScopeName.TypeVariable },
                            { 6, ScopeName.Keyword},

                            { 7, ScopeName.Keyword},        // data , newtype
                            { 8, ScopeName.Keyword},
                            { 9, ScopeName.NameSpace },
                            { 10, ScopeName.TypeVariable },
                            { 11, ScopeName.Keyword },       // = conid
                            { 12, ScopeName.Constructor },

                            { 13, ScopeName.Keyword },       // | conid
                            { 14, ScopeName.Constructor },

                            { 15, ScopeName.Keyword},
                            { 16, ScopeName.NameSpace },
                            { 17, ScopeName.TypeVariable },

                            { 18, ScopeName.Keyword },
                            { 19, ScopeName.NameSpace },
                            { 20, ScopeName.TypeVariable },

                            { 21, ScopeName.Keyword },
                            { 22, ScopeName.NameSpace },
                            { 23, ScopeName.TypeVariable },
                        }),

                        
                    // Special sequences
                    new(
                        @"\b(module)\s+(" + qconid + @")(?:\s*\(" + inexports + @"\))?",
                        new Dictionary<int, string?>
                            {
                                { 1, ScopeName.Keyword },
                                { 2, ScopeName.NameSpace },
                                { 4, ScopeName.Type },
                                { 5, ScopeName.Comment },
                                { 6, ScopeName.Constructor }
                            }),
                    new(
                        @"\b(module|as)\s+(" + qconid + ")",
                        new Dictionary<int, string?>
                            {
                                { 1, ScopeName.Keyword },
                                { 2, ScopeName.NameSpace }
                            }),

                    new(
                        @"\b(import)\s+(qualified\s+)?(" + qconid + @")\s*"
                            + @"(?:\(" + inexports + @"\))?"
                            + @"(?:(hiding)(?:\s*\(" + inexports + @"\)))?",
                        new Dictionary<int, string?>
                            {
                                { 1, ScopeName.Keyword },
                                { 2, ScopeName.Keyword },
                                { 3, ScopeName.NameSpace },
                                { 5, ScopeName.Type },
                                { 6, ScopeName.Comment },
                                { 7, ScopeName.Constructor },
                                { 8, ScopeName.Keyword},
                                { 9, ScopeName.Type },
                                { 10, ScopeName.Comment },
                                { 11, ScopeName.Constructor }
                            }),
    
                    // Keywords
                    new(
                        @"\b(" + keywords + @")\b",
                        new Dictionary<int, string?>
                            {
                                { 1, ScopeName.Keyword },
                            }),
                    new(
                        @"(?<!" + symbol +")(" + opKeywords + ")(?!" + symbol + ")",
                        new Dictionary<int, string?>
                            {
                                { 1, ScopeName.Keyword },
                            }),
                                   
                    // Names
                    new(
                        qvarid,
                        new Dictionary<int, string?>
                            {
                                { 1, ScopeName.NameSpace }
                            }),
                    new(
                        qconid,
                        new Dictionary<int, string?>
                            {
                                { 0, ScopeName.Constructor },
                                { 1, ScopeName.NameSpace },
                            }),

                    // Operators and punctuation
                    new(
                        varop,
                        new Dictionary<int, string?>
                            {
                                { 0, ScopeName.Operator }
                            }),
                    new(
                        conop,
                        new Dictionary<int, string?>
                            {
                                { 0, ScopeName.Constructor }
                            }),

                    new(
                        @"[{}\(\)\[\];,]",
                        new Dictionary<int, string?>
                            {
                                { 0, ScopeName.Delimiter }
                            }),

                    // Literals
                    new(
                        @"0[xX][\da-fA-F]+|\d+(\.\d+([eE][\-+]?\d+)?)?",
                        new Dictionary<int, string?>
                            {
                                { 0, ScopeName.Number }
                            }),


                    new(
                        @"'[^\n]*?'",
                        new Dictionary<int, string?>
                            {
                                { 0, ScopeName.String },
                            }),
                    new(
                        @"""[^\n]*?""",
                        new Dictionary<int, string?>
                            {
                                { 0, ScopeName.String },
                            }),
                ];

        public bool HasAlias(string lang) => lang.ToLower() switch
        {
            "hs" => true,
            _ => false,
        };

        public override string ToString() => Name;
    }

    public class Html : ILanguage
    {
        public string Id => LanguageId.Html;

        public string Name => "HTML";

        public string CssClassName => "html";

        public string? FirstLinePattern => null;

        public IList<LanguageRule> Rules => [
                               new(
                                   @"\<![ \r\n\t]*(--([^\-]|[\r\n]|-[^\-])*--[ \r\n\t]*)\>",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.HtmlComment },
                                       }),
                               new(
                                   @"(?i)(<!)(DOCTYPE)(?:\s+([a-zA-Z0-9]+))*(?:\s+(""[^""]*?""))*(>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.HtmlTagDelimiter },
                                           { 2, ScopeName.HtmlElementName },
                                           { 3, ScopeName.HtmlAttributeName },
                                           { 4, ScopeName.HtmlAttributeValue },
                                           { 5, ScopeName.HtmlTagDelimiter }
                                       }),
                               new(
                                   @"(?xis)(<)
                                          (script)
                                          (?:
                                             [\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*([^\s\n""']+?)
                                            |[\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*(""[^\n]+?"")
                                            |[\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*('[^\n]+?')
                                            |[\s\n]+([a-z0-9-_]+) )*
                                          [\s\n]*
                                          (>)
                                          (.*?)
                                          (</)(script)(>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.HtmlTagDelimiter },
                                           { 2, ScopeName.HtmlElementName },
                                           { 3, ScopeName.HtmlAttributeName },
                                           { 4, ScopeName.HtmlOperator },
                                           { 5, ScopeName.HtmlAttributeValue },
                                           { 6, ScopeName.HtmlAttributeName },
                                           { 7, ScopeName.HtmlOperator },
                                           { 8, ScopeName.HtmlAttributeValue },
                                           { 9, ScopeName.HtmlAttributeName },
                                           { 10, ScopeName.HtmlOperator },
                                           { 11, ScopeName.HtmlAttributeValue },
                                           { 12, ScopeName.HtmlAttributeName },
                                           { 13, ScopeName.HtmlTagDelimiter },
                                           { 14, string.Format("{0}{1}", ScopeName.LanguagePrefix, LanguageId.JavaScript) },
                                           { 15, ScopeName.HtmlTagDelimiter },
                                           { 16, ScopeName.HtmlElementName },
                                           { 17, ScopeName.HtmlTagDelimiter },
                                       }),
                               new(
                                   @"(?xis)(</?)
                                          (?: ([a-z][a-z0-9-]*)(:) )*
                                          ([a-z][a-z0-9-_]*)
                                          (?:
                                             [\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*([^\s\n""']+?)
                                            |[\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*(""[^\n]+?"")
                                            |[\s\n]+([a-z0-9-_]+)[\s\n]*(=)[\s\n]*('[^\n]+?')
                                            |[\s\n]+([a-z0-9-_]+) )*
                                          [\s\n]*
                                          (/?>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.HtmlTagDelimiter },
                                           { 2, ScopeName.HtmlElementName },
                                           { 3, ScopeName.HtmlTagDelimiter },
                                           { 4, ScopeName.HtmlElementName },
                                           { 5, ScopeName.HtmlAttributeName },
                                           { 6, ScopeName.HtmlOperator },
                                           { 7, ScopeName.HtmlAttributeValue },
                                           { 8, ScopeName.HtmlAttributeName },
                                           { 9, ScopeName.HtmlOperator },
                                           { 10, ScopeName.HtmlAttributeValue },
                                           { 11, ScopeName.HtmlAttributeName },
                                           { 12, ScopeName.HtmlOperator },
                                           { 13, ScopeName.HtmlAttributeValue },
                                           { 14, ScopeName.HtmlAttributeName },
                                           { 15, ScopeName.HtmlTagDelimiter }
                                       }),
                               new(
                                   @"(?i)&\#?[a-z0-9]+?;",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.HtmlEntity }
                                       }),
                           ];

        public bool HasAlias(string lang) => lang.ToLower() switch
        {
            "htm" => true,
            _ => false,
        };

        public override string ToString() => Name;
    }

    public class Java : ILanguage
    {
        public string Id => LanguageId.Java;

        public string Name => "Java";

        public string CssClassName => "java";

        public string? FirstLinePattern => null;

        public IList<LanguageRule> Rules => [
                               new(
                                   @"/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.Comment },
                                       }),
                               new(
                                   @"(//.*?)\r?$",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.Comment }
                                       }),
                               new(
                                   @"'[^\n]*?(?<!\\)'",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.String }
                                       }),
                               new(
                                   @"(?s)(""[^\n]*?(?<!\\)"")",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.String }
                                       }),
                               new(
                                   @"\b(abstract|assert|boolean|break|byte|case|catch|char|class|const|continue|default|do|double|else|enum|extends|false|final|finally|float|for|goto|if|implements|import|instanceof|int|interface|long|native|new|null|package|private|protected|public|return|short|static|strictfp|super|switch|synchronized|this|throw|throws|transient|true|try|void|volatile|while)\b",
                                   new Dictionary<int, string?>
                                       {
                                           {0, ScopeName.Keyword},
                                       }),
                               new(
                                   @"\b[0-9]{1,}\b",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.Number }
                                       }),
                           ];

        public bool HasAlias(string lang) => false;

        public override string ToString() => Name;
    }

    public class JavaScript : ILanguage
    {
        public string Id => LanguageId.JavaScript;

        public string Name => "JavaScript";

        public string CssClassName => "javascript";

        public string? FirstLinePattern => null;

        public IList<LanguageRule> Rules => [
                               new(
                                   @"/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.Comment },
                                       }),
                               new(
                                   @"(//.*?)\r?$",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.Comment },
                                       }),
                               new(
                                   @"'[^\n]*?'",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.String },
                                       }),
                               new(
                                   @"""[^\n]*?""",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.String },
                                       }),
                               new(
                                   @"\b(abstract|boolean|break|byte|case|catch|char|class|const|continue|debugger|default|delete|do|double|else|enum|export|extends|false|final|finally|float|for|function|goto|if|implements|import|in|instanceof|int|interface|long|native|new|null|package|private|protected|public|return|short|static|super|switch|synchronized|this|throw|throws|transient|true|try|typeof|var|void|volatile|while|with)\b",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.Keyword },
                                       }),
                           ];

        public bool HasAlias(string lang) => lang.ToLower() switch
        {
            "js" => true,
            "json" => true,
            _ => false,
        };

        public override string ToString() => Name;
    }

    public class Koka : ILanguage
    {
        public string Id => LanguageId.Koka;

        public string Name => "Koka";

        public string CssClassName => "koka";

        public string? FirstLinePattern => null;

        private const string incomment = @"([^*/]|/(?!\*)|\*(?!/))*";

        private const string plainKeywords = @"infix|infixr|infixl|type|cotype|rectype|struct|alias|interface|instance|external|fun|function|val|var|con|module|import|as|public|private|abstract|yield";
        private const string controlKeywords = @"if|then|else|elif|match|return";
        private const string typeKeywords = @"forall|exists|some|with";
        private const string pseudoKeywords = @"include|inline|cs|js|file";
        private const string opkeywords = @"[=\.\|]|\->|\:=";

        private const string intype = @"\b(" + typeKeywords + @")\b|(?:[a-z]\w*/)*[a-z]\w+\b|(?:[a-z]\w*/)*[A-Z]\w*\b|([a-z][0-9]*\b|_\w*\b)|\->|[\s\|]";
        private const string toptype = "(?:" + intype + "|::)";
        private const string nestedtype = @"(?:([a-z]\w*)\s*[:]|" + intype + ")";

        private const string symbol = @"[$%&\*\+@!\\\^~=\.:\-\?\|<>/]";
        private const string symbols = @"(?:" + symbol + @")+";

        private const string escape = @"\\(?:[nrt\\""']|x[\da-fA-F]{2}|u[\da-fA-F]{4}|U[\da-fA-F]{6})";

        public IList<LanguageRule> Rules => [
                    // Nested block comments. note: does not match on unclosed comments 
                    new(
                      // Handle nested block comments using named balanced groups
                      @"/\*" + incomment +
                      @"(" +
                       @"((?<comment>/\*)" + incomment + ")+" +
                       @"((?<-comment>\*/)" + incomment + ")+" +
                      @")*" +
                      @"(\*+/)",
                      new Dictionary<int, string?>
                          {
                              { 0, ScopeName.Comment },
                          }),
           
                   // Line comments
                   new(
                        @"(//.*?)\r?$",
                        new Dictionary<int, string?>
                            {
                                { 1, ScopeName.Comment }
                            }),            
            
                    // operator keywords
                    new(
                        @"(?<!" + symbol + ")(" + opkeywords + @")(?!" + symbol + @")",
                        new Dictionary<int, string?>
                            {
                                { 1, ScopeName.Keyword }
                            }),
            
                    // Types
                    new(
                        // Type highlighting using named balanced groups to balance parenthesized sub types
                        // 'toptype' captures two groups: type keyword and type variables 
                        // each 'nestedtype' captures three groups: parameter names, type keywords and type variables
                        @"(?:" + @"\b(type|struct|cotype|rectype)\b|"
                               + @"::?(?!" + symbol + ")|"
                               + @"\b(alias)\s+[a-z]\w+\s*(?:<[^>]*>\s*)?(=)" + ")"
                               + toptype + "*" +
                        @"(?:" +
                         @"(?:(?<type>[\(\[<])(?:" + nestedtype + @"|[,]" + @")*)+" +
                         @"(?:(?<-type>[\)\]>])(?:" + nestedtype + @"|(?(type)[,])" + @")*)+" +
                        @")*" +
                        @"", //(?=(?:[,\)\{\}\]<>]|(" + keywords +")\b))",
                        new Dictionary<int,string?> {
                            { 0, ScopeName.Type },

                            { 1, ScopeName.Keyword },   // type struct etc
                            { 2, ScopeName.Keyword },   // alias
                            { 3, ScopeName.Keyword },   //  =
                    
                            { 4, ScopeName.Keyword},
                            { 5, ScopeName.TypeVariable },

                            { 6, ScopeName.PlainText },
                            { 7, ScopeName.Keyword },
                            { 8, ScopeName.TypeVariable },

                            { 9, ScopeName.PlainText },
                            { 10, ScopeName.Keyword },
                            { 11, ScopeName.TypeVariable },
                        }),

                    // module and imports
                    new(
                        @"\b(module)\s+(interface\s+)?((?:[a-z]\w*/)*[a-z]\w*)",
                        new Dictionary<int, string?>
                            {
                                { 1, ScopeName.Keyword },
                                { 2, ScopeName.Keyword },
                                { 3, ScopeName.NameSpace },
                            }),

                    new(
                        @"\b(import)\s+((?:[a-z]\w*/)*[a-z]\w*)\s*(?:(=)\s*(qualified\s+)?((?:[a-z]\w*/)*[a-z]\w*))?",
                        new Dictionary<int, string?>
                            {
                                { 1, ScopeName.Keyword },
                                { 2, ScopeName.NameSpace },
                                { 3, ScopeName.Keyword },
                                { 4, ScopeName.Keyword },
                                { 5, ScopeName.NameSpace },
                            }),
            
                    // keywords
                    new(
                        @"\b(" + plainKeywords + "|" + typeKeywords + @")\b",
                        new Dictionary<int, string?>
                            {
                                { 1, ScopeName.Keyword }
                            }),
                    new(
                        @"\b(" + controlKeywords + @")\b",
                        new Dictionary<int, string?>
                            {
                                { 1, ScopeName.ControlKeyword }
                            }),
                    new(
                        @"\b(" + pseudoKeywords + @")\b",
                        new Dictionary<int, string?>
                            {
                                { 1, ScopeName.PseudoKeyword }
                            }),
            
                    // Names
                    new(
                        @"([a-z]\w*/)*([a-z]\w*|\(" + symbols + @"\))",
                        new Dictionary<int, string?>
                            {
                                { 1, ScopeName.NameSpace }
                            }),
                    new(
                        @"([a-z]\w*/)*([A-Z]\w*)",
                        new Dictionary<int, string?>
                            {
                                { 1, ScopeName.NameSpace },
                                { 2, ScopeName.Constructor }
                            }),

                    // Operators and punctuation
                    new(
                        symbols,
                        new Dictionary<int, string?>
                            {
                                { 0, ScopeName.Operator }
                            }),
                    new(
                        @"[{}\(\)\[\];,]",
                        new Dictionary<int, string?>
                            {
                                { 0, ScopeName.Delimiter }
                            }),

                    // Literals
                    new(
                        @"0[xX][\da-fA-F]+|\d+(\.\d+([eE][\-+]?\d+)?)?",
                        new Dictionary<int, string?>
                            {
                                { 0, ScopeName.Number }
                            }),

                    new(
                        @"(?s)'(?:[^\t\n\\']+|(" + escape + @")|\\)*'",
                        new Dictionary<int, string?>
                            {
                                { 0, ScopeName.String },
                                { 1, ScopeName.StringEscape },
                            }),
                    new(
                        @"(?s)@""(?:("""")|[^""]+)*""(?!"")",
                        new Dictionary<int, string?>
                            {
                                { 0, ScopeName.StringCSharpVerbatim },
                                { 1, ScopeName.StringEscape }
                            }),
                    new(
                                @"(?s)""(?:[^\t\n\\""]+|(" + escape + @")|\\)*""",
                                new Dictionary<int, string?>
                                    {
                                        { 0, ScopeName.String },
                                        { 1, ScopeName.StringEscape }
                                    }),
                            new(
                                @"^\s*(\#error|\#line|\#pragma|\#warning|\#!/usr/bin/env\s+koka|\#).*?$",
                                new Dictionary<int, string?>
                                    {
                                        { 1, ScopeName.PreprocessorKeyword }
                                    }),
                 ];

        public bool HasAlias(string lang) => lang.ToLower() switch
        {
            "kk" or "kki" => true,
            _ => false,
        };

        public override string ToString() => Name;
    }

    public class Markdown : ILanguage
    {
        public string Id => LanguageId.Markdown;
        public string Name => "Markdown";
        public string CssClassName => "markdown";
        public string? FirstLinePattern => null;

        private static string Link(string content = @"([^\]\n]+)") => @"\!?\[" + content + @"\][ \t]*(\([^\)\n]*\)|\[[^\]\n]*\])";

        public IList<LanguageRule> Rules => [
                               // Heading
                               new(
                                   @"^(\#.*)\r?|^.*\r?\n([-]+|[=]+)\r?$",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.MarkdownHeader },
                                       }),


                               // code block
                               new(
                                   @"^([ ]{4}(?![ ])(?:.|\r?\n[ ]{4})*)|^(```+[ \t]*\w*)((?:[ \t\r\n]|.)*?)(^```+)[ \t]*\r?$",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.MarkdownCode },
                                           { 2, ScopeName.XmlDocTag },
                                           { 3, ScopeName.MarkdownCode },
                                           { 4, ScopeName.XmlDocTag },
                                       }),

                               // Line
                               new(
                                   @"^\s*((-\s*){3}|(\*\s*){3}|(=\s*){3})[ \t\-\*=]*\r?$",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.MarkdownHeader },
                                       }),

                               
                               // List
                               new(
                                   @"^[ \t]*([\*\+\-]|\d+\.)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.MarkdownListItem },
                                       }),

                               // escape
                               new(
                                   @"\\[\\`\*_{}\[\]\(\)\#\+\-\.\!]",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.StringEscape },
                                       }),

                               // link
                               new(
                                   Link(Link()) + "|" + Link(),  // support nested links (mostly for images)
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.MarkdownBold },
                                           { 2, ScopeName.XmlDocTag },
                                           { 3, ScopeName.XmlDocTag },
                                           { 4, ScopeName.MarkdownBold },
                                           { 5, ScopeName.XmlDocTag },
                                       }),
                               new(
                                   @"^[ ]{0,3}\[[^\]\n]+\]:(.|\r|\n[ \t]+(?![\r\n]))*$",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.XmlDocTag },    // nice gray
                                       }),

                               // bold
                               new(
                                   @"\*(?!\*)([^\*\n]|\*\w)+?\*(?!\w)|\*\*([^\*\n]|\*(?!\*))+?\*\*",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.MarkdownBold },
                                       }),

                               // emphasized 
                               new(
                                   @"_([^_\n]|_\w)+?_(?!\w)|__([^_\n]|_(?=[\w_]))+?__(?!\w)",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.MarkdownEmph },
                                       }),
                               
                               // inline code
                               new(
                                   @"`[^`\n]+?`|``([^`\n]|`(?!`))+?``",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.MarkdownCode },
                                       }),

                               // strings
                               new(
                                   @"""[^""\n]+?""|'[\w\-_]+'",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.String },
                                       }),

                               // html tag
                               new(
                                   @"</?\w.*?>",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.HtmlTagDelimiter },
                                       }),

                               // html entity
                               new(
                                   @"\&\#?\w+?;",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.HtmlEntity },
                                       }),
                           ];

        public bool HasAlias(string lang) => lang.ToLower() switch
        {
            "md" or "markdown" => true,
            _ => false,
        };

        public override string ToString() => Name;
    }

    public class Php : ILanguage
    {
        public string Id => LanguageId.Php;

        public string Name => "PHP";

        public string CssClassName => "php";

        public string? FirstLinePattern => null;

        public IList<LanguageRule> Rules => [
                               new(
                                   @"/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.Comment },
                                       }),
                               new(
                                   @"(//.*?)\r?$",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.Comment },
                                       }),
                               new(
                                   @"(#.*?)\r?$",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.Comment },
                                       }),
                               new(
                                   @"'[^\n]*?(?<!\\)'",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.String },
                                       }),
                               new(
                                   @"""[^\n]*?(?<!\\)""",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.String },
                                       }),
                               new(
                                   // from http://us.php.net/manual/en/reserved.keywords.php
                                   @"\b(abstract|and|array|as|break|case|catch|cfunction|class|clone|const|continue|declare|default|do|else|elseif|enddeclare|endfor|endforeach|endif|endswitch|endwhile|exception|extends|fclose|file|final|for|foreach|function|global|goto|if|implements|interface|instanceof|mysqli_fetch_object|namespace|new|old_function|or|php_user_filter|private|protected|public|static|switch|throw|try|use|var|while|xor|__CLASS__|__DIR__|__FILE__|__FUNCTION__|__LINE__|__METHOD__|__NAMESPACE__|die|echo|empty|exit|eval|include|include_once|isset|list|require|require_once|return|print|unset)\b",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.Keyword },
                                       }),
                           ];

        public bool HasAlias(string lang) => lang.ToLower() switch
        {
            "php3" or "php4" or "php5" => true,
            _ => false,
        };

        public override string ToString() => Name;
    }

    public class PowerShell : ILanguage
    {
        public string Id => LanguageId.PowerShell;

        public string Name => "PowerShell";

        public string CssClassName => "powershell";

        public string? FirstLinePattern => null;

        public IList<LanguageRule> Rules => [
                               new(
                                   @"(?s)(<\#.*?\#>)",
                                   new Dictionary<int, string?>
                                       {
                                           {1, ScopeName.Comment}
                                       }),
                               new(
                                   @"(\#.*?)\r?$",
                                   new Dictionary<int, string?>
                                       {
                                           {1, ScopeName.Comment}
                                       }),
                               new(
                                   @"'[^\n]*?(?<!\\)'",
                                   new Dictionary<int, string?>
                                       {
                                           {0, ScopeName.String}
                                       }),
                               new(
                                   @"(?s)@"".*?""@",
                                   new Dictionary<int, string?>
                                       {
                                           {0, ScopeName.StringCSharpVerbatim}
                                       }),
                               new(
                                   @"(?s)(""[^\n]*?(?<!`)"")",
                                   new Dictionary<int, string?>
                                       {
                                           {0, ScopeName.String}
                                       }),
                               new(
                                   @"\$(?:[\d\w\-]+(?::[\d\w\-]+)?|\$|\?|\^)",
                                   new Dictionary<int, string?>
                                       {
                                           {0, ScopeName.PowerShellVariable}
                                       }),
                               new(
                                   @"\${[^}]+}",
                                   new Dictionary<int, string?>
                                       {
                                           {0, ScopeName.PowerShellVariable}
                                       }),
                               new(
                                   @"\b(begin|break|catch|continue|data|do|dynamicparam|elseif|else|end|exit|filter|finally|foreach|for|from|function|if|in|param|process|return|switch|throw|trap|try|until|while)\b",
                                   new Dictionary<int, string?>
                                       {
                                           {1, ScopeName.Keyword}
                                       }),
                               new(
                                   @"-(?:c|i)?(?:eq|ne|gt|ge|lt|le|notlike|like|notmatch|match|notcontains|contains|replace)",
                                   new Dictionary<int, string?>
                                       {
                                           {0, ScopeName.PowerShellOperator}
                                       }
                                   ),
                               new(
                                   @"-(?:band|and|as|join|not|bxor|xor|bor|or|isnot|is|split)",
                                   new Dictionary<int, string?>
                                       {
                                           {0, ScopeName.PowerShellOperator}
                                       }
                                   ),
                               new(
                                   @"(?:\+=|-=|\*=|/=|%=|=|\+\+|--|\+|-|\*|/|%)",
                                   new Dictionary<int, string?>
                                       {
                                           {0, ScopeName.PowerShellOperator}
                                       }
                                   ),
                               new(
                                   @"(?:\>\>|2\>&1|\>|2\>\>|2\>)",
                                   new Dictionary<int, string?>
                                       {
                                           {0, ScopeName.PowerShellOperator}
                                       }
                                   ),
                               new(
                                   @"(?s)\[(CmdletBinding)[^\]]+\]",
                                   new Dictionary<int, string?>
                                       {
                                           {1, ScopeName.PowerShellAttribute}
                                       }),
                               new(
                                   @"(\[)([^\]]+)(\])(::)?",
                                   new Dictionary<int, string?>
                                       {
                                           {1, ScopeName.PowerShellOperator},
                                           {2, ScopeName.PowerShellType},
                                           {3, ScopeName.PowerShellOperator},
                                           {4, ScopeName.PowerShellOperator}
                                       })
                           ];

        public bool HasAlias(string lang) => lang.ToLower() switch
        {
            "posh" or "ps1" => true,
            _ => false,
        };
    }

    public class Sql : ILanguage
    {
        public string Id => LanguageId.Sql;

        public string Name => "SQL";

        public string CssClassName => "sql";

        public string? FirstLinePattern => null;

        public IList<LanguageRule> Rules => [
                               new(
                                   @"(?s)/\*.*?\*/",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.Comment },
                                       }),
                               new(
                                   @"(--.*?)\r?$",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.Comment },
                                       }),
                               new(
                                   @"'(?>[^\']*)'",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.String },
                                       }),
                               new(
                                   @"""(?>[^\""]*)""",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.String },
                                       }),
                               new(
                                   @"\[(?>[^\]]*)]",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, null },
                                       }),
                               new(
                                   @"(?i)\b(?<![.@""])(ADA|ADD|AFTER|ALL|ALTER|AND|ANY|AS|ASC|AT|AUTHORIZATION|AUTO|BACKUP|BEGIN|BETWEEN|BINARY|BIT|BIT_LENGTH|BREAK|BROWSE|BULK|BY|CASCADE|CASE|CHAR|CHARACTER|CHARACTER_LENGTH|CHAR_LENGTH|CHECK|CHECKPOINT|CHECKSUM_AGG|CLOSE|CLUSTERED|COLLATE|COL_LENGTH|COL_NAME|COLUMN|COLUMNPROPERTY|COMMIT|COMPUTE|CONNECT|CONNECTION|CONSTRAINT|CONTAINS|CONTAINSTABLE|CONTINUE|CREATE|CROSS|CUBE|CURRENT|CURRENT_DATE|CURRENT_TIME|CURSOR|DATABASE|DATABASEPROPERTY|DATE|DATETIME|DBCC|DEALLOCATE|DEC|DECIMAL|DECLARE|DEFAULT|DEFERRED|DELETE|DENY|DESC|DISK|DISTINCT|DISTRIBUTED|DOUBLE|DROP|DUMMY|DUMP|ELSE|ENCRYPTION|END-EXEC|END|ERRLVL|ESCAPE|EXCEPT|EXEC|EXECUTE|EXISTS|EXIT|EXTERNAL|EXTRACT|FALSE|FETCH|FILE|FILE_ID|FILE_NAME|FILEGROUP_ID|FILEGROUP_NAME|FILEGROUPPROPERTY|FILEPROPERTY|FILLFACTOR|FIRST|FLOAT|FOR|FOREIGN|FORTRAN|FREE|FREETEXT|FREETEXTTABLE|FROM|FULL|FULLTEXTCATALOGPROPERTY|FULLTEXTSERVICEPROPERTY|FUNCTION|GLOBAL|GOTO|GRANT|GROUP|HAVING|HOLDLOCK|HOUR|IDENTITY|IDENTITYCOL|IDENTITY_INSERT|IF|IGNORE|IMAGE|IMMEDIATE|IN|INCLUDE|INDEX|INDEX_COL|INDEXKEY_PROPERTY|INDEXPROPERTY|INNER|INSENSITIVE|INSERT|INSTEAD|INT|INTEGER|INTERSECT|INTO|IS|ISOLATION|JOIN|KEY|KILL|LANGUAGE|LAST|LEFT|LEVEL|LIKE|LINENO|LOAD|LOCAL|MINUTE|MODIFY|MONEY|NAME|NATIONAL|NCHAR|NEXT|NOCHECK|NONCLUSTERED|NOCOUNT|NONE|NOT|NULL|NUMERIC|NVARCHAR|OBJECT_ID|OBJECT_NAME|OBJECTPROPERTY|OCTET_LENGTH|OF|OFF|OFFSETS|ON|ONLY|OPEN|OPENDATASOURCE|OPENQUERY|OPENROWSET|OPENXML|OPTION|OR|ORDER|OUTER|OUTPUT|OVER|OVERLAPS|PARTIAL|PASCAL|PERCENT|PLAN|POSITION|PRECISION|PREPARE|PRIMARY|PRINT|PRIOR|PRIVILEGES|PROC|PROCEDURE|PUBLIC|RAISERROR|READ|READTEXT|REAL|RECONFIGURE|REFERENCES|REPLICATION|RESTORE|RESTRICT|RETURN|RETURNS|REVERT|REVOKE|RIGHT|ROLLBACK|ROLLUP|ROWCOUNT|ROWGUIDCOL|ROWS|RULE|SAVE|SCHEMA|SCROLL|SECOND|SECTION|SELECT|SEQUENCE|SET|SETUSER|SHUTDOWN|SIZE|SMALLINT|SMALLMONEY|SOME|SQLCA|SQLERROR|SQUARE|STATISTICS|TABLE|TEMPORARY|TEXT|TEXTPTR|TEXTSIZE|TEXTVALID|THEN|TIME|TIMESTAMP|TO|TOP|TRAN|TRANSACTION|TRANSLATION|TRIGGER|TRUE|TRUNCATE|TSEQUAL|TYPEPROPERTY|UNION|UNIQUE|UPDATE|UPDATETEXT|USE|VALUES|VARCHAR|VARYING|VIEW|WAITFOR|WHEN|WHERE|WHILE|WITH|WORK|WRITETEXT|IS_MEMBER|IS_SRVROLEMEMBER|PERMISSIONS|SUSER_SID|SUSER_SNAME|SYSNAME|UNIQUEIDENTIFIER|USER_ID|VARBINARY|ABSOLUTE|DATEPART|DATEDIFF|WEEK|WEEKDAY|MILLISECOND|GETUTCDATE|DATENAME|DATEADD|BIGINT|TINYINT|SMALLDATETIME|NTEXT|XML|ASSEMBLY|AGGREGATE|TYPE)\b",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.Keyword },
                                       }),
                               new(
                                   @"(?i)\b(?<![.@""])(ABS|ACOS|APP_NAME|ASCII|ASIN|ATAN|ATN2|AVG|BINARY_CHECKSUM|CAST|CEILING|CHARINDEX|CHECKSUM|CONVERT|COALESCE|COLLATIONPROPERTY|COLUMNS_UPDATED|COUNT|COS|COT|COUNT_BIG|CURRENT_TIMESTAMP|CURRENT_USER|CURSOR_STATUS|DATALENGTH|DAY|DB_ID|DB_NAME|DEGREES|DIFFERENCE|ERROR_LINE|ERROR_MESSAGE|ERROR_NUMBER|ERROR_PROCEDURE|ERROR_SEVERITY|ERROR_STATE|EXP|FLOOR|FORMATMESSAGE|GETANSINULL|GETDATE|GROUPING|HOST_ID|HOST_NAME|IDENT_CURRENT|IDENT_INCR|IDENT_SEED|ISDATE|ISNULL|ISNUMERIC|LEN|LOG|LOG10|LOWER|LTRIM|MAX|MIN|MONTH|NEWID|NULLIF|PARSENAME|PATINDEX|PI|POWER|ORIGINAL_LOGIN|QUOTENAME|UNICODE|ROW_NUMBER|RADIANS|RAND|ROUND|RTRIM|REPLACE|REPLICATE|REVERSE|SCOPE_IDENTITY|SOUNDEX|STR|STUFF|SERVERPROPERTY|SESSIONPROPERTY|SESSION_USER|SIGN|SIN|SPACE|STATS_DATE|STDEV|STDEVP|SQRT|SUBSTRING|SUM|SUSER_NAME|SQL_VARIANT|SQL_VARIANT_PROPERTY|SYSTEM_USER|TAN|UPPER|USER|USER_NAME|VAR|VARP|XACT_STATE|YEAR)\b",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.SqlSystemFunction }
                                       }
                                   ),
                               new(
                                   @"(?i)\B(@@(?:ERROR|IDENTITY|ROWCOUNT|TRANCOUNT|FETCH_STATUS))\b",
                                   new Dictionary<int, string?>
                                       {
                                           {1, ScopeName.SqlSystemFunction}
                                       }
                                   ),
                           ];

        public bool HasAlias(string lang) => false;

        public override string ToString() => Name;
    }

    public class Typescript : ILanguage
    {
        public string Id => LanguageId.TypeScript;

        public string Name => "Typescript";

        public string CssClassName => "typescript";

        public string? FirstLinePattern => null;

        public IList<LanguageRule> Rules => [
                               new(
                                   @"/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.Comment },
                                       }),
                               new(
                                   @"(//.*?)\r?$",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.Comment },
                                       }),
                               new(
                                   @"'[^\n]*?'",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.String },
                                       }),
                               new(
                                   @"""[^\n]*?""",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.String },
                                       }),
                               new(
                                   @"\b(abstract|any|bool|boolean|break|byte|case|catch|char|class|const|constructor|continue|debugger|declare|default|delete|do|double|else|enum|export|extends|false|final|finally|float|for|function|goto|if|implements|import|in|instanceof|int|interface|long|module|native|new|number|null|package|private|protected|public|return|short|static|string|super|switch|synchronized|this|throw|throws|transient|true|try|typeof|var|void|volatile|while|with)\b",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.Keyword },
                                       }),
                           ];

        public bool HasAlias(string lang) => lang.ToLower() switch
        {
            "ts" => true,
            _ => false,
        };

        public override string ToString() => Name;
    }

    public class VbDotNet : ILanguage
    {
        public string Id => LanguageId.VbDotNet;

        public string Name => "VB.NET";

        public string CssClassName => "vb-net";

        public string? FirstLinePattern => null;

        public IList<LanguageRule> Rules => [
                               new(
                                   @"('''[^\n]*?)\r?$",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.Comment },
                                       }),
                               new(
                                   @"((?:'|REM\s+).*?)\r?$",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.Comment },
                                       }),
                               new(
                                   @"""(?:""""|[^""\n])*""",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.String },
                                       }),
                               new(
                                   @"(?:\s|^)(\#End\sRegion|\#Region|\#Const|\#End\sExternalSource|\#ExternalSource|\#If|\#Else|\#End\sIf)(?:\s|\(|\r?$)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.PreprocessorKeyword },
                                       }),
                               new(
                                   @"(?i)\b(AddHandler|AddressOf|Aggregate|Alias|All|And|AndAlso|Ansi|Any|As|Ascending|(?<!<)Assembly|Auto|Average|Boolean|By|ByRef|Byte|ByVal|Call|Case|Catch|CBool|CByte|CChar|CDate|CDec|CDbl|Char|CInt|Class|CLng|CObj|Const|Continue|Count|CShort|CSng|CStr|CType|Date|Decimal|Declare|Default|DefaultStyleSheet|Delegate|Descending|Dim|DirectCast|Distinct|Do|Double|Each|Else|ElseIf|End|Enum|Equals|Erase|Error|Event|Exit|Explicit|False|Finally|For|Friend|From|Function|Get|GetType|GoSub|GoTo|Group|Group|Handles|If|Implements|Imports|In|Inherits|Integer|Interface|Into|Is|IsNot|Join|Let|Lib|Like|Long|LongCount|Loop|Max|Me|Min|Mod|Module|MustInherit|MustOverride|My|MyBase|MyClass|Namespace|New|Next|Not|Nothing|NotInheritable|NotOverridable|(?<!\.)Object|Off|On|Option|Optional|Or|Order|OrElse|Overloads|Overridable|Overrides|ParamArray|Partial|Preserve|Private|Property|Protected|Public|RaiseEvent|ReadOnly|ReDim|RemoveHandler|Resume|Return|Select|Set|Shadows|Shared|Short|Single|Skip|Static|Step|Stop|String|Structure|Sub|Sum|SyncLock|Take|Then|Throw|To|True|Try|TypeOf|Unicode|Until|Variant|When|Where|While|With|WithEvents|WriteOnly|Xor|SByte|UInteger|ULong|UShort|Using|CSByte|CUInt|CULng|CUShort|Async|Await)\b",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.Keyword },
                                       }),
                           ];

        public bool HasAlias(string lang) => lang.ToLower() switch
        {
            "vb.net" or "vbnet" or "vb" or "visualbasic" or "visual basic" => true,
            _ => false,
        };

        public override string ToString() => Name;
    }

    public class Xml : ILanguage
    {
        public string Id => LanguageId.Xml;

        public string Name => "XML";

        public string CssClassName => "xml";

        public string? FirstLinePattern => null;

        public IList<LanguageRule> Rules => [
                               new(
                                   @"\<![ \r\n\t]*(--([^\-]|[\r\n]|-[^\-])*--[ \r\n\t]*)\>",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.HtmlComment },
                                       }),
                               new(
                                   @"(?i)(<!)(doctype)(?:\s+([a-z0-9]+))*(?:\s+("")([^\n]*?)(""))*(>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.XmlDelimiter },
                                           { 2, ScopeName.XmlName },
                                           { 3, ScopeName.XmlAttribute },
                                           { 4, ScopeName.XmlAttributeQuotes },
                                           { 5, ScopeName.XmlAttributeValue },
                                           { 6, ScopeName.XmlAttributeQuotes },
                                           { 7, ScopeName.XmlDelimiter }
                                       }),
                               new(
                                   @"(?i)(<\?)(xml-stylesheet)((?:\s+[a-z0-9]+=""[^\n""]*"")*(?:\s+[a-z0-9]+=\'[^\n\']*\')*\s*?)(\?>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.XmlDelimiter },
                                           { 2, ScopeName.XmlName },
                                           { 3, ScopeName.XmlDocTag },
                                           { 4, ScopeName.XmlDelimiter }
                                       }
                                   ),
                               new(
                                   @"(?i)(<\?)([a-z][a-z0-9-]*)(?:\s+([a-z0-9]+)(=)("")([^\n]*?)(""))*(?:\s+([a-z0-9]+)(=)(\')([^\n]*?)(\'))*\s*?(\?>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.XmlDelimiter },
                                           { 2, ScopeName.XmlName },
                                           { 3, ScopeName.XmlAttribute },
                                           { 4, ScopeName.XmlDelimiter },
                                           { 5, ScopeName.XmlAttributeQuotes },
                                           { 6, ScopeName.XmlAttributeValue },
                                           { 7, ScopeName.XmlAttributeQuotes },
                                           { 8, ScopeName.XmlAttribute },
                                           { 9, ScopeName.XmlDelimiter },
                                           { 10, ScopeName.XmlAttributeQuotes },
                                           { 11, ScopeName.XmlAttributeValue },
                                           { 12, ScopeName.XmlAttributeQuotes },
                                           { 13, ScopeName.XmlDelimiter }
                                       }),
                               new(
                                   @"(?xi)(</?)
                                          (?: ([a-z][a-z0-9-]*)(:) )*
                                          ([a-z][a-z0-9-_\.]*)
                                          (?:
                                            |[\s\n]+([a-z0-9-_\.:]+)[\s\n]*(=)[\s\n]*("")([^\n]+?)("")
                                            |[\s\n]+([a-z0-9-_\.:]+)[\s\n]*(=)[\s\n]*(')([^\n]+?)(')
                                            |[\s\n]+([a-z0-9-_\.:]+) )*
                                          [\s\n]*
                                          (/?>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.XmlDelimiter },
                                           { 2, ScopeName.XmlName },
                                           { 3, ScopeName.XmlDelimiter },
                                           { 4, ScopeName.XmlName },
                                           { 5, ScopeName.XmlAttribute },
                                           { 6, ScopeName.XmlDelimiter },
                                           { 7, ScopeName.XmlAttributeQuotes },
                                           { 8, ScopeName.XmlAttributeValue },
                                           { 9, ScopeName.XmlAttributeQuotes },
                                           { 10, ScopeName.XmlAttribute },
                                           { 11, ScopeName.XmlDelimiter },
                                           { 12, ScopeName.XmlAttributeQuotes },
                                           { 13, ScopeName.XmlAttributeValue },
                                           { 14, ScopeName.XmlAttributeQuotes },
                                           { 15, ScopeName.XmlAttribute },
                                           { 16, ScopeName.XmlDelimiter }
                                       }),
                               new(
                                   @"(?i)&[a-z0-9]+?;",
                                   new Dictionary<int, string?>
                                       {
                                           { 0, ScopeName.XmlAttribute },
                                       }),
                               new(
                                   @"(?s)(<!\[CDATA\[)(.*?)(\]\]>)",
                                   new Dictionary<int, string?>
                                       {
                                           { 1, ScopeName.XmlDelimiter },
                                           { 2, ScopeName.XmlCDataSection },
                                           { 3, ScopeName.XmlDelimiter }
                                       }),
                           ];

        public bool HasAlias(string lang) => lang.ToLower() switch
        {
            "xaml" or "axml" => true,
            _ => false,
        };

        public override string ToString() => Name;
    }

    public interface ILanguageParser
    {
        void Parse(string sourceCode, ILanguage language, Action<string, IList<Scope>> parseHandler);
    }

    public class LanguageParser(ILanguageCompiler languageCompiler, ILanguageRepository languageRepository) : ILanguageParser
    {
        private readonly ILanguageCompiler languageCompiler = languageCompiler;
        private readonly ILanguageRepository languageRepository = languageRepository;

        public void Parse(string sourceCode, ILanguage language, Action<string, IList<Scope>> parseHandler)
        {
            if (string.IsNullOrEmpty(sourceCode))
                return;

            var compiledLanguage = languageCompiler.Compile(language);
            Parse(sourceCode, compiledLanguage, parseHandler);
        }

        private void Parse(string sourceCode, CompiledLanguage compiledLanguage, Action<string, IList<Scope>> parseHandler)
        {
            var regexMatch = compiledLanguage.Regex.Match(sourceCode);
            if (!regexMatch.Success)
            {
                parseHandler(sourceCode, []);
            }
            else
            {
                var currentIndex = 0;
                while (regexMatch.Success)
                {
#if NETFRAMEWORK
                    var sourceCodeBeforeMatch = sourceCode.Substring(currentIndex, regexMatch.Index - currentIndex);
#else
                    var sourceCodeBeforeMatch = sourceCode[currentIndex..regexMatch.Index];
#endif
                    if (!string.IsNullOrEmpty(sourceCodeBeforeMatch))
                    {
                        parseHandler(sourceCodeBeforeMatch, []);
                    }

                    var matchedSourceCode = sourceCode.Substring(regexMatch.Index, regexMatch.Length);
                    if (!string.IsNullOrEmpty(matchedSourceCode))
                    {
                        var capturedStylesForMatchedFragment = GetCapturedStyles(regexMatch, regexMatch.Index, compiledLanguage);
                        var capturedStyleTree = CreateCapturedStyleTree(capturedStylesForMatchedFragment);
                        parseHandler(matchedSourceCode, capturedStyleTree);
                    }

                    currentIndex = regexMatch.Index + regexMatch.Length;
                    regexMatch = regexMatch.NextMatch();
                }

#if NETFRAMEWORK
                var sourceCodeAfterAllMatches = sourceCode.Substring(currentIndex);
#else
                var sourceCodeAfterAllMatches = sourceCode[currentIndex..];
#endif
                if (!string.IsNullOrEmpty(sourceCodeAfterAllMatches))
                {
                    parseHandler(sourceCodeAfterAllMatches, []);
                }
            }
        }

        private static List<Scope> CreateCapturedStyleTree(IList<Scope> capturedStyles)
        {
            capturedStyles.SortStable((x, y) => x.Index.CompareTo(y.Index));

            var capturedStyleTree = new List<Scope>(capturedStyles.Count);
            Scope? currentScope = null;

            foreach (var capturedStyle in capturedStyles)
            {
                if (currentScope == null)
                {
                    capturedStyleTree.Add(capturedStyle);
                    currentScope = capturedStyle;
                    continue;
                }

                AddScopeToNestedScopes(capturedStyle, ref currentScope, capturedStyleTree);
            }

            return capturedStyleTree;
        }

        private static void AddScopeToNestedScopes(Scope scope, ref Scope? currentScope, ICollection<Scope> capturedStyleTree)
        {
            if (currentScope != null && scope.Index >= currentScope.Index && (scope.Index + scope.Length <= currentScope.Index + currentScope.Length))
            {
                currentScope.AddChild(scope);
                currentScope = scope;
            }
            else
            {
                currentScope = currentScope?.Parent;

                if (currentScope != null)
                {
                    AddScopeToNestedScopes(scope, ref currentScope, capturedStyleTree);
                }
                else
                {
                    capturedStyleTree.Add(scope);
                }
            }
        }


        private List<Scope> GetCapturedStyles(Match regexMatch, int currentIndex, CompiledLanguage compiledLanguage)
        {
            var capturedStyles = new List<Scope>();
            for (var i = 0; i < regexMatch.Groups.Count; i++)
            {
                var regexGroup = regexMatch.Groups[i];
                if (regexGroup.Length > 0 && i < compiledLanguage.Captures.Count)
                {  //note: i can be >= Captures.Count due to named groups; these do capture a group but always get added after all non-named groups (which is why we do not count them in numberOfCaptures)
                    var styleName = compiledLanguage.Captures[i];
                    if (!string.IsNullOrEmpty(styleName))
                    {
                        foreach (Capture regexCapture in regexGroup.Captures)
                        {
                            AppendCapturedStylesForRegexCapture(regexCapture, currentIndex, styleName!, capturedStyles);
                        }
                    }
                }
            }

            return capturedStyles;
        }

        private void AppendCapturedStylesForRegexCapture(Capture regexCapture, int currentIndex, string styleName, List<Scope> capturedStyles)
        {
            if (styleName.StartsWith(ScopeName.LanguagePrefix))
            {
#if NETFRAMEWORK
                var nestedGrammarId = styleName.Substring(1);
#else
                var nestedGrammarId = styleName[1..];
#endif
                AppendCapturedStylesForNestedLanguage(regexCapture, regexCapture.Index - currentIndex, nestedGrammarId, capturedStyles);
            }
            else
            {
                capturedStyles.Add(new Scope(styleName, regexCapture.Index - currentIndex, regexCapture.Length));
            }
        }

        private void AppendCapturedStylesForNestedLanguage(Capture regexCapture, int offset, string nestedLanguageId, List<Scope> capturedStyles)
        {
            ILanguage nestedLanguage = languageRepository.FindById(nestedLanguageId);
            if (nestedLanguage == null)
                throw new InvalidOperationException("The nested language was not found in the language repository.");

            var nestedCompiledLanguage = languageCompiler.Compile(nestedLanguage);
            var regexMatch = nestedCompiledLanguage.Regex.Match(regexCapture.Value, 0, regexCapture.Value.Length);
            if (!regexMatch.Success)
                return;

            while (regexMatch.Success)
            {
                var capturedStylesForMatchedFragment = GetCapturedStyles(regexMatch, 0, nestedCompiledLanguage);
                var capturedStyleTree = CreateCapturedStyleTree(capturedStylesForMatchedFragment);
                foreach (var nestedCapturedStyle in capturedStyleTree)
                {
                    IncreaseCapturedStyleIndicies(capturedStyleTree, offset);
                    capturedStyles.Add(nestedCapturedStyle);
                }

                regexMatch = regexMatch.NextMatch();
            }
        }

        private static void IncreaseCapturedStyleIndicies(IList<Scope> capturedStyles, int amountToIncrease)
        {
            for (var i = 0; i < capturedStyles.Count; i++)
            {
                var scope = capturedStyles[i];
                scope.Index += amountToIncrease;
                if (scope.Children.Count > 0)
                {
                    IncreaseCapturedStyleIndicies(scope.Children, amountToIncrease);
                }
            }
        }
    }

    public class Scope
    {
        public Scope(string name, int index, int length)
        {
            Guard.ArgNotNullAndNotEmpty(name, nameof(name));

            Name = name;
            Index = index;
            Length = length;
            Children = [];
        }

        public IList<Scope> Children { get; set; }
        public int Index { get; set; }
        public int Length { get; set; }
        public Scope? Parent { get; set; }
        public string Name { get; set; }

        public void AddChild(Scope childScope)
        {
            if (childScope.Parent != null)
                throw new InvalidOperationException("The child scope already has a parent.");

            childScope.Parent = this;
            Children.Add(childScope);
        }
    }

    /// <summary>
    /// Defines the styling for a given scope.
    /// </summary>
    public class Style
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Style"/> class.
        /// </summary>
        /// <param name="scopeName">The name of the scope the style defines.</param>
        public Style(string scopeName)
        {
            Guard.ArgNotNullAndNotEmpty(scopeName, nameof(scopeName));

            ScopeName = scopeName;
        }

        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        /// <value>The background color.</value>
        public string? Background { get; set; }

        /// <summary>
        /// Gets or sets the foreground color.
        /// </summary>
        /// <value>The foreground color.</value>
        public string? Foreground { get; set; }

        /// <summary>
        /// Gets or sets the name of the scope the style defines.
        /// </summary>
        /// <value>The name of the scope the style defines.</value>
        public string ScopeName { get; set; }

        /// <summary>
        /// Gets or sets the reference name of the scope, such as in CSS.
        /// </summary>
        /// <value>The plain text Reference name.</value>
        public string? ReferenceName { get; set; }

        /// <summary>
        /// Gets or sets italic font style.
        /// </summary>
        /// <value>True if italic.</value>
        public bool Italic { get; set; }

        /// <summary>
        /// Gets or sets bold font style.
        /// </summary>
        /// <value>True if bold.</value>
        public bool Bold { get; set; }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString() => ScopeName ?? string.Empty;
    }

    /// <summary>
    /// A dictionary of <see cref="Style" /> instances, keyed by the styles' scope name.
    /// </summary>
    public partial class StyleDictionary : KeyedCollection<string, Style>
    {
        /// <summary>
        /// When implemented in a derived class, extracts the key from the specified element.
        /// </summary>
        /// <param name="item">The element from which to extract the key.</param>
        /// <returns>The key for the specified element.</returns>
        protected override string GetKeyForItem(Style item) => item.ScopeName;

        public const string Blue = "#FF0000FF";
        public const string White = "#FFFFFFFF";
        public const string Black = "#FF000000";
        public const string DullRed = "#FFA31515";
        public const string Yellow = "#FFFFFF00";
        public const string Green = "#FF008000";
        public const string PowderBlue = "#FFB0E0E6";
        public const string Teal = "#FF008080";
        public const string Gray = "#FF808080";
        public const string Navy = "#FF000080";
        public const string OrangeRed = "#FFFF4500";
        public const string Purple = "#FF800080";
        public const string Red = "#FFFF0000";
        public const string MediumTurqoise = "FF48D1CC";
        public const string Magenta = "FFFF00FF";
        public const string OliveDrab = "#FF6B8E23";
        public const string DarkOliveGreen = "#FF556B2F";
        public const string DarkCyan = "#FF008B8B";
    }

    /// <summary>
    /// Defines the Default Dark Theme.
    /// </summary>
    public partial class StyleDictionary
    {
        private const string VSDarkBackground = "#FF1E1E1E";
        private const string VSDarkPlainText = "#FFDADADA";

        private const string VSDarkXMLDelimeter = "#FF808080";
        private const string VSDarkXMLName = "#FF#E6E6E6";
        private const string VSDarkXMLAttribute = "#FF92CAF4";
        private const string VSDarkXAMLCData = "#FFC0D088";
        private const string VSDarkXMLComment = "#FF608B4E";

        private const string VSDarkComment = "#FF57A64A";
        private const string VSDarkKeyword = "#FF569CD6";
        private const string VSDarkGray = "#FF9B9B9B";
        private const string VSDarkNumber = "#FFB5CEA8";
        private const string VSDarkClass = "#FF4EC9B0";
        private const string VSDarkString = "#FFD69D85";

        /// <summary>
        /// A theme with Dark Colors.
        /// </summary>
        public static StyleDictionary DefaultDark => [
                    new Style(ScopeName.PlainText)
                    {
                        Foreground = VSDarkPlainText,
                        Background = VSDarkBackground,
                        ReferenceName = "plainText"
                    },
                    new Style(ScopeName.HtmlServerSideScript)
                    {
                        Background = Yellow,
                        ReferenceName = "htmlServerSideScript"
                    },
                    new Style(ScopeName.HtmlComment)
                    {
                        Foreground = VSDarkComment,
                        ReferenceName = "htmlComment"
                    },
                    new Style(ScopeName.HtmlTagDelimiter)
                    {
                        Foreground = VSDarkKeyword,
                        ReferenceName = "htmlTagDelimiter"
                    },
                    new Style(ScopeName.HtmlElementName)
                    {
                        Foreground = DullRed,
                        ReferenceName = "htmlElementName"
                    },
                    new Style(ScopeName.HtmlAttributeName)
                    {
                        Foreground = Red,
                        ReferenceName = "htmlAttributeName"
                    },
                    new Style(ScopeName.HtmlAttributeValue)
                    {
                        Foreground = VSDarkKeyword,
                        ReferenceName = "htmlAttributeValue"
                    },
                    new Style(ScopeName.HtmlOperator)
                    {
                        Foreground = VSDarkKeyword,
                        ReferenceName = "htmlOperator"
                    },
                    new Style(ScopeName.Comment)
                    {
                        Foreground = VSDarkComment,
                        ReferenceName = "comment"
                    },
                    new Style(ScopeName.XmlDocTag)
                    {
                        Foreground = VSDarkXMLComment,
                        ReferenceName = "xmlDocTag"
                    },
                    new Style(ScopeName.XmlDocComment)
                    {
                        Foreground = VSDarkXMLComment,
                        ReferenceName = "xmlDocComment"
                    },
                    new Style(ScopeName.String)
                    {
                        Foreground = VSDarkString,
                        ReferenceName = "string"
                    },
                    new Style(ScopeName.StringCSharpVerbatim)
                    {
                        Foreground = VSDarkString,
                        ReferenceName = "stringCSharpVerbatim"
                    },
                    new Style(ScopeName.Keyword)
                    {
                        Foreground = VSDarkKeyword,
                        ReferenceName = "keyword"
                    },
                    new Style(ScopeName.PreprocessorKeyword)
                    {
                        Foreground = VSDarkKeyword,
                        ReferenceName = "preprocessorKeyword"
                    },
                    new Style(ScopeName.HtmlEntity)
                    {
                        Foreground = Red,
                        ReferenceName = "htmlEntity"
                    },
                    new Style(ScopeName.XmlAttribute)
                    {
                        Foreground = VSDarkXMLAttribute,
                        ReferenceName = "xmlAttribute"
                    },
                    new Style(ScopeName.XmlAttributeQuotes)
                    {
                        Foreground = VSDarkKeyword,
                        ReferenceName = "xmlAttributeQuotes"
                    },
                    new Style(ScopeName.XmlAttributeValue)
                    {
                        Foreground = VSDarkKeyword,
                        ReferenceName = "xmlAttributeValue"
                    },
                    new Style(ScopeName.XmlCDataSection)
                    {
                        Foreground = VSDarkXAMLCData,
                        ReferenceName = "xmlCDataSection"
                    },
                    new Style(ScopeName.XmlComment)
                    {
                        Foreground = VSDarkComment,
                        ReferenceName = "xmlComment"
                    },
                    new Style(ScopeName.XmlDelimiter)
                    {
                        Foreground = VSDarkXMLDelimeter,
                        ReferenceName = "xmlDelimiter"
                    },
                    new Style(ScopeName.XmlName)
                    {
                        Foreground = VSDarkXMLName,
                        ReferenceName = "xmlName"
                    },
                    new Style(ScopeName.ClassName)
                    {
                        Foreground = VSDarkClass,
                        ReferenceName = "className"
                    },
                    new Style(ScopeName.CssSelector)
                    {
                        Foreground = DullRed,
                        ReferenceName = "cssSelector"
                    },
                    new Style(ScopeName.CssPropertyName)
                    {
                        Foreground = Red,
                        ReferenceName = "cssPropertyName"
                    },
                    new Style(ScopeName.CssPropertyValue)
                    {
                        Foreground = VSDarkKeyword,
                        ReferenceName = "cssPropertyValue"
                    },
                    new Style(ScopeName.SqlSystemFunction)
                    {
                        Foreground = Magenta,
                        ReferenceName = "sqlSystemFunction"
                    },
                    new Style(ScopeName.PowerShellAttribute)
                    {
                        Foreground = PowderBlue,
                        ReferenceName = "powershellAttribute"
                    },
                    new Style(ScopeName.PowerShellOperator)
                    {
                        Foreground = VSDarkGray,
                        ReferenceName = "powershellOperator"
                    },
                    new Style(ScopeName.PowerShellType)
                    {
                        Foreground = Teal,
                        ReferenceName = "powershellType"
                    },
                    new Style(ScopeName.PowerShellVariable)
                    {
                        Foreground = OrangeRed,
                        ReferenceName = "powershellVariable"
                    },

                    new Style(ScopeName.Type)
                    {
                        Foreground = Teal,
                        ReferenceName = "type"
                    },
                    new Style(ScopeName.TypeVariable)
                    {
                        Foreground = Teal,
                        Italic = true,
                        ReferenceName = "typeVariable"
                    },
                    new Style(ScopeName.NameSpace)
                    {
                        Foreground = Navy,
                        ReferenceName = "namespace"
                    },
                    new Style(ScopeName.Constructor)
                    {
                        Foreground = Purple,
                        ReferenceName = "constructor"
                    },
                    new Style(ScopeName.Predefined)
                    {
                        Foreground = Navy,
                        ReferenceName = "predefined"
                    },
                    new Style(ScopeName.PseudoKeyword)
                    {
                        Foreground = Navy,
                        ReferenceName = "pseudoKeyword"
                    },
                    new Style(ScopeName.StringEscape)
                    {
                        Foreground = VSDarkGray,
                        ReferenceName = "stringEscape"
                    },
                    new Style(ScopeName.ControlKeyword)
                    {
                        Foreground = VSDarkKeyword,
                        ReferenceName = "controlKeyword"
                    },
                    new Style(ScopeName.Number)
                    {
                        ReferenceName = "number",
                        Foreground = VSDarkNumber
                    },
                    new Style(ScopeName.Operator)
                    {
                        ReferenceName = "operator"
                    },
                    new Style(ScopeName.Delimiter)
                    {
                        ReferenceName = "delimiter"
                    },

                    new Style(ScopeName.MarkdownHeader)
                    {
                        Foreground = VSDarkKeyword,
                        Bold = true,
                        ReferenceName = "markdownHeader"
                    },
                    new Style(ScopeName.MarkdownCode)
                    {
                        Foreground = VSDarkString,
                        ReferenceName = "markdownCode"
                    },
                    new Style(ScopeName.MarkdownListItem)
                    {
                        Bold = true,
                        ReferenceName = "markdownListItem"
                    },
                    new Style(ScopeName.MarkdownEmph)
                    {
                        Italic = true,
                        ReferenceName = "italic"
                    },
                    new Style(ScopeName.MarkdownBold)
                    {
                        Bold = true,
                        ReferenceName = "bold"
                    },

                    new Style(ScopeName.BuiltinFunction)
                    {
                        Foreground = OliveDrab,
                        Bold = true,
                        ReferenceName = "builtinFunction"
                    },
                    new Style(ScopeName.BuiltinValue)
                    {
                        Foreground = DarkOliveGreen,
                        Bold = true,
                        ReferenceName = "builtinValue"
                    },
                    new Style(ScopeName.Attribute)
                    {
                        Foreground = DarkCyan,
                        Italic = true,
                        ReferenceName = "attribute"
                    },
                    new Style(ScopeName.SpecialCharacter)
                    {
                        ReferenceName = "specialChar"
                    },
                ];
    }

    /// <summary>
    /// Defines the Default Light Theme.
    /// </summary>
    public partial class StyleDictionary
    {
        /// <summary>
        /// A theme with Light Colors.
        /// </summary>
        public static StyleDictionary DefaultLight => [
                    new Style(ScopeName.PlainText)
                    {
                        Foreground = Black,
                        Background = White,
                        ReferenceName = "plainText"
                    },
                    new Style(ScopeName.HtmlServerSideScript)
                    {
                        Background = Yellow,
                        ReferenceName = "htmlServerSideScript"
                    },
                    new Style(ScopeName.HtmlComment)
                    {
                        Foreground = Green,
                        ReferenceName = "htmlComment"
                    },
                    new Style(ScopeName.HtmlTagDelimiter)
                    {
                        Foreground = Blue,
                        ReferenceName = "htmlTagDelimiter"
                    },
                    new Style(ScopeName.HtmlElementName)
                    {
                        Foreground = DullRed,
                        ReferenceName = "htmlElementName"
                    },
                    new Style(ScopeName.HtmlAttributeName)
                    {
                        Foreground = Red,
                        ReferenceName = "htmlAttributeName"
                    },
                    new Style(ScopeName.HtmlAttributeValue)
                    {
                        Foreground = Blue,
                        ReferenceName = "htmlAttributeValue"
                    },
                    new Style(ScopeName.HtmlOperator)
                    {
                        Foreground = Blue,
                        ReferenceName = "htmlOperator"
                    },
                    new Style(ScopeName.Comment)
                    {
                        Foreground = Green,
                        ReferenceName = "comment"
                    },
                    new Style(ScopeName.XmlDocTag)
                    {
                        Foreground = Gray,
                        ReferenceName = "xmlDocTag"
                    },
                    new Style(ScopeName.XmlDocComment)
                    {
                        Foreground = Green,
                        ReferenceName = "xmlDocComment"
                    },
                    new Style(ScopeName.String)
                    {
                        Foreground = DullRed,
                        ReferenceName = "string"
                    },
                    new Style(ScopeName.StringCSharpVerbatim)
                    {
                        Foreground = DullRed,
                        ReferenceName = "stringCSharpVerbatim"
                    },
                    new Style(ScopeName.Keyword)
                    {
                        Foreground = Blue,
                        ReferenceName = "keyword"
                    },
                    new Style(ScopeName.PreprocessorKeyword)
                    {
                        Foreground = Blue,
                        ReferenceName = "preprocessorKeyword"
                    },
                    new Style(ScopeName.HtmlEntity)
                    {
                        Foreground = Red,
                        ReferenceName = "htmlEntity"
                    },
                    new Style(ScopeName.XmlAttribute)
                    {
                        Foreground = Red,
                        ReferenceName = "xmlAttribute"
                    },
                    new Style(ScopeName.XmlAttributeQuotes)
                    {
                        Foreground = Black,
                        ReferenceName = "xmlAttributeQuotes"
                    },
                    new Style(ScopeName.XmlAttributeValue)
                    {
                        Foreground = Blue,
                        ReferenceName = "xmlAttributeValue"
                    },
                    new Style(ScopeName.XmlCDataSection)
                    {
                        Foreground = Gray,
                        ReferenceName = "xmlCDataSection"
                    },
                    new Style(ScopeName.XmlComment)
                    {
                        Foreground = Green,
                        ReferenceName = "xmlComment"
                    },
                    new Style(ScopeName.XmlDelimiter)
                    {
                        Foreground = Blue,
                        ReferenceName = "xmlDelimiter"
                    },
                    new Style(ScopeName.XmlName)
                    {
                        Foreground = DullRed,
                        ReferenceName = "xmlName"
                    },
                    new Style(ScopeName.ClassName)
                    {
                        Foreground = MediumTurqoise,
                        ReferenceName = "className"
                    },
                    new Style(ScopeName.CssSelector)
                    {
                        Foreground = DullRed,
                        ReferenceName = "cssSelector"
                    },
                    new Style(ScopeName.CssPropertyName)
                    {
                        Foreground = Red,
                        ReferenceName = "cssPropertyName"
                    },
                    new Style(ScopeName.CssPropertyValue)
                    {
                        Foreground = Blue,
                        ReferenceName = "cssPropertyValue"
                    },
                    new Style(ScopeName.SqlSystemFunction)
                    {
                        Foreground = Magenta,
                        ReferenceName = "sqlSystemFunction"
                    },
                    new Style(ScopeName.PowerShellAttribute)
                    {
                        Foreground = PowderBlue,
                        ReferenceName = "powershellAttribute"
                    },
                    new Style(ScopeName.PowerShellOperator)
                    {
                        Foreground = Gray,
                        ReferenceName = "powershellOperator"
                    },
                    new Style(ScopeName.PowerShellType)
                    {
                        Foreground = Teal,
                        ReferenceName = "powershellType"
                    },
                    new Style(ScopeName.PowerShellVariable)
                    {
                        Foreground = OrangeRed,
                        ReferenceName = "powershellVariable"
                    },

                    new Style(ScopeName.Type)
                    {
                        Foreground = Teal,
                        ReferenceName = "type"
                    },
                    new Style(ScopeName.TypeVariable)
                    {
                        Foreground = Teal,
                        Italic = true,
                        ReferenceName = "typeVariable"
                    },
                    new Style(ScopeName.NameSpace)
                    {
                        Foreground = Navy,
                        ReferenceName = "namespace"
                    },
                    new Style(ScopeName.Constructor)
                    {
                        Foreground = Purple,
                        ReferenceName = "constructor"
                    },
                    new Style(ScopeName.Predefined)
                    {
                        Foreground = Navy,
                        ReferenceName = "predefined"
                    },
                    new Style(ScopeName.PseudoKeyword)
                    {
                        Foreground = Navy,
                        ReferenceName = "pseudoKeyword"
                    },
                    new Style(ScopeName.StringEscape)
                    {
                        Foreground = Gray,
                        ReferenceName = "stringEscape"
                    },
                    new Style(ScopeName.ControlKeyword)
                    {
                        Foreground = Blue,
                        ReferenceName = "controlKeyword"
                    },
                    new Style(ScopeName.Number)
                    {
                        ReferenceName = "number"
                    },
                    new Style(ScopeName.Operator)
                    {
                        ReferenceName = "operator"
                    },
                    new Style(ScopeName.Delimiter)
                    {
                        ReferenceName = "delimiter"
                    },

                    new Style(ScopeName.MarkdownHeader)
                    {
                        Foreground = Blue,
                        Bold = true,
                        ReferenceName = "markdownHeader"
                    },
                    new Style(ScopeName.MarkdownCode)
                    {
                        Foreground = Teal,
                        ReferenceName = "markdownCode"
                    },
                    new Style(ScopeName.MarkdownListItem)
                    {
                        Bold = true,
                        ReferenceName = "markdownListItem"
                    },
                    new Style(ScopeName.MarkdownEmph)
                    {
                        Italic = true,
                        ReferenceName = "italic"
                    },
                    new Style(ScopeName.MarkdownBold)
                    {
                        Bold = true,
                        ReferenceName = "bold"
                    },

                    new Style(ScopeName.BuiltinFunction)
                    {
                        Foreground = OliveDrab,
                        Bold = true,
                        ReferenceName = "builtinFunction"
                    },
                    new Style(ScopeName.BuiltinValue)
                    {
                        Foreground = DarkOliveGreen,
                        Bold = true,
                        ReferenceName = "builtinValue"
                    },
                    new Style(ScopeName.Attribute)
                    {
                        Foreground = DarkCyan,
                        Italic = true,
                        ReferenceName = "attribute"
                    },
                    new Style(ScopeName.SpecialCharacter)
                    {
                        ReferenceName = "specialChar"
                    },
                ];
    }
}
#pragma warning restore CA2211 // Non-constant fields should not be visible
