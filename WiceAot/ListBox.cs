namespace Wice;

/// <summary>
/// Vertical item presenter with selectable items and simple data-binding support.
/// </summary>
public partial class ListBox : Visual, IDataSourceVisual, ISelectorVisual
{
    /// <summary>
    /// Data source backing the list. Accepts any enumerable or a <see cref="DataSource"/>. Changes trigger Measure invalidation.
    /// </summary>
    public static VisualProperty DataSourceProperty { get; } = VisualProperty.Add<object>(typeof(ListBox), nameof(DataSource), VisualPropertyInvalidateModes.Measure);

    /// <summary>
    /// Optional member name projected from each item during enumeration. Null is normalized via <c>NullifyString</c>.
    /// </summary>
    public static VisualProperty DataItemMemberProperty { get; } = VisualProperty.Add<string>(typeof(ListBox), nameof(DataItemMember), VisualPropertyInvalidateModes.Measure, convert: NullifyString);

    /// <summary>
    /// Optional item display format applied after member projection (e.g., "{0:G}"). Null is normalized via <c>NullifyString</c>.
    /// </summary>
    public static VisualProperty DataItemFormatProperty { get; } = VisualProperty.Add<string>(typeof(ListBox), nameof(DataItemFormat), VisualPropertyInvalidateModes.Measure, convert: NullifyString);

    /// <summary>
    /// Binder used to create/bind item visuals. When null, a new <see cref="DataBinder"/> is used.
    /// </summary>
    public static VisualProperty DataBinderProperty { get; } = VisualProperty.Add<DataBinder>(typeof(ListBox), nameof(DataBinder), VisualPropertyInvalidateModes.Measure);

    /// <summary>
    /// When true, arranges to an integral multiple of the first item's height (trims bottom). Defaults to false.
    /// </summary>
    public static VisualProperty IntegralHeightProperty { get; } = VisualProperty.Add(typeof(ListBox), nameof(IntegralHeight), VisualPropertyInvalidateModes.Measure, false);

    /// <summary>
    /// Selection mode. Changing validates current selection and triggers arrange invalidation. Defaults to <see cref="SelectionMode.Single"/>.
    /// </summary>
    public static VisualProperty SelectionModeProperty { get; } = VisualProperty.Add(typeof(ListBox), nameof(SelectionMode), VisualPropertyInvalidateModes.Arrange, SelectionMode.Single);

    /// <summary>
    /// Raised when the selection set changes (after validation).
    /// </summary>
    public event EventHandler<EventArgs>? SelectionChanged;

    /// <summary>
    /// Raised after data-binding has rebuilt children.
    /// </summary>
    public event EventHandler<EventArgs>? DataBound;

    /// <summary>
    /// Raised for each item created/bound during data-binding.
    /// </summary>
    public event EventHandler<ValueEventArgs<ListBoxDataBindContext>>? ItemDataBound;

    /// <summary>
    /// Initializes a new <see cref="ListBox"/>. Focusable by default; centered alignment; pinned to top in a <see cref="Canvas"/>.
    /// </summary>
    public ListBox()
    {
        IsFocusable = true;
        RaiseOnSelectionChanged = true;
        VerticalAlignment = Alignment.Center; // by default we don't stretch to parent
        HorizontalAlignment = Alignment.Center;

        // make sure we show the beginning otherwise it will be centered vertically
        Canvas.SetTop(this, 0);
    }

    bool ISelectorVisual.RaiseOnSelectionChanged { get => RaiseOnSelectionChanged; set => RaiseOnSelectionChanged = value; }

    /// <summary>
    /// When true, <see cref="OnSelectionChanged()"/> raises <see cref="SelectionChanged"/> automatically.
    /// </summary>
    protected virtual bool RaiseOnSelectionChanged { get; set; }

    /// <summary>
    /// Gets or sets the selection mode (<see cref="SelectionMode.Single"/> or multi-select).
    /// </summary>
    [Category(CategoryBehavior)]
    public SelectionMode SelectionMode { get => (SelectionMode)GetPropertyValue(SelectionModeProperty)!; set => SetPropertyValue(SelectionModeProperty, value); }

    /// <summary>
    /// Gets or sets whether the arranged height snaps to an integral number of item heights.
    /// </summary>
    [Category(CategoryBehavior)]
    public bool IntegralHeight { get => (bool)GetPropertyValue(IntegralHeightProperty)!; set => SetPropertyValue(IntegralHeightProperty, value); }

    /// <summary>
    /// Gets or sets the data source. Can be any enumerable, array, or <see cref="DataSource"/>.
    /// Setting rebinds items.
    /// </summary>
    [Category(CategoryBehavior)]
    public object? DataSource { get => GetPropertyValue(DataSourceProperty); set => SetPropertyValue(DataSourceProperty, value); }

    /// <summary>
    /// Gets or sets the item member name to display/project during enumeration.
    /// </summary>
    [Category(CategoryBehavior)]
    public string? DataItemMember { get => (string?)GetPropertyValue(DataItemMemberProperty); set => SetPropertyValue(DataItemMemberProperty, value); }

    /// <summary>
    /// Gets or sets the format string applied to the projected value.
    /// </summary>
    [Category(CategoryBehavior)]
    public string? DataItemFormat { get => (string?)GetPropertyValue(DataItemFormatProperty); set => SetPropertyValue(DataItemFormatProperty, value); }

    /// <summary>
    /// Gets or sets the binder used for item creation/binding. When null, a <see cref="DataBinder"/> is created on demand.
    /// </summary>
    [Browsable(false)]
    public DataBinder? DataBinder { get => (DataBinder?)GetPropertyValue(DataBinderProperty); set => SetPropertyValue(DataBinderProperty, value); }

    /// <summary>
    /// Enumerates item visuals (excludes separator visuals).
    /// </summary>
    [Browsable(false)]
    public IEnumerable<ItemVisual> Items => Children.OfType<ItemVisual>();

    /// <summary>
    /// Enumerates selected item visuals.
    /// </summary>
    [Browsable(false)]
    public IEnumerable<ItemVisual> SelectedItems => Items.Where(v => v.IsSelected);

    /// <summary>
    /// Gets the first selected item or null.
    /// </summary>
    [Browsable(false)]
    public ItemVisual? SelectedItem => Items.FirstOrDefault(v => v.IsSelected);

    /// <inheritdoc />
    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
    {
        var children = VisibleChildren.ToArray();
        if (children.Length == 0)
            return base.MeasureCore(constraint);

        var childConstraint = new D2D_SIZE_F(constraint.width, constraint.height / children.Length);
        var maxChildDesiredWidth = 0f;
        var height = 0f;

        foreach (var child in children.Where(c => c.Parent != null))
        {
            child.Measure(childConstraint);
            var childDesiredSize = child.DesiredSize;

            if (maxChildDesiredWidth < childDesiredSize.width)
            {
                maxChildDesiredWidth = childDesiredSize.width;
            }

            height += childDesiredSize.height;
        }

        return new D2D_SIZE_F(maxChildDesiredWidth, height);
    }

    /// <inheritdoc />
    protected override void ArrangeCore(D2D_RECT_F finalRect)
    {
        if (IntegralHeight)
        {
            var first = VisibleChildren.FirstOrDefault();
            if (first != null)
            {
                var mod = finalRect.Height % first.DesiredSize.height;
                if (mod != 0)
                {
                    finalRect = new D2D_RECT_F(finalRect.left, finalRect.top, finalRect.right, finalRect.bottom - mod);
                    Height = finalRect.Height;
                }
            }
        }

        var children = VisibleChildren.ToArray();
        if (children.Length == 0)
        {
            base.ArrangeCore(finalRect);
            return;
        }

        var finalSize = finalRect.Size;
        var top = 0f;
        var width = finalSize.width;

        foreach (var child in children.Where(c => c.Parent != null))
        {
            var h = child.DesiredSize.height;
            var bounds = D2D_RECT_F.Sized(0, top, width, h);
            child.Arrange(bounds);
            top += h;
        }
    }

    /// <summary>
    /// Raises <see cref="DataBound"/>.
    /// </summary>
    protected virtual void OnDataBound(object? sender, EventArgs e) => DataBound?.Invoke(sender, e);

    /// <summary>
    /// Raises <see cref="ItemDataBound"/> for a newly created/bound item.
    /// </summary>
    protected virtual void OnItemDataBound(object sender, ValueEventArgs<ListBoxDataBindContext> e) => ItemDataBound?.Invoke(sender, e);

    /// <summary>
    /// Raises <see cref="SelectionChanged"/>.
    /// </summary>
    protected virtual void OnSelectionChanged(object? sender, EventArgs e) => SelectionChanged?.Invoke(sender, e);

    /// <summary>
    /// Validates selection then raises <see cref="SelectionChanged"/> if <see cref="RaiseOnSelectionChanged"/> is true.
    /// </summary>
    protected virtual void OnSelectionChanged()
    {
        ValidateSelection();

        if (RaiseOnSelectionChanged)
        {
            OnSelectionChanged(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Enforces <see cref="SelectionMode.Single"/> by clearing extra selected items if any.
    /// </summary>
    protected virtual void ValidateSelection()
    {
        if (SelectionMode == SelectionMode.Single)
        {
            var count = 0;
            var changed = false;
            foreach (var item in Items.ToArray())
            {
                if (item.IsSelected)
                {
                    count++;
                }

                if (count > 1)
                {
                    if (UpdateItemSelection(item, false))
                    {
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                OnSelectionChanged();
            }
        }
    }

    /// <inheritdoc />
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        if (property == SelectionModeProperty)
        {
            ValidateSelection();
        }
        else if (property == DataSourceProperty)
        {
            BindDataSource();
        }
        return true;
    }

    /// <inheritdoc />
    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        BindDataSource();
        OnThemeDpiEvent(Window, ThemeDpiEventArgs.FromWindow(Window));
        Window!.ThemeDpiEvent += OnThemeDpiEvent;
    }

    /// <inheritdoc />
    protected override void OnDetachingFromComposition(object? sender, EventArgs e)
    {
        base.OnDetachingFromComposition(sender, e);
        Window!.ThemeDpiEvent -= OnThemeDpiEvent;
    }

    /// <summary>
    /// Called when theme DPI changes. Override to adjust item visuals. No action by default.
    /// </summary>
    protected virtual void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
    {
        // do nothing by default
    }

    /// <summary>
    /// Returns the content visual for a mouse event hit by resolving the last <see cref="IOneChildParent"/> child.
    /// </summary>
    public virtual Visual? GetVisualForMouseEvent(MouseEventArgs e)
    {
        if (e == null)
            return null;

        return ((IOneChildParent?)e.VisualsStack.OfType<Visual>().LastOrDefault(v => IsChild(v) && v is IOneChildParent))?.Child;
    }

    /// <summary>
    /// Resolves an <see cref="ItemVisual"/> from either an <see cref="ItemVisual"/> instance or a data item.
    /// </summary>
    public virtual ItemVisual? GetItemVisual(object? obj)
    {
        if (obj is ItemVisual visual)
            return Items.FirstOrDefault(v => Equals(visual, v));

        return Items.FirstOrDefault(v => Equals(obj, v.Data));
    }

    /// <summary>
    /// Toggles selection for the specified item or data item.
    /// </summary>
    public virtual void Toggle(object? obj) => Toggle(GetItemVisual(obj));
    private void Toggle(ItemVisual? obj)
    {
        if (obj == null)
            return;

        if (obj.IsSelected)
        {
            UnselectChild(obj);
        }
        else
        {
            SelectChild(obj);
        }
    }

    /// <summary>
    /// Unselects the specified item or data item.
    /// </summary>
    public virtual void Unselect(object? obj) => UnselectChild(GetItemVisual(obj));
    private void UnselectChild(ItemVisual? obj)
    {
        if (obj == null)
            return;

        if (UpdateItemSelection(obj, false))
        {
            OnSelectionChanged();
        }
    }

    /// <summary>
    /// Selects the specified item or data item.
    /// </summary>
    public virtual void Select(object? obj) => SelectChild(GetItemVisual(obj));
    private void SelectChild(ItemVisual? obj)
    {
        if (obj == null)
            return;

        var changed = false;
        if (SelectionMode == SelectionMode.Single)
        {
            foreach (var item in Items.ToArray())
            {
                if (UpdateItemSelection(item, item == obj))
                {
                    changed = true;
                }
            }
        }
        else
        {
            if (UpdateItemSelection(obj, true))
            {
                changed = true;
            }
        }

        if (changed)
        {
            OnSelectionChanged();
        }
    }

    /// <summary>
    /// Applies a selection state to an item visual and updates its brushes. When <paramref name="select"/> is null,
    /// only visual state (brush) is refreshed. Returns true when selection state changed.
    /// </summary>
    public virtual bool UpdateItemSelection(ItemVisual visual, bool? select)
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));

        var selected = visual.IsSelected;
        if (select.HasValue)
        {
            visual.IsSelected = select.Value;
        }

        if (visual.IsSelected)
        {
            visual.DoWhenAttachedToComposition(() =>
            {
                visual.RenderBrush = Compositor?.CreateColorBrush(GetWindowTheme().SelectedColor.ToColor());
            });
        }
        else
        {
            visual.RenderBrush = Compositor?.CreateColorBrush(GetWindowTheme().ListBoxItemColor.ToColor());
        }

        return selected != visual.IsSelected;
    }

    /// <summary>
    /// Selects a set of visuals. Raises <see cref="SelectionChanged"/> once if any change occurred.
    /// </summary>
    public virtual void Select(IEnumerable<ItemVisual> visuals)
    {
        if (visuals == null)
            return;

        var once = false;
        foreach (var visual in visuals.OfType<ItemVisual>())
        {
            if (visual == null)
                continue;

            Select(visual);
            once = true;
        }

        if (once)
        {
            OnSelectionChanged();
        }
    }

    /// <summary>
    /// Unselects a set of visuals. Raises <see cref="SelectionChanged"/> once if any change occurred.
    /// </summary>
    public virtual void Unselect(IEnumerable<ItemVisual> visuals)
    {
        if (visuals == null)
            return;

        var once = false;
        foreach (var visual in visuals.OfType<ItemVisual>())
        {
            if (visual == null)
                continue;

            Unselect(visual);
            once = true;
        }

        if (once)
        {
            OnSelectionChanged();
        }
    }

    /// <summary>
    /// Selects all items. In single-select mode, only the first item is selected.
    /// </summary>
    public virtual void SelectAll()
    {
        var changed = false;
        var mode = SelectionMode;
        foreach (var item in Items)
        {
            if (UpdateItemSelection(item, true))
            {
                changed = true;
            }
            if (mode == SelectionMode.Single)
                break;
        }

        if (changed)
        {
            OnSelectionChanged();
        }
    }

    /// <summary>
    /// Unselects all items.
    /// </summary>
    public virtual void UnselectAll()
    {
        var changed = false;
        foreach (var item in Items)
        {
            if (UpdateItemSelection(item, false))
            {
                changed = true;
            }
        }

        if (changed)
        {
            OnSelectionChanged();
        }
    }

    /// <summary>
    /// Ensures the specified item (or its data) is scrolled into the viewable area of a parent <see cref="ScrollViewer"/>.
    /// </summary>
    /// <returns>True if a parent <see cref="ScrollViewer"/> was found; otherwise, false.</returns>
    public virtual bool ScrollIntoView(object obj)
    {
        if (Parent?.Parent is not ScrollViewer sv)
            return false;

        var visual = GetItemVisual(obj);
        if (visual == null)
            return false;

        if (sv.ArrangedRect.IsValid)
        {
            sv.VerticalOffset = visual.ArrangedRect.bottom + Margin.top - sv.ArrangedRect.Height;
            sv.HorizontalOffset = visual.ArrangedRect.right + Margin.left - sv.ArrangedRect.Width;
        }
        else
        {
            sv.Arranged += onArranged;
        }

        void onArranged(object? sender, EventArgs e)
        {
            sv.Arranged -= onArranged;
            ScrollIntoView(visual);
        }
        return true;
    }

    void IDataSourceVisual.BindDataSource() => BindDataSource();

    /// <summary>
    /// Binds the data source to the visual elements of the control, creating and configuring item visuals based on the
    /// data source and binding logic.
    /// </summary>
    protected virtual void BindDataSource()
    {
        var ds = DataSource;
        if (ds is not DataSource source)
        {
            source = new DataSource(ds);
        }

        updateSource();
        source.SourceChanged += (s, e) => updateSource();

        void updateSource()
        {
            if (Compositor == null)
                return;

            var selected = SelectedItems.Select(i => i.Data).ToArray();
            Children.Clear();
            if (ds != null)
            {
                var binder = DataBinder ?? new DataBinder();
                binder.ItemVisualAdder ??= AddItemVisual;
                binder.DataItemVisualCreator ??= CreateDataItemVisual;
                binder.DataItemVisualBinder ??= BindDataItemVisual;
                var lbdb = binder as ListBoxDataBinder;
                if (lbdb != null)
                {
                    lbdb.SeparatorVisualCreator ??= CreateSeparatorVisual;
                }

                var options = new DataSourceEnumerateOptions { Member = DataItemMember, Format = DataItemFormat };
                object? last = null;
                var lastSet = false;
                foreach (var data in source.Enumerate(options))
                {
                    if (lastSet)
                    {
                        bindItem(last, false);
                    }
                    last = data;
                    lastSet = true;
                }

                if (lastSet)
                {
                    bindItem(last, true);
                }

                void bindItem(object? data, bool isLast)
                {
                    var ctx = new ListBoxDataBindContext(data, Children.Count, isLast);
                    binder.DataItemVisualCreator(ctx);
                    if (ctx.DataVisual != null && binder.ItemVisualAdder != null)
                    {
                        binder.ItemVisualAdder(ctx);
                        var item = ctx.ItemVisual;
                        if (item != null)
                        {
                            item.Children.Add(ctx.DataVisual!);
                            Children.Add(item);
                            binder.DataItemVisualBinder(ctx);
                            UpdateItemSelection(item, null);

                            OnItemDataBound(this, new ValueEventArgs<ListBoxDataBindContext>(ctx));

                            if (!isLast && lbdb != null && lbdb.SeparatorVisualCreator != null)
                            {
                                lbdb.SeparatorVisualCreator(ctx);
                                if (ctx.SeparatorVisual != null)
                                {
                                    Children.Add(ctx.SeparatorVisual);
                                }
                            }
                        }
                    }
                }
            }

            OnDataBound(this, EventArgs.Empty);
            if (!selected.SequenceEqual(SelectedItems.Select(i => i.Data)))
            {
                OnSelectionChanged();
            }
        }
    }

    /// <summary>
    /// Binds the data item visual representation to the specified data binding context.
    /// </summary>
    /// <param name="context">The data binding context that provides the visual element and associated data. Cannot be <see langword="null"/>.</param>
    protected virtual void BindDataItemVisual(DataBindContext context)
    {
        ExceptionExtensions.ThrowIfNull(context, nameof(context));
        if (context.DataVisual is not TextBox tb)
            return;

        tb.Text = context.GetDisplayName();
    }

    /// <summary>
    /// Default creator: creates a focusable <see cref="TextBox"/> for data display.
    /// </summary>
    protected virtual void CreateDataItemVisual(DataBindContext context)
    {
        ExceptionExtensions.ThrowIfNull(context, nameof(context));
        var visual = new TextBox { IsEnabled = false, IsFocusable = true };
        context.DataVisual = visual;
    }

    /// <summary>
    /// Default separator creator: no-op. Override in <see cref="ListBoxDataBinder"/> scenarios to add separators.
    /// </summary>
    protected virtual void CreateSeparatorVisual(DataBindContext context)
    {
        ExceptionExtensions.ThrowIfNull(context, nameof(context));
        return;
    }

    /// <summary>
    /// Creates a new instance of an <see cref="ItemVisual"/> to represent an item.
    /// </summary>
    /// <returns>A new instance of <see cref="ItemVisual"/> representing the item.</returns>
    protected virtual ItemVisual CreateItemVisual() => new();

    /// <summary>
    /// Adds a new visual representation of an item to the control, binding it to the provided data context.
    /// </summary>
    /// <param name="context">The data binding context that provides the data and receives the created visual item. Cannot be <see
    /// langword="null"/>.</param>
    protected virtual void AddItemVisual(DataBindContext context)
    {
        ExceptionExtensions.ThrowIfNull(context, nameof(context));
        var item = CreateItemVisual();
        if (item == null)
            return;

#if DEBUG
        item.Name ??= nameof(ItemVisual) + string.Format(" '{0}'", context.Data);
#endif

        item.ReceivesInputEvenWithModalShown = ReceivesInputEvenWithModalShown;
        item.DataBinder = DataBinder;
        item.ColorAnimationDuration = GetWindowTheme().SelectionBrushAnimationDuration;
        item.RenderBrush = Compositor?.CreateColorBrush(GetWindowTheme().ListBoxItemColor.ToColor());

        void updateHover()
        {
            item.HoverRenderBrush = item.IsSelected || !IsEnabled ? null : Compositor?.CreateColorBrush(GetWindowTheme().ListBoxHoverColor.ToColor());
        }

        item.IsSelectedChanged += OnItemIsSelectedChanged;
        item.MouseOverChanged += (s, e) => updateHover();
        item.Data = context.Data;

        var mode = SelectionMode;
        if (context.Data is ISelectable selectable && selectable.IsSelected)
        {
            if (UpdateItemSelection(item, true))
            {
                OnSelectionChanged();
            }
        }
        context.ItemVisual = item;
    }

    /// <summary>
    /// Syncs selection when the item container selection state changes.
    /// </summary>
    protected virtual void OnItemIsSelectedChanged(object? sender, ValueEventArgs<bool> e)
    {
        if (e.Value)
        {
            Select(sender);
        }
        else
        {
            Unselect(sender);
        }
    }

    private int GetFocusedIndex() => Children.IndexOf(c => c.IsFocusedOrAnyChildrenFocused);

    /// <inheritdoc/>
    protected override void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (!IsEnabled)
            return;

        if (Children.Count > 0)
        {
            Visual? newChild = null;
            int index;
            switch (e.Key)
            {
                case VIRTUAL_KEY.VK_DOWN:
                    index = GetFocusedIndex();
                    if (index < 0)
                    {
                        newChild = Children[0];
                    }
                    else
                    {
                        if ((index + 1) < Children.Count)
                        {
                            newChild = Children[index + 1];
                        }
                        else
                        {
                            newChild = Children[0];
                        }
                    }
                    e.Handled = true;
                    break;

                case VIRTUAL_KEY.VK_UP:
                    index = GetFocusedIndex();
                    if (index < 0)
                    {
                        newChild = Children[0];
                    }
                    else
                    {
                        if ((index - 1) >= 0)
                        {
                            newChild = Children[index - 1];
                        }
                        else
                        {
#if NETFRAMEWORK
                            newChild = Children[Children.Count - 1];
#else
                            newChild = Children[^1];
#endif
                        }
                    }
                    e.Handled = true;
                    break;

                case VIRTUAL_KEY.VK_HOME:
                    newChild = Children[0];
                    e.Handled = true;
                    break;

                case VIRTUAL_KEY.VK_END:
#if NETFRAMEWORK
                    newChild = Children[Children.Count - 1];
#else
                    newChild = Children[^1];
#endif
                    e.Handled = true;
                    break;

                case VIRTUAL_KEY.VK_SPACE:
                    index = GetFocusedIndex();
                    if (index >= 0)
                    {
                        Toggle(Children[index] as ItemVisual);
                    }
                    e.Handled = true;
                    break;
            }

            if (newChild != null)
            {
                newChild.Focus();
                ScrollIntoView(newChild);
                if (SelectionMode == SelectionMode.Single && newChild is ItemVisual itemVisual)
                {
                    SelectChild(itemVisual);
                }
            }
        }
        base.OnKeyDown(sender, e);
    }
}
