namespace Wice.PropertyGrid;

/// <summary>
/// Displays and edits the public properties of a selected object of type <typeparamref name="T"/> in a two-column grid:
/// - Left column shows property names.
/// - Right column hosts editors for property values.
/// </summary>
/// <typeparam name="T">
/// The selected object type. Public properties are required for trimming via <see cref="DynamicallyAccessedMemberTypes.PublicProperties"/>.
/// </typeparam>
/// <remarks>
/// Key features:
/// - Live synchronization: updates editors when the selected object's properties change (when it implements <see cref="INotifyPropertyChanged"/>).
/// - Read-only detection via <see cref="ReadOnlyAttribute"/> on the selected object type.
/// - Optional grouping by category (infrastructure exposed; layout binding performed in <see cref="BindDimensions"/>).
/// - Splitter between name and value columns for runtime resizing.
/// </remarks>
public partial class PropertyGrid<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : Grid
{
    /// <summary>
    /// Visual property backing <see cref="LiveSync"/>. When enabled, editors update in response to source property changes.
    /// </summary>
    public static VisualProperty LiveSyncProperty { get; } = VisualProperty.Add(typeof(PropertyGrid<T>), nameof(LiveSync), VisualPropertyInvalidateModes.Render, false);

    /// <summary>
    /// Visual property backing <see cref="IsReadOnly"/>. When true, editors render in a disabled state.
    /// </summary>
    public static VisualProperty IsReadOnlyProperty { get; } = VisualProperty.Add(typeof(PropertyGrid<T>), nameof(IsReadOnly), VisualPropertyInvalidateModes.Render, false);

    /// <summary>
    /// Visual property backing <see cref="GroupByCategory"/>. When true, properties can be laid out by category.
    /// </summary>
    public static VisualProperty GroupByCategoryProperty { get; } = VisualProperty.Add(typeof(PropertyGrid<T>), nameof(GroupByCategory), VisualPropertyInvalidateModes.Render, false);

    /// <summary>
    /// Visual property backing <see cref="SelectedObject"/>. Changing this rebuilds the grid layout (measure invalidation).
    /// </summary>
    public static VisualProperty SelectedObjectProperty { get; } = VisualProperty.Add<object?>(typeof(PropertyGrid<T>), nameof(SelectedObject), VisualPropertyInvalidateModes.Measure, null);

    /// <summary>
    /// Visual property backing <see cref="CellMargin"/>. Controls the margin applied to both name and value cells.
    /// </summary>
    public static VisualProperty CellMarginProperty { get; } = VisualProperty.Add(typeof(PropertyGrid<T>), nameof(CellMargin), VisualPropertyInvalidateModes.Measure, new D2D_RECT_F());

    /// <summary>
    /// Maps property names to their corresponding name and value visuals (one row per property).
    /// </summary>
    private readonly ConcurrentDictionary<string, PropertyVisuals<T>> _propertyVisuals = new();

    /// <summary>
    /// Initializes the grid with three columns:
    /// - Column 0: property names (Auto size).
    /// - Column 1: splitter (fixed theme thickness).
    /// - Column 2: property editors (fills remaining space).
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
    public PropertyGridSource<T>? Source { get; private set; }

    /// <summary>
    /// Gets the current category source used to organize properties when <see cref="GroupByCategory"/> is enabled.
    /// </summary>
    [Browsable(false)]
    public PropertyGridCategorySource<T>? CategorySource { get; private set; }

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
    public T? SelectedObject { get => (T?)GetPropertyValue(SelectedObjectProperty); set => SetPropertyValue(SelectedObjectProperty, value); }

    /// <summary>
    /// Overrides property setting to:
    /// - Propagate <see cref="LiveSync"/> to all property descriptors.
    /// - Manage <see cref="INotifyPropertyChanged"/> subscriptions on <see cref="SelectedObject"/>.
    /// - Derive <see cref="IsReadOnly"/> from a <see cref="ReadOnlyAttribute"/> on the selected object's type.
    /// - Rebind layout and editors when <see cref="SelectedObject"/> changes.
    /// </summary>
    /// <param name="property">The property descriptor.</param>
    /// <param name="value">The new value.</param>
    /// <param name="options">Set options.</param>
    /// <returns>True if the underlying value changed; otherwise false.</returns>
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

    /// <summary>
    /// Applies theme-specific visuals once attached to composition (window exists).
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
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
    public PropertyVisuals<T>? GetVisuals(string propertyName)
    {
        ArgumentNullException.ThrowIfNull(propertyName);
        _propertyVisuals.TryGetValue(propertyName, out var prop);
        return prop;
    }

    /// <summary>
    /// Creates the flat property source for the current <see cref="SelectedObject"/>.
    /// </summary>
    /// <returns>The created source.</returns>
    protected virtual PropertyGridSource<T> CreateSource() => new(this, SelectedObject);

    /// <summary>
    /// Creates the category source used to organize properties.
    /// </summary>
    /// <returns>The created category source.</returns>
    protected virtual PropertyGridCategorySource<T> CreateCategorySource() => new(this);

    /// <summary>
    /// Rebuilds sources and relays out the grid for the current <see cref="SelectedObject"/>.
    /// </summary>
    protected virtual void BindSelectedObject()
    {
        Source = CreateSource();
        CategorySource = CreateCategorySource();
        BindDimensions();
    }

    /// <summary>
    /// Creates an <see cref="EditorHost"/> for the provided value visual and configures its header and interactions.
    /// </summary>
    /// <param name="visual">The value visual.</param>
    /// <returns>An editor host ready to display the property's editor.</returns>
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

    /// <summary>
    /// Creates the value visual for a property and initializes its editor.
    /// </summary>
    /// <param name="property">The property descriptor.</param>
    /// <returns>The created value visual.</returns>
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

    /// <summary>
    /// Creates the read-only text visual for a property's display name (left column).
    /// </summary>
    /// <param name="property">The property descriptor.</param>
    /// <returns>The created text visual.</returns>
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

    /// <summary>
    /// Rebuilds grid rows and child visuals from the current <see cref="Source"/>.
    /// Clears existing rows and visuals, then creates a new row per property with its
    /// name visual (column 0) and value visual (column 2). Column 1 is the splitter.
    /// </summary>
    protected virtual void BindDimensions()
    {
        Rows.Clear();
        foreach (var kv in _propertyVisuals)
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
            _propertyVisuals[property.Name] = visuals;

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

            rowIndex++;
        }
    }
}
