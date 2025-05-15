namespace Wice;

public partial class Tab : Visual
{
    public event EventHandler<ValueEventArgs<TabPage>>? PageAdded;
    public event EventHandler<ValueEventArgs<TabPage>>? PageRemoved;

    public Tab()
    {
        Pages = CreatePages();
        Pages.CollectionChanged += (s, e) => OnPagesCollectionChanged(e);

        PagesContent = CreatePagesContent();
        if (PagesContent == null)
            throw new InvalidOperationException();

        PagesHeader = CreatePagesHeader();
        if (PagesHeader == null)
            throw new InvalidOperationException();
    }

    [Browsable(false)]
    public BaseObjectCollection<TabPage> Pages { get; }

    [Browsable(false)]
    public Dock PagesHeader { get; }

    [Browsable(false)]
    public Canvas PagesContent { get; }

    protected virtual BaseObjectCollection<TabPage> CreatePages() => [];
    protected virtual Dock CreatePagesHeader() => new();
    protected virtual Canvas CreatePagesContent() => new();

    protected virtual void OnPageAdded(object? sender, ValueEventArgs<TabPage> e) => PageAdded?.Invoke(sender, e);
    protected virtual void OnPageRemoved(object? sender, ValueEventArgs<TabPage> e) => PageRemoved?.Invoke(sender, e);

    private void OnPagesCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
                if (e.OldItems == null)
                    break;

                foreach (var page in e.OldItems.OfType<TabPage>())
                {
                    if (page == null)
                        continue;

                    page.Tab = null;
                    PagesHeader.Children.Remove(page.Header);
                    PagesContent.Children.Remove(page.Content);
                    OnPageRemoved(this, new ValueEventArgs<TabPage>(page));
                }

                if (e.Action == NotifyCollectionChangedAction.Replace)
                {
                    add();
                }
                break;

            case NotifyCollectionChangedAction.Add:
                add();
                break;
        }

        void add()
        {
            if (e.NewItems == null)
                return;

            foreach (var page in e.NewItems.OfType<TabPage>())
            {
                if (page == null)
                    continue;

                if (page.Tab != null)
                    throw new WiceException("0032: Page '" + page.Name + "' of type " + page.GetType().Name + " is already a children of tab '" + page.Tab.Name + "' of type " + page.Tab.GetType().Name + ".");

                page.Tab = this;
                PagesHeader.Children.Add(page.Header);
                PagesContent.Children.Add(page.Content);
                OnPageAdded(this, new ValueEventArgs<TabPage>(page));
            }
        }
    }
}
