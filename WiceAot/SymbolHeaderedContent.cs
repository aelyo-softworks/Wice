namespace Wice;

public partial class SymbolHeaderedContent : HeaderedContent
{
    public new SymbolHeader Header => (SymbolHeader)base.Header;

    protected override Header CreateHeader() => new SymbolHeader();
}
