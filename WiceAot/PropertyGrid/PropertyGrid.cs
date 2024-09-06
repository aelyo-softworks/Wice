namespace Wice.PropertyGrid;

public partial class PropertyGrid<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : Grid
{
    public static VisualProperty LiveSyncProperty { get; } = VisualProperty.Add(typeof(PropertyGrid<T>), nameof(LiveSync), VisualPropertyInvalidateModes.Render, false);
    public static VisualProperty IsReadOnlyProperty { get; } = VisualProperty.Add(typeof(PropertyGrid<T>), nameof(IsReadOnly), VisualPropertyInvalidateModes.Render, false);
    public static VisualProperty GroupByCategoryProperty { get; } = VisualProperty.Add(typeof(PropertyGrid<T>), nameof(GroupByCategory), VisualPropertyInvalidateModes.Render, false);
    public static VisualProperty SelectedObjectProperty { get; } = VisualProperty.Add<object?>(typeof(PropertyGrid<T>), nameof(SelectedObject), VisualPropertyInvalidateModes.Measure, null);
    public static VisualProperty CellMarginProperty { get; } = VisualProperty.Add(typeof(PropertyGrid<T>), nameof(CellMargin), VisualPropertyInvalidateModes.Measure, new D2D_RECT_F());

    private readonly ConcurrentDictionary<string, PropertyVisuals<T>> _propertyVisuals = new();
    //private EditorHost _openedHost;

    public PropertyGrid()
    {
#if DEBUG
        Columns[0].Name = "namesCol";
#endif
        Columns[0].Size = float.NaN;
        var column = new GridColumn();
#if DEBUG
        column.Name = "splitterCol";
#endif
        Columns.Add(column);

        column = new GridColumn
        {
#if DEBUG
            Name = "valuesCol",
#endif
            Size = float.NaN
        };
        Columns.Add(column);

        Splitter = new GridSplitter();
        Children.Add(Splitter);
        SetColumn(Splitter, 1);
#if DEBUG
        Splitter.Name = "splitter" + Environment.TickCount;
#endif
    }

    [Browsable(false)]
    public GridSplitter Splitter { get; }

    [Category(CategoryRender)]
    public string? UnspecifiedCategoryName { get; set; }

    [Browsable(false)]
    public PropertyGridSource<T>? Source { get; private set; }

    [Browsable(false)]
    public PropertyGridCategorySource<T>? CategorySource { get; private set; }

    [Category(CategoryBehavior)]
    public bool IsReadOnly { get => (bool)GetPropertyValue(IsReadOnlyProperty)!; set => SetPropertyValue(IsReadOnlyProperty, value); }

    [Category(CategoryBehavior)]
    public bool GroupByCategory { get => (bool)GetPropertyValue(GroupByCategoryProperty)!; set => SetPropertyValue(GroupByCategoryProperty, value); }

    [Category(CategoryLayout)]
    public D2D_RECT_F CellMargin { get => (D2D_RECT_F)GetPropertyValue(CellMarginProperty)!; set => SetPropertyValue(CellMarginProperty, value); }

    [Category(CategoryBehavior)]
    public bool LiveSync { get => (bool)GetPropertyValue(LiveSyncProperty)!; set => SetPropertyValue(LiveSyncProperty, value); }

    [Category(CategoryLive)]
    public T? SelectedObject { get => (T?)GetPropertyValue(SelectedObjectProperty); set => SetPropertyValue(SelectedObjectProperty, value); }

    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        object? oldValue = null;
        if (property == SelectedObjectProperty)
        {
            oldValue = SelectedObject;
        }

        if (!base.SetPropertyValue(property, value, options))
            return false;

        if (property == LiveSyncProperty)
        {
            var source = Source;
            if (source != null)
            {
                var newValue = LiveSync;
                foreach (var prop in source.Properties)
                {
                    prop.LiveSync = newValue;
                }
            }
            return true;
        }

        if (property == SelectedObjectProperty)
        {
            if (oldValue is INotifyPropertyChanged pc)
            {
                pc.PropertyChanged -= OnSelectedObjectPropertyChanged;
            }

            var newValue = SelectedObject;
            if (newValue != null)
            {
                var roa = newValue.GetType().GetCustomAttribute<ReadOnlyAttribute>();
                IsReadOnly = roa?.IsReadOnly == true;

                if (newValue is INotifyPropertyChanged npc)
                {
                    npc.PropertyChanged += OnSelectedObjectPropertyChanged;
                }
            }

            BindSelectedObject();
            return true;
        }
        return true;
    }

    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        Splitter.RenderBrush = Compositor!.CreateColorBrush(Application.CurrentTheme.SplitterColor.ToColor());
    }

    protected virtual void OnSelectedObjectPropertyChanged(object? sender, PropertyChangedEventArgs e) => GetVisuals(e.PropertyName ?? string.Empty)?.ValueVisual?.UpdateEditor();

    public PropertyVisuals<T>? GetVisuals(string propertyName)
    {
        ArgumentNullException.ThrowIfNull(propertyName);
        _propertyVisuals.TryGetValue(propertyName, out var prop);
        return prop;
    }

    protected virtual PropertyGridSource<T> CreateSource() => new(this, SelectedObject);
    protected virtual PropertyGridCategorySource<T> CreateCategorySource() => new(this);
    protected virtual void BindSelectedObject()
    {
        Source = CreateSource();
        CategorySource = CreateCategorySource();
        BindDimensions();
    }

    public virtual EditorHost CreateEditorHost(PropertyValueVisual<T> visual)
    {
        ArgumentNullException.ThrowIfNull(visual);

        var host = new EditorHost
        {
#if DEBUG
            Name = "editor[" + visual.Property.Name + "]",
#endif

            EditorMode = EditorMode.NonModal,
            HorizontalAlignment = Alignment.Stretch
        };
        host.Header.Text.Text = visual.Property.TextValue ?? string.Empty;
        host.Header.Text.CopyFrom(this);
        host.Header.Panel.Margin = 0;
        host.Header.SelectedButtonText.Opacity = 0;
        host.MouseOverChanged += (s, e) => host.Header.SelectedButtonText.Opacity = host.IsMouseOver ? 1 : 0;
        host.Header.Text.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(TextBox.FontSize))
            {
                var size = host.Header.Text.FontSize;
                if (size.HasValue)
                {
                    host.Header.SelectedButtonText.FontSize = size.Value * 0.7f;
                }
                else
                {
                    host.Header.SelectedButtonText.FontSize = null;
                }
            }
        };

        return host;
    }

    protected virtual PropertyValueVisual<T> CreatePropertyValueVisual(PropertyGridProperty<T> property)
    {
        ArgumentNullException.ThrowIfNull(property);

        var value = new PropertyValueVisual<T>(property)
        {
            Margin = CellMargin,
            RenderBrush = RenderBrush
        };
        value.CreateEditor();
        return value;
    }

    protected virtual Visual CreatePropertyTextVisual(PropertyGridProperty<T> property)
    {
        ArgumentNullException.ThrowIfNull(property);

        var text = new TextBox
        {
            IsEditable = false,
            Margin = CellMargin,
            TrimmingGranularity = DWRITE_TRIMMING_GRANULARITY.DWRITE_TRIMMING_GRANULARITY_CHARACTER
        };
        text.CopyFrom(this);
        text.Text = property.DisplayName ?? string.Empty;
        return text;
    }

    protected virtual void BindDimensions()
    {
        Rows.Clear();
        _propertyVisuals.Clear();
        if (Source == null)
            return;

        var rowIndex = 0;
        foreach (var property in Source.Properties)
        {
            if (property.Name == null)
                continue;

            if (rowIndex > 0)
            {
                var row = new GridRow();
                Rows.Add(row);
            }

            Rows[rowIndex].Size = float.NaN;

            var visuals = new PropertyVisuals<T>
            {
                Text = CreatePropertyTextVisual(property)
            };
#if DEBUG
            visuals.Text.Name = "pgText#" + rowIndex + "[" + property.Name + "]";
#endif
            SetRow(visuals.Text, rowIndex);
            Children.Add(visuals.Text);

            visuals.ValueVisual = CreatePropertyValueVisual(property);
#if DEBUG
            visuals.ValueVisual.Name = "pgValue#" + rowIndex + "[" + property.Name + "]";
#endif
            SetColumn(visuals.ValueVisual, 2);
            SetRow(visuals.ValueVisual, rowIndex);
            Children.Add(visuals.ValueVisual);

            _propertyVisuals[property.Name] = visuals;
            rowIndex++;
        }
    }
}
