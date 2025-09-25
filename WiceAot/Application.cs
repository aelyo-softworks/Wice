namespace Wice;

/// <summary>
/// Represents the Wice application host for a single UI thread.
/// Manages the message loop, window lifetime, resource management, and fatal error handling.
/// </summary>
public partial class Application : IDisposable
{
    private bool _disposedValue;

    /// <summary>
    /// Occurs when the application is exiting.
    /// </summary>
    public event EventHandler? ApplicationExit;

    /// <summary>
    /// Initializes a new instance of the <see cref="Application"/> class on the current thread.
    /// </summary>
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
    /// <summary>
    /// Gets the dispatcher queue controller used to pump and dispatch work on this application's UI thread.
    /// </summary>
    public IDispatcherQueueController DispatcherQueueController { get; }
#else
    /// <summary>
    /// Gets the dispatcher queue controller used to pump and dispatch work on this application's UI thread.
    /// </summary>
    public WindowsDispatcherQueueController DispatcherQueueController { get; }
#endif

    /// <summary>
    /// Gets or sets a value indicating whether the application should exit when all non-background windows are removed.
    /// </summary>
    public bool ExitOnLastWindowRemoved { get; set; } = true;

    /// <summary>
    /// Gets the managed thread ID of the application's main thread (the thread on which it was created).
    /// </summary>
    public int MainThreadId { get; }

    /// <summary>
    /// Gets the native thread ID of the application's main thread.
    /// </summary>
    public uint ThreadId { get; }

    /// <summary>
    /// Gets a value indicating whether the current code is executing on the application's main thread.
    /// </summary>
    public bool IsRunningAsMainThread => MainThreadId == Environment.CurrentManagedThreadId;

    /// <summary>
    /// Gets a value indicating whether the application has been disposed.
    /// </summary>
    public bool IsDisposed => _disposedValue;

    /// <summary>
    /// Gets the resource manager associated with this application.
    /// </summary>
    public ResourceManager ResourceManager { get; }

    /// <summary>
    /// Gets a value indicating whether this application instance handles fatal errors (only on the first UI thread).
    /// </summary>
    public virtual bool HandleErrors => MainThreadId == 1; // we handle errors only on the first UI thread

    /// <summary>
    /// Gets the collection of windows owned by this application on its main thread.
    /// </summary>
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

    /// <summary>
    /// Gets the collection of background windows for this application on its main thread.
    /// </summary>
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

    /// <summary>
    /// Gets the collection of non-background windows for this application on its main thread.
    /// </summary>
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

    /// <summary>
    /// Throws if the current call is not executing on the application's main thread.
    /// </summary>
    public void CheckRunningAsMainThread() { if (!IsRunningAsMainThread) throw new WiceException("0008: This method must be called on the render thread."); }

    /// <inheritdoc/>
    public override string ToString() => MainThreadId + " (" + ThreadId + ")";

    /// <summary>
    /// Retrieves a message from the calling thread's message queue.
    /// </summary>
    /// <param name="msg">When this method returns, contains the message information.</param>
    /// <param name="hWnd">A handle to the window whose messages are to be retrieved, or <see cref="HWND.Null"/> for any window.</param>
    /// <param name="wMsgFilterMin">The integer value of the lowest message value to be retrieved.</param>
    /// <param name="wMsgFilterMax">The integer value of the highest message value to be retrieved.</param>
    /// <returns>
    /// A <see cref="BOOL"/> where a value greater than 0 indicates success, 0 indicates WM_QUIT, and -1 indicates error.
    /// Returns -1 and sets a synthetic <see cref="WM_HOSTQUIT"/> when <see cref="IsDisposed"/> is <see langword="true"/>.
    /// </returns>
    protected virtual BOOL GetMessage(out MSG msg, HWND hWnd, uint wMsgFilterMin, uint wMsgFilterMax)
    {
        if (IsDisposed)
        {
            msg = new MSG { message = (int)WM_HOSTQUIT };
            return -1;
        }

        return WiceCommons.GetMessageW(out msg, hWnd, wMsgFilterMin, wMsgFilterMax);
    }

    /// <summary>
    /// Handles a single message by translating and dispatching it.
    /// </summary>
    /// <param name="msg">The message to handle.</param>
    /// <returns>
    /// <see langword="true"/> to continue processing the message loop; <see langword="false"/> to break the loop.
    /// Returns <see langword="false"/> for <see cref="WM_HOSTQUIT"/> or when disposed.
    /// </returns>
    protected virtual bool HandleMessage(in MSG msg)
    {
        if (IsDisposed)
            return false;

        var hwnd = msg.hwnd;
        if (_applications?.Count > 1 && hwnd != IntPtr.Zero)
        {
            var window = Windows.FirstOrDefault(w => w.Handle.Equals(hwnd));
            if (window != null && window.ManagedThreadId != MainThreadId)
            {
                // that may be normal if we have multiple applications running since we don't filter messages in GetMessageW
#if DEBUG
                Trace($"Message [{MessageDecoder.Decode(msg)}] was received for unhandled window '{window}' ({hwnd}).");
#endif
                return true; // not our window, skip it
            }
        }

        if (msg.message == WM_HOSTQUIT)
        {
#if DEBUG
            //Trace("WM_HOSTQUIT was received.");
#endif
            return false;
        }

        // call this just at init time
        if (hwnd == IntPtr.Zero)
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

    /// <summary>
    /// Runs the standard message loop until a quit or host-quit message is received or an error occurs.
    /// </summary>
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

    /// <summary>
    /// Runs a custom message loop using PeekMessage until the provided predicate requests exit.
    /// </summary>
    /// <param name="exitLoopFunc">A predicate invoked for each message; return <see langword="true"/> to exit the loop.</param>
    /// <returns>
    /// An <see cref="ExitLoopReason"/> indicating why the loop exited.
    /// </returns>
    public virtual ExitLoopReason RunMessageLoop(Func<MSG, bool> exitLoopFunc)
    {
        ExceptionExtensions.ThrowIfNull(exitLoopFunc, nameof(exitLoopFunc));
        CheckRunningAsMainThread();
        do
        {
            if (WiceCommons.PeekMessageW(out var msg, HWND.Null, 0, 0, PEEK_MESSAGE_REMOVE_TYPE.PM_REMOVE))
            {
                if (exitLoopFunc(msg))
                    return ExitLoopReason.Func;

                if (IsDisposed)
                {
                    // repost
                    WiceCommons.PostMessageW(HWND.Null, WM_HOSTQUIT, WPARAM.Null, LPARAM.Null);
                    return ExitLoopReason.Disposed;
                }

                if (msg.message == MessageDecoder.WM_QUIT)
                {
                    // repost
                    WiceCommons.PostQuitMessage(0);
                    return ExitLoopReason.Quit;
                }

                if (msg.message == WM_HOSTQUIT)
                {
                    // repost
                    WiceCommons.PostMessageW(HWND.Null, WM_HOSTQUIT, WPARAM.Null, LPARAM.Null);
                    return ExitLoopReason.AppQuit;
                }

                if (!HandleMessage(msg))
                    return ExitLoopReason.UnhandledMessage;
            }
        } while (true);
    }

    /// <summary>
    /// Initiates application exit by raising <see cref="ApplicationExit"/>.
    /// </summary>
    public virtual void Exit()
    {
        if (IsDisposed)
            return;

        CheckRunningAsMainThread();
        OnApplicationExit(this, EventArgs.Empty);

        // we don't use PostQuitMessage because it prevents any other windows from showing (like messagebox, taskdialog, etc.)
        WiceCommons.PostMessageW(HWND.Null, WM_HOSTQUIT, WPARAM.Null, LPARAM.Null);
    }

    /// <summary>
    /// Raises the <see cref="ApplicationExit"/> event.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event data.</param>
    protected virtual void OnApplicationExit(object sender, EventArgs e) => ApplicationExit?.Invoke(sender, e);

    /// <summary>
    /// Creates the <see cref="ResourceManager"/> for this application.
    /// </summary>
    /// <returns>A new <see cref="ResourceManager"/> instance.</returns>
    protected virtual ResourceManager CreateResourceManager()
    {
#if NETFRAMEWORK
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(Application));
#else
        ObjectDisposedException.ThrowIf(IsDisposed, nameof(Application));
#endif

        CheckRunningAsMainThread();
        return new ResourceManager(this);
    }

    /// <summary>
    /// Releases resources used by the application.
    /// </summary>
    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }

    /// <summary>
    /// Releases unmanaged and optionally managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to dispose managed resources; otherwise, <see langword="false"/>.
    /// </param>
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

    /// <summary>
    /// The last possible application-defined message used to signal a host quit.
    /// </summary>
    public const uint WM_HOSTQUIT = MessageDecoder.WM_APP + 0x3FFF; // last possible app message

    private static readonly Lock _windowsLock = new();
    private static readonly Lock _errorsLock = new();
    private readonly static List<Window> _allWindows = [];
    private readonly static List<Exception> _errors = [];
    private static ConcurrentDictionary<int, Application>? _applications = [];

    /// <summary>
    /// Occurs when a window is removed from any application.
    /// </summary>
    public static event EventHandler<ValueEventArgs<Window>>? WindowRemoved;

    /// <summary>
    /// Occurs when a window is added to any application.
    /// </summary>
    public static event EventHandler<ValueEventArgs<Window>>? WindowAdded;

    /// <summary>
    /// Occurs when all applications have posted quit and are exiting.
    /// </summary>
    public static event EventHandler? AllApplicationsExit;

    /// <summary>
    /// Gets the <see cref="Application"/> for the current managed thread, if any.
    /// </summary>
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

    /// <summary>
    /// Gets all windows across all applications.
    /// </summary>
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

    /// <summary>
    /// Gets the current thread's <see cref="ResourceManager"/>.
    /// </summary>
    public static ResourceManager CurrentResourceManager => Current?.ResourceManager ?? throw new WiceException("0031: Resource Manager is not available at this time.");

    /// <summary>
    /// Gets or sets a value indicating whether to enable the DXGI debug layer when available.
    /// </summary>
    public static bool UseDebugLayer { get => _useDebugLayer && DXGIFunctions.IsDebugLayerAvailable; set => _useDebugLayer = true; }

    /// <summary>
    /// Gets the HMODULE of the current process.
    /// </summary>
    public static HMODULE ModuleHandle => WiceCommons.GetModuleHandleW(PWSTR.Null);

    /// <summary>
    /// Gets a value indicating whether a fatal error dialog is currently being shown.
    /// </summary>
    public static bool IsFatalErrorShowing { get; protected set; }

    /// <summary>
    /// Gets or sets a value indicating whether to exit all applications when the last non-background window is removed.
    /// </summary>
    public static bool ExitAllOnLastWindowRemoved { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether fatal error messages should be displayed when an unhandled exception
    /// occurs.
    /// </summary>
    public static bool ShowFatalErrorsOnUnhandledException { get; set; } = !Debugger.IsAttached;

    /// <summary>
    /// Gets or sets a value indicating whether a debugger is currently attached to the process.
    /// Wice will use this value instead of using Debugger.IsAttached.
    /// </summary>
    public static bool IsDebuggerAttached { get; set; } = Debugger.IsAttached;

    /// <summary>
    /// Gets or sets a delegate that displays a fatal error message for a specified window handle.
    /// </summary>
    public static Func<HWND, bool>? ShowFatalErrorFunc { get; set; }

    static Application()
    {
        // Subscribes to the AppDomain unhandled exception to capture and display fatal errors.
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
    }

    /// <summary>
    /// Gets the <see cref="Application"/> that owns the specified <paramref name="window"/>, if any.
    /// </summary>
    /// <param name="window">The window whose owning application to resolve.</param>
    public static Application? GetApplication(Window? window) => GetApplication(window?.ManagedThreadId ?? 0);

    /// <summary>
    /// Gets the <see cref="Application"/> associated with a specific managed <paramref name="threadId"/>, if any.
    /// </summary>
    /// <param name="threadId">The managed thread ID.</param>
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

    /// <summary>
    /// Requests all applications to exit raises <see cref="AllApplicationsExit"/>.
    /// </summary>
    public static void AllExit()
    {
        var apps = Interlocked.Exchange(ref _applications, null);
        if (apps == null)
            return;

        foreach (var kv in apps)
        {
            // call from one thread
            kv.Value.OnApplicationExit(kv.Value, EventArgs.Empty);

            WiceCommons.PostThreadMessageW(kv.Value.ThreadId, WM_HOSTQUIT, 0, 0);
        }

        AllApplicationsExit?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>
    /// Gets a value indicating whether any fatal errors have been recorded but not shown.
    /// </summary>
    public static bool HasErrors => _errors.Count > 0;

    /// <summary>
    /// Retrieves the list of errors that have been recorded.
    /// </summary>
    /// <param name="clear">A value indicating whether to clear the error list after retrieval. <see langword="true"/> to clear the list;
    /// otherwise, <see langword="false"/>.</param>
    /// <returns>A read-only list of <see cref="Exception"/> objects representing the recorded errors. The list will be empty if
    /// no errors have been recorded.</returns>
    public static IReadOnlyList<Exception> GetErrors(bool clear = false)
    {
        lock (_errorsLock)
        {
            var errors = _errors.ToArray();
            if (clear)
            {
                _errors.Clear();
            }
            return errors;
        }
    }

    /// <summary>
    /// Clears all errors from the internal error collection.
    /// </summary>
    /// <returns>The number of errors that were cleared from the collection.</returns>
    public static int ClearErrors()
    {
        lock (_errorsLock)
        {
            var count = _errors.Count;
            _errors.Clear();
            return count;
        }
    }

    /// <summary>
    /// Adds an error to the internal error collection and optionally initiates a quit operation.
    /// </summary>
    /// <param name="error">The exception to add. Must not be <see langword="null"/>.</param>
    /// <param name="quit">A value indicating whether to initiate a quit operation after adding the error. If <see langword="true"/>, a
    /// quit operation is triggered; otherwise, it is not.</param>
    /// <param name="methodName">The name of the calling method. This parameter is automatically populated by the compiler and is optional.</param>
    public static void AddError(Exception error, bool quit = true, [CallerMemberName] string? methodName = null)
    {
        if (error == null)
            return;

        lock (_errorsLock)
        {
            var ts = error.ToString();
            if (_errors.Any(e => e.ToString() == ts))
            {
                handleQuit();
                return;
            }

            Trace("ERROR[" + methodName + "]:" + error);
            _errors.Add(error);
        }
        handleQuit();

        void handleQuit()
        {
            if (!quit)
                return;

            // we don't use PostQuitMessage because it prevents any other windows from showing (like messagebox, taskdialog, etc.)
            WiceCommons.PostMessageW(HWND.Null, WM_HOSTQUIT, WPARAM.Null, LPARAM.Null);
        }
    }

    /// <summary>
    /// Displays a fatal error dialog to the user, allowing customization of the dialog's appearance and behavior.
    /// </summary>
    /// <param name="hwnd">The handle to the parent window for the dialog. If the handle is invalid, it will be reset to 0.</param>
    /// <param name="configureOptions">An optional delegate to configure the <see cref="ShowFatalErrorOptions"/> used to customize the dialog. If <see
    /// langword="null"/>, default options are used.</param>
    /// <param name="callCustomFunc">A value indicating whether to invoke a custom error handling function, if one is defined. If <see
    /// langword="true"/>, the custom function is called before displaying the dialog.</param>
    /// <returns><see langword="true"/> if the dialog was successfully shown and the user interacted with it; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool ShowFatalError(HWND hwnd, Action<ShowFatalErrorOptions>? configureOptions = null, bool callCustomFunc = true)
    {
        if (hwnd != 0 && !WiceCommons.IsWindow(hwnd))
        {
            hwnd.Value = 0;
        }

        if (callCustomFunc)
        {
            var func = ShowFatalErrorFunc;
            if (func != null)
                return func(hwnd);
        }

        if (_errors.Count == 0)
            return false;

        if (IsFatalErrorShowing)
            return false;

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

            var options = new ShowFatalErrorOptions(errors, td, hwnd);
            if (configureOptions != null)
            {
                configureOptions(options);
                if (!options.ShowDialog)
                    return false;
            }

            // note if something goes wrong (dialog wasn't show), Show will possibly return cancel (TaskDialogIndirect won't fail).
            var result = td.Show(hwnd);

            options.ShownFunc?.Invoke(options, result);

#if NETFRAMEWORK
            var shown = result != DialogResult.Cancel;
#else
            var shown = result != MESSAGEBOX_RESULT.IDCANCEL;
#endif
            if (shown && options.ClearErrorsOnShown)
            {
                _errors.Clear(); // clear errors to prevent showing them again
            }
            return shown;
        }
        finally
        {
            IsFatalErrorShowing = false;
        }
    }

    /// <summary>
    /// Represents the options for displaying a fatal error dialog, including the errors to display, the dialog
    /// configuration, and the parent window handle.
    /// </summary>
    /// <param name="dialog">The task dialog instance that will be used to display the fatal error message.</param>
    /// <param name="errors">The list of errors to be displayed in the fatal error dialog.</param>
    /// <param name="hwnd">The handle to the parent window for the dialog.</param>
    public class ShowFatalErrorOptions(IReadOnlyList<Exception> errors, TaskDialog dialog, HWND hwnd)
    {
        /// <summary>
        /// Gets the list of errors to be displayed in the fatal error dialog.
        /// </summary>
        public IReadOnlyList<Exception> Errors { get; } = errors;

        /// <summary>
        /// Gets the task dialog instance that will be used to display the fatal error message.
        /// </summary>
        public TaskDialog Dialog { get; } = dialog;

        /// <summary>
        /// Gets the handle to the parent window for the dialog.
        /// </summary>
        public HWND Hwnd { get; } = hwnd;

        /// <summary>
        /// Gets or sets a value indicating whether the dialog should be displayed.
        /// </summary>
        public bool ShowDialog { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether errors should be cleared after the dialog has been shown.
        /// </summary>
        public virtual bool ClearErrorsOnShown { get; set; } = true;

#if NETFRAMEWORK
        /// <summary>
        /// Gets or sets the action to be invoked when a dialog is shown.
        /// </summary>
        public virtual Action<ShowFatalErrorOptions, DialogResult>? ShownFunc { get; set; }
#else
        /// <summary>
        /// Gets or sets the action to be invoked when a dialog is shown.
        /// </summary>
        public virtual Action<ShowFatalErrorOptions, MESSAGEBOX_RESULT>? ShownFunc { get; set; }
#endif
    }

    /// <summary>
    /// Writes a trace message using the default event provider, tagged with the current thread name or ID.
    /// </summary>
    /// <param name="message">The message to write.</param>
    /// <param name="methodName">The calling method name (automatically supplied by the compiler).</param>
    public static void Trace(string? message = null, [CallerMemberName] string? methodName = null)
    {
        if (!string.IsNullOrEmpty(methodName))
        {
            methodName += "|";
        }

        var name = Thread.CurrentThread.Name.Nullify() ?? Environment.CurrentManagedThreadId.ToString();
        EventProvider.Default.WriteMessageEvent(name + "|" + methodName + message);
    }

    /// <summary>
    /// Gets a suitable title for the current application.
    /// </summary>
    /// <returns>
    /// The window text if available; otherwise the entry assembly title, product, or assembly name; defaults to "Wice".
    /// </returns>
    public static string GetTitle() => GetTitle(HWND.Null);

    /// <summary>
    /// Gets a suitable title for a given window.
    /// </summary>
    /// <param name="hwnd">The target window handle.</param>
    /// <returns>
    /// The window text if available; otherwise the entry assembly title, product, or assembly name; defaults to "Wice".
    /// </returns>
    public static string GetTitle(HWND hwnd)
    {
        string? text;
        if (hwnd != 0)
        {
            text = NativeWindow.GetWindowText(hwnd).Nullify();
            if (text != null)
                return text;
        }

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
            if (ShowFatalErrorsOnUnhandledException)
            {
                AddError(error, false);
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
            else
            {
                AddError(error);
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
