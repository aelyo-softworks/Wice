namespace Wice;

/// <summary>
/// Horizontal scrollbar implementation of <see cref="ScrollBar"/>.
/// </summary>
public partial class HorizontalScrollBar : ScrollBar
{
    /// <summary>
    /// Initializes a new <see cref="HorizontalScrollBar"/> docked at the bottom.
    /// </summary>
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

    /// <inheritdoc/>
    protected override ScrollBarButton CreateSmallDecrease() => new(DockType.Left);

    /// <inheritdoc/>
    protected override ButtonBase CreateLargeDecrease() => new();

    /// <inheritdoc/>
    protected override ScrollBarButton CreateSmallIncrease() => new(DockType.Right);

    /// <inheritdoc/>
    protected override ButtonBase CreateLargeIncrease() => new();

    /// <inheritdoc/>
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
    /// Updates the scrollbar height when theme/DPI metrics change.
    /// </summary>
    /// <param name="sender">Theme/DPI source (typically the <see cref="Window"/>).</param>
    /// <param name="e">Theme/DPI event args.</param>
    protected virtual void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
    {
        Height = GetWindowTheme().GetHorizontalScrollBarHeight(WiceCommons.USER_DEFAULT_SCREEN_DPI);
    }
}
