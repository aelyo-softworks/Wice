namespace Wice.Animation;

/// <summary>
/// A <see cref="Storyboard"/> implementation that drives ticks using a <see cref="System.Threading.Timer"/>.
/// </summary>
/// <remarks>
/// - Ticks occur on a ThreadPool thread, not on the UI thread. If child animations require UI-thread affinity,
///   ensure they marshal to the UI thread as needed (e.g., via <see cref="Window.RequestRender"/> or equivalent).
/// - Starting the storyboard via the parameterless <see cref="Start()"/> is not supported; use one of the
///   overloads that accept a period.
/// - Safe to call <see cref="Stop"/> and <see cref="Dispose()"/> multiple times; they no-op when already stopped/disposed.
/// </remarks>
/// <param name="window">The window associated with this storyboard.</param>
public partial class TimerStoryboard(Window window) : Storyboard(window), IDisposable
{
    private Timer? _timer;

    // ~15.6ms (approx. 64 Hz), matches typical desktop timer granularity.
    private const int _defaultPeriod = 999 / 64;

    /// <summary>
    /// Not supported. Use <see cref="Start(int, int?)"/> or <see cref="Start(TimeSpan, TimeSpan?)"/> to
    /// specify a tick period (and optionally a due time).
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed override void Start() => throw new NotSupportedException();

    /// <summary>
    /// Starts the storyboard using a period expressed in milliseconds and an optional due time in milliseconds.
    /// </summary>
    /// <param name="period">
    /// Period in milliseconds between ticks. Defaults to approximately 15.6 ms (64 Hz).
    /// </param>
    /// <param name="dueTime">
    /// Optional initial delay in milliseconds before the first tick. When <see langword="null"/>,
    /// the first tick occurs immediately after <see cref="Start(TimeSpan, TimeSpan?)"/> schedules the timer.
    /// </param>
    /// <remarks>
    /// Internally converts to <see cref="TimeSpan"/> and calls <see cref="Start(TimeSpan, TimeSpan?)"/>.
    /// </remarks>
    public void Start(int period = _defaultPeriod, int? dueTime = null)
    {
        TimeSpan? ts;
        if (dueTime.HasValue)
        {
            ts = TimeSpan.FromMilliseconds(dueTime.Value);
        }
        else
        {
            ts = null;
        }

        Start(TimeSpan.FromMilliseconds(period), ts);
    }

    /// <summary>
    /// Starts the storyboard with the specified <paramref name="period"/> and optional <paramref name="dueTime"/>.
    /// </summary>
    /// <param name="period">The interval between ticks.</param>
    /// <param name="dueTime">
    /// Optional initial delay before the first tick. Defaults to <see cref="TimeSpan.Zero"/> when <see langword="null"/>.
    /// </param>
    /// <remarks>
    /// - Stops any existing timer instance before starting.<br/>
    /// - Calls <see cref="Storyboard.Start()"/> to reset internal state and optionally tick immediately depending on base behavior.<br/>
    /// - Creates a <see cref="System.Threading.Timer"/> that invokes <see cref="OnTick()"/> on a ThreadPool thread.
    /// </remarks>
    public virtual void Start(TimeSpan period, TimeSpan? dueTime = null)
    {
        Stop();
        dueTime ??= TimeSpan.Zero;
        DisposeTimer();
        base.Start();
        _timer = new Timer((state) => OnTick(), null, dueTime.Value, period);
    }

    /// <summary>
    /// Stops the storyboard and disposes the underlying timer if active.
    /// </summary>
    /// <remarks>
    /// Safe to call multiple times.
    /// </remarks>
    public override void Stop()
    {
        DisposeTimer();
        base.Stop();
    }

    // Disposes the current timer atomically, if any.
    private void DisposeTimer() => Interlocked.Exchange(ref _timer, null)?.SafeDispose();

    /// <summary>
    /// Releases resources used by this instance.
    /// </summary>
    /// <remarks>
    /// Equivalent to calling <see cref="Dispose(bool)"/> with <see langword="true"/>.
    /// </remarks>
    public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }

    /// <summary>
    /// Core dispose pattern. Disposes the timer when <paramref name="disposing"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="disposing">
    /// True when called from <see cref="Dispose()"/>; false when called from a finalizer.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisposeTimer();
        }
    }
}
