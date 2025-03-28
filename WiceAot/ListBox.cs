namespace Wice
{
    public partial class ListBox : Visual, IDataSourceVisual, ISelectorVisual
    {
        public static VisualProperty DataSourceProperty { get; } = VisualProperty.Add<object>(typeof(ListBox), nameof(DataSource), VisualPropertyInvalidateModes.Measure);
        public static VisualProperty DataItemMemberProperty { get; } = VisualProperty.Add<string>(typeof(ListBox), nameof(DataItemMember), VisualPropertyInvalidateModes.Measure, convert: NullifyString);
        public static VisualProperty DataItemFormatProperty { get; } = VisualProperty.Add<string>(typeof(ListBox), nameof(DataItemFormat), VisualPropertyInvalidateModes.Measure, convert: NullifyString);
        public static VisualProperty DataBinderProperty { get; } = VisualProperty.Add<DataBinder>(typeof(ListBox), nameof(DataBinder), VisualPropertyInvalidateModes.Measure);
        public static VisualProperty IntegralHeightProperty { get; } = VisualProperty.Add(typeof(ListBox), nameof(IntegralHeight), VisualPropertyInvalidateModes.Measure, false);
        public static VisualProperty SelectionModeProperty { get; } = VisualProperty.Add(typeof(ListBox), nameof(SelectionMode), VisualPropertyInvalidateModes.Arrange, SelectionMode.Single);

        public event EventHandler<EventArgs>? SelectionChanged;
        public event EventHandler<EventArgs>? DataBound;
        public event EventHandler<ValueEventArgs<ListBoxDataBindContext>>? ItemDataBound;

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
        protected virtual bool RaiseOnSelectionChanged { get; set; }

        [Category(CategoryBehavior)]
        public SelectionMode SelectionMode { get => (SelectionMode)GetPropertyValue(SelectionModeProperty)!; set => SetPropertyValue(SelectionModeProperty, value); }

        [Category(CategoryBehavior)]
        public bool IntegralHeight { get => (bool)GetPropertyValue(IntegralHeightProperty)!; set => SetPropertyValue(IntegralHeightProperty, value); }

        [Category(CategoryBehavior)]
        public object? DataSource { get => GetPropertyValue(DataSourceProperty); set => SetPropertyValue(DataSourceProperty, value); }

        [Category(CategoryBehavior)]
        public string? DataItemMember { get => (string?)GetPropertyValue(DataItemMemberProperty); set => SetPropertyValue(DataItemMemberProperty, value); }

        [Category(CategoryBehavior)]
        public string? DataItemFormat { get => (string?)GetPropertyValue(DataItemFormatProperty); set => SetPropertyValue(DataItemFormatProperty, value); }

        [Browsable(false)]
        public DataBinder? DataBinder { get => (DataBinder?)GetPropertyValue(DataBinderProperty); set => SetPropertyValue(DataBinderProperty, value); }

        [Browsable(false)]
        public IEnumerable<ItemVisual> Items => Children.OfType<ItemVisual>();

        [Browsable(false)]
        public IEnumerable<ItemVisual> SelectedItems => Items.Where(v => v.IsSelected);

        [Browsable(false)]
        public ItemVisual? SelectedItem => Items.FirstOrDefault(v => v.IsSelected);

        protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
        {
            var children = VisibleChildren.ToArray();
            if (children.Length == 0)
                return base.MeasureCore(constraint);

            var childConstraint = new D2D_SIZE_F(constraint.width, constraint.height / children.Length);
            var maxChildDesiredWidth = 0f;
            var height = 0f;

            foreach (var child in children)
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

            foreach (var child in children)
            {
                var h = child.DesiredSize.height;
                var bounds = D2D_RECT_F.Sized(0, top, width, h);
                child.Arrange(bounds);
                top += h;
            }
        }

        protected virtual void OnDataBound(object? sender, EventArgs e) => DataBound?.Invoke(sender, e);
        protected virtual void OnItemDataBound(object sender, ValueEventArgs<ListBoxDataBindContext> e) => ItemDataBound?.Invoke(sender, e);
        protected virtual void OnSelectionChanged(object? sender, EventArgs e) => SelectionChanged?.Invoke(sender, e);

        protected virtual void OnSelectionChanged()
        {
            ValidateSelection();

            if (RaiseOnSelectionChanged)
            {
                OnSelectionChanged(this, EventArgs.Empty);
            }
        }

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

        protected override void OnAttachedToComposition(object? sender, EventArgs e)
        {
            base.OnAttachedToComposition(sender, e);
            BindDataSource();
        }

        public virtual Visual? GetVisualForMouseEvent(MouseEventArgs e)
        {
            if (e == null)
                return null;

            return ((IOneChildParent?)e.VisualsStack.OfType<Visual>().LastOrDefault(v => IsChild(v) && v is IOneChildParent))?.Child;
        }

        public virtual ItemVisual? GetItemVisual(object? obj)
        {
            if (obj is ItemVisual visual)
                return Items.FirstOrDefault(v => Equals(visual, v));

            return Items.FirstOrDefault(v => Equals(obj, v.Data));
        }

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

        public virtual bool UpdateItemSelection(ItemVisual visual, bool? select)
        {
            ArgumentNullException.ThrowIfNull(visual);

            var selected = visual.IsSelected;
            if (select.HasValue)
            {
                visual.IsSelected = select.Value;
            }

            if (visual.IsSelected)
            {
                visual.DoWhenAttachedToComposition(() =>
                {
                    visual.RenderBrush = Compositor?.CreateColorBrush(Application.CurrentTheme.SelectedColor.ToColor());
                });
            }
            else
            {
                visual.RenderBrush = Compositor?.CreateColorBrush(Application.CurrentTheme.ListBoxItemColor.ToColor());
            }

            return selected != visual.IsSelected;
        }

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
                //var children = new List<Visual>();
                if (ds != null)
                {
                    var binder = DataBinder ?? new DataBinder();
                    binder.ItemVisualCreator ??= CreateItemVisual;
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
                        if (ctx.DataVisual != null && binder.ItemVisualCreator != null)
                        {
                            binder.ItemVisualCreator(ctx);
                            var item = ctx.ItemVisual;
                            if (item != null)
                            {
                                item.Children.Add(ctx.DataVisual!);
                                Children.Add(item);
                                //children.Add(item);
                                binder.DataItemVisualBinder(ctx);
                                UpdateItemSelection(item, null);

                                OnItemDataBound(this, new ValueEventArgs<ListBoxDataBindContext>(ctx));

                                if (!isLast && lbdb != null && lbdb.SeparatorVisualCreator != null)
                                {
                                    lbdb.SeparatorVisualCreator(ctx);
                                    if (ctx.SeparatorVisual != null)
                                    {
                                        Children.Add(ctx.SeparatorVisual);
                                        //children.Add(ctx.SeparatorVisual);
                                    }
                                }
                            }
                        }
                    }
                }

                //Children.Clear();
                //Children.AddRange(children);

                OnDataBound(this, EventArgs.Empty);
                if (!selected.SequenceEqual(SelectedItems.Select(i => i.Data)))
                {
                    OnSelectionChanged();
                }
            }
        }

        protected virtual void BindDataItemVisual(DataBindContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.DataVisual is not TextBox tb)
                return;

            tb.Text = context.GetDisplayName();
        }

        protected virtual void CreateDataItemVisual(DataBindContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var visual = new TextBox
            {
                IsFocusable = true
            };
            context.DataVisual = visual;
        }

        protected virtual void CreateSeparatorVisual(DataBindContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return;
        }

        protected virtual ItemVisual NewItemVisual() => new();

        protected virtual void CreateItemVisual(DataBindContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var item = NewItemVisual();
            if (item == null)
                return;

#if DEBUG
            item.Name = nameof(ItemVisual) + string.Format(" '{0}'", context.Data);
#endif

            item.DataBinder = DataBinder;
            item.ColorAnimationDuration = Application.CurrentTheme.SelectionBrushAnimationDuration;
            item.RenderBrush = Compositor?.CreateColorBrush(Application.CurrentTheme.ListBoxItemColor.ToColor());

            void updateHover()
            {
                item.HoverRenderBrush = item.IsSelected || !IsEnabled ? null : Compositor?.CreateColorBrush(Application.CurrentTheme.ListBoxHoverColor.ToColor());
            }

            //item.SelectionChanged += (s, e) => updateHover();
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

                // review: do we really want to focus so early?
                //if (mode == SelectionMode.Single)
                //{
                //    item.Focus();
                //}
            }
            context.ItemVisual = item;
        }

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

        protected override void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (!IsEnabled)
                return;

            if (Children.Count > 0)
            {
                Visual? newChild = null;
                // note contrary to most UI, we loop when under or over, we don't stop
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
                                newChild = Children[^1];
                            }
                        }
                        e.Handled = true;
                        break;

                    case VIRTUAL_KEY.VK_HOME:
                        newChild = Children[0];
                        e.Handled = true;
                        break;

                    case VIRTUAL_KEY.VK_END:
                        newChild = Children[^1];
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
}
