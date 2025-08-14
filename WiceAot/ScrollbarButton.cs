namespace Wice;

public partial class ScrollBarButton : ButtonBase
{
    public static VisualProperty IsArrowOpenProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(IsArrowOpen), VisualPropertyInvalidateModes.Arrange, true);
    public static VisualProperty ArrowRatioProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(ArrowRatio), VisualPropertyInvalidateModes.Arrange, float.NaN);

    public ScrollBarButton(DockType type)
    {
#if DEBUG
        Name = nameof(ScrollBarButton) + type;
#endif

        base.Child = new Path();
#if DEBUG
        Child.Name = Name + nameof(Path);
#endif
        Dock.SetDockType(this, type);

        Child.Margin = GetWindowTheme().ScrollBarArrowMargin; // vary per scrollbar width/height?
    }

    [Browsable(false)]
    public new virtual Path Child { get => (Path)base.Child!; set => throw new NotSupportedException(); }

    [Category(CategoryBehavior)]
    public bool IsArrowOpen { get => (bool)GetPropertyValue(IsArrowOpenProperty)!; set => SetPropertyValue(IsArrowOpenProperty, value); }

    [Category(CategoryBehavior)]
    public float ArrowRatio { get => (float)GetPropertyValue(ArrowRatioProperty)!; set => SetPropertyValue(ArrowRatioProperty, value); }

    protected override void OnArranged(object? sender, EventArgs e)
    {
        base.OnArranged(sender, e);

        var size = (Child.ArrangedRect - Child.Margin).Size;
        var width = size.width;
        var type = Dock.GetDockType(this);
        var geoSource = Application.CurrentResourceManager.GetScrollBarButtonGeometrySource(type, width, ArrowRatio, IsArrowOpen);
        Child.GeometrySource2D = geoSource;
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
        var theme = GetWindowTheme();
        if (Child.Shape != null)
        {
            Child.Shape.StrokeThickness = theme.ScrollBarButtonStrokeThickness;
        }
        Child.Margin = theme.ScrollBarArrowMargin; // TODO: vary per scrollbar width/height?
        Child.StrokeBrush = Compositor!.CreateColorBrush(theme.ScrollBarButtonStrokeColor.ToColor());
        Child.RenderBrush = Child.StrokeBrush;
    }
}
