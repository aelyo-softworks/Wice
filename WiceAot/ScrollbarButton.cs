namespace Wice;

/// <summary>
/// Scrollbar arrow button that renders a vector chevron via a <see cref="Path"/> child.
/// </summary>
/// <remarks>
/// Behavior:
/// - Layout: on <see cref="OnArranged(object?, EventArgs)"/>, computes the available width and requests an
///   arrow geometry from <c>Application.CurrentResourceManager</c> using this button's <see cref="DockType"/>,
///   <see cref="ArrowRatio"/>, and <see cref="IsArrowOpen"/> state, then assigns it to <see cref="Child.GeometrySource2D"/>.
/// - Theme/DPI: on composition attach and when the window raises <see cref="Window.ThemeDpiEvent"/>,
///   updates the arrow margin, stroke thickness, and brushes via <see cref="OnThemeDpiEvent(object?, ThemeDpiEventArgs)"/>.
/// - Interaction: inherits clickable behavior from <see cref="ButtonBase"/>.
/// </remarks>
public partial class ScrollBarButton : ButtonBase
{
    /// <summary>
    /// Dynamic property descriptor for <see cref="IsArrowOpen"/>.
    /// </summary>
    /// <remarks>
    /// - Type: <see cref="bool"/> (default: <see langword="true"/>).
    /// - Invalidation: <see cref="VisualPropertyInvalidateModes.Arrange"/>.
    /// </remarks>
    public static VisualProperty IsArrowOpenProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(IsArrowOpen), VisualPropertyInvalidateModes.Arrange, true);

    /// <summary>
    /// Dynamic property descriptor for <see cref="ArrowRatio"/>.
    /// </summary>
    /// <remarks>
    /// - Type: <see cref="float"/> (default: <see cref="float.NaN"/> to use theme default).
    /// - Invalidation: <see cref="VisualPropertyInvalidateModes.Arrange"/>.
    /// </remarks>
    public static VisualProperty ArrowRatioProperty { get; } = VisualProperty.Add(typeof(Visual), nameof(ArrowRatio), VisualPropertyInvalidateModes.Arrange, float.NaN);

    /// <summary>
    /// Creates a new scrollbar button for the given docked arrow direction.
    /// </summary>
    /// <param name="type">The <see cref="DockType"/> indicating arrow direction/placement (e.g., Left/Right/Top/Bottom).</param>
    /// <remarks>
    /// Initializes a <see cref="Path"/> child, assigns the dock type, and sets the initial margin from the current theme.
    /// </remarks>
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

    /// <summary>
    /// Gets the strongly-typed <see cref="Path"/> child that renders the arrow geometry.
    /// </summary>
    /// <remarks>
    /// The child is created by the constructor and cannot be replaced.
    /// </remarks>
    /// <exception cref="NotSupportedException">Setting this property is not supported.</exception>
    [Browsable(false)]
    public new virtual Path Child { get => (Path)base.Child!; set => throw new NotSupportedException(); }

    /// <summary>
    /// Gets or sets whether the arrow is rendered as an open chevron (<c>true</c>) or a filled/closed variant (<c>false</c>).
    /// </summary>
    /// <remarks>
    /// Changing this value triggers an arrange pass to rebuild the geometry sized to the current layout.
    /// </remarks>
    [Category(CategoryBehavior)]
    public bool IsArrowOpen { get => (bool)GetPropertyValue(IsArrowOpenProperty)!; set => SetPropertyValue(IsArrowOpenProperty, value); }

    /// <summary>
    /// Gets or sets the ratio used to compute the arrow geometry within the available size.
    /// </summary>
    /// <remarks>
    /// - Use <see cref="float.NaN"/> to defer to the theme's default ratio.
    /// - Changing this value triggers an arrange pass to rebuild the geometry sized to the current layout.
    /// </remarks>
    [Category(CategoryBehavior)]
    public float ArrowRatio { get => (float)GetPropertyValue(ArrowRatioProperty)!; set => SetPropertyValue(ArrowRatioProperty, value); }

    /// <summary>
    /// Rebuilds the arrow geometry sized to the arranged bounds and applies it to the <see cref="Path"/> child.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">Event args.</param>
    protected override void OnArranged(object? sender, EventArgs e)
    {
        base.OnArranged(sender, e);

        var size = (Child.ArrangedRect - Child.Margin).Size;
        var width = size.width;
        var type = Dock.GetDockType(this);
        var geoSource = Application.CurrentResourceManager.GetScrollBarButtonGeometrySource(type, width, ArrowRatio, IsArrowOpen);
        Child.GeometrySource2D = geoSource;
    }

    /// <summary>
    /// Applies theme/DPI-dependent styling and subscribes to subsequent theme DPI updates.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">Event args.</param>
    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        OnThemeDpiEvent(Window, ThemeDpiEventArgs.FromWindow(Window));
        Window!.ThemeDpiEvent += OnThemeDpiEvent;
    }

    /// <summary>
    /// Unsubscribes from theme DPI updates when detaching from composition.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">Event args.</param>
    protected override void OnDetachingFromComposition(object? sender, EventArgs e)
    {
        base.OnDetachingFromComposition(sender, e);
        Window!.ThemeDpiEvent -= OnThemeDpiEvent;
    }

    /// <summary>
    /// Updates stroke thickness, margins, and brushes to match the current theme and DPI.
    /// </summary>
    /// <param name="sender">The event sender (typically the <see cref="Window"/>).</param>
    /// <param name="e">Theme/DPI event arguments.</param>
    /// <remarks>
    /// - Sets <see cref="Path.Shape"/> stroke thickness from <c>ScrollBarButtonStrokeThickness</c>.
    /// - Sets arrow <see cref="Visual.Margin"/> from <c>ScrollBarArrowMargin</c>.
    /// - Sets both <see cref="Visual.StrokeBrush"/> and <see cref="Visual.RenderBrush"/> using <c>ScrollBarButtonStrokeColor</c>.
    /// </remarks>
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
