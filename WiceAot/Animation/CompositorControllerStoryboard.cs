namespace Wice.Animation;

public class CompositorControllerStoryboard(Window window) : Storyboard(window)
{
    private int _tickDivider = 1;
    private volatile bool _stop;

    public virtual int TickDivider { get => _tickDivider; set => _tickDivider = value.Clamp(1); }

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

            await controller.EnsurePreviousCommitCompletedAsync().AsTask().ConfigureAwait(false);
            if (_stop)
                return;

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

    public override void Start()
    {
        var controller = Window?.CompositorController;
        if (controller == null)
            throw new InvalidOperationException();

        base.Start();
        var t = new Thread(Callback)
        {
            Name = "_ccsb_",
            IsBackground = true
        };
        t.Start(controller);
    }
}
