
namespace Wice.Samples.Gallery.Utilities;

// so we can add our own colors for Wice's classes
public class WiceLanguage : ILanguage
{
    private static readonly Lazy<WiceLanguage> _default = new(() =>
    {
        var lang = new WiceLanguage();
        Languages.Load(lang);
        return lang;
    }, true);
    public static WiceLanguage Default => _default.Value;

    private readonly List<LanguageRule> _rules = [];

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public WiceLanguage()
    {
        // re-use c# rules
        _rules.AddRange(new CSharp().Rules);

        var types = typeof(Visual).Assembly.GetTypes().Where(t => t.IsPublic && t.GenericTypeArguments.Length == 0).Select(t => t.Name).ToList();

        // usual types
        types.Add(nameof(D3DCOLORVALUE));
        types.Add(nameof(D2D_RECT_F));
        types.Add(nameof(D2D_POINT_2F));
        types.Add(nameof(D2D_SIZE_F));
        types.Add(nameof(Vector2));
        types.Add(nameof(Vector3));
        types.Add(nameof(Random));
        types.Add(nameof(Environment));
        types.Add(nameof(MDL2GlyphResource));

        // hexa
        _rules.Add(new LanguageRule(@"\b0x[0-9A-F]{1,}\b", new Dictionary<int, string?> { { 0, ScopeName.Number } }));

        // floats
        _rules.Add(new LanguageRule(@"\b[0-9\.]{1,}f\b", new Dictionary<int, string?> { { 0, ScopeName.Number } }));

        // wice classes
        _rules.Add(new LanguageRule(@"\b(" + string.Join("|", types) + @")\b", new Dictionary<int, string?> { { 1, ScopeName.ClassName } }));

        // methods
        _rules.Add(new LanguageRule(@"\b\.[a-zA-Z]{1,}\(\b", new Dictionary<int, string?> { { 0, ScopeName.BuiltinFunction } }));
    }

    public const string LanguageId = "wice";
    public string Id => LanguageId;
    public string Name => LanguageId;
    public string CssClassName => LanguageId;
    public bool HasAlias(string lang) => false;
    public string? FirstLinePattern => null;
    public IList<LanguageRule> Rules => _rules;
}
