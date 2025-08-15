namespace Wice;

/// <summary>
/// Header specialized to render its icon using a symbol font from the current window theme.
/// </summary>
/// <remarks>
/// This class configures the header's icon to be a <see cref="TextBox"/> that uses
/// <see cref="Theme.SymbolFontName"/> and centers its glyph vertically within the layout box
/// via <see cref="TextBox.ParagraphAlignment"/>.
/// </remarks>
public partial class SymbolHeader : Header
{
    /// <summary>
    /// Gets the header icon as a strongly-typed <see cref="TextBox"/>.
    /// </summary>
    /// <remarks>
    /// This hides the base icon with a more specific type for convenience.
    /// </remarks>
    public new TextBox Icon => (TextBox)base.Icon;

    /// <inheritdoc/>
    /// <remarks>
    /// Creates a <see cref="TextBox"/> configured to:
    /// - Use the window theme's <see cref="Theme.SymbolFontName"/> for rendering glyphs.
    /// - Center the symbol vertically via <see cref="DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER"/>.
    /// </remarks>
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
