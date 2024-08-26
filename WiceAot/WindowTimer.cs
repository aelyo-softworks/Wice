namespace Wice;

public partial class WindowTimer : IDisposable
{
    private Timer? _timer;
    private readonly Action? _action;

    public event EventHandler? Tick;

    public WindowTimer(Window window, Action? action = null, int dueTime = Timeout.Infinite, int period = Timeout.Infinite)
    {
        ArgumentNullException.ThrowIfNull(window);
        Window = window;
        _action = action;
        Window.AddTimer(this);
        _timer = new Timer(state => DoTick());
        Change(dueTime, period);
    }

    public Window Window { get; }

    public virtual void Change(int dueTime, int period = Timeout.Infinite) => _timer?.Change(dueTime, period);
    protected virtual void DoTick()
    {
        if (_action != null)
        {
            Window.RunTaskOnMainThread(_action);
        }
        OnTick(this, EventArgs.Empty);
    }

    protected virtual void OnTick(object? sender, EventArgs e) => Tick?.Invoke(sender, e);

    protected virtual void Dispose(bool disposing)
    {
        var timer = Interlocked.Exchange(ref _timer, null);
        if (timer != null)
        {
            if (disposing)
            {
                // dispose managed state (managed objects)
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            // set large fields to null
            Window.RemoveTimer(this);
            timer.Dispose();
        }
    }

    ~WindowTimer() { Dispose(disposing: false); }
    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }
}
