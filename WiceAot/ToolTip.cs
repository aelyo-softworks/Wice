namespace Wice;

public partial class ToolTip : PopupWindow, IContentParent
{
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
        Content.RenderShadow = CreateShadow(); // may be null if overriden

        VerticalOffset = GetWindowTheme().ToolTipVerticalOffset * NativeWindow.GetAccessibilityCursorSize();
    }

    [Browsable(false)]
    public Visual Content { get; }

    protected override int MaxChildrenCount => 1;
    protected override bool HasCaret => false;

    protected virtual Visual CreateContent()
    {
        var canvas = new Canvas
        {
            MeasureToContent = DimensionOptions.WidthAndHeight
        };
        return canvas;
    }

    public override bool Show(SHOW_WINDOW_CMD command = SHOW_WINDOW_CMD.SW_SHOWNOACTIVATE) => base.Show(command);
    protected override MA OnMouseActivate(HWND parentWindowHandle, int mouseMessage, HT hitTest) => MA.MA_NOACTIVATE;
    protected override void ExtendFrame(HWND handle)
    {
        // don't extend frame
        //base.ExtendFrame(handle);
    }

    protected virtual CompositionShadow? CreateShadow()
    {
        if (Compositor == null)
            return null;

        var shadow = Compositor.CreateDropShadow();
        shadow.BlurRadius = GetWindowTheme().ToolTipShadowBlurRadius;
        return shadow;
    }

    protected override PlacementParameters CreatePlacementParameters()
    {
        var parameters = base.CreatePlacementParameters();
        var offset = Content.Margin;
        parameters.HorizontalOffset -= offset.left;
        parameters.VerticalOffset -= offset.top;
        return parameters;
    }
}
