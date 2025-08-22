namespace Wice;

/// <summary>
/// An <see cref="Application"/> variant that uses an <see cref="ApplicationScheduler"/> to drive
/// the Windows message loop and to schedule work onto the application's main thread.
/// </summary>
/// <remarks>
/// - The <see cref="Scheduler"/> is created during construction via <see cref="CreateApplicationScheduler"/> and is never null.
/// - Use <see cref="RunTaskOnMainThread(System.Action, bool)"/> to marshal work to the UI thread.
/// - Message retrieval (<see cref="GetMessage(out MSG, HWND, uint, uint)"/>) is delegated to the scheduler,
///   enabling custom message pumping strategies.
/// </remarks>
public partial class ApplicationWithScheduler : Application
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationWithScheduler"/> class and
    /// creates the associated <see cref="Scheduler"/> via <see cref="CreateApplicationScheduler"/>.
    /// </summary>
    /// <exception cref="WiceException">Thrown when the created scheduler is null ("0033").</exception>
    public ApplicationWithScheduler()
    {
        Scheduler = CreateApplicationScheduler();
        if (Scheduler == null)
            throw new WiceException("0033: Scheduler cannot be null.");
    }

    /// <summary>
    /// Creates the <see cref="ApplicationScheduler"/> used by this application instance.
    /// </summary>
    /// <returns>A new <see cref="ApplicationScheduler"/> instance.</returns>
    /// <remarks>
    /// Override to provide a custom scheduler implementation. The returned instance must not be null.
    /// </remarks>
    protected virtual ApplicationScheduler CreateApplicationScheduler() => new();

    /// <summary>
    /// Gets the <see cref="ApplicationScheduler"/> responsible for message pumping and task scheduling
    /// on the application's main thread.
    /// </summary>
    public ApplicationScheduler Scheduler { get; }

    /// <summary>
    /// Retrieves a message from the calling thread's message queue using the <see cref="Scheduler"/>.
    /// </summary>
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
    /// <remarks>
    /// When not executing on the main thread, work is scheduled using <see cref="Task.Factory.StartNew(Action, System.Threading.CancellationToken, TaskCreationOptions, TaskScheduler)"/>
    /// with <see cref="Scheduler"/> as the target scheduler.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null.</exception>
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

    /// <summary>
    /// Releases resources used by this instance.
    /// </summary>
    /// <param name="disposing">
    /// True to release managed resources; otherwise, false.
    /// </param>
    /// <remarks>
    /// Disposes the <see cref="Scheduler"/> when <paramref name="disposing"/> is true, then calls the base implementation.
    /// </remarks>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Scheduler.Dispose();
        }
        base.Dispose(disposing);
    }
}
