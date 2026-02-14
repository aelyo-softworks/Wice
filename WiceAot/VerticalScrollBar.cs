namespace Wice;

/// <summary>
/// Represents a vertical scrollbar visual that provides scrolling functionality for content within a <see
/// cref="ScrollViewer"/>. This visual is docked to the right edge of the parent container and includes a customizable
/// corner visual for scenarios where both horizontal and vertical scrollbars are visible.
/// </summary>
public partial class VerticalScrollBar : ScrollBar
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VerticalScrollBar"/> class, configuring its layout and child
    /// elements.
    /// </summary>
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
    [Browsable(false)]
    public Visual Corner { get; }

    /// <inheritdoc/>
    protected override ScrollBarButton CreateSmallDecrease() => new(DockType.Top);

    /// <inheritdoc/>
    protected override ButtonBase CreateLargeDecrease() => new();

    /// <inheritdoc/>
    protected override ScrollBarButton CreateSmallIncrease() => new(DockType.Bottom);

    /// <inheritdoc/>
    protected override ButtonBase CreateLargeIncrease() => new();

    /// <summary>
    /// Creates the corner visual shown when both scrollbars are visible.
    /// </summary>
    /// <returns>A new <see cref="Border"/> by default. Derived classes may override to customize.</returns>
    protected virtual Visual CreateCorner() => new Border();

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (property == RenderBrushProperty)
        {
            Corner.RenderBrush = RenderBrush;
        }
        return base.SetPropertyValue(property, value, options);
    }

    /// <inheritdoc/>
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
    /// Handles DPI-related changes for the current theme and updates the dimensions of the scroll bar components
    /// accordingly.
    /// </summary>
    /// <param name="sender">The source of the event. This parameter may be <see langword="null"/>.</param>
    /// <param name="e">An instance of <see cref="ThemeDpiEventArgs"/> containing the new DPI value and related event data.</param>
    protected virtual void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
    {
        Width = GetWindowTheme().GetVerticalScrollBarWidth(e.NewDpi);
        SmallDecrease.Height = Width;
        SmallIncrease.Height = Width;
        Thumb.Width = Width;
    }
}
