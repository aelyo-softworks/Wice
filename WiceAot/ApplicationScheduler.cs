namespace Wice;

public partial class ApplicationScheduler : TaskScheduler, IDisposable
{
    private const int StopIndex = 0;
    private const int DequeueIndex = StopIndex + 1;

    private readonly ConcurrentQueue<Task> _tasks = new();
    private readonly AutoResetEvent _stop = new(false);
    private readonly AutoResetEvent _dequeue = new(false);
    private readonly nint[] _waitHandles = new nint[DequeueIndex + 1];
    private bool _disposedValue;

    public event EventHandler? Executing;

    public ApplicationScheduler()
    {
        WaitTimeout = 1000;
        _waitHandles[StopIndex] = _stop.SafeWaitHandle.DangerousGetHandle();
        _waitHandles[DequeueIndex] = _dequeue.SafeWaitHandle.DangerousGetHandle();
    }

    public int QueueCount => _tasks.Count;
    public DateTime LastDequeue { get; protected set; }
    public virtual int DequeueTimeout { get; set; }
    public virtual bool DequeueOnDispose { get; set; }
    public virtual int WaitTimeout { get; set; }

    protected override IEnumerable<Task>? GetScheduledTasks() => _tasks;
    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => false;
    protected override void QueueTask(Task task)
    {
        ArgumentNullException.ThrowIfNull(task);
        if (_disposedValue)
            return;

        _tasks.Enqueue(task);
        TriggerDequeue();
    }

    protected unsafe internal bool GetMessage(out MSG msg, HWND hWnd, uint wMsgFilterMin, uint wMsgFilterMax)
    {
        if (_disposedValue || _stop.SafeWaitHandle.IsInvalid || _dequeue.SafeWaitHandle.IsInvalid)
        {
            msg = new MSG { message = Application.WM_HOSTQUIT };
            return false;
        }

        var count = _waitHandles.Length;
        fixed (nint* waitHandles = _waitHandles)
        {
            do
            {
                var evt = Functions.MsgWaitForMultipleObjectsEx(
                    (uint)count,
                    (nint)waitHandles,
                    (uint)WaitTimeout,
                    QUEUE_STATUS_FLAGS.QS_ALLINPUT,
                    MSG_WAIT_FOR_MULTIPLE_OBJECTS_EX_FLAGS.MWMO_INPUTAVAILABLE);
                if (evt == WAIT_EVENT.WAIT_FAILED)
                {
#if DEBUG
                    throw new WiceException("0034: GetMessage wait failed.");
#else
                    msg = new MSG { message = Application.WM_HOSTQUIT };
                    return false;
#endif
                }

                var ie = (int)evt;
                if (ie == count) // new input available
                {
                    var ret = Functions.PeekMessageW(out msg, hWnd, wMsgFilterMin, wMsgFilterMax, PEEK_MESSAGE_REMOVE_TYPE.PM_REMOVE);
                    if (ret)
                        return ret;

                    continue;
                }

                if (ie == StopIndex)
                {
                    msg = new MSG { message = Application.WM_HOSTQUIT };
                    return false;
                }

                if (ie == DequeueIndex)
                {
                    Dequeue(true);
                }
            }
            while (true);
        }
    }

    protected virtual void OnExecuting(object sender, EventArgs e) => Executing?.Invoke(this, e);
    public virtual void ClearQueue() => Dequeue(false);
    public virtual bool TriggerDequeue()
    {
        if (_disposedValue)
            return false;

        if (DequeueTimeout <= 0)
            return _dequeue != null && _dequeue.Set();

        var ts = DateTime.Now - LastDequeue;
        if (ts.TotalMilliseconds < DequeueTimeout)
            return false;

        LastDequeue = DateTime.Now;
        return _dequeue != null && _dequeue.Set();
    }

    protected virtual int Dequeue(bool execute)
    {
        if (_disposedValue)
            return 0;

        var count = 0;
        do
        {
            if (!_tasks.TryDequeue(out var task))
                break;

            if (execute)
            {
                OnExecuting(this, EventArgs.Empty);
                TryExecuteTask(task);
            }
            count++;
        }
        while (true);
        return count;
    }

    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _stop.Set();
                _stop.SafeDispose();

                _dequeue.SafeDispose();

                if (DequeueOnDispose)
                {
                    Dequeue(true);
                }
            }

            _disposedValue = true;
        }
    }
}
