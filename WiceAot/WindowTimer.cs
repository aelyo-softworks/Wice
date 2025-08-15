namespace Wice;

/// <summary>
/// Provides a window-affined timer that marshals an optional callback onto the UI thread and raises a <see cref="Tick"/> event on each tick.
/// </summary>
/// <remarks>
/// - Wraps <see cref="System.Threading.Timer"/> to schedule ticks.
/// - If <paramref name="_action"/> is provided, it is executed on the window's main thread via <see cref="Window.RunTaskOnMainThread(Action, bool)"/>.
/// - The <see cref="Tick"/> event is raised on the timer's callback thread (not the UI thread). Consumers that update UI must marshal to the UI thread.
/// </remarks>
/// <seealso cref="Window"/>
/// <seealso cref="System.Threading.Timer"/>
public partial class WindowTimer : IDisposable
{
    /// <summary>
    /// The underlying <see cref="System.Threading.Timer"/> driving ticks. May be <see langword="null"/> once disposed.
    /// </summary>
    private Timer? _timer;

    /// <summary>
    /// Optional action to run on each tick. Executed on the window's main thread when present.
    /// </summary>
    private readonly Action? _action;

    /// <summary>
    /// Occurs after the timer fires and the optional <see cref="_action"/> has been queued to the UI thread.
    /// </summary>
    /// <remarks>
    /// This event is raised on the timer's callback thread (thread-pool) rather than the UI thread.
    /// If you need to interact with UI, marshal to the UI thread using <see cref="Window.RunTaskOnMainThread(Action, bool)"/>.
    /// </remarks>
    public event EventHandler? Tick;

    /// <summary>
    /// Initializes a new instance of <see cref="WindowTimer"/>.
    /// </summary>
    /// <param name="window">The window whose main thread will execute <paramref name="action"/>.</param>
    /// <param name="action">Optional action executed on each tick on the window's main thread.</param>
    /// <param name="dueTime">
    /// The amount of time to delay before the first tick occurs, in milliseconds.
    /// Use <see cref="Timeout.Infinite"/> to prevent the timer from starting.
    /// </param>
    /// <param name="period">
    /// The time interval between subsequent ticks, in milliseconds.
    /// Use <see cref="Timeout.Infinite"/> to disable periodic signaling.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="window"/> is <see langword="null"/>.</exception>
    public WindowTimer(Window window, Action? action = null, int dueTime = Timeout.Infinite, int period = Timeout.Infinite)
    {
        ExceptionExtensions.ThrowIfNull(window, nameof(window));
        Window = window;
        _action = action;
        _timer = new Timer(state => DoTick());
        Change(dueTime, period);
    }

    /// <summary>
    /// Gets the window associated with this timer. Used to marshal callbacks to the UI thread.
    /// </summary>
    public Window Window { get; }

    /// <summary>
    /// Changes the start time and the interval between ticks.
    /// </summary>
    /// <param name="dueTime">
    /// The amount of time to delay before the timer starts, in milliseconds.
    /// Use <see cref="Timeout.Infinite"/> to prevent the timer from starting.
    /// </param>
    /// <param name="period">
    /// The time interval between ticks, in milliseconds.
    /// Use <see cref="Timeout.Infinite"/> to disable periodic signaling.
    /// </param>
    /// <remarks>
    /// Delegates to <see cref="System.Threading.Timer.Change(int, int)"/> on the underlying timer.
    /// No-op if the timer has been disposed.
    /// </remarks>
    public virtual void Change(int dueTime, int period = Timeout.Infinite) => _timer?.Change(dueTime, period);

    /// <summary>
    /// Called when the underlying timer fires.
    /// </summary>
    /// <remarks>
    /// Queues <see cref="_action"/> to execute on the window's main thread (if provided), then raises <see cref="Tick"/> on the timer thread.
    /// </remarks>
    protected virtual void DoTick()
    {
        if (_action != null)
        {
            Window.RunTaskOnMainThread(_action);
        }
        OnTick(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="Tick"/> event.
    /// </summary>
    /// <param name="sender">The event sender (typically <see langword="this"/>).</param>
    /// <param name="e">The event data.</param>
    protected virtual void OnTick(object? sender, EventArgs e) => Tick?.Invoke(sender, e);

    /// <summary>
    /// Disposes the timer and releases associated resources.
    /// </summary>
    /// <remarks>Safe to call multiple times.</remarks>
    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }

    /// <summary>
    /// Disposes managed resources when <paramref name="disposing"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Interlocked.Exchange(ref _timer, null)?.SafeDispose();
        }
    }
}
