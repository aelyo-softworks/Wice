namespace Wice;

/// <summary>
/// A lightweight caret visual that manages its own blinking and visibility without participating
/// in the full parent invalidation pipeline. Intended to be hosted directly under a <see cref="Window"/> (Canvas).
/// </summary>
/// <remarks>
/// - The caret disables key and pointer events to remain non-interactive.
/// - Blinking is driven by a <see cref="WindowTimer"/> created when attached to a <see cref="Window"/>.
/// - The render brush is initialized using the window theme caret color once attached to composition.
/// - The caret z-index is set to <see cref="int.MaxValue"/> so it renders on top of other visuals.
/// </remarks>
public partial class Caret : Border, IDisposable
{
    /// <summary>
    /// Gets the dynamic property that controls whether the caret should blink.
    /// Changing this value invalidates rendering.
    /// </summary>
    public static VisualProperty BlinkProperty { get; } = VisualProperty.Add(typeof(Caret), nameof(Blink), VisualPropertyInvalidateModes.Render, true);

    /// <summary>
    /// Gets the dynamic property that controls whether the caret is shown.
    /// Changing this value invalidates rendering.
    /// </summary>
    public static VisualProperty IsShownProperty { get; } = VisualProperty.Add(typeof(Caret), nameof(IsShown), VisualPropertyInvalidateModes.Render, false);

    private WindowTimer? _blinkTimer;

    /// <summary>
    /// Initializes a new <see cref="Caret"/>. Disables input, sets blink time based on system settings
    /// (with a safe fallback), hides the caret by default, and prepares the render brush upon composition attach.
    /// </summary>
    public Caret()
    {
        DisableKeyEvents = true;
        DisablePointerEvents = true;
        BlinkTime = WiceCommons.GetCaretBlinkTime();
        if (BlinkTime == 0 || BlinkTime == uint.MaxValue) // we always want to display a caret
        {
            // default seems 530
            // https://github.com/microsoft/terminal/blob/master/src/interactivity/onecore/SystemConfigurationProvider.hpp#L43
            BlinkTime = 530;
        }
        IsVisible = false;
        ZIndex = int.MaxValue;

        //Opacity = 0.5f;
        //Width = 5;

        DoWhenAttachedToComposition(() => RenderBrush = Compositor!.CreateColorBrush(GetWindowTheme().CaretColor.ToColor()));
    }

    /// <summary>
    /// Gets the caret blink interval in milliseconds. Derived from system settings by default,
    /// with a fallback when the system indicates no blinking or an invalid value.
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual uint BlinkTime { get; protected set; }

    /// <summary>
    /// Gets or sets a value indicating whether the caret should blink.
    /// When enabled and <see cref="IsShown"/> is true, the caret toggles <see cref="Visual.IsVisible"/> on each tick.
    /// </summary>
    [Category(CategoryBehavior)]
    public bool Blink { get => (bool)GetPropertyValue(BlinkProperty)!; set => SetPropertyValue(BlinkProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the caret is shown.
    /// Setting this to true makes the caret visible and starts blinking when <see cref="Blink"/> is true;
    /// setting to false hides the caret, stops blinking, and destroys the native caret for UIA support.
    /// </summary>
    [Category(CategoryLayout)]
    public bool IsShown { get => (bool)GetPropertyValue(IsShownProperty)!; set => SetPropertyValue(IsShownProperty, value); }

    /// <summary>
    /// Gets the effective width of the caret:
    /// - If an explicit <see cref="Visual.Width"/> is set, returns that value.
    /// - Otherwise, queries the system caret width via SPI_GETCARETWIDTH.
    /// </summary>
    [Category(CategoryLayout)]
    public float FinalWidth
    {
        get
        {
            if (TryGetPropertyValue(WidthProperty, out var obj) && obj is float value)
                return value;

            uint width = 0;
            unsafe
            {
                WiceCommons.SystemParametersInfoW(SYSTEM_PARAMETERS_INFO_ACTION.SPI_GETCARETWIDTH, 0, (nint)(&width), 0);
            }
            return width;
        }
    }

    /// <summary>
    /// Gets or sets the caret location on its canvas parent in device-independent pixels.
    /// Setting the location schedules an arrange pass once the caret has been measured.
    /// </summary>
    [Category(CategoryLayout)]
    public D2D_POINT_2F Location
    {
        get => new(Canvas.GetLeft(this), Canvas.GetTop(this));
        set { Canvas.SetLeft(this, value.x); Canvas.SetTop(this, value.y); DoWhenMeasured(ArrangeWithParent); }
    }

    /// <summary>
    /// Prevents bubbling invalidate modes to the parent once measured to avoid costly relayouts.
    /// Caret manages its own arrange based on explicit coordinates.
    /// </summary>
    /// <param name="mode">The child invalidate mode.</param>
    /// <param name="defaultParentModes">Default parent modes.</param>
    /// <param name="reason">Invalidate reason.</param>
    /// <returns><see cref="VisualPropertyInvalidateModes.None"/> once measured; otherwise the default modes.</returns>
    // this would be too costly, we manage our own life
    protected internal override VisualPropertyInvalidateModes GetParentInvalidateModes(InvalidateMode mode, VisualPropertyInvalidateModes defaultParentModes, InvalidateReason reason)
    {
        if (!LastMeasureSize.HasValue)
            return defaultParentModes;

        return VisualPropertyInvalidateModes.None;
    }

    /// <summary>
    /// Arranges the caret within its parent canvas using its desired size and current canvas coordinates.
    /// No-op if the caret has not been measured or the size is invalid.
    /// </summary>
    // this presumes parent is a Window (which is a Canvas)
    private void ArrangeWithParent()
    {
        //Application.Trace(this + " Ds:" + DesiredSize);
        if (DesiredSize.IsInvalid)
            return;

        var childRect = Canvas.GetRect(DesiredSize, this);
        Arrange(childRect);
    }

    /// <summary>
    /// Stops the blinking timer if active. Safe to call multiple times.
    /// </summary>
    public virtual void StopBlinking()
    {
        try
        {
            _blinkTimer?.Change(Timeout.Infinite);
        }
        catch (ObjectDisposedException)
        {
        }
    }

    /// <summary>
    /// Starts or resumes blinking if <see cref="Blink"/> is enabled. Does nothing when blinking is disabled.
    /// </summary>
    public virtual void StartBlinking()
    {
        if (Blink)
        {
            _blinkTimer?.Change((int)BlinkTime);
        }
    }

    /// <summary>
    /// Handles a blink tick: toggles visibility and re-arms the timer when blinking and shown.
    /// </summary>
    private void OnCaretBlink()
    {
        if (!Blink || !IsShown)
            return;

        IsVisible = !IsVisible;
        StartBlinking();
    }

    /// <summary>
    /// Intercepts property setting to react to <see cref="IsShown"/> transitions
    /// by starting/stopping blinking and managing visibility.
    /// </summary>
    /// <param name="property">The property being set.</param>
    /// <param name="value">The new value.</param>
    /// <param name="options">Set options.</param>
    /// <returns>true if the stored value changed; otherwise false.</returns>
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        if (property == IsShownProperty)
        {
            if ((bool)value!)
            {
                IsVisible = true;
                StartBlinking();
            }
            else
            {
                WiceCommons.DestroyCaret(); // mostly for UI automation support
                StopBlinking();
                IsVisible = false;
            }
            return true;
        }
        return true;
    }

    /// <summary>
    /// Creates the blink timer when attached to a <see cref="Window"/> parent. The timer is not started here.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">Args.</param>
    /// <exception cref="InvalidOperationException">Thrown when the parent is not a <see cref="Wice.Window"/>.</exception>
    protected override void OnAttachedToParent(object? sender, EventArgs e)
    {
        if (Parent is not Wice.Window)
            throw new InvalidOperationException();

        Interlocked.Exchange(ref _blinkTimer, null)?.SafeDispose();
        _blinkTimer = new WindowTimer(Window!, OnCaretBlink); // not started
        base.OnAttachedToParent(sender, e);
    }

    /// <summary>
    /// Disposes the blink timer when detaching from the parent.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">Args.</param>
    protected override void OnDetachingFromParent(object? sender, EventArgs e)
    {
        Interlocked.Exchange(ref _blinkTimer, null)?.SafeDispose();
        base.OnDetachingFromParent(sender, e);
    }

    /// <summary>
    /// Disposes managed resources used by the caret (timer). Safe to call multiple times.
    /// </summary>
    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }

    /// <summary>
    /// Core dispose pattern. Disposes the blink timer when <paramref name="disposing"/> is true.
    /// </summary>
    /// <param name="disposing">True when called from <see cref="Dispose()"/>; false when from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Interlocked.Exchange(ref _blinkTimer, null)?.SafeDispose();
        }
    }
}
