namespace Wice;

public partial class TabPage : BaseObject
{
    public TabPage()
    {
        Content = CreateContent();
        if (Content == null)
            throw new InvalidOperationException();

        Header = CreateHeader();
        if (Header == null)
            throw new InvalidOperationException();
    }

    protected virtual Visual CreateContent() => new Border();
    protected virtual Header CreateHeader() => new();

    public Tab? Tab { get; internal set; }
    public int Index => Tab?.Pages.IndexOf(this) ?? -1;

    [Browsable(false)]
    public Visual Content { get; }

    [Browsable(false)]
    public Header Header { get; }
}
