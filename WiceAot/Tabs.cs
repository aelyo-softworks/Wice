namespace Wice;

/// <summary>
/// A tabbed container that manages a collection of <see cref="TabPage"/> instances, a header strip,
/// and a content host. The header strip hosts page headers (tabs) and the content host shows the
/// currently selected page's content.
/// </summary>
public partial class Tabs : Dock
{
    /// <summary>
    /// Raised after a <see cref="TabPage"/> has been added to <see cref="Pages"/> and to the visual tree
    /// (its header/content inserted and initialized).
    /// </summary>
    public event EventHandler<ValueEventArgs<TabPage>>? PageAdded;

    /// <summary>
    /// Raised after a <see cref="TabPage"/> has been removed from <see cref="Pages"/> and from the visual tree
    /// (its header removed and content detached/disposed as applicable).
    /// </summary>
    public event EventHandler<ValueEventArgs<TabPage>>? PageRemoved;

    /// <summary>
    /// Raised when the selected tab header changes to a different page.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="Tabs"/> control.
    /// Sets up the pages collection, header strip docked at the top, and content host docked at the bottom.
    /// </summary>
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

    /// <summary>
    /// Gets the collection of pages hosted by this control.
    /// </summary>
    [Browsable(false)]
    public BaseObjectCollection<TabPage> Pages { get; }

    /// <summary>
    /// Gets the header strip that hosts page headers (tabs). Generally docked at the top.
    /// </summary>
    [Browsable(false)]
    public Stack PagesHeader { get; }

    /// <summary>
    /// Gets the content host that holds each page content at its corresponding index.
    /// </summary>
    [Browsable(false)]
    public Canvas PagesContent { get; }

    /// <summary>
    /// Gets the currently selected page if any. Only considers pages that are selectable.
    /// </summary>
    [Browsable(false)]
    public TabPage? SelectedPage => Pages.FirstOrDefault(p => p.IsSelectable && p.Header.IsSelected);

    /// <summary>
    /// Creates the pages collection. Override to provide a custom collection type or behavior.
    /// </summary>
    /// <returns>A collection suitable for hosting <see cref="TabPage"/> items.</returns>
    protected virtual BaseObjectCollection<TabPage> CreatePages() => [];

    /// <summary>
    /// Creates the header strip that hosts page headers. Override to customize the header container.
    /// </summary>
    /// <returns>A <see cref="Stack"/> configured for horizontal layout without last-child fill.</returns>
    protected virtual Stack CreatePagesHeader() => new() { Orientation = Orientation.Horizontal, LastChildFill = false };

    /// <summary>
    /// Creates the content host that displays page contents. Override to customize the content container.
    /// </summary>
    /// <returns>A <see cref="Canvas"/> used as the pages content host.</returns>
    protected virtual Canvas CreatePagesContent() => new();

    /// <summary>
    /// Called after a page has been added to the control and initialized.
    /// </summary>
    /// <param name="sender">The source of the event, typically this control.</param>
    /// <param name="e">The added page.</param>
    protected virtual void OnPageAdded(object sender, ValueEventArgs<TabPage> e) => PageAdded?.Invoke(sender, e);

    /// <summary>
    /// Called after a page has been removed from the control and detached.
    /// </summary>
    /// <param name="sender">The source of the event, typically this control.</param>
    /// <param name="e">The removed page.</param>
    protected virtual void OnPageRemoved(object sender, ValueEventArgs<TabPage> e) => PageRemoved?.Invoke(sender, e);

    /// <summary>
    /// Called when the selected header changes. Override to react to selection changes.
    /// </summary>
    /// <param name="sender">The header or component that initiated the selection change.</param>
    /// <param name="e">Event args (unused).</param>
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

                    // Ensure at least one selectable page remains selected when possible.
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

        // Adds new pages (for Add/Replace): inserts headers/contents at proper index and initializes header behavior.
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

    /// <summary>
    /// Removes a page's content from the content host and optionally disposes it when configured.
    /// Invokes <see cref="TabPage.RemovedFromTabs(Tabs)"/> on the page.
    /// </summary>
    /// <param name="page">The page owning the content.</param>
    /// <param name="index">The index at which the page was removed.</param>
    /// <param name="content">The content visual to remove.</param>
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

    /// <summary>
    /// Inserts a page's content into the content host at the specified index, aligns its visibility to the
    /// page selection state, and positions it at origin. Invokes <see cref="TabPage.AddedToTabs(Tabs)"/>.
    /// </summary>
    /// <param name="page">The page owning the content.</param>
    /// <param name="index">Insertion index matching the header index.</param>
    /// <param name="content">The content visual to add.</param>
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

    /// <summary>
    /// Replaces a page's content in the content host, removing the old visual and inserting the new one at the
    /// same index while preserving visibility logic.
    /// </summary>
    /// <param name="page">The page whose content changed.</param>
    /// <param name="newContent">The new content visual (optional).</param>
    /// <param name="oldContent">The old content visual (optional).</param>
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
    /// <summary>
    /// Responds to a change in a page's <c>IsSelectable</c> flag. If the page is currently selected and becomes
    /// non-selectable, it is unselected to maintain valid selection invariants.
    /// </summary>
    /// <param name="page">The page whose selectability changed.</param>
    /// <param name="newValue">The new selectability value.</param>
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