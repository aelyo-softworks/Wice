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
    protected override Header CreateHeader() => new SymbolHeader();
}
