namespace Wice;

public partial class ScrollBarButton : ButtonBase
{
    private GeometrySource2D? _lastGeometrySource2D;

    public static VisualProperty IsArrowOpenProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(IsArrowOpen), VisualPropertyInvalidateModes.Render, true);
    public static VisualProperty ArrowRatioProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(ArrowRatio), VisualPropertyInvalidateModes.Render, float.NaN);

    public ScrollBarButton(DockType type)
    {
#if DEBUG
        Name = nameof(ScrollBarButton) + type;
#endif

        Child = new Path();
#if DEBUG
        Child.Name = Name + nameof(Path);
#endif
        Dock.SetDockType(this, type);
        Child.Margin = Application.CurrentTheme.ScrollBarArrowMargin; // TODO: vary per scrollbar width/height?
    }

    [Browsable(false)]
    public new virtual Path? Child { get => (Path?)base.Child; set => base.Child = value; }

    [Category(CategoryBehavior)]
    public bool IsArrowOpen { get => (bool)GetPropertyValue(IsArrowOpenProperty)!; set => SetPropertyValue(IsArrowOpenProperty, value); }

    [Category(CategoryBehavior)]
    public float ArrowRatio { get => (float)GetPropertyValue(ArrowRatioProperty)!; set => SetPropertyValue(ArrowRatioProperty, value); }

    protected override void OnArranged(object? sender, EventArgs e)
    {
        base.OnArranged(sender, e);
        if (Child == null)
            return;

        var size = (Child.ArrangedRect - Child.Margin).Size;
        var open = IsArrowOpen;
        var type = Dock.GetDockType(this);
        var geoSource = Application.Current.ResourceManager.GetScrollBarButtonGeometrySource(type, size.width, ArrowRatio, open);
        if (geoSource.Equals(_lastGeometrySource2D))
            return;

        Child.GeometrySource2D = geoSource;
        _lastGeometrySource2D = geoSource;
    }

    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        if (Child?.Shape == null)
            return;

        Child.Shape.StrokeThickness = Application.CurrentTheme.ScrollBarButtonStrokeThickness;
        Child.StrokeBrush = Compositor!.CreateColorBrush(Application.CurrentTheme.ScrollBarButtonStrokeColor.ToColor());
        Child.RenderBrush = Child.StrokeBrush;
    }
}
