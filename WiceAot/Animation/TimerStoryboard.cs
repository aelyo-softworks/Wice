namespace Wice.Animation;

/// <summary>
/// A <see cref="Storyboard"/> implementation that drives ticks using a <see cref="System.Threading.Timer"/>.
/// </summary>
public partial class TimerStoryboard(Window window) : Storyboard(window), IDisposable
{
    private Timer? _timer;

    // ~15.6ms (approx. 64 Hz), matches typical desktop timer granularity.
    private const int _defaultPeriod = 999 / 64;

    /// <summary>
    /// Not supported. Use <see cref="Start(int, int?)"/> or <see cref="Start(TimeSpan, TimeSpan?)"/> to
    /// specify a tick period (and optionally a due time).
    /// </summary>
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
    public virtual void Start(TimeSpan period, TimeSpan? dueTime = null)
    {
        Stop();
        dueTime ??= TimeSpan.Zero;
        DisposeTimer();
        base.Start();
        _timer = new Timer((state) => OnTick(), null, dueTime.Value, period);
    }

    /// <inheritdoc/>
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
