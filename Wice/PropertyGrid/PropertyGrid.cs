namespace Wice.PropertyGrid;

public class PropertyGrid : Grid
{
    public static VisualProperty LiveSyncProperty { get; } = VisualProperty.Add(typeof(PropertyGrid), nameof(LiveSync), VisualPropertyInvalidateModes.Render, false);
    public static VisualProperty IsReadOnlyProperty { get; } = VisualProperty.Add(typeof(PropertyGrid), nameof(IsReadOnly), VisualPropertyInvalidateModes.Render, false);
    public static VisualProperty GroupByCategoryProperty { get; } = VisualProperty.Add(typeof(PropertyGrid), nameof(GroupByCategory), VisualPropertyInvalidateModes.Render, false);
    public static VisualProperty SelectedObjectProperty { get; } = VisualProperty.Add<object?>(typeof(PropertyGrid), nameof(SelectedObject), VisualPropertyInvalidateModes.Measure, null);
    public static VisualProperty CellMarginProperty { get; } = VisualProperty.Add(typeof(PropertyGrid), nameof(CellMargin), VisualPropertyInvalidateModes.Measure, new D2D_RECT_F());

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
        Splitter.Name ??= "splitter" + Environment.TickCount;
#endif
    }

    protected IDictionary<string, PropertyVisuals> PropertyVisuals { get; } = new ConcurrentDictionary<string, PropertyVisuals>();

    private static IEnumerable<PropertyGridProperty> EnumerateSourceProperties(PropertyGrid grid)
    {
        var properties = grid.Source?.Properties ?? [];
        return properties.OrderBy(p => p.DisplayName);
    }

    protected virtual Func<PropertyGrid, IEnumerable<PropertyGridProperty>>? PropertiesSelector { get; } = EnumerateSourceProperties;

    [Browsable(false)]
    public GridSplitter Splitter { get; }

    [Category(CategoryRender)]
    public string UnspecifiedCategoryName { get; set; }

    [Browsable(false)]
    public PropertyGridSource Source { get; private set; }

    [Category(CategoryBehavior)]
    public bool IsReadOnly { get => (bool)GetPropertyValue(IsReadOnlyProperty); set => SetPropertyValue(IsReadOnlyProperty, value); }

    [Category(CategoryBehavior)]
    public bool GroupByCategory { get => (bool)GetPropertyValue(GroupByCategoryProperty); set => SetPropertyValue(GroupByCategoryProperty, value); }

    [Category(CategoryLayout)]
    public D2D_RECT_F CellMargin { get => (D2D_RECT_F)GetPropertyValue(CellMarginProperty); set => SetPropertyValue(CellMarginProperty, value); }

    [Category(CategoryBehavior)]
    public bool LiveSync { get => (bool)GetPropertyValue(LiveSyncProperty); set => SetPropertyValue(LiveSyncProperty, value); }

    [Category(CategoryLive)]
    public object? SelectedObject { get => GetPropertyValue(SelectedObjectProperty); set => SetPropertyValue(SelectedObjectProperty, value); }

    protected override bool SetPropertyValue(BaseObjectProperty property, object value, BaseObjectSetOptions options = null)
    {
        object oldValue = null;
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

                pc = newValue as INotifyPropertyChanged;
                if (pc != null)
                {
                    pc.PropertyChanged += OnSelectedObjectPropertyChanged;
                }
            }

            BindSelectedObject();
            return true;
        }
        return true;
    }

    protected override void OnAttachedToComposition(object sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        Splitter.RenderBrush = Compositor.CreateColorBrush(GetWindowTheme().SplitterColor.ToColor());
    }

    protected virtual void OnSelectedObjectPropertyChanged(object sender, PropertyChangedEventArgs e) => GetVisuals(e.PropertyName)?.ValueVisual?.UpdateEditor();

    public PropertyVisuals GetVisuals(string propertyName)
    {
        if (propertyName == null)
            throw new ArgumentNullException(nameof(propertyName));

        PropertyVisuals.TryGetValue(propertyName, out var prop);
        return prop;
    }

    protected virtual PropertyGridSource CreateSource() => new(this, SelectedObject);
    protected virtual void BindSelectedObject()
    {
        Source = CreateSource();
        if (Source == null)
            throw new InvalidOperationException();

        BindDimensions();
    }

    public virtual EditorHost CreateEditorHost(PropertyValueVisual visual)
    {
        if (visual == null)
            throw new ArgumentNullException(nameof(visual));

        var host = new EditorHost
        {
#if DEBUG
            Name = "editor[" + visual.Property.Name + "]",
#endif

            EditorMode = EditorMode.NonModal,
            HorizontalAlignment = Alignment.Stretch
        };
        host.Header.Text.Text = visual.Property.TextValue;
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

    protected virtual PropertyValueVisual CreatePropertyValueVisual(PropertyGridProperty property)
    {
        if (property == null)
            throw new ArgumentNullException(nameof(property));

        var value = new PropertyValueVisual(property)
        {
            Margin = CellMargin,
            RenderBrush = RenderBrush
        };
        value.CreateEditor();
        return value;
    }

    protected virtual Visual CreatePropertyTextVisual(PropertyGridProperty property)
    {
        if (property == null)
            throw new ArgumentNullException(nameof(property));

        var text = new TextBox
        {
            IsEditable = false,
            Margin = CellMargin,
            TrimmingGranularity = DWRITE_TRIMMING_GRANULARITY.DWRITE_TRIMMING_GRANULARITY_CHARACTER,
            Text = property.DisplayName,
        };
        text.CopyFrom(this);
        return text;
    }

    protected virtual Visual CreateCategoryVisual(string category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        var text = new TextBox
        {
            IsEditable = false,
            TrimmingGranularity = DWRITE_TRIMMING_GRANULARITY.DWRITE_TRIMMING_GRANULARITY_CHARACTER,
            Text = category,
        };
        text.CopyFrom(this);
        text.FontWeight = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_BOLD;
        return text;
    }

    protected IEnumerable<PropertyGridProperty> EnumerateSourceProperties() => Source.Properties;

    protected virtual void BindDimensions()
    {
        Rows.Clear();
        foreach (var kv in PropertyVisuals)
        {
            if (kv.Value.Text != null)
            {
                Children.Remove(kv.Value.Text);
            }

            if (kv.Value.ValueVisual != null)
            {
                Children.Remove(kv.Value.ValueVisual);
            }
        }

        PropertyVisuals.Clear();
        if (Source == null)
            return;

        var rowIndex = 0;
        var selector = PropertiesSelector ?? EnumerateSourceProperties;

        if (GroupByCategory)
        {
            foreach (var category in selector(this)
                .GroupBy(p => p.Category ?? string.Empty)
                .OrderBy(g => g.Key))
            {
                if (!string.IsNullOrWhiteSpace(category.Key))
                {
                    rowIndex += AddCategoryVisualsToRow(category.Key, rowIndex);
                }

                foreach (var property in category.OrderBy(p => p.DisplayName))
                {
                    rowIndex += AddPropertyVisualsToRow(property, rowIndex);
                }
            }
        }
        else
        {
            foreach (var property in selector(this))
            {
                rowIndex += AddPropertyVisualsToRow(property, rowIndex);
            }
        }
    }

    protected virtual int AddCategoryVisualsToRow(string category, int rowIndex)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        if (rowIndex > 0)
        {
            var row = new GridRow();
            Rows.Add(row);
        }

        Rows[rowIndex].Size = float.NaN;

        var visual = CreateCategoryVisual(category) ?? throw new InvalidOperationException();
        SetRow(visual, rowIndex);
        SetColumnSpan(visual, Columns.Count);
        Children.Add(visual);
        return 1;
    }

    protected virtual int AddPropertyVisualsToRow(PropertyGridProperty property, int rowIndex)
    {
        if (property == null)
            throw new ArgumentNullException(nameof(property));

        if (property.Name == null)
            return 0;

        if (rowIndex > 0)
        {
            var row = new GridRow();
            Rows.Add(row);
        }

        Rows[rowIndex].Size = float.NaN;

        var visuals = new PropertyVisuals
        {
            Text = CreatePropertyTextVisual(property)
        };
        PropertyVisuals[property.Name] = visuals;

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
        return 1;
    }
}
