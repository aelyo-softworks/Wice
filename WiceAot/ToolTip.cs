namespace Wice;

/// <summary>
/// Non-activating tooltip window shown near the mouse cursor.
/// Uses a single content visual (default: <see cref="Canvas"/>) with an optional drop shadow.
/// Adjusts its placement offsets to account for the content margin required by the shadow.
/// </summary>
public partial class ToolTip : PopupWindow, IContentParent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ToolTip"/> class.
    /// Configures placement, sizing, frame, click-through, content, shadow, and initial offsets.
    /// </summary>
    public ToolTip()
    {
        PlacementMode = PlacementMode.Mouse;
        MeasureToContent = DimensionOptions.WidthAndHeight;
        FrameSize = 0;
        ClickThrough = true;

        // this should be transparent too
        Content = CreateContent();
        if (Content == null)
            throw new InvalidOperationException();

        Children.Add(Content);

        // we need a margin for the drop shadow so we need to force 1 child only
        Content.Margin = GetWindowTheme().ToolTipBaseSize;
        Content.RenderShadow = CreateShadow(); // may be null if overridden
        VerticalOffset = GetWindowTheme().ToolTipVerticalOffset * NativeWindow.GetAccessibilityCursorSize();
    }

    /// <summary>
    /// Gets the single content visual hosted by the tooltip.
    /// Derived types can override <see cref="CreateContent"/> to customize the visual.
    /// </summary>
    [Browsable(false)]
    public Visual Content { get; }

    /// <summary>
    /// Gets the enforced maximum number of children.
    /// Tooltips require a single content child to preserve shadow margins and layout accounting.
    /// </summary>
    protected override int MaxChildrenCount => 1;

    /// <summary>
    /// Gets a value indicating whether this window displays a text caret.
    /// Tooltips are non-interactive and never own a caret.
    /// </summary>
    protected override bool HasCaret => false;

    /// <summary>
    /// Creates the content visual for the tooltip.
    /// The default is a <see cref="Canvas"/> measuring to its content.
    /// </summary>
    /// <returns>The visual to host inside the tooltip; never null.</returns>
    protected virtual Visual CreateContent()
    {
        var canvas = new Canvas
        {
            MeasureToContent = DimensionOptions.WidthAndHeight
        };
        return canvas;
    }

    /// <summary>
    /// Shows the tooltip without activating its window.
    /// </summary>
    /// <param name="command">Ignored: coerced to <see cref="SHOW_WINDOW_CMD.SW_SHOWNOACTIVATE"/>.</param>
    /// <returns>true if the window became visible; otherwise false.</returns>
    public override bool Show(SHOW_WINDOW_CMD command = SHOW_WINDOW_CMD.SW_SHOWNOACTIVATE) => base.Show(command);

    /// <summary>
    /// Prevents window activation from mouse input (tooltip must not steal focus).
    /// </summary>
    /// <param name="parentWindowHandle">Parent window handle.</param>
    /// <param name="mouseMessage">Mouse message id.</param>
    /// <param name="hitTest">Hit-test result.</param>
    /// <returns><see cref="MA.MA_NOACTIVATE"/> to keep the window non-activating.</returns>
    protected override MA OnMouseActivate(HWND parentWindowHandle, int mouseMessage, HT hitTest) => MA.MA_NOACTIVATE;

    /// <summary>
    /// Suppresses glass frame extension for tooltips.
    /// </summary>
    /// <param name="handle">Native window handle.</param>
    protected override void ExtendFrame(HWND handle)
    {
        // don't extend frame
        //base.ExtendFrame(handle);
    }

    /// <summary>
    /// Creates the composition shadow used by the tooltip content.
    /// Returns null when the compositor is unavailable or when a derived class disables shadows.
    /// </summary>
    /// <returns>A configured drop shadow or null.</returns>
    protected virtual CompositionShadow? CreateShadow()
    {
        if (Compositor == null)
            return null;

        var shadow = Compositor.CreateDropShadow();
        shadow.BlurRadius = GetWindowTheme().ToolTipShadowBlurRadius;
        return shadow;
    }

    /// <summary>
    /// Produces placement parameters and compensates the popup position for the content's margin,
    /// so that the visible content aligns with the desired point while preserving shadow padding.
    /// </summary>
    /// <returns>The adjusted placement parameters.</returns>
    protected override PlacementParameters CreatePlacementParameters()
    {
        var parameters = base.CreatePlacementParameters();
        var offset = Content.Margin;
        parameters.HorizontalOffset -= offset.left;
        parameters.VerticalOffset -= offset.top;
        return parameters;
    }

    /// <summary>
    /// Responds to theme/DPI changes by updating content margin, shadow blur radius,
    /// and vertical offset (scaled by the accessibility cursor size).
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">DPI event data.</param>
    protected internal override void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
    {
        base.OnThemeDpiEvent(sender, e);
        Content.Margin = GetWindowTheme().ToolTipBaseSize;
        VerticalOffset = GetWindowTheme().ToolTipVerticalOffset * NativeWindow.GetAccessibilityCursorSize();
        if (Content.RenderShadow is DropShadow dropShadow)
        {
            dropShadow.BlurRadius = GetWindowTheme().ToolTipShadowBlurRadius;
        }
    }
}
