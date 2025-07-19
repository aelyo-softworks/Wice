namespace Wice;

public partial class HorizontalScrollBar : ScrollBar
{
    public HorizontalScrollBar()
    {
        Canvas.SetLeft(this, 0);
        Canvas.SetBottom(this, 0);
        SetDockType(this, DockType.Bottom);
        Height = GetWindowTheme().GetHorizontalScrollBarHeight(WiceCommons.USER_DEFAULT_SCREEN_DPI);

        SmallDecrease.Width = Height; // square
        SetDockType(LargeDecrease, DockType.Left);
        SmallIncrease.Width = Height; // square
        SetDockType(LargeIncrease, DockType.Right);
        Thumb.Height = Height;
    }

    protected override ScrollBarButton CreateSmallDecrease() => new(DockType.Left);
    protected override ButtonBase CreateLargeDecrease() => new();
    protected override ScrollBarButton CreateSmallIncrease() => new(DockType.Right);
    protected override ButtonBase CreateLargeIncrease() => new();
    protected override Thumb CreateThumb() => new();

    protected override void OnRendered(object? sender, EventArgs e)
    {
        base.OnRendered(sender, e);
        if (IsOverlay)
        {
            if (IsMouseOver)
            {
                Thumb.Height = Height - 4; //keep an empty border on bottom
            }
            else
            {
                Thumb.Height = GetWindowTheme().GetScrollBarOverlaySize(WiceCommons.USER_DEFAULT_SCREEN_DPI);
            }
        }
        else
        {
            Thumb.Height = Height;
        }
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
        Height = GetWindowTheme().GetHorizontalScrollBarHeight(WiceCommons.USER_DEFAULT_SCREEN_DPI);
    }
}
