namespace Wice;

/// <summary>
/// A binary on/off toggle visual composed of a path track and a circular button (knob).
/// </summary>
public partial class ToggleSwitch : ButtonBase, IValueable, ISelectable
{
    /// <summary>
    /// Dynamic property descriptor for <see cref="Value"/>.
    /// Changing this property invalidates rendering (<see cref="VisualPropertyInvalidateModes.Render"/>).
    /// </summary>
    public static VisualProperty ValueProperty { get; } = VisualProperty.Add<bool>(typeof(ToggleSwitch), nameof(Value), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Dynamic property descriptor for <see cref="OnButtonBrush"/>.
    /// Changing this property invalidates rendering (<see cref="VisualPropertyInvalidateModes.Render"/>).
    /// </summary>
    public static VisualProperty OnButtonBrushProperty { get; } = VisualProperty.Add<CompositionBrush>(typeof(ToggleSwitch), nameof(OnButtonBrush), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Dynamic property descriptor for <see cref="OffButtonBrush"/>.
    /// Changing this property invalidates rendering (<see cref="VisualPropertyInvalidateModes.Render"/>).
    /// </summary>
    public static VisualProperty OffButtonBrushProperty { get; } = VisualProperty.Add<CompositionBrush>(typeof(ToggleSwitch), nameof(OffButtonBrush), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Dynamic property descriptor for <see cref="OnPathBrush"/>.
    /// Changing this property invalidates rendering (<see cref="VisualPropertyInvalidateModes.Render"/>).
    /// </summary>
    public static VisualProperty OnPathBrushProperty { get; } = VisualProperty.Add<CompositionBrush>(typeof(ToggleSwitch), nameof(OnPathBrush), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Dynamic property descriptor for <see cref="OffPathBrush"/>.
    /// Changing this property invalidates rendering (<see cref="VisualPropertyInvalidateModes.Render"/>).
    /// </summary>
    public static VisualProperty OffPathBrushProperty { get; } = VisualProperty.Add<CompositionBrush>(typeof(ToggleSwitch), nameof(OffPathBrush), VisualPropertyInvalidateModes.Render);

    private event EventHandler<ValueEventArgs>? _valueChanged;
    event EventHandler<ValueEventArgs> IValueable.ValueChanged { add { _valueChanged += value; } remove { _valueChanged -= value; } }

    /// <summary>
    /// Occurs when <see cref="Value"/> changes.
    /// </summary>
    public event EventHandler<ValueEventArgs<bool>>? ValueChanged;

    /// <summary>
    /// Occurs when <see cref="ISelectable.IsSelected"/> changes and <see cref="ISelectable.RaiseIsSelectedChanged"/> is true.
    /// Mirrors <see cref="ValueChanged"/>.
    /// </summary>
    public event EventHandler<ValueEventArgs<bool>>? IsSelectedChanged;

    bool IValueable.CanChangeValue { get => IsEnabled; set => IsEnabled = value; }
    object IValueable.Value => Value;
    bool IValueable.TrySetValue(object? value)
    {
        if (value is bool b)
        {
            Value = b;
            return true;
        }

        if (Conversions.TryChangeType(value, out b))
        {
            Value = b;
            return true;
        }
        return false;
    }

    bool ISelectable.IsSelected { get => Value; set => Value = value; }

    /// <summary>
    /// Initializes a new instance of <see cref="ToggleSwitch"/>.
    /// Sets up child visuals, geometry updates on arrange, default alignment and tooltip content (On/Off).
    /// </summary>
    public ToggleSwitch()
    {
        OnThemeDpiEvent(null, ThemeDpiEventArgs.FromWindow(Window));

        Child = CreateContent();
        if (Child == null)
            throw new InvalidOperationException();

        Button = CreateButton();
        Path = CreatePath();

        Child.Arranged += (s, e) => UpdatePath();

        if (Path != null)
        {
            Child.Children.Add(Path);
        }

        if (Button != null)
        {
            Child.Children.Add(Button);
        }

#if DEBUG
        Child?.Name = nameof(ToggleSwitch) + nameof(Canvas);
        Path?.Name = nameof(ToggleSwitch) + nameof(Path);
        Button?.Name = nameof(ToggleSwitch) + nameof(Button);
#endif
#if NETFRAMEWORK
        var on = WindowsUtilities.LoadString("shell32.dll", 50225);
        var off = WindowsUtilities.LoadString("shell32.dll", 50224);
#else
        var on = DirectN.Extensions.Utilities.Extensions.LoadString("shell32.dll", 50225);
        var off = DirectN.Extensions.Utilities.Extensions.LoadString("shell32.dll", 50224);
#endif

        ToolTipContentCreator = tt => Window.CreateDefaultToolTipContent(tt, Value ? on : off);
    }

    /// <summary>
    /// Gets the visual representation associated with the button of the visual.
    /// </summary>
    [Browsable(false)]
    public Visual? Button { get; }

    /// <summary>
    /// Gets the visual representation associated with the button of the visual.
    /// </summary>
    [Browsable(false)]
    public Path? Path { get; }

    /// <summary>
    /// Gets or sets whether the visual sizes itself based on theme metrics on DPI/theme changes.
    /// Default is true.
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual bool AutoSize { get; set; } = true;

    /// <summary>
    /// Gets or sets whether <see cref="IValueable.ValueChanged"/> is raised automatically when <see cref="Value"/> changes.
    /// </summary>
    [Browsable(false)]
    public virtual bool RaiseValueChanged { get; set; } = true;

    /// <summary>
    /// Gets or sets whether <see cref="ISelectable.RaiseIsSelectedChanged"/> is raised automatically when <see cref="Value"/> changes.
    /// </summary>
    [Browsable(false)]
    public virtual bool RaiseIsSelectedChanged { get; set; } = true;

    /// <summary>
    /// Gets or sets the current on/off state of the control.
    /// Changing this property updates the knob alignment, invalidates rendering and raises <see cref="ValueChanged"/>.
    /// </summary>
    [Category(CategoryBehavior)]
    public bool Value { get => (bool)GetPropertyValue(ValueProperty)!; set => SetPropertyValue(ValueProperty, value); }

    /// <summary>
    /// Gets or sets the brush used to fill the knob when <see cref="Value"/> is true.
    /// When null, a brush based on <c>theme.UnselectedColor</c> is created.
    /// </summary>
    [Category(CategoryRender)]
    public CompositionBrush OnButtonBrush { get => (CompositionBrush)GetPropertyValue(OnButtonBrushProperty)!; set => SetPropertyValue(OnButtonBrushProperty, value); }

    /// <summary>
    /// Gets or sets the brush used to fill the knob when <see cref="Value"/> is false.
    /// When both this and <see cref="OffPathBrush"/> are null, falls back to a brush based on <c>theme.BorderColor</c>.
    /// </summary>
    [Category(CategoryRender)]
    public CompositionBrush OffButtonBrush { get => (CompositionBrush)GetPropertyValue(OffButtonBrushProperty)!; set => SetPropertyValue(OffButtonBrushProperty, value); }

    /// <summary>
    /// Gets or sets the brush used to fill the track when <see cref="Value"/> is true.
    /// When null, a brush based on <c>theme.SelectedColor</c> is created.
    /// </summary>
    [Category(CategoryRender)]
    public CompositionBrush OnPathBrush { get => (CompositionBrush)GetPropertyValue(OnPathBrushProperty)!; set => SetPropertyValue(OnPathBrushProperty, value); }

    /// <summary>
    /// Gets or sets the brush used to stroke the track when <see cref="Value"/> is false.
    /// When null and <see cref="OffButtonBrush"/> is also null, a brush based on <c>theme.BorderColor</c> is created.
    /// </summary>
    [Category(CategoryRender)]
    public CompositionBrush OffPathBrush { get => (CompositionBrush)GetPropertyValue(OffPathBrushProperty)!; set => SetPropertyValue(OffPathBrushProperty, value); }

    /// <summary>
    /// Creates a new visual element that represents the button component for the current context.
    /// </summary>
    /// <returns>A <see cref="Visual "/> instance that provides the visual representation of the button.</returns>
    protected virtual Visual CreateButton() => new Ellipse { HorizontalAlignment = Alignment.Near };

    /// <summary>
    /// Creates a new visual element that represents the path component around the button.
    /// </summary>
    /// <returns>A <see cref="Path"/> instance that provides the visual representation of the path.</returns>
    protected virtual Path CreatePath() => new();

    /// <summary>
    /// Creates the content visual for the visual.
    /// </summary>
    /// <returns>The visual to host inside the visual; never null.</returns>
    protected virtual Visual CreateContent() => new Canvas();

    /// <summary>
    /// Copies style-related properties from the specified base object to the current instance.
    /// </summary>
    /// <param name="baseObject">The base object from which to copy style properties. This parameter cannot be null.</param>
    /// <param name="options">Optional settings that determine how style properties are copied. If null, default options are used.</param>
    public virtual void CopyStyleFrom(BaseObject baseObject, BaseObjectSetOptions? options = null)
    {
        ExceptionExtensions.ThrowIfNull(baseObject, nameof(baseObject));
        OnButtonBrushProperty.CopyValue(baseObject, this, options);
        OffButtonBrushProperty.CopyValue(baseObject, this, options);
        OnPathBrushProperty.CopyValue(baseObject, this, options);
        OffPathBrushProperty.CopyValue(baseObject, this, options);
    }

    /// <summary>
    /// Updates the visual path and geometry of the toggle switch control to reflect the current theme and layout
    /// dimensions.
    /// </summary>
    protected virtual void UpdatePath()
    {
        if (Path == null || Button is not Ellipse ellipse)
            return;

        var theme = GetWindowTheme();

        var thickness = 0.5f;
        var ratio = theme.ToggleBorderRatio;
        if (ratio > 0)
        {
            thickness = Math.Max(thickness, ellipse.Height * ratio);
        }

        var ar = Child!.ArrangedRect;
        var geoSource = Application.CurrentResourceManager.GetToggleSwitchGeometrySource(Math.Max(0, ar.Width - thickness), Math.Max(0, ar.Height - thickness), thickness / 2);
        Path.GeometrySource2D = geoSource;

        var radiusRatio = Math.Max(theme.ToggleRadiusRatio, 0.4f);
        var radius = ar.Height / 2 - thickness / radiusRatio;
        ellipse.Radius = new Vector2(radius, radius);

        Path.StrokeThickness = Math.Max(0.5f, thickness);
    }

    /// <inheritdoc/>
    protected override void OnClick(object? sender, EventArgs e)
    {
        Value = !Value;
        base.OnClick(sender, e);
    }

    /// <inheritdoc/>
    protected override void Render()
    {
        if (Compositor == null)
            return;

        var theme = GetWindowTheme();
        if (Value)
        {
            Path?.StrokeBrush = null;
            Path?.FillBrush = OnPathBrush ?? Compositor.CreateColorBrush(theme.SelectedColor.ToColor());
            if (Button is Shape btn)
            {
                btn.FillBrush = OnButtonBrush ?? Compositor.CreateColorBrush(theme.UnselectedColor.ToColor());
            }
        }
        else
        {
            var path = OffPathBrush;
            var button = OffButtonBrush;
            if (path == null)
            {
                button ??= Compositor.CreateColorBrush(theme.BorderColor.ToColor());
                path = button;
            }
            else
            {
                button ??= path;
            }

            Path?.StrokeBrush = path;
            Path?.FillBrush = null;
            if (Button is Shape btn)
            {
                btn.FillBrush = button;
            }
        }
        base.Render();
    }

    /// <summary>
    /// Raises <see cref="IsSelectedChanged"/> when enabled via <see cref="ISelectable.RaiseIsSelectedChanged"/>.
    /// </summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">New selected value.</param>
    protected virtual void OnIsSelectedChanged(object? sender, ValueEventArgs<bool> e)
    {
        if (RaiseIsSelectedChanged)
        {
            IsSelectedChanged?.Invoke(sender, e);
        }
    }

    /// <summary>
    /// Raises <see cref="ValueChanged"/>, the explicit <see cref="IValueable.ValueChanged"/>, and then
    /// chains to <see cref="OnIsSelectedChanged(object?, ValueEventArgs{bool})"/>.
    /// </summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">New value.</param>
    protected virtual void OnValueChanged(object? sender, ValueEventArgs<bool> e)
    {
        if (RaiseValueChanged)
        {
            ValueChanged?.Invoke(sender, e);
            _valueChanged?.Invoke(sender, e);
        }

        OnIsSelectedChanged(sender, e);
    }

    /// <inheritdoc/>
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        if (property == ValueProperty)
        {
            Button?.HorizontalAlignment = (bool)value! ? Alignment.Far : Alignment.Near;
            OnValueChanged(this, new ValueEventArgs<bool>((bool)value!));
        }
        return true;
    }

    /// <inheritdoc/>
    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        OnThemeDpiEvent(Window, ThemeDpiEventArgs.FromWindow(Window));
        Window!.ThemeDpiEvent += OnThemeDpiEvent;
    }

    /// <inheritdoc/>
    protected override void OnDetachingFromComposition(object? sender, EventArgs e)
    {
        base.OnDetachingFromComposition(sender, e);
        Window!.ThemeDpiEvent -= OnThemeDpiEvent;
    }

    /// <summary>
    /// Applies automatic sizing based on current theme metrics when <see cref="AutoSize"/> is true.
    /// Sets width to twice the theme box size and height/knob size to the theme box size.
    /// </summary>
    /// <param name="sender">Event source (window).</param>
    /// <param name="e">Theme/DPI event args.</param>
    protected virtual void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
    {
        if (!AutoSize)
            return;

        var theme = GetWindowTheme();
        var boxSize = theme.BoxSize;
        Width = boxSize * 2;
        Height = boxSize;

        var btn = Button;
        if (btn != null)
        {
            btn.Width = boxSize;
            btn.Height = boxSize;
        }
    }
}
