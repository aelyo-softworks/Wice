namespace Wice;

public partial class Tab : Visual
{
    public Tab()
    {
        Pages = CreatePages();
        Pages.CollectionChanged += (s, e) => OnPagesCollectionChanged(e);
    }

    [Browsable(false)]
    public BaseObjectCollection<TabPage> Pages { get; }

    protected virtual BaseObjectCollection<TabPage> CreatePages() => [];

    private void OnPagesCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
    }
}
