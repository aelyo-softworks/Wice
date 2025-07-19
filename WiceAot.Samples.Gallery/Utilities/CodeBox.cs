namespace Wice.Samples.Gallery.Utilities;

public partial class CodeBox : RichTextBox
{
    public static VisualProperty CodeLanguageProperty { get; } = VisualProperty.Add(typeof(CodeBox), nameof(CodeLanguage), typeof(string), VisualPropertyInvalidateModes.Measure, convert: ValidateNonNullString);
    public static VisualProperty CodeTextProperty { get; } = VisualProperty.Add(typeof(CodeBox), nameof(CodeText), typeof(string), VisualPropertyInvalidateModes.Measure, convert: ValidateNonNullString);

    private Lazy<ILanguage> _language;

    public CodeBox()
    {
        _language = new Lazy<ILanguage>(GetLanguage, true);
    }

    [Category(CategoryLayout)]
    public string CodeLanguage { get => (string?)GetPropertyValue(CodeLanguageProperty) ?? string.Empty; set => SetPropertyValue(CodeLanguageProperty, value); }

    [Category(CategoryLayout)]
    public string CodeText { get => (string?)GetPropertyValue(CodeTextProperty) ?? string.Empty; set => SetPropertyValue(CodeTextProperty, value); }

    public override string HtmlText { get => base.HtmlText; set => throw new NotSupportedException(); }
    public override string RtfText { get => base.RtfText; set => throw new NotSupportedException(); }
    public override string Text { get => base.Text; set => throw new NotSupportedException(); }
    public ILanguage Language => _language.Value;

    private ILanguage GetLanguage()
    {
        var language = CodeLanguage.Nullify() ?? LanguageId.CSharp;
        return Languages.FindById(language) ?? Languages.CSharp;
    }

    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        if (property == CodeLanguageProperty)
        {
            _language = new Lazy<ILanguage>(GetLanguage, true);
        }
        else if (property == CodeTextProperty)
        {
            var sw = new StringWriter();
            var formatter = new RtfFormatter(sw);
            formatter.Format((string)value!, Language);
            var text = sw.ToString();
            base.RtfText = text;
        }
        return true;
    }

    protected override void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
    {
        base.OnThemeDpiEvent(sender, e);

        // not sure why codebox (rich text box) doesn't render always properly when DPI is not 96
        // we fix it using a fixed ZoomFactor and RenderScale, result is less than perfect but works
        var zf = e.NewDpi / (float)WiceCommons.USER_DEFAULT_SCREEN_DPI;
        Zoom = zf;
        RenderScale = new Vector3(zf, zf, zf);
    }

    public override float ZoomFactor { get => 1; set { } }
}