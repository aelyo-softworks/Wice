namespace Wice;

public partial class VerticalScrollBar : ScrollBar
{
    public VerticalScrollBar()
    {
        Corner = CreateCorner();
        if (Corner == null)
            throw new InvalidOperationException();
#if DEBUG
        Corner.Name = nameof(Corner);
#endif
        SetDockType(Corner, DockType.Bottom);
        Children.Insert(0, Corner); // must be first child

        Canvas.SetRight(this, 0);
        Canvas.SetTop(this, 0);
        Width = Application.CurrentTheme.GetVerticalScrollBarWidth(96);
        SetDockType(this, DockType.Right);

        SmallDecrease.Height = Width; // square
        SetDockType(LargeDecrease, DockType.Top);
        SmallIncrease.Height = Width; // square
        SetDockType(LargeIncrease, DockType.Bottom);
        Thumb.Width = Width;
    }

    [Browsable(false)]
    public Visual Corner { get; }

    protected override ScrollBarButton CreateSmallDecrease() => new(DockType.Top);
    protected override ButtonBase CreateLargeDecrease() => new();
    protected override ScrollBarButton CreateSmallIncrease() => new(DockType.Bottom);
    protected override ButtonBase CreateLargeIncrease() => new();
    protected override Thumb CreateThumb() => new();
    protected virtual Visual CreateCorner() => new Border();

    protected internal override void UpdateCorner(ScrollViewer view)
    {
        base.UpdateCorner(view);
        Corner.Height = view.HorizontalScrollBar.Height;
        Corner.Width = Width;
        Corner.IsVisible = IsVisible && view.HorizontalScrollBar.IsVisible;
        if (view.ScrollMode == ScrollViewerMode.Overlay && Corner.IsVisible)
        {
            view.HorizontalScrollBar.Width = view.Viewer.ArrangedRect.Width - Width;
        }
        else
        {
            view.HorizontalScrollBar.Width = view.Viewer.ArrangedRect.Width;
        }
    }

    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (property == RenderBrushProperty)
        {
            Corner.RenderBrush = RenderBrush;
        }
        return base.SetPropertyValue(property, value, options);
    }

    protected override void OnRendered(object? sender, EventArgs e)
    {
        base.OnRendered(sender, e);
        if (IsOverlay)
        {
            if (IsMouseOver)
            {
                Thumb.Width = Width - 4; //keep an empty border on right
            }
            else
            {
                Thumb.Width = Application.CurrentTheme.GetHorizontalScrollBarHeight(96);
            }
        }
        else
        {
            Thumb.Width = Width;
        }
    }
}
