namespace Wice.Animation;

/// <summary>
/// Storyboard that drives animation ticks synchronized with the Windows Composition
/// <c>CompositorController</c> commit cycle.
/// </summary>
/// <remarks>
/// - When started, this storyboard spawns a dedicated background thread that repeatedly:
///   1) Awaits the previous composition commit to complete via <c>EnsurePreviousCommitCompletedAsync</c>.
///   2) Optionally throttles ticks using <see cref="TickDivider"/>.
///   3) Invokes <see cref="Storyboard.OnTick"/> to progress child animations.
/// - No implicit marshaling to the UI thread is performed here; derived animations or the
///   <see cref="Window"/> infrastructure are responsible for any required thread affinity.
/// - Call <see cref="Stop"/> to request termination; the running loop observes a volatile flag for fast exit.
/// </remarks>
public partial class CompositorControllerStoryboard(Window window) : Storyboard(window)
{
    /// <summary>
    /// Stores the effective tick divider. Values are clamped to be at least 1.
    /// </summary>
    private int _tickDivider = 1;

    /// <summary>
    /// Cooperative cancellation flag observed by the driver loop in <see cref="Callback(object?)"/>.
    /// Volatile to ensure timely visibility across threads.
    /// </summary>
    private volatile bool _stop;

    /// <summary>
    /// Gets or sets the tick divider used to throttle calls to <see cref="Storyboard.OnTick"/>.
    /// </summary>
    /// <remarks>
    /// - 1: call <see cref="OnTick"/> on every completed composition commit (no throttling).
    /// - N &gt; 1: call <see cref="OnTick"/> every Nth completed commit to reduce animation frequency.
    /// The value is clamped to a minimum of 1.
    /// </remarks>
    public virtual int TickDivider { get => _tickDivider; set => _tickDivider = value.Clamp(1); }

    /// <summary>
    /// Requests the storyboard driver loop to stop and delegates to the base implementation.
    /// </summary>
    /// <remarks>
    /// Sets a volatile flag observed by the background loop so it can exit gracefully.
    /// </remarks>
    public override void Stop()
    {
        _stop = true;
        base.Stop();
    }

    /// <summary>
    /// Background thread entry point that synchronizes ticks with composition commits.
    /// </summary>
    /// <param name="state">A <see cref="CompositorController"/> instance passed when the thread starts.</param>
    /// <remarks>
    /// Loop:
    /// - If stop is requested, exit immediately.
    /// - Await completion of previous compositor commit (non-blocking, does not capture context).
    /// - Apply <see cref="TickDivider"/> to throttle tick frequency.
    /// - Invoke <see cref="OnTick"/> to progress animations.
    /// The method is intentionally <c>async void</c> because it is used as a thread callback.
    /// </remarks>
    private async void Callback(object? state)
    {
        //Application.Trace("t:" + Thread.CurrentThread.ManagedThreadId + " init");
        var controller = (CompositorController)state!;
        var smallCount = 0;

        do
        {
            if (_stop)
                return;

            // Await completion of the previous compositor commit before attempting to tick.
            await controller.EnsurePreviousCommitCompletedAsync().AsTask().ConfigureAwait(false);

            if (_stop)
                return;

            // Apply tick throttling if requested through TickDivider.
            if (_tickDivider > 1)
            {
                smallCount++;
                if (smallCount != _tickDivider)
                    continue;

                smallCount = 0;
            }

            //Application.Trace("t:" + Thread.CurrentThread.ManagedThreadId + " tick");
            OnTick();
        }
        while (true);
    }

    /// <summary>
    /// Starts the storyboard by creating a background thread bound to the window's compositor controller.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the owning <see cref="Window"/> has no <c>CompositorController</c>.
    /// </exception>
    public override void Start()
    {
        var controller = Window?.CompositorController;
        if (controller == null)
            throw new InvalidOperationException();

        base.Start();

        // Run the driver loop on a dedicated background thread to avoid blocking UI or composition threads.
        var t = new Thread(Callback)
        {
            Name = "_ccsb_",
            IsBackground = true
        };
        t.Start(controller);
    }
}
