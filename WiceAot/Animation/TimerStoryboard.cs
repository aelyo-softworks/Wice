namespace Wice.Animation;

public partial class TimerStoryboard(Window window) : Storyboard(window), IDisposable
{
    private Timer? _timer;
    private const int _defaultPeriod = 999 / 64;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed override void Start() => throw new NotSupportedException();

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

    public virtual void Start(TimeSpan period, TimeSpan? dueTime = null)
    {
        Stop();
        dueTime ??= TimeSpan.Zero;
        DisposeTimer();
        base.Start();
        _timer = new Timer((state) => OnTick(), null, dueTime.Value, period);
    }

    public override void Stop()
    {
        DisposeTimer();
        base.Stop();
    }

    private void DisposeTimer() => Interlocked.Exchange(ref _timer, null)?.Dispose();

    ~TimerStoryboard() { Dispose(false); }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisposeTimer();
        }
    }
}
