namespace Wice.Animation;

/// <summary>
/// Storyboard that drives animation ticks synchronized with the Windows Composition
/// <c>CompositorController</c> commit cycle.
/// </summary>
public partial class CompositorControllerStoryboard(Window window) : Storyboard(window)
{
    private int _tickDivider = 1;
    private volatile bool _stop;

    /// <summary>
    /// Gets or sets the tick divider used to throttle calls.
    /// </summary>
    public virtual int TickDivider { get => _tickDivider; set => _tickDivider = value.Clamp(1); }

    /// <inheritdoc/>
    public override void Stop()
    {
        _stop = true;
        base.Stop();
    }

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

    /// <inheritdoc/>
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
