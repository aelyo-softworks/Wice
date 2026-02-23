namespace Wice;

/// <summary>
/// Non-activating tooltip window shown near the mouse cursor.
/// Uses a single content visual (default: <see cref="Canvas"/>) with an optional drop shadow.
/// Adjusts its placement offsets to account for the content margin required by the shadow.
/// </summary>
public partial class ToolTip : PopupWindow, IToolTip, IContentParent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ToolTip"/> class.
    /// Configures placement, sizing, frame, click-through, content, shadow, and initial offsets.
    /// </summary>
    public ToolTip()
    {
        ExtendedStyle |= WINDOW_EX_STYLE.WS_EX_NOACTIVATE;
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

    /// <inheritdoc/>
    protected override int MaxChildrenCount => 1;

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public override bool Show(SHOW_WINDOW_CMD command = SHOW_WINDOW_CMD.SW_SHOWNOACTIVATE) => base.Show(command);

    /// <inheritdoc/>
    protected override MA OnMouseActivate(HWND parentWindowHandle, int mouseMessage, HT hitTest) => MA.MA_NOACTIVATE;

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    protected override PlacementParameters CreatePlacementParameters()
    {
        var parameters = base.CreatePlacementParameters();
        var offset = Content.Margin;
        parameters.HorizontalOffset -= offset.left;
        parameters.VerticalOffset -= offset.top;
        return parameters;
    }

    /// <inheritdoc/>
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
