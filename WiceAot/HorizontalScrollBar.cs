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

    /// <summary>
    /// Creates the left-pointing small decrease arrow button.
    /// </summary>
    /// <returns>A <see cref="ScrollBarButton"/> configured for <see cref="DockType.Left"/>.</returns>
    protected override ScrollBarButton CreateSmallDecrease() => new(DockType.Left);

    /// <summary>
    /// Creates the large left (page-left) clickable segment.
    /// </summary>
    /// <returns>A <see cref="ButtonBase"/> representing the large decrease area.</returns>
    protected override ButtonBase CreateLargeDecrease() => new();

    /// <summary>
    /// Creates the right-pointing small increase arrow button.
    /// </summary>
    /// <returns>A <see cref="ScrollBarButton"/> configured for <see cref="DockType.Right"/>.</returns>
    protected override ScrollBarButton CreateSmallIncrease() => new(DockType.Right);

    /// <summary>
    /// Creates the large right (page-right) clickable segment.
    /// </summary>
    /// <returns>A <see cref="ButtonBase"/> representing the large increase area.</returns>
    protected override ButtonBase CreateLargeIncrease() => new();

    /// <summary>
    /// Creates the draggable thumb for the horizontal track.
    /// </summary>
    /// <returns>A new <see cref="Thumb"/> instance.</returns>
    protected override Thumb CreateThumb() => new();

    /// <summary>
    /// Applies visual adjustments after rendering is prepared.
    /// </summary>
    /// <param name="sender">Render source.</param>
    /// <param name="e">Event args.</param>
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

    /// <summary>
    /// Subscribes to theme/DPI notifications and applies initial DPI-dependent sizing.
    /// </summary>
    /// <param name="sender">Attach source.</param>
    /// <param name="e">Event args.</param>
    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        OnThemeDpiEvent(Window, ThemeDpiEventArgs.FromWindow(Window));
        Window!.ThemeDpiEvent += OnThemeDpiEvent;
    }

    /// <summary>
    /// Unsubscribes from theme/DPI notifications.
    /// </summary>
    /// <param name="sender">Detach source.</param>
    /// <param name="e">Event args.</param>
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
