namespace Wice;

public partial class Tabs : Stack
{
    public event EventHandler<ValueEventArgs<TabPage>>? PageAdded;
    public event EventHandler<ValueEventArgs<TabPage>>? PageRemoved;
    public event EventHandler? PageSelected;

    public Tabs()
    {
        Orientation = Orientation.Vertical;
        Pages = CreatePages();
        Pages.CollectionChanged += (s, e) => OnPagesCollectionChanged(e);

        PagesContent = CreatePagesContent();
        if (PagesContent == null)
            throw new InvalidOperationException();

        Children.Add(PagesContent);
        PagesContent.IsVisible = false;

        PagesHeader = CreatePagesHeader();
        if (PagesHeader == null)
            throw new InvalidOperationException();

        Children.Add(PagesHeader);
    }

    [Browsable(false)]
    public BaseObjectCollection<TabPage> Pages { get; }

    [Browsable(false)]
    public Stack PagesHeader { get; }

    [Browsable(false)]
    public Canvas PagesContent { get; }

    [Browsable(false)]
    public TabPage? SelectedPage => Pages.FirstOrDefault(p => p.Header.IsSelected);

    protected virtual BaseObjectCollection<TabPage> CreatePages() => [];
    protected virtual Stack CreatePagesHeader() => new() { Orientation = Orientation.Horizontal, LastChildFill = false };
    protected virtual Canvas CreatePagesContent() => new();

    protected virtual void OnPageAdded(object sender, ValueEventArgs<TabPage> e) => PageAdded?.Invoke(sender, e);
    protected virtual void OnPageRemoved(object sender, ValueEventArgs<TabPage> e) => PageRemoved?.Invoke(sender, e);
    protected virtual void OnPageSelected(object? sender, EventArgs e) => PageSelected?.Invoke(sender, e);

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
                    Invalidate(VisualPropertyInvalidateModes.Measure);
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

                if (e.NewStartingIndex >= PagesHeader.Children.Count)
                {
                    PagesHeader.Children.Add(page.Header);
                }
                else
                {
                    PagesHeader.Children.Insert(e.NewStartingIndex, page.Header);
                }

                if (e.NewStartingIndex >= PagesContent.Children.Count)
                {
                    PagesContent.Children.Add(page.Content);
                }
                else
                {
                    PagesContent.Children.Insert(e.NewStartingIndex, page.Content);
                }

                page.Header.HorizontalAlignment = Alignment.Near;
                page.Header.Panel.HorizontalAlignment = Alignment.Near;
                page.Header.SelectedButton.IsVisible = false;
                page.Header.IsSelectedChanged += OnHeaderIsSelectedChanged;
                page.Header.AccessKeys.Add(new AccessKey(VIRTUAL_KEY.VK_SPACE));
                if (SelectedPage == null)
                {
                    page.Header.IsSelected = true;
                }
                OnPageAdded(this, new ValueEventArgs<TabPage>(page));
            }
        }
    }

    private void OnHeaderIsSelectedChanged(object? sender, ValueEventArgs<bool> e)
    {
        if (e.Value)
        {
            foreach (var page in Pages)
            {
                if (!page.Header.Equals(sender))
                {
                    page.Header.IsSelected = false;
                }
            }
        }
        OnPageSelected(sender, e);
    }
}