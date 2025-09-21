namespace Wice;

/// <summary>
/// Header specialized to render its icon using a symbol font from the current window theme.
/// </summary>
public partial class SymbolHeader : Header
{
    /// <summary>
    /// Gets the header icon as a strongly-typed <see cref="TextBox"/>.
    /// </summary>
    public new TextBox Icon => (TextBox)base.Icon;

    /// <inheritdoc/>
    protected sealed override Visual CreateIcon()
    {
        var tb = new TextBox
        {
            FontFamilyName = GetWindowTheme().SymbolFontName,
            ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER
        };
        return tb;
    }
}
