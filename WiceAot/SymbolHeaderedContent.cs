namespace Wice;

/// <summary>
/// A <see cref="HeaderedContent"/> specialized to use a <see cref="SymbolHeader"/> as its header.
/// </summary>
public partial class SymbolHeaderedContent : HeaderedContent
{
    /// <summary>
    /// Gets the header as a <see cref="SymbolHeader"/>.
    /// </summary>
    public new SymbolHeader Header => (SymbolHeader)base.Header;

    /// <inheritdoc/>
    /// <summary>
    /// Creates the specialized header instance used by this control.
    /// </summary>
    /// <returns>A new <see cref="SymbolHeader"/>.</returns>
    protected override Header CreateHeader() => new SymbolHeader();
}
