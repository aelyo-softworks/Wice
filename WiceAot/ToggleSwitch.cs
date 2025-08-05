namespace Wice;

public partial class ToggleSwitch : ButtonBase, IValueable, ISelectable
{
    public static VisualProperty ValueProperty { get; } = VisualProperty.Add<bool>(typeof(ToggleSwitch), nameof(Value), VisualPropertyInvalidateModes.Render);
    public static VisualProperty OnButtonBrushProperty { get; } = VisualProperty.Add<CompositionBrush>(typeof(ToggleSwitch), nameof(OnButtonBrush), VisualPropertyInvalidateModes.Render);
    public static VisualProperty OffButtonBrushProperty { get; } = VisualProperty.Add<CompositionBrush>(typeof(ToggleSwitch), nameof(OffButtonBrush), VisualPropertyInvalidateModes.Render);
    public static VisualProperty OnPathBrushProperty { get; } = VisualProperty.Add<CompositionBrush>(typeof(ToggleSwitch), nameof(OnPathBrush), VisualPropertyInvalidateModes.Render);
    public static VisualProperty OffPathBrushProperty { get; } = VisualProperty.Add<CompositionBrush>(typeof(ToggleSwitch), nameof(OffPathBrush), VisualPropertyInvalidateModes.Render);

    private readonly Canvas _canvas = new();
    private readonly Path _path = new();
    private readonly Ellipse _button = new();

    private event EventHandler<ValueEventArgs>? _valueChanged;
    event EventHandler<ValueEventArgs> IValueable.ValueChanged { add { _valueChanged += value; } remove { _valueChanged -= value; } }

    public event EventHandler<ValueEventArgs<bool>>? ValueChanged;
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

    bool ISelectable.RaiseIsSelectedChanged { get; set; }
    bool ISelectable.IsSelected { get => Value; set => Value = value; }

    [Category(CategoryBehavior)]
    public virtual bool AutoSize { get; set; } = true;

    public ToggleSwitch()
    {
        OnThemeDpiEvent(null, ThemeDpiEventArgs.FromWindow(Window));

        _button.HorizontalAlignment = Alignment.Near;

        _canvas.Arranged += (s, e) =>
        {
            var theme = GetWindowTheme();

            var thickness = 0.5f;
            var ratio = theme.ToggleBorderRatio;
            if (ratio > 0)
            {
                thickness = Math.Max(thickness, _button.Height * ratio);
            }

            var ar = _canvas.ArrangedRect;
            var geoSource = Application.CurrentResourceManager.GetToggleSwitchGeometrySource(ar.Width - thickness, ar.Height - thickness, thickness / 2);
            _path.GeometrySource2D = geoSource;

            var radiusRatio = Math.Max(theme.ToggleRadiusRatio, 0.4f);
            var radius = ar.Height / 2 - thickness / radiusRatio;
            _button.Radius = new Vector2(radius, radius);

            _path.StrokeThickness = Math.Max(0.5f, thickness);
        };

        _canvas.Children.Add(_path);
        _canvas.Children.Add(_button);

#if DEBUG
        _canvas.Name = nameof(ToggleSwitch) + nameof(_canvas);
        _path.Name = nameof(ToggleSwitch) + nameof(_path);
        _button.Name = nameof(ToggleSwitch) + nameof(_button);
#endif
        Child = _canvas;
#if NETFRAMEWORK
        var on = WindowsUtilities.LoadString("shell32.dll", 50225);
        var off = WindowsUtilities.LoadString("shell32.dll", 50224);
#else
        var on = DirectN.Extensions.Utilities.Extensions.LoadString("shell32.dll", 50225);
        var off = DirectN.Extensions.Utilities.Extensions.LoadString("shell32.dll", 50224);
#endif

        ToolTipContentCreator = tt => Window.CreateDefaultToolTipContent(tt, Value ? on : off);
    }

    [Category(CategoryBehavior)]
    public bool Value { get => (bool)GetPropertyValue(ValueProperty)!; set => SetPropertyValue(ValueProperty, value); }

    [Category(CategoryRender)]
    public CompositionBrush OnButtonBrush { get => (CompositionBrush)GetPropertyValue(OnButtonBrushProperty)!; set => SetPropertyValue(OnButtonBrushProperty, value); }

    [Category(CategoryRender)]
    public CompositionBrush OffButtonBrush { get => (CompositionBrush)GetPropertyValue(OffButtonBrushProperty)!; set => SetPropertyValue(OffButtonBrushProperty, value); }

    [Category(CategoryRender)]
    public CompositionBrush OnPathBrush { get => (CompositionBrush)GetPropertyValue(OnPathBrushProperty)!; set => SetPropertyValue(OnPathBrushProperty, value); }

    [Category(CategoryRender)]
    public CompositionBrush OffPathBrush { get => (CompositionBrush)GetPropertyValue(OffPathBrushProperty)!; set => SetPropertyValue(OffPathBrushProperty, value); }

    protected override void OnClick(object? sender, EventArgs e)
    {
        Value = !Value;
        base.OnClick(sender, e);
    }

    protected override void Render()
    {
        if (Compositor == null)
            return;

        var theme = GetWindowTheme();
        if (Value)
        {
            _path.StrokeBrush = null;
            _path.FillBrush = OnPathBrush ?? Compositor.CreateColorBrush(theme.SelectedColor.ToColor());
            _button.FillBrush = OnButtonBrush ?? Compositor.CreateColorBrush(theme.UnselectedColor.ToColor());
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

            _path.StrokeBrush = path;
            _path.FillBrush = null;
            _button.FillBrush = button;
        }
        base.Render();
    }

    protected virtual void OnIsSelectedChanged(object? sender, ValueEventArgs<bool> e)
    {
        if (((ISelectable)this).RaiseIsSelectedChanged)
        {
            IsSelectedChanged?.Invoke(sender, e);
        }
    }

    protected virtual void OnValueChanged(object? sender, ValueEventArgs<bool> e)
    {
        ValueChanged?.Invoke(sender, e);
        _valueChanged?.Invoke(sender, e);
        OnIsSelectedChanged(sender, e);
    }

    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        if (property == ValueProperty)
        {
            _button.HorizontalAlignment = (bool)value! ? Alignment.Far : Alignment.Near;
            OnValueChanged(this, new ValueEventArgs<bool>((bool)value));
        }
        return true;
    }

    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        OnThemeDpiEvent(Window, ThemeDpiEventArgs.FromWindow(Window));
        Window!.ThemeDpiEvent += OnThemeDpiEvent;
    }

    protected override void OnDetachingFromComposition(object? sender, EventArgs e)
    {
        base.OnDetachingFromComposition(sender, e);
        Window!.ThemeDpiEvent -= OnThemeDpiEvent;
    }

    protected virtual void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
    {
        if (!AutoSize)
            return;

        var theme = GetWindowTheme();
        var boxSize = theme.BoxSize;
        Width = boxSize * 2;
        Height = boxSize;
        _button.Width = boxSize;
        _button.Height = boxSize;
    }
}
