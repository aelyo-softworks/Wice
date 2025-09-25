namespace Wice.PropertyGrid;

/// <summary>
/// Displays and edits the public properties of a selected object of type <typeparamref name="T"/> in a two-column grid:
/// - Left column shows property names.
/// - Right column hosts editors for property values.
/// </summary>
/// <typeparam name="T">
/// The selected object type. Public properties are required for trimming via <see cref="DynamicallyAccessedMemberTypes.PublicProperties"/>.
/// </typeparam>
#if NETFRAMEWORK
public partial class PropertyGrid : Grid
#else
public partial class PropertyGrid<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : Grid
#endif
{
    /// <summary>
    /// Visual property backing <see cref="LiveSync"/>. When enabled, editors update in response to source property changes.
    /// </summary>
#if NETFRAMEWORK
    public static VisualProperty LiveSyncProperty { get; } = VisualProperty.Add(typeof(PropertyGrid), nameof(LiveSync), VisualPropertyInvalidateModes.Render, false);
#else
    public static VisualProperty LiveSyncProperty { get; } = VisualProperty.Add(typeof(PropertyGrid<T>), nameof(LiveSync), VisualPropertyInvalidateModes.Render, false);
#endif

    /// <summary>
    /// Visual property backing <see cref="IsReadOnly"/>. When true, editors render in a disabled state.
    /// </summary>
#if NETFRAMEWORK
    public static VisualProperty IsReadOnlyProperty { get; } = VisualProperty.Add(typeof(PropertyGrid), nameof(IsReadOnly), VisualPropertyInvalidateModes.Render, false);
#else
    public static VisualProperty IsReadOnlyProperty { get; } = VisualProperty.Add(typeof(PropertyGrid<T>), nameof(IsReadOnly), VisualPropertyInvalidateModes.Render, false);
#endif

    /// <summary>
    /// Visual property backing <see cref="GroupByCategory"/>. When true, properties can be laid out by category.
    /// </summary>
#if NETFRAMEWORK
    public static VisualProperty GroupByCategoryProperty { get; } = VisualProperty.Add(typeof(PropertyGrid), nameof(GroupByCategory), VisualPropertyInvalidateModes.Render, false);
#else
    public static VisualProperty GroupByCategoryProperty { get; } = VisualProperty.Add(typeof(PropertyGrid<T>), nameof(GroupByCategory), VisualPropertyInvalidateModes.Render, false);
#endif

    /// <summary>
    /// Visual property backing <see cref="SelectedObject"/>. Changing this rebuilds the grid layout (measure invalidation).
    /// </summary>
#if NETFRAMEWORK
    public static VisualProperty SelectedObjectProperty { get; } = VisualProperty.Add<object?>(typeof(PropertyGrid), nameof(SelectedObject), VisualPropertyInvalidateModes.Measure, null);
#else
    public static VisualProperty SelectedObjectProperty { get; } = VisualProperty.Add<object?>(typeof(PropertyGrid<T>), nameof(SelectedObject), VisualPropertyInvalidateModes.Measure, null);
#endif

    /// <summary>
    /// Visual property backing <see cref="CellMargin"/>. Controls the margin applied to both name and value cells.
    /// </summary>
#if NETFRAMEWORK
    public static VisualProperty CellMarginProperty { get; } = VisualProperty.Add(typeof(PropertyGrid), nameof(CellMargin), VisualPropertyInvalidateModes.Measure, new D2D_RECT_F());
#else
    public static VisualProperty CellMarginProperty { get; } = VisualProperty.Add(typeof(PropertyGrid<T>), nameof(CellMargin), VisualPropertyInvalidateModes.Measure, new D2D_RECT_F());
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyGrid"/> class.
    /// </summary>
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

    /// <summary>
    /// Gets a dictionary that maps property names to their corresponding visual representations.
    /// </summary>
#if NETFRAMEWORK
    protected IDictionary<string, PropertyVisuals> PropertyVisuals { get; } = new ConcurrentDictionary<string, PropertyVisuals>();
#else
    protected IDictionary<string, PropertyVisuals<T>> PropertyVisuals { get; } = new ConcurrentDictionary<string, PropertyVisuals<T>>();
#endif

#if NETFRAMEWORK
    private static IEnumerable<PropertyGridProperty> EnumerateSourceProperties(PropertyGrid grid)
#else
    private static IEnumerable<PropertyGridProperty<T>> EnumerateSourceProperties(PropertyGrid<T> grid)
#endif
    {
        var properties = grid.Source?.Properties ?? [];
        return properties.OrderBy(p => p.DisplayName);
    }

    /// <summary>
    /// Gets the function used to select and enumerate properties for the <see cref="PropertyGrid{T}"/>.
    /// </summary>
#if NETFRAMEWORK
    protected virtual Func<PropertyGrid, IEnumerable<PropertyGridProperty>>? PropertiesSelector { get; } = EnumerateSourceProperties;
#else
    protected virtual Func<PropertyGrid<T>, IEnumerable<PropertyGridProperty<T>>>? PropertiesSelector { get; } = EnumerateSourceProperties;
#endif

    /// <summary>
    /// Gets the splitter that resizes the name and value columns.
    /// </summary>
    [Browsable(false)]
    public GridSplitter Splitter { get; }

    /// <summary>
    /// Gets or sets the display name for properties that do not specify a category.
    /// </summary>
    [Category(CategoryRender)]
    public string? UnspecifiedCategoryName { get; set; }

    /// <summary>
    /// Gets the current, flat property source for the selected object.
    /// </summary>
    [Browsable(false)]
#if NETFRAMEWORK
    public PropertyGridSource? Source { get; private set; }
#else
    public PropertyGridSource<T>? Source { get; private set; }
#endif

    /// <summary>
    /// Gets or sets a value indicating whether editing is disabled for all properties.
    /// </summary>
    [Category(CategoryBehavior)]
    public bool IsReadOnly { get => (bool)GetPropertyValue(IsReadOnlyProperty)!; set => SetPropertyValue(IsReadOnlyProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether properties should be grouped by their categories.
    /// </summary>
    [Category(CategoryBehavior)]
    public bool GroupByCategory { get => (bool)GetPropertyValue(GroupByCategoryProperty)!; set => SetPropertyValue(GroupByCategoryProperty, value); }

    /// <summary>
    /// Gets or sets the margin applied to both the name and value visuals in each row.
    /// </summary>
    [Category(CategoryLayout)]
    public D2D_RECT_F CellMargin { get => (D2D_RECT_F)GetPropertyValue(CellMarginProperty)!; set => SetPropertyValue(CellMarginProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether editors should live-update when the source object raises property changes.
    /// </summary>
    [Category(CategoryBehavior)]
    public bool LiveSync { get => (bool)GetPropertyValue(LiveSyncProperty)!; set => SetPropertyValue(LiveSyncProperty, value); }

    /// <summary>
    /// Gets or sets the object whose properties are shown and edited by this grid.
    /// </summary>
    [Category(CategoryLive)]
#if NETFRAMEWORK
    public object? SelectedObject { get => GetPropertyValue(SelectedObjectProperty); set => SetPropertyValue(SelectedObjectProperty, value); }
#else
    public T? SelectedObject { get => (T?)GetPropertyValue(SelectedObjectProperty); set => SetPropertyValue(SelectedObjectProperty, value); }
#endif

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        Splitter.RenderBrush = Compositor!.CreateColorBrush(GetWindowTheme().SplitterColor.ToColor());
    }

    /// <summary>
    /// Handles property changes from <see cref="SelectedObject"/> when <see cref="LiveSync"/> is enabled,
    /// by updating the corresponding editor visual (if present).
    /// </summary>
    /// <param name="sender">The selected object.</param>
    /// <param name="e">Property change event args.</param>
    protected virtual void OnSelectedObjectPropertyChanged(object? sender, PropertyChangedEventArgs e) => GetVisuals(e.PropertyName ?? string.Empty)?.ValueVisual?.UpdateEditor();

    /// <summary>
    /// Gets the UI visuals for a given property name.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The visuals for that property, or null if not found.</returns>
#if NETFRAMEWORK
    public PropertyVisuals? GetVisuals(string propertyName)
#else
    public PropertyVisuals<T>? GetVisuals(string propertyName)
#endif
    {
        ExceptionExtensions.ThrowIfNull(propertyName, nameof(propertyName));
        PropertyVisuals.TryGetValue(propertyName, out var prop);
        return prop;
    }

    /// <summary>
    /// Creates the flat property source for the current <see cref="SelectedObject"/>.
    /// </summary>
    /// <returns>The created source.</returns>
#if NETFRAMEWORK
    protected virtual PropertyGridSource CreateSource() => new(this, SelectedObject);
#else
    protected virtual PropertyGridSource<T> CreateSource() => new(this, SelectedObject);
#endif

    /// <summary>
    /// Rebuilds sources and relays out the grid for the current <see cref="SelectedObject"/>.
    /// </summary>
    protected virtual void BindSelectedObject()
    {
        Source = CreateSource();
        if (Source == null)
            throw new InvalidOperationException();

        BindDimensions();
    }

    /// <summary>
    /// Creates an <see cref="EditorHost"/> for the provided value visual and configures its header and interactions.
    /// </summary>
    /// <param name="visual">The value visual.</param>
    /// <returns>An editor host ready to display the property's editor.</returns>
#if NETFRAMEWORK
    public virtual EditorHost CreateEditorHost(PropertyValueVisual visual)
#else
    public virtual EditorHost CreateEditorHost(PropertyValueVisual<T> visual)
#endif
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));

        var host = new EditorHost
        {
#if DEBUG
            Name = "editor[" + visual.Property.Name + "]",
#endif

            EditorMode = EditorMode.NonModal,
        };

        host.Header.Panel.VerticalAlignment = Alignment.Center;
        host.Header.Text.IsFocusable = false;
        host.Header.Text.IsEnabled = false;
        host.Header.Text.Text = visual.Property.TextValue ?? string.Empty;
        host.Header.Text.CopyFrom(this);
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

    /// <summary>
    /// Creates the value visual for a property and initializes its editor.
    /// </summary>
    /// <param name="property">The property descriptor.</param>
    /// <returns>The created value visual.</returns>
#if NETFRAMEWORK
    protected virtual PropertyValueVisual CreatePropertyValueVisual(PropertyGridProperty property)
#else
    protected virtual PropertyValueVisual<T> CreatePropertyValueVisual(PropertyGridProperty<T> property)
#endif
    {
        ExceptionExtensions.ThrowIfNull(property, nameof(property));

#if NETFRAMEWORK
        var value = new PropertyValueVisual(property)
#else
        var value = new PropertyValueVisual<T>(property)
#endif
        {
            Margin = CellMargin,
            RenderBrush = RenderBrush
        };
        value.CreateEditor();
        return value;
    }

    /// <summary>
    /// Creates the read-only visual for a property's display name (left column).
    /// </summary>
    /// <param name="property">The property descriptor.</param>
    /// <returns>The created visual.</returns>
#if NETFRAMEWORK
    protected virtual Visual CreatePropertyTextVisual(PropertyGridProperty property)
#else
    protected virtual Visual CreatePropertyTextVisual(PropertyGridProperty<T> property)
#endif
    {
        ExceptionExtensions.ThrowIfNull(property, nameof(property));

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

    /// <summary>
    /// Creates the read-only visual for a property's category (left column).
    /// </summary>
    /// <param name="category">The category.</param>
    /// <returns>The created visual.</returns>
    protected virtual Visual CreateCategoryVisual(string category)
    {
        ExceptionExtensions.ThrowIfNull(category, nameof(category));

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

    /// <summary>
    /// Rebuilds grid rows and child visuals from the current <see cref="Source"/>.
    /// Clears existing rows and visuals, then creates a new row per property with its
    /// name visual (column 0) and value visual (column 2). Column 1 is the splitter.
    /// </summary>
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

    /// <summary>
    /// Adds a category visual element to the specified row in the grid.
    /// </summary>
    protected virtual int AddCategoryVisualsToRow(string category, int rowIndex)
    {
        ExceptionExtensions.ThrowIfNull(category, nameof(category));
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

    /// <summary>
    /// Adds the visual elements for a property to a specified row in the grid.
    /// </summary>
    /// <param name="property">The property for which visual elements are to be created and added. Cannot be <see langword="null"/>.</param>
    /// <param name="rowIndex">The index of the row where the visual elements will be added. Must be a valid row index.</param>
    /// <returns>The number of rows added. Returns 1 or more if the operation is successful; otherwise, 0.</returns>
#if NETFRAMEWORK
    protected virtual int AddPropertyVisualsToRow(PropertyGridProperty property, int rowIndex)
#else
    protected virtual int AddPropertyVisualsToRow(PropertyGridProperty<T> property, int rowIndex)
#endif
    {
        ExceptionExtensions.ThrowIfNull(property, nameof(property));
        if (property.Name == null)
            return 0;

        if (rowIndex > 0)
        {
            var row = new GridRow();
            Rows.Add(row);
        }

        Rows[rowIndex].Size = float.NaN;

#if NETFRAMEWORK
        var visuals = new PropertyVisuals()
#else
        var visuals = new PropertyVisuals<T>
#endif
        {
            Text = CreatePropertyTextVisual(property) ?? throw new InvalidOperationException()
        };
        PropertyVisuals[property.Name] = visuals;

#if DEBUG
        visuals.Text.Name = "pgText#" + rowIndex + "[" + property.Name + "]";
#endif
        SetRow(visuals.Text, rowIndex);
        Children.Add(visuals.Text);

        visuals.ValueVisual = CreatePropertyValueVisual(property) ?? throw new InvalidOperationException();
#if DEBUG
        visuals.ValueVisual.Name = "pgValue#" + rowIndex + "[" + property.Name + "]";
#endif
        SetColumn(visuals.ValueVisual, 2);
        SetRow(visuals.ValueVisual, rowIndex);
        Children.Add(visuals.ValueVisual);
        return 1;
    }
}
