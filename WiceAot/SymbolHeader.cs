namespace Wice;

public partial class SymbolHeader : Header
{
    public new TextBox Icon => (TextBox)base.Icon;

    protected sealed override Visual CreateIcon()
    {
        var tb = new TextBox
        {
            FontFamilyName = Application.CurrentTheme.SymbolFontName,
            ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER
        };
        return tb;
    }
}
