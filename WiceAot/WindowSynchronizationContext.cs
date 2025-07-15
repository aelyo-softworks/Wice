namespace Wice;

public class WindowSynchronizationContext : SynchronizationContext
{
    private SynchronizationContext? _previous;

    public int ManagedThreadId { get; } = Environment.CurrentManagedThreadId;
    public override SynchronizationContext CreateCopy() => new WindowSynchronizationContext();
    public override void Send(SendOrPostCallback d, object? state) => GetWindow()?.RunTaskOnMainThread(() => { d(state); });
    public override void Post(SendOrPostCallback d, object? state) => GetWindow()?.RunTaskOnMainThread(() => { d(state); }, true);

    public static void Install()
    {
        var current = Current;
        if (current is WindowSynchronizationContext)
            return;

        var context = new WindowSynchronizationContext { _previous = current };
        SetSynchronizationContext(context);
    }

    public static void Uninstall()
    {
        if (Current is WindowSynchronizationContext context)
        {
            SetSynchronizationContext(context._previous);
        }
    }

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

    protected virtual Window? GetWindow()
    {
        var windows = Application.GetApplication(ManagedThreadId)?.Windows;
        if (windows == null || windows.Count == 0)
            return null;

        return windows.FirstOrDefault(w => w.TaskScheduler != null && !w.IsBackground) ?? windows.FirstOrDefault(w => w.TaskScheduler != null);
    }
}
