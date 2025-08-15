namespace Wice;

/// <summary>
/// A vertical <see cref="ScrollBar"/> implementation that:
/// - Creates a square corner visual to align with the horizontal scrollbar when both are visible.
/// - Docks itself to the right and sizes using the current theme's vertical scrollbar width (DPI-aware).
/// - In overlay mode, adapts the thumb width on hover to reveal a right-side margin.
/// - Propagates its <see cref="RenderBrush"/> to the corner for consistent styling.
/// - Subscribes to window DPI updates to keep dimensions in sync.
/// </summary>
/// <remarks>
/// Composition:
/// - Children order matters: the corner must be the first child to reserve bottom-right space when both scrollbars are shown.
/// Layout:
/// - Small arrow buttons are square (height equals the scrollbar width).
/// - The thumb width tracks the overall scrollbar width, with special handling in overlay mode.
/// Overlay:
/// - When overlaying, the horizontal scrollbar width is reduced by this scrollbar width so both fit the bottom row without overlap.
/// </remarks>
public partial class VerticalScrollBar : ScrollBar
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VerticalScrollBar"/>:
    /// - Creates and inserts the corner visual as the first child, docked to bottom.
    /// - Docks this scrollbar to the right edge and positions it at the window's right/top.
    /// - Sizes itself using the theme's vertical scrollbar width for the default DPI.
    /// - Sets square sizes for small arrow buttons and aligns the thumb width to the bar width.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the required corner visual cannot be created.</exception>
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
        Width = GetWindowTheme().GetVerticalScrollBarWidth(WiceCommons.USER_DEFAULT_SCREEN_DPI);
        SetDockType(this, DockType.Right);

        SmallDecrease.Height = Width; // square
        SetDockType(LargeDecrease, DockType.Top);
        SmallIncrease.Height = Width; // square
        SetDockType(LargeIncrease, DockType.Bottom);
        Thumb.Width = Width;
    }

    /// <summary>
    /// Gets the bottom-right "corner" visual that fills the junction area when both scrollbars are visible.
    /// </summary>
    /// <remarks>
    /// - Height mirrors the horizontal scrollbar height.
    /// - Width mirrors this vertical scrollbar width.
    /// - Visibility tracks this scrollbar and the horizontal scrollbar visibility.
    /// - Render brush is synchronized with the parent scrollbar.
    /// </remarks>
    [Browsable(false)]
    public Visual Corner { get; }

    /// <summary>
    /// Creates the small-decrease arrow button for the top direction.
    /// </summary>
    /// <returns>A <see cref="ScrollBarButton"/> docked to <see cref="DockType.Top"/>.</returns>
    protected override ScrollBarButton CreateSmallDecrease() => new(DockType.Top);

    /// <summary>
    /// Creates the large-decrease (track) button above the thumb.
    /// </summary>
    /// <returns>A button base instance used as the track segment.</returns>
    protected override ButtonBase CreateLargeDecrease() => new();

    /// <summary>
    /// Creates the small-increase arrow button for the bottom direction.
    /// </summary>
    /// <returns>A <see cref="ScrollBarButton"/> docked to <see cref="DockType.Bottom"/>.</returns>
    protected override ScrollBarButton CreateSmallIncrease() => new(DockType.Bottom);

    /// <summary>
    /// Creates the large-increase (track) button below the thumb.
    /// </summary>
    /// <returns>A button base instance used as the track segment.</returns>
    protected override ButtonBase CreateLargeIncrease() => new();

    /// <summary>
    /// Creates the draggable thumb element.
    /// </summary>
    /// <returns>A new <see cref="Thumb"/>.</returns>
    protected override Thumb CreateThumb() => new();

    /// <summary>
    /// Creates the corner visual shown when both scrollbars are visible.
    /// </summary>
    /// <returns>A new <see cref="Border"/> by default. Derived classes may override to customize.</returns>
    protected virtual Visual CreateCorner() => new Border();

    /// <summary>
    /// Updates the corner sizing/visibility to match the horizontal scrollbar and adjusts the horizontal bar width in overlay mode.
    /// </summary>
    /// <param name="view">The owning <see cref="ScrollViewer"/>.</param>
    protected internal override void UpdateCorner(ScrollViewer view)
    {
        base.UpdateCorner(view);
        Corner.Height = view.HorizontalScrollBar.Height;
        Corner.Width = Width;
        Corner.IsVisible = IsVisible && view.HorizontalScrollBar.IsVisible;
        if (view.ScrollMode == ScrollViewerMode.Overlay && Corner.IsVisible)
        {
            // Reduce horizontal scrollbar width so the bottom-right corner is reserved for the vertical bar.
            view.HorizontalScrollBar.Width = view.Viewer.ArrangedRect.Width - Width;
        }
        else
        {
            view.HorizontalScrollBar.Width = view.Viewer.ArrangedRect.Width;
        }
    }

    /// <summary>
    /// Mirrors the scrollbar's <see cref="RenderBrush"/> to the corner when that property changes.
    /// </summary>
    /// <param name="property">The property being set.</param>
    /// <param name="value">The new value.</param>
    /// <param name="options">Optional set options.</param>
    /// <returns><c>true</c> if the stored value changed; otherwise, <c>false</c>.</returns>
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (property == RenderBrushProperty)
        {
            Corner.RenderBrush = RenderBrush;
        }
        return base.SetPropertyValue(property, value, options);
    }

    /// <summary>
    /// Applies overlay-dependent thumb width:
    /// - In overlay mode and when hovered, shrinks the thumb width by a small right-side margin.
    /// - In overlay mode and not hovered, sets thumb width to the theme's horizontal scrollbar height (thin overlay thumb).
    /// - Otherwise, matches the full scrollbar width.
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
                // Keep an empty border on right; when Window is null, fallback to 4.
                Thumb.Width = Width - Window?.DipsToPixels(4) ?? 4; //keep an empty border on right
            }
            else
            {
                // Use themed thin overlay width (uses horizontal scrollbar height as thickness).
                Thumb.Width = GetWindowTheme().GetHorizontalScrollBarHeight(Window?.Dpi ?? WiceCommons.USER_DEFAULT_SCREEN_DPI);
            }
        }
        else
        {
            Thumb.Width = Width;
        }
    }

    /// <summary>
    /// Subscribes to DPI changes and performs an initial DPI-aware size update.
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
    /// Unsubscribes from DPI changes when detaching from composition.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">Event args.</param>
    protected override void OnDetachingFromComposition(object? sender, EventArgs e)
    {
        base.OnDetachingFromComposition(sender, e);
        Window!.ThemeDpiEvent -= OnThemeDpiEvent;
    }

    /// <summary>
    /// Updates sizes to match the current DPI:
    /// - Bar <see cref="Width"/>
    /// - Square small arrow button heights
    /// - Thumb width
    /// </summary>
    /// <param name="sender">The event sender (typically the <see cref="Window"/>).</param>
    /// <param name="e">Theme DPI event arguments.</param>
    protected virtual void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
    {
        Width = GetWindowTheme().GetVerticalScrollBarWidth(e.NewDpi);
        SmallDecrease.Height = Width;
        SmallIncrease.Height = Width;
        Thumb.Width = Width;
    }
}
