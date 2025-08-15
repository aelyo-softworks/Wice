namespace Wice.Animation;

/// <summary>
/// A storyboard driven by the system's vertical blank (VBlank) ticks.
/// </summary>
/// <remarks>
/// - Wraps a <c>VerticalBlankTicker</c> and relays its <c>Tick</c> event to <see cref="Storyboard.OnTick()"/>.
/// - Subscribes to the ticker on <see cref="Start"/> and unsubscribes on <see cref="Stop"/> to avoid leaks.
/// - <see cref="TickDivider"/> allows throttling the effective tick rate by integer division.
/// </remarks>
/// <param name="window">
/// The owning <see cref="Window"/> used by the base <see cref="Storyboard"/>.
/// </param>
/// <seealso cref="Storyboard"/>
public partial class VerticalBlankStoryboard(Window window) : Storyboard(window)
{
    // Drives ticks off the display's vertical blank.
    private readonly VerticalBlankTicker _ticker = new();

    /// <summary>
    /// Gets or sets the integer divisor applied to the VBlank frequency to reduce the number of storyboard ticks.
    /// </summary>
    /// <remarks>
    /// For example, a value of 2 will invoke <see cref="OnTick()"/> every other VBlank. The value is forwarded
    /// to the underlying <c>VerticalBlankTicker</c>.
    /// </remarks>
    public virtual int TickDivider { get => _ticker.TickDivider; set => _ticker.TickDivider = value; }

    /// <summary>
    /// Forwards the ticker's <c>Tick</c> event to the storyboard's tick handler.
    /// </summary>
    /// <param name="sender">The ticker raising the event.</param>
    /// <param name="e">Event arguments.</param>
    private void OnTickerTick(object? sender, EventArgs e) => OnTick();

    /// <summary>
    /// Stops the storyboard and detaches from the ticker to prevent further callbacks.
    /// </summary>
    public override void Stop()
    {
        _ticker.Tick -= OnTickerTick;
        base.Stop();
    }

    /// <summary>
    /// Starts the storyboard by subscribing to the ticker and ensuring it is running.
    /// </summary>
    public override void Start()
    {
        base.Start();
        _ticker.Tick += OnTickerTick;
        _ticker.EnsureStarted();
    }
}
