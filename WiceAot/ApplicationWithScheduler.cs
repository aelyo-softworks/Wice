namespace Wice;

public partial class ApplicationWithScheduler : Application
{
    public ApplicationWithScheduler()
    {
        Scheduler = CreateApplicationScheduler();
        if (Scheduler == null)
            throw new WiceException("0033: Scheduler cannot be null.");
    }

    protected virtual ApplicationScheduler CreateApplicationScheduler() => new();
    public ApplicationScheduler Scheduler { get; }

    protected override bool GetMessage(out MSG msg, HWND hWnd, uint wMsgFilterMin, uint wMsgFilterMax) => Scheduler.GetMessage(out msg, hWnd, wMsgFilterMin, wMsgFilterMax);
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

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Scheduler.Dispose();
        }
        base.Dispose(disposing);
    }
}
