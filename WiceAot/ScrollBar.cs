namespace Wice;

/// <summary>
/// Represents a scroll bar control that provides a user interface for scrolling content.
/// </summary>
public abstract class ScrollBar : Dock
{
    /// <summary>
    /// Identifies the <see cref="Mode"/> property.
    /// Changing the mode triggers a measure pass.
    /// </summary>
    public static VisualProperty ModeProperty { get; } = VisualProperty.Add(typeof(ScrollBar), nameof(Mode), VisualPropertyInvalidateModes.Measure, ScrollBarMode.Standard);

    /// <summary>
    /// Occurs when the "small increase" button is clicked (e.g., arrow/page forward by a small unit).
    /// </summary>
    public event EventHandler<EventArgs>? SmallIncreaseClick;

    /// <summary>
    /// Occurs when the "small decrease" button is clicked (e.g., arrow/page back by a small unit).
    /// </summary>
    public event EventHandler<EventArgs>? SmallDecreaseClick;

    /// <summary>
    /// Occurs when the "large increase" segment is clicked (e.g., track/page forward by a large unit).
    /// </summary>
    public event EventHandler<EventArgs>? LargeIncreaseClick;

    /// <summary>
    /// Occurs when the "large decrease" segment is clicked (e.g., track/page back by a large unit).
    /// </summary>
    public event EventHandler<EventArgs>? LargeDecreaseClick;

    /// <summary>
    /// Initializes a new <see cref="ScrollBar"/> and builds its constituent parts
    /// by invoking the abstract factory methods. Wires click handlers and adds
    /// parts to the <see cref="Visual.Children"/> collection.
    /// </summary>
    protected ScrollBar()
    {
        HandlePointerEvents = true;
        SmallDecrease = CreateSmallDecrease();
        if (SmallDecrease == null)
            throw new InvalidOperationException();

        SmallDecrease.Click += OnSmallDecreaseClick;
        SmallDecrease.FocusIndex = 0;
        Children.Add(SmallDecrease);

        LargeDecrease = CreateLargeDecrease();
        if (LargeDecrease == null)
            throw new InvalidOperationException();

        LargeDecrease.Click += OnLargeDecreaseClick;
        LargeDecrease.FocusIndex = 1;
        Children.Add(LargeDecrease);
#if DEBUG
        LargeDecrease.Name = nameof(LargeDecrease);
#endif

        SmallIncrease = CreateSmallIncrease();
        if (SmallIncrease == null)
            throw new InvalidOperationException();

        SmallIncrease.Click += OnSmallIncreaseClick;
        SmallIncrease.FocusIndex = 4;
        Children.Add(SmallIncrease);

        LargeIncrease = CreateLargeIncrease();
        if (LargeIncrease == null)
            throw new InvalidOperationException();

        LargeIncrease.Click += OnLargeIncreaseClick;
        LargeIncrease.FocusIndex = 3;
        Children.Add(LargeIncrease);
#if DEBUG
        LargeIncrease.Name = nameof(LargeIncrease);
#endif

        Thumb = CreateThumb();
        if (Thumb == null)
            throw new InvalidOperationException();

        Thumb.FocusIndex = 2;
        Children.Add(Thumb);
#if DEBUG
        Thumb.Name = nameof(Thumb);
#endif
    }

    /// <summary>
    /// Gets the small "decrease" button (typically the up/left arrow). Hidden in overlay mode.
    /// </summary>
    [Browsable(false)]
    public ButtonBase SmallDecrease { get; }

    /// <summary>
    /// Gets the large "decrease" clickable segment (track area before the thumb).
    /// </summary>
    [Browsable(false)]
    public ButtonBase LargeDecrease { get; }

    /// <summary>
    /// Gets the draggable thumb that represents the viewport position/size.
    /// </summary>
    [Browsable(false)]
    public Thumb Thumb { get; }

    /// <summary>
    /// Gets the large "increase" clickable segment (track area after the thumb).
    /// </summary>
    [Browsable(false)]
    public ButtonBase LargeIncrease { get; }

    /// <summary>
    /// Gets the small "increase" button (typically the down/right arrow). Hidden in overlay mode.
    /// </summary>
    [Browsable(false)]
    public ButtonBase SmallIncrease { get; }

    /// <summary>
    /// Gets the owning <see cref="ScrollViewer"/> when hosted inside one; otherwise null.
    /// </summary>
    internal ScrollViewer? View => Parent as ScrollViewer;

    /// <summary>
    /// Gets a value indicating whether this scroll bar should render as an overlay
    /// according to the owning viewer's <see cref="ScrollViewer.ScrollMode"/>.
    /// </summary>
    internal bool IsOverlay => View?.ScrollMode == ScrollViewerMode.Overlay;

    /// <summary>
    /// Creates the "small decrease" button. Called once by the constructor.
    /// </summary>
    /// <returns>A non-null instance representing the small decrease button.</returns>
    protected abstract ScrollBarButton CreateSmallDecrease();

    /// <summary>
    /// Creates the large "decrease" clickable segment. Called once by the constructor.
    /// </summary>
    /// <returns>A non-null instance representing the large decrease segment.</returns>
    protected abstract ButtonBase CreateLargeDecrease();

    /// <summary>
    /// Creates the draggable <see cref="Thumb"/>. Called once by the constructor.
    /// </summary>
    /// <returns>A non-null thumb instance.</returns>
    protected abstract Thumb CreateThumb();

    /// <summary>
    /// Creates the large "increase" clickable segment. Called once by the constructor.
    /// </summary>
    /// <returns>A non-null instance representing the large increase segment.</returns>
    protected abstract ButtonBase CreateLargeIncrease();

    /// <summary>
    /// Creates the "small increase" button. Called once by the constructor.
    /// </summary>
    /// <returns>A non-null instance representing the small increase button.</returns>
    protected abstract ScrollBarButton CreateSmallIncrease();

    /// <summary>
    /// Raises <see cref="SmallIncreaseClick"/>. Override to customize handling before/after the event.
    /// </summary>
    protected virtual void OnSmallIncreaseClick(object? sender, EventArgs e) => SmallIncreaseClick?.Invoke(sender, e);

    /// <summary>
    /// Raises <see cref="SmallDecreaseClick"/>. Override to customize handling before/after the event.
    /// </summary>
    protected virtual void OnSmallDecreaseClick(object? sender, EventArgs e) => SmallDecreaseClick?.Invoke(sender, e);

    /// <summary>
    /// Raises <see cref="LargeIncreaseClick"/>. Override to customize handling before/after the event.
    /// </summary>
    protected virtual void OnLargeIncreaseClick(object? sender, EventArgs e) => LargeIncreaseClick?.Invoke(sender, e);

    /// <summary>
    /// Raises <see cref="LargeDecreaseClick"/>. Override to customize handling before/after the event.
    /// </summary>
    protected virtual void OnLargeDecreaseClick(object? sender, EventArgs e) => LargeDecreaseClick?.Invoke(sender, e);

    /// <summary>
    /// Gets or sets the scroll bar presentation mode.
    /// </summary>
    [Category(CategoryLayout)]
    public ScrollBarMode Mode { get => (ScrollBarMode)GetPropertyValue(ModeProperty)!; set => SetPropertyValue(ModeProperty, value); }

    /// <summary>
    /// Updates the viewer's "corner" (intersection area of two scroll bars) if applicable.
    /// Intended for vertical scroll bar implementations; no-op by default.
    /// </summary>
    /// <param name="view">The owning viewer.</param>
    protected internal virtual void UpdateCorner(ScrollViewer view)
    {
        // should only be implemented by vsb
    }

    /// <inheritdoc/>
    protected override void IsMouseOverChanged(bool newValue)
    {
        base.IsMouseOverChanged(newValue);
        if (IsOverlay)
        {
            // force redraw when overlay to show expansion
            Invalidate(VisualPropertyInvalidateModes.Render, new PropertyInvalidateReason(IsMouseOverProperty));
        }
    }

    /// <inheritdoc/>
    protected override void OnRendered(object? sender, EventArgs e)
    {
        base.OnRendered(sender, e);
        var compositor = Window?.Compositor;
        if (compositor == null)
            return;

        var theme = GetWindowTheme();
        if (IsOverlay)
        {
            RenderBrush = compositor.CreateColorBrush(D3DCOLORVALUE.Transparent.ToColor());
            Thumb.RenderBrush = compositor.CreateColorBrush(theme.ScrollBarOverlayThumbColor.ToColor());
            Thumb.CornerRadius = new Vector2(theme.ScrollBarOverlayCornerRadius);
            SmallDecrease.IsVisible = false;
            SmallIncrease.IsVisible = false;
        }
        else
        {
            RenderBrush = compositor.CreateColorBrush(theme.ScrollBarBackgroundColor.ToColor());
            Thumb.RenderBrush = compositor.CreateColorBrush(theme.ScrollBarThumbColor.ToColor());
            Thumb.CornerRadius = new Vector2(0);
            SmallDecrease.IsVisible = true;
            SmallIncrease.IsVisible = true;
        }
    }
}
