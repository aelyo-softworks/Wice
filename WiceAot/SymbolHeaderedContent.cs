namespace Wice;

/// <summary>
/// A <see cref="HeaderedContent"/> specialized to use a <see cref="SymbolHeader"/> as its header.
/// </summary>
/// <remarks>
/// - Exposes a strongly-typed <see cref="Header"/> property that returns the underlying <see cref="SymbolHeader"/>.
/// - Overrides <see cref="CreateHeader"/> to instantiate a <see cref="SymbolHeader"/> so that the content can
///   be toggled via the header selection while rendering its icon as a symbol font glyph.
/// </remarks>
/// <seealso cref="HeaderedContent"/>
/// <seealso cref="SymbolHeader"/>
public partial class SymbolHeaderedContent : HeaderedContent
{
    /// <summary>
    /// Gets the header as a <see cref="SymbolHeader"/>.
    /// </summary>
    /// <remarks>
    /// This hides <see cref="HeaderedContent.Header"/> with a more specific type for convenience.
    /// Returns the same instance created by <see cref="CreateHeader"/>.
    /// </remarks>
    /// <seealso cref="HeaderedContent.Header"/>
    public new SymbolHeader Header => (SymbolHeader)base.Header;

    /// <inheritdoc/>
    /// <summary>
    /// Creates the specialized header instance used by this control.
    /// </summary>
    /// <returns>A new <see cref="SymbolHeader"/>.</returns>
    protected override Header CreateHeader() => new SymbolHeader();
}
