namespace Wice;

/// <summary>
/// A custom TaskScheduler that integrates with the Win32 message loop to schedule and execute tasks
/// on the thread processing window messages.
/// </summary>
/// <remarks>
/// - Tasks are enqueued via <see cref="QueueTask(Task)"/> and are executed when the internal dequeue
///   event is signaled (see <see cref="TriggerDequeue()"/>) or when <see cref="Dequeue(bool)"/> is called
///   with <c>execute</c> set to <see langword="true"/>.
/// - The scheduler waits alongside the thread's message queue by using <c>MsgWaitForMultipleObjectsEx</c>
///   in <see cref="GetMessage(out MSG, HWND, uint, uint)"/>. When messages arrive, they are returned to the caller;
///   when the dequeue event fires, pending tasks are drained and optionally executed.
/// - Call <see cref="Dispose()"/> to stop the scheduler and release OS handles. If <see cref="DequeueOnDispose"/>
///   is <see langword="true"/>, any remaining tasks are executed during disposal.
/// - This scheduler is typically used from a UI thread and is not intended to run tasks inline
///   (see <see cref="TryExecuteTaskInline(Task, bool)"/>).
/// </remarks>
public partial class ApplicationScheduler : TaskScheduler, IDisposable
{
    private const int StopIndex = 0;
    private const int DequeueIndex = StopIndex + 1;

    private readonly ConcurrentQueue<Task> _tasks = new();
    private readonly AutoResetEvent _stop = new(false);
    private readonly AutoResetEvent _dequeue = new(false);
    private readonly nint[] _waitHandles = new nint[DequeueIndex + 1];
    private bool _disposedValue;

    /// <summary>
    /// Occurs immediately before a queued task is executed by <see cref="Dequeue(bool)"/>.
    /// </summary>
    public event EventHandler? Executing;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationScheduler"/> class.
    /// </summary>
    /// <remarks>
    /// Sets <see cref="WaitTimeout"/> to 1000 ms and initializes the native wait handles used by
    /// the message loop integration (stop and dequeue events).
    /// </remarks>
    public ApplicationScheduler()
    {
        WaitTimeout = 1000;
        _waitHandles[StopIndex] = _stop.SafeWaitHandle.DangerousGetHandle();
        _waitHandles[DequeueIndex] = _dequeue.SafeWaitHandle.DangerousGetHandle();
    }

    /// <summary>
    /// Gets the number of tasks currently queued.
    /// </summary>
    public int QueueCount => _tasks.Count;

    /// <summary>
    /// Gets the timestamp of the last successful dequeue trigger, used to throttle <see cref="TriggerDequeue()"/>.
    /// </summary>
    public DateTime LastDequeue { get; protected set; }

    /// <summary>
    /// Gets or sets the minimum time, in milliseconds, between consecutive dequeue triggers.
    /// </summary>
    /// <remarks>
    /// - If set to 0 or less, dequeue triggers are not throttled and are signaled immediately.
    /// - If greater than 0, <see cref="TriggerDequeue()"/> only signals when at least this interval has elapsed
    ///   since <see cref="LastDequeue"/>.
    /// </remarks>
    public virtual int DequeueTimeout { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to dequeue and execute remaining tasks during disposal.
    /// </summary>
    public virtual bool DequeueOnDispose { get; set; }

    /// <summary>
    /// Gets or sets the maximum wait time, in milliseconds, for <see cref="GetMessage(out MSG, HWND, uint, uint)"/>
    /// when waiting for either new input or internal events.
    /// </summary>
    public virtual int WaitTimeout { get; set; }

    /// <summary>
    /// Returns an enumerable of the tasks currently scheduled to this scheduler.
    /// </summary>
    /// <remarks>
    /// Used by the debugger and the TPL infrastructure to enumerate scheduled tasks.
    /// </remarks>
    protected override IEnumerable<Task>? GetScheduledTasks() => _tasks;

    /// <summary>
    /// Returns <see langword="false"/> to prevent inline task execution; tasks are executed via the dequeue mechanism.
    /// </summary>
    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => false;

    /// <summary>
    /// Queues a <see cref="Task"/> for execution and signals the dequeue event (subject to <see cref="DequeueTimeout"/>).
    /// </summary>
    /// <param name="task">The task to queue.</param>
    protected override void QueueTask(Task task)
    {
        ArgumentNullException.ThrowIfNull(task);
        if (_disposedValue)
            return;

        _tasks.Enqueue(task);
        TriggerDequeue();
    }

    /// <summary>
    /// Waits for either window messages, a stop request, or a dequeue signal and optionally executes queued tasks.
    /// </summary>
    /// <param name="msg">When the method returns TRUE, receives the next message removed from the queue.</param>
    /// <param name="hWnd">A window handle to filter messages, or <see cref="HWND.Null"/> for all.</param>
    /// <param name="wMsgFilterMin">The integer value of the lowest message value to be retrieved.</param>
    /// <param name="wMsgFilterMax">The integer value of the highest message value to be retrieved.</param>
    /// <returns>
    /// - TRUE if a message was removed and returned in <paramref name="msg"/>.<br/>
    /// - FALSE if a quit/stop condition was encountered; in this case, <paramref name="msg"/> contains a sentinel
    ///   (e.g., <c>WM_QUIT</c> or <see cref="Application.WM_HOSTQUIT"/>).<br/>
    /// - In non-DEBUG builds, may return -1 (error) on wait failure.
    /// </returns>
    /// <remarks>
    /// - When input is available, the function removes one message via <c>PeekMessageW</c> and returns its status.<br/>
    /// - When the internal dequeue event is signaled, pending tasks are dequeued and executed on the calling thread.<br/>
    /// - When the stop event is signaled or the scheduler is disposed/invalid, a host-quit message is returned and the function stops.
    /// </remarks>
    protected unsafe internal BOOL GetMessage(out MSG msg, HWND hWnd, uint wMsgFilterMin, uint wMsgFilterMax)
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
                    return -1;
#endif
                }

                var ie = (int)evt;
                if (ie == count) // new input available
                {
                    var ret = Functions.PeekMessageW(out msg, hWnd, wMsgFilterMin, wMsgFilterMax, PEEK_MESSAGE_REMOVE_TYPE.PM_REMOVE);
                    if (msg.message == MessageDecoder.WM_QUIT) // WM_QUIT is a special message that indicates the application should stop processing messages
                        return false;

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

    /// <summary>
    /// Raises the <see cref="Executing"/> event.
    /// </summary>
    /// <param name="sender">The sender, typically <see langword="this"/>.</param>
    /// <param name="e">Event arguments.</param>
    protected virtual void OnExecuting(object sender, EventArgs e) => Executing?.Invoke(this, e);

    /// <summary>
    /// Removes all queued tasks without executing them.
    /// </summary>
    public virtual void ClearQueue() => Dequeue(false);

    /// <summary>
    /// Signals the internal dequeue event, optionally throttled by <see cref="DequeueTimeout"/>.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the signal was sent; otherwise, <see langword="false"/> (e.g., throttled or disposed).
    /// </returns>
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

    /// <summary>
    /// Dequeues all pending tasks and optionally executes them on the current thread.
    /// </summary>
    /// <param name="execute">
    /// If <see langword="true"/>, each dequeued task is executed via <see cref="TryExecuteTask(Task)"/> and
    /// <see cref="OnExecuting(object, EventArgs)"/> is raised beforehand.
    /// </param>
    /// <returns>The number of tasks dequeued.</returns>
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

    /// <summary>
    /// Releases resources used by the scheduler and signals termination to any waiting loops.
    /// </summary>
    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }

    /// <summary>
    /// Disposes the scheduler, signaling stop and releasing native handles. Optionally executes remaining tasks.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> when called from <see cref="Dispose()"/>; otherwise, from a finalizer (not used).
    /// </param>
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
