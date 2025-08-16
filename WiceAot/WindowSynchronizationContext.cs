namespace Wice;

/// <summary>
/// A <see cref="SynchronizationContext"/> that marshals work to the main (UI) thread
/// of the owning <see cref="Window"/> within a Wice <see cref="Application"/>.
/// </summary>
/// <remarks>
/// Usage:
/// - Call <see cref="Install"/> on the UI thread to make this context current for the calling thread.
///   This enables async continuations (await) and <see cref="TaskScheduler.FromCurrentSynchronizationContext()"/>
///   to post back to the window's main thread.
/// - Use <see cref="WithContext(System.Action)"/> or <see cref="WithContext{T}(System.Func{T})"/> to scope the installation
///   and guarantee restoration of the previous context.
/// - Call <see cref="Uninstall"/> to restore the previously installed context.
/// Behavior notes:
/// - <see cref="Send(SendOrPostCallback, object?)"/> and <see cref="Post(SendOrPostCallback, object?)"/> forward to
///   <see cref="Window.RunTaskOnMainThread(System.Action, bool)"/> when a window can be resolved via <see cref="GetWindow"/>.
///   If no window is available, the calls are ignored (no-op).
/// - The instance captures <see cref="ManagedThreadId"/> at construction time and uses it to resolve the
///   <see cref="Application"/> and its windows.
/// </remarks>
public class WindowSynchronizationContext : SynchronizationContext
{
    private SynchronizationContext? _previous;

    /// <summary>
    /// Gets the managed thread id captured when this context instance was created.
    /// </summary>
    public int ManagedThreadId { get; } = Environment.CurrentManagedThreadId;

    /// <summary>
    /// Creates a shallow copy of this synchronization context.
    /// </summary>
    /// <returns>A new <see cref="WindowSynchronizationContext"/> instance.</returns>
    public override SynchronizationContext CreateCopy() => new WindowSynchronizationContext();

    /// <summary>
    /// Dispatches a synchronous operation to the window's main thread.
    /// </summary>
    /// <param name="d">The delegate to invoke.</param>
    /// <param name="state">An optional state object passed to <paramref name="d"/>.</param>
    /// <remarks>
    /// If no window is available (<see cref="GetWindow"/> returns null), this call is ignored (no-op).
    /// </remarks>
    public override void Send(SendOrPostCallback d, object? state) => GetWindow()?.RunTaskOnMainThread(() => { d(state); });

    /// <summary>
    /// Posts an asynchronous operation to the window's main thread.
    /// </summary>
    /// <param name="d">The delegate to invoke.</param>
    /// <param name="state">An optional state object passed to <paramref name="d"/>.</param>
    /// <remarks>
    /// If no window is available (<see cref="GetWindow"/> returns null), this call is ignored (no-op).
    /// </remarks>
    public override void Post(SendOrPostCallback d, object? state) => GetWindow()?.RunTaskOnMainThread(() => { d(state); }, true);

    /// <summary>
    /// Installs a <see cref="WindowSynchronizationContext"/> as the current context for the calling thread,
    /// preserving the previous context for later restoration via <see cref="Uninstall"/>.
    /// </summary>
    /// <remarks>
    /// If the current context is already a <see cref="WindowSynchronizationContext"/>, this method does nothing.
    /// </remarks>
    public static void Install()
    {
        var current = Current;
        if (current is WindowSynchronizationContext)
            return;

        var context = new WindowSynchronizationContext { _previous = current };
        SetSynchronizationContext(context);
    }

    /// <summary>
    /// Restores the previously active <see cref="SynchronizationContext"/> that was captured at <see cref="Install"/> time.
    /// </summary>
    /// <remarks>
    /// If the current context is not a <see cref="WindowSynchronizationContext"/>, this method does nothing.
    /// </remarks>
    public static void Uninstall()
    {
        if (Current is WindowSynchronizationContext context)
        {
            SetSynchronizationContext(context._previous);
        }
    }

    /// <summary>
    /// Executes <paramref name="action"/> with a <see cref="WindowSynchronizationContext"/> installed
    /// and restores the previous context afterward.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
    public static void WithContext(Action action)
    {
        ExceptionExtensions.ThrowIfNull(action, nameof(action));
        try
        {
            Install();
            action();
        }
        finally
        {
            Uninstall();
        }
    }

    /// <summary>
    /// Executes <paramref name="action"/> with a <see cref="WindowSynchronizationContext"/> installed,
    /// returns its result, and restores the previous context afterward.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="action">The function to execute.</param>
    /// <returns>The value returned by <paramref name="action"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
    public static T WithContext<T>(Func<T> action)
    {
        ExceptionExtensions.ThrowIfNull(action, nameof(action));
        try
        {
            Install();
            return action();
        }
        finally
        {
            Uninstall();
        }
    }

    /// <summary>
    /// Resolves a window to use for marshaling work to the main thread.
    /// </summary>
    /// <returns>
    /// The first non-background <see cref="Window"/> with a scheduler when available; otherwise the first window
    /// with a scheduler; or <see langword="null"/> when no suitable window exists.
    /// </returns>
    /// <remarks>
    /// The lookup is performed against the <see cref="Application"/> instance associated with <see cref="ManagedThreadId"/>.
    /// </remarks>
    protected virtual Window? GetWindow()
    {
        var windows = Application.GetApplication(ManagedThreadId)?.Windows;
        if (windows == null || windows.Count == 0)
            return null;

        return windows.FirstOrDefault(w => w.TaskScheduler != null && !w.IsBackground) ?? windows.FirstOrDefault(w => w.TaskScheduler != null);
    }
}
