namespace Wice;

/// <summary>
/// An <see cref="Application"/> variant that uses an <see cref="ApplicationScheduler"/> to drive
/// the Windows message loop and to schedule work onto the application's main thread.
/// </summary>
public partial class ApplicationWithScheduler : Application
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationWithScheduler"/> class and
    /// creates the associated <see cref="Scheduler"/> via <see cref="CreateApplicationScheduler"/>.
    /// </summary>
    public ApplicationWithScheduler()
    {
        Scheduler = CreateApplicationScheduler();
        if (Scheduler == null)
            throw new WiceException("0035: Scheduler cannot be null.");
    }

    /// <summary>
    /// Creates the <see cref="ApplicationScheduler"/> used by this application instance.
    /// </summary>
    /// <returns>A new <see cref="ApplicationScheduler"/> instance.</returns>
    protected virtual ApplicationScheduler CreateApplicationScheduler() => new();

    /// <summary>
    /// Gets the <see cref="ApplicationScheduler"/> responsible for message pumping and task scheduling
    /// on the application's main thread.
    /// </summary>
    public ApplicationScheduler Scheduler { get; }

    /// <inheritdoc/>
    protected override BOOL GetMessage(out MSG msg, HWND hWnd, uint wMsgFilterMin, uint wMsgFilterMax) => Scheduler.GetMessage(out msg, hWnd, wMsgFilterMin, wMsgFilterMax);

    /// <summary>
    /// Executes the specified <paramref name="action"/> on the application's main thread.
    /// </summary>
    /// <param name="action">The work to execute.</param>
    /// <param name="startNew">
    /// If false and already on the main thread, runs inline and returns a completed task; otherwise schedules via <see cref="Scheduler"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the scheduled work when marshaled, or <see cref="Task.CompletedTask"/> when executed inline.
    /// </returns>
    public virtual Task RunTaskOnMainThread(Action action, bool startNew = false)
    {
        ExceptionExtensions.ThrowIfNull(action, nameof(action));
        if (!startNew && IsRunningAsMainThread)
        {
            action();
            return Task.CompletedTask;
        }

        return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, Scheduler);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Scheduler.Dispose();
        }
        base.Dispose(disposing);
    }
}
