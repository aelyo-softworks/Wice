namespace Wice;

public partial class Application : IDisposable
{
    private bool _disposedValue;

    public event EventHandler? ApplicationExit;

    public Application()
    {
        var apps = _applications;
        if (apps == null)
            throw new WiceException("0030: It's not possible to create applications any more.");

        // no, we can't use Windows.System.DispatcherQueueController.CreateOnDedicatedThread();
        // or compositor will raise an access denied error
#if NETFRAMEWORK
        DispatcherQueueController = DirectN.DispatcherQueueController.Create();
#else
        DispatcherQueueController = new WindowsDispatcherQueueController();
        DispatcherQueueController.EnsureOnCurrentThread();
#endif

        apps.AddOrUpdate(Environment.CurrentManagedThreadId, this, (k, o) =>
        {
            throw new WiceException("0006: There can be only one Application instance for a given thread.");
        });

        MainThreadId = Environment.CurrentManagedThreadId;
        ThreadId = WiceCommons.GetCurrentThreadId();
        ResourceManager = CreateResourceManager();
    }

#if NETFRAMEWORK
    public IDispatcherQueueController DispatcherQueueController { get; }
#else
    public WindowsDispatcherQueueController DispatcherQueueController { get; }
#endif
    public bool ExitOnLastWindowRemoved { get; set; } = true;
    public int MainThreadId { get; }
    public uint ThreadId { get; }
    public bool IsRunningAsMainThread => MainThreadId == Environment.CurrentManagedThreadId;
    public bool IsDisposed => _disposedValue;
    public ResourceManager ResourceManager { get; }
    public virtual bool HandleErrors => MainThreadId == 1; // we handle errors only on the first UI thread

    public IReadOnlyList<Window> Windows
    {
        get
        {
            lock (_windowsLock)
            {
                return [.. _allWindows.Where(w => w.ManagedThreadId == MainThreadId)];
            }
        }
    }

    public IReadOnlyList<Window> BackgroundWindows
    {
        get
        {
            lock (_windowsLock)
            {
                return [.. _allWindows.Where(w => w.ManagedThreadId == MainThreadId && w.IsBackground)];
            }
        }
    }

    public IReadOnlyList<Window> NonBackgroundWindows
    {
        get
        {
            lock (_windowsLock)
            {
                return [.. _allWindows.Where(w => w.ManagedThreadId == MainThreadId && !w.IsBackground)];
            }
        }
    }

    public void CheckRunningAsMainThread() { if (!IsRunningAsMainThread) throw new WiceException("0008: This method must be called on the render thread."); }
    public override string ToString() => MainThreadId + " (" + ThreadId + ")";

    protected virtual BOOL GetMessage(out MSG msg, HWND hWnd, uint wMsgFilterMin, uint wMsgFilterMax)
    {
        if (IsDisposed)
        {
            msg = new MSG { message = (int)WM_HOSTQUIT };
            return -1;
        }

        return WiceCommons.GetMessageW(out msg, hWnd, wMsgFilterMin, wMsgFilterMax);
    }

    protected virtual bool HandleMessage(MSG msg)
    {
        if (IsDisposed)
            return false;

        if (_applications?.Count > 1 && msg.hwnd != IntPtr.Zero)
        {
            var window = Windows.FirstOrDefault(w => w.ManagedThreadId == MainThreadId && w.Handle.Equals(msg.hwnd));
            if (window == null)
            {
                // that may be normal if we have multiple applications running since we don't filter messages in GetMessageW
#if DEBUG
                Trace($"Message [{MessageDecoder.Decode(msg)}] was received for unhandled window ({msg.hwnd}).");
#endif
                return true; // not our window, skip it
            }
        }

        if (msg.message == WM_HOSTQUIT)
        {
#if DEBUG
            Trace("WM_HOSTQUIT was received.");
#endif
            return false;
        }

        // call this just at init time
        if (msg.hwnd == IntPtr.Zero)
        {
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                foreach (var window in Windows)
                {
                    window.RegisterForDragDrop();
                }
            }
        }

        WiceCommons.TranslateMessage(msg);
        WiceCommons.DispatchMessageW(msg);
        return true;
    }

    public virtual void Run()
    {
        if (_errors.Count == 0 || !HandleErrors)
        {
            do
            {
                var ret = GetMessage(out var msg, HWND.Null, 0, 0);
                if (ret.Value <= 0) // WM_QUIT or error
                    break;

                if (!HandleMessage(msg))
                    break;
            }
            while (true);
        }

        if (HandleErrors)
        {
            ShowFatalError(HWND.Null);
        }
    }

    public virtual void Exit()
    {
        if (IsDisposed)
            return;

        CheckRunningAsMainThread();
        OnApplicationExit(this, EventArgs.Empty);
        WiceCommons.PostQuitMessage(0);
    }

    protected virtual void OnApplicationExit(object sender, EventArgs e) => ApplicationExit?.Invoke(sender, e);
    protected virtual ResourceManager CreateResourceManager()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(Application));

        CheckRunningAsMainThread();
        return new ResourceManager(this);
    }

    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            // note: we don't check thread id here
            var windows = Windows;
            foreach (var window in windows)
            {
                RemoveWindow(window, true);
            }

#if NETFRAMEWORK
            Marshal.ReleaseComObject(DispatcherQueueController);
#else
            DispatcherQueueController.Dispose();
#endif
            _applications?.TryRemove(Environment.CurrentManagedThreadId, out _);
            OnApplicationExit(this, EventArgs.Empty);
            _disposedValue = true;
        }
    }

#if DEBUG
    private static bool _useDebugLayer = true;
#else
    private static bool _useDebugLayer;
#endif

    public const uint WM_HOSTQUIT = MessageDecoder.WM_APP + 0x3FFF; // last possible app message

    private static readonly Lock _windowsLock = new();
    private static readonly Lock _errorsLock = new();
    private readonly static List<Window> _allWindows = [];
    private readonly static List<Exception> _errors = [];
    private static ConcurrentDictionary<int, Application>? _applications = [];

    public static event EventHandler<ValueEventArgs<Window>>? WindowRemoved;
    public static event EventHandler<ValueEventArgs<Window>>? WindowAdded;
    public static event EventHandler? AllApplicationsExit;

    public static Application? Current
    {
        get
        {
            var apps = _applications;
            if (apps == null)
                return null;

            apps.TryGetValue(Environment.CurrentManagedThreadId, out var app);
            return app;
        }
    }

    public static IReadOnlyList<Window> AllWindows
    {
        get
        {
            lock (_windowsLock)
            {
                return [.. _allWindows];
            }
        }
    }

    public static ResourceManager CurrentResourceManager => Current?.ResourceManager ?? throw new WiceException("0031: Resource Manager is not available at this time.");
    public static Theme CurrentTheme => CurrentResourceManager.Theme ?? throw new WiceException("0035: Theme is not available at this time.");
    public static bool UseDebugLayer { get => _useDebugLayer && DXGIFunctions.IsDebugLayerAvailable; set => _useDebugLayer = true; }
    public static HMODULE ModuleHandle => WiceCommons.GetModuleHandleW(PWSTR.Null);
    public static bool IsFatalErrorShowing { get; private set; }
    public static bool ExitAllOnLastWindowRemoved { get; set; } = true;

    static Application()
    {
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
    }

    public static Application? GetApplication(Window? window) => GetApplication(window?.ManagedThreadId ?? 0);
    public static Application? GetApplication(int threadId)
    {
        if (threadId == 0)
            return null;

        var apps = _applications;
        if (apps == null)
            return null;

        apps.TryGetValue(threadId, out var app);
        return app;
    }

    public static void AllExit()
    {
        var apps = Interlocked.Exchange(ref _applications, null);
        if (apps == null)
            return;

        foreach (var kv in apps)
        {
            // call from one thread
            kv.Value.OnApplicationExit(kv.Value, EventArgs.Empty);
            WiceCommons.PostThreadMessageW(kv.Value.ThreadId, MessageDecoder.WM_QUIT, 0, 0);
        }

        AllApplicationsExit?.Invoke(null, EventArgs.Empty);
    }

    public static bool HasErrors => _errors.Count > 0;
    public static void AddError(Exception error, [CallerMemberName] string? methodName = null)
    {
        if (error == null)
            return;

        lock (_errorsLock)
        {
            var ts = error.ToString();
            if (_errors.Any(e => e.ToString() == ts))
                return;

            Trace("ERROR[" + methodName + "]:" + error);
            _errors.Add(error);
        }

        // we don't use PostQuitMessage because it prevents any other windows from showing (like messagebox, taskdialog, etc.)
        WiceCommons.PostMessageW(HWND.Null, WM_HOSTQUIT, WPARAM.Null, LPARAM.Null);
    }

    public static void ShowFatalError(HWND hwnd)
    {
        if (_errors.Count == 0)
            return;

        if (IsFatalErrorShowing)
            return;

        var errors = _errors.ToArray();
        IsFatalErrorShowing = true;
        try
        {
#if DEBUG
            var i = 0;
            foreach (var error in errors)
            {
                Trace("Error[" + i++ + "]: " + error);
            }
#endif

            var td = new TaskDialog();
            td.Flags |= TASKDIALOG_FLAGS.TDF_SIZE_TO_CONTENT;
            td.MainIcon = new HICON { Value = TaskDialog.TD_ERROR_ICON };
            td.Title = GetTitle(hwnd);
            if (errors.Length == 1)
            {
                td.MainInstruction = "A fatal error has occured. Press OK to quit.";
                td.Content = errors[0].ToString();
            }
            else
            {
                td.MainInstruction = errors.Length + " fatal errors have occured. Press OK to quit.";
                var j = 1;
                var sb = new StringBuilder();
                foreach (var error in errors)
                {
                    if (j > 0)
                    {
                        sb.AppendLine();
                        sb.AppendLine();
                    }

                    sb.AppendLine("Error #" + j);
                    j++;

                    sb.Append(error);
                }
                td.Content = sb.ToString();
            }

            td.Show(hwnd);
        }
        finally
        {
            IsFatalErrorShowing = false;
        }
    }

    public static void Trace(string? message = null, [CallerMemberName] string? methodName = null)
    {
        if (!string.IsNullOrEmpty(methodName))
        {
            methodName += "|";
        }

        var name = Thread.CurrentThread.Name.Nullify() ?? Environment.CurrentManagedThreadId.ToString();
        EventProvider.Default.WriteMessageEvent(name + "|" + methodName + message);
    }

    public static string GetTitle() => GetTitle(HWND.Null);
    public static string GetTitle(HWND hwnd)
    {
        var text = NativeWindow.GetWindowText(hwnd).Nullify();
        if (text != null)
            return text;

        text = Assembly.GetEntryAssembly().GetTitle().Nullify();
        if (text != null)
            return text;

        text = Assembly.GetEntryAssembly().GetProduct().Nullify();
        if (text != null)
            return text;

        return Assembly.GetEntryAssembly()?.GetName().Name ?? "Wice";
    }

    private static void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception error)
        {
            AddError(error);
            var allWindows = AllWindows;
            if (allWindows.Count > 0)
            {
                // show error in the first window
                var win = allWindows[0].NativeIfCreated?.Handle;
                ShowFatalError(win.GetValueOrDefault());
            }
            else
            {
                // no windows, show error in the current thread
                ShowFatalError(HWND.Null);
            }
        }
    }

    internal static void AddWindow(Window window)
    {
        lock (_windowsLock)
        {
            _allWindows.Add(window);

            var apps = _applications;
            if (apps != null)
            {
                apps.TryGetValue(window.ManagedThreadId, out var app);
                if (app != null)
                {
                    app.ResourceManager.AddWindow(window);

                    // default is for new windows to be background (don't prevent to quit)
                    if (app.Windows.Count > 1)
                    {
                        window.IsBackground = true;
                    }

                    WindowAdded?.Invoke(app, new ValueEventArgs<Window>(window));
                }
            }
        }
    }

    internal static void RemoveWindow(Window window, bool appDisposing)
    {
        lock (_windowsLock)
        {
            _allWindows.Remove(window);
            if (appDisposing)
            {
                window.Destroy();
            }

            var apps = _applications;
            if (apps != null)
            {
                apps.TryGetValue(window.ManagedThreadId, out var app);
                if (app != null)
                {
                    WindowRemoved?.Invoke(app, new ValueEventArgs<Window>(window));
                    app.ResourceManager.RemoveWindow(window);
                    if (app.ExitOnLastWindowRemoved && app.NonBackgroundWindows.Count == 0)
                    {
                        foreach (var bw in app.BackgroundWindows)
                        {
                            bw.Destroy();
                        }

                        app.Exit();
                    }
                }
            }

            if (ExitAllOnLastWindowRemoved && !_allWindows.Any(w => !w.IsBackground))
            {
                var backgroundWindows = _allWindows.Where(w => w.IsBackground).ToArray();
                foreach (var bw in backgroundWindows)
                {
                    bw.Destroy();
                }

                if (appDisposing)
                {
                    AllExit();
                }
            }
        }
    }
}
