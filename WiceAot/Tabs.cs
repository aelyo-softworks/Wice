namespace Wice;

public partial class Tabs : Dock
{
    public event EventHandler<ValueEventArgs<TabPage>>? PageAdded;
    public event EventHandler<ValueEventArgs<TabPage>>? PageRemoved;
    public event EventHandler? SelectionChanged;

    public Tabs()
    {
        LastChildFill = true;
        Pages = CreatePages();
        Pages.CollectionChanged += (s, e) => OnPagesCollectionChanged(e);

        PagesHeader = CreatePagesHeader();
        if (PagesHeader == null)
            throw new InvalidOperationException();

#if DEBUG
        PagesHeader.Name ??= nameof(PagesHeader);
#endif

        SetDockType(PagesHeader, DockType.Top);
        Children.Add(PagesHeader);

        PagesContent = CreatePagesContent();
        PagesContent.MeasureToContent = DimensionOptions.WidthAndHeight;
        if (PagesContent == null)
            throw new InvalidOperationException();

#if DEBUG
        PagesContent.Name ??= nameof(PagesContent);
#endif

        SetDockType(PagesContent, DockType.Bottom);
        Children.Add(PagesContent);
    }

    [Browsable(false)]
    public BaseObjectCollection<TabPage> Pages { get; }

    [Browsable(false)]
    public Stack PagesHeader { get; }

    [Browsable(false)]
    public Canvas PagesContent { get; }

    [Browsable(false)]
    public TabPage? SelectedPage => Pages.FirstOrDefault(p => p.IsSelectable && p.Header.IsSelected);

    protected virtual BaseObjectCollection<TabPage> CreatePages() => [];
    protected virtual Stack CreatePagesHeader() => new() { Orientation = Orientation.Horizontal, LastChildFill = false };
    protected virtual Canvas CreatePagesContent() => new();

    protected virtual void OnPageAdded(object sender, ValueEventArgs<TabPage> e) => PageAdded?.Invoke(sender, e);
    protected virtual void OnPageRemoved(object sender, ValueEventArgs<TabPage> e) => PageRemoved?.Invoke(sender, e);
    protected virtual void OnSelectionChanged(object? sender, EventArgs e) => SelectionChanged?.Invoke(sender, e);

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
                    if (page.Content != null)
                    {
                        RemovePageContent(page, e.OldStartingIndex, page.Content);
                    }

                    Invalidate(VisualPropertyInvalidateModes.Measure);
                    OnPageRemoved(this, new ValueEventArgs<TabPage>(page));

                    // make sure we have at least one selected page if possible
                    if (SelectedPage == null)
                    {
                        var selectablePages = Pages.Where(p => p.IsSelectable).ToArray();
                        if (selectablePages.Length > 0)
                        {
                            if (selectablePages.Length == 1)
                            {
                                selectablePages[0].Header.IsSelected = true;
                            }
                            else
                            {
                                var index = Math.Min(e.OldStartingIndex, selectablePages.Length - 1);
                                selectablePages[index].Header.IsSelected = true;
                            }
                        }
                    }
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

                if (page.Content != null)
                {
                    AddPageContent(page, e.NewStartingIndex, page.Content);
                }

#if DEBUG
                page.Header.Name ??= "tabPageHeader#" + e.NewStartingIndex;
#endif

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

    protected virtual void RemovePageContent(TabPage page, int index, Visual content)
    {
        ExceptionExtensions.ThrowIfNull(page, nameof(page));
        ExceptionExtensions.ThrowIfNull(content, nameof(content));
        PagesContent.Children.Remove(content);
        page.RemovedFromTabs(this);
        if (content.DisposeOnDetachFromComposition != false && content is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    protected virtual void AddPageContent(TabPage page, int index, Visual content)
    {
        ExceptionExtensions.ThrowIfNull(page, nameof(page));
        ExceptionExtensions.ThrowIfNull(content, nameof(content));
        if (index >= PagesContent.Children.Count)
        {
            PagesContent.Children.Add(content);
        }
        else
        {
            PagesContent.Children.Insert(index, content);
        }

        content.IsVisible = page.IsSelectable && page.Header.IsSelected;
        Canvas.SetLeft(content, 0);
        Canvas.SetTop(content, 0);
#if DEBUG
        content.Name ??= "tabPageContent#" + index;
#endif
        page.AddedToTabs(this);
    }

    protected internal void OnPageContentChanged(TabPage page, Visual? newContent, Visual? oldContent)
    {
        ExceptionExtensions.ThrowIfNull(page, nameof(page));
        var index = page.Index;
        if (oldContent != null)
        {
            PagesContent.Children.Remove(oldContent);
        }

        if (newContent != null)
        {
            AddPageContent(page, index, newContent);
        }
    }

#pragma warning disable CA1822  // Mark members as static
    protected internal void OnPageIsSelectableChanged(TabPage page, bool newValue)
#pragma warning restore CA1822  // Mark members as static
    {
        ExceptionExtensions.ThrowIfNull(page, nameof(page));
        if (!page.Header.IsSelected)
            return;

        if (!newValue)
        {
            page.Header.IsSelected = false;
        }
    }

    private void OnHeaderIsSelectedChanged(object? sender, ValueEventArgs<bool> e)
    {
        if (e.Value)
        {
            foreach (var page in Pages)
            {
                if (!page.Header.Equals(sender) || !page.IsSelectable)
                {
                    page.Header.IsSelected = false;
                    if (page.Content != null)
                    {
                        page.Content.IsVisible = false;
                    }
                }
                else
                {
                    if (page.Content != null)
                    {
                        page.Content.IsVisible = true;
                    }
                }
            }
            OnSelectionChanged(sender, e);
        }
    }
}