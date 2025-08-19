namespace Wice;

/// <summary>
/// Represents the Wice application host for a single UI thread.
/// Manages the message loop, window lifetime, resource management, and fatal error handling.
/// </summary>
/// <remarks>
/// - Exactly one <see cref="Application"/> can exist per managed thread; attempting to create a second instance on the same thread throws.
/// - The instance associated with a thread can be retrieved via <see cref="Current"/> or <see cref="GetApplication(int)"/>.
/// - Use <see cref="Run"/> to process messages until the application exits, or <see cref="RunMessageLoop(System.Func{MSG, bool})"/> for a custom loop.
/// - Fatal errors are collected with <see cref="AddError(Exception, string?)"/> and can be shown via <see cref="ShowFatalError(HWND)"/>.
/// </remarks>
public partial class Application : IDisposable
{
    private bool _disposedValue;

    /// <summary>
    /// Occurs when the application is exiting.
    /// </summary>
    /// <remarks>
    /// Raised by <see cref="Exit"/> (before posting WM_QUIT) and during <see cref="Dispose()"/>.
    /// Handlers execute on the application's main thread.
    /// </remarks>
    public event EventHandler? ApplicationExit;

    /// <summary>
    /// Initializes a new instance of the <see cref="Application"/> class on the current thread.
    /// </summary>
    /// <exception cref="WiceException">
    /// Thrown if applications can no longer be created ("0030") or if an application already exists on the current thread ("0006").
    /// </exception>
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
    /// <remarks>Default is <see langword="true"/>.</remarks>
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
    /// <remarks>
    /// When <see langword="true"/>, <see cref="Run"/> will display a dialog for accumulated fatal errors.
    /// </remarks>
    public virtual bool HandleErrors => MainThreadId == 1; // we handle errors only on the first UI thread

    /// <summary>
    /// Gets the collection of windows owned by this application on its main thread.
    /// </summary>
    /// <remarks>
    /// Includes both background and non-background windows created on <see cref="MainThreadId"/>.
    /// </remarks>
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
    /// <exception cref="WiceException">Thrown when invoked from a non-main thread ("0008").</exception>
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
    /// <remarks>
    /// On first initialization (MSG.hwnd == 0) in STA, registers all windows for drag and drop.
    /// When multiple applications exist, messages destined for other windows are skipped.
    /// </remarks>
    protected virtual bool HandleMessage(MSG msg)
    {
        if (IsDisposed)
            return false;

        if (_applications?.Count > 1 && msg.hwnd != IntPtr.Zero)
        {
            var window = Windows.FirstOrDefault(w => w.Handle.Equals(msg.hwnd));
            if (window != null && window.ManagedThreadId != MainThreadId)
            {
                // that may be normal if we have multiple applications running since we don't filter messages in GetMessageW
#if DEBUG
                Trace($"Message [{MessageDecoder.Decode(msg)}] was received for unhandled window '{window}' ({msg.hwnd}).");
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

    /// <summary>
    /// Runs the standard message loop until a quit or host-quit message is received or an error occurs.
    /// </summary>
    /// <remarks>
    /// If <see cref="HandleErrors"/> is <see langword="true"/>, a fatal error dialog is shown when the loop exits and errors were collected.
    /// </remarks>
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
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exitLoopFunc"/> is <see langword="null"/>.</exception>
    /// <exception cref="WiceException">Thrown if called from a non-main thread.</exception>
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
    /// <remarks>Must be called from the application's main thread.</remarks>
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
    /// <exception cref="ObjectDisposedException">Thrown if the application is disposed.</exception>
    /// <exception cref="WiceException">Thrown if called from a non-main thread.</exception>
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
    /// <remarks>
    /// Disposes the dispatcher queue controller, removes and destroys owned windows, and raises <see cref="ApplicationExit"/>.
    /// </remarks>
    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }

    /// <summary>
    /// Releases unmanaged and optionally managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to dispose managed resources; otherwise, <see langword="false"/>.
    /// </param>
    /// <remarks>
    /// Called once. Destroys windows, disposes the dispatcher controller, detaches from the application map, and raises <see cref="ApplicationExit"/>.
    /// </remarks>
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
    /// <exception cref="WiceException">Thrown when no current application exists ("0031").</exception>
    public static ResourceManager CurrentResourceManager => Current?.ResourceManager ?? throw new WiceException("0031: Resource Manager is not available at this time.");

    /// <summary>
    /// Gets or sets a value indicating whether to enable the DXGI debug layer when available.
    /// </summary>
    /// <remarks>
    /// The getter also checks <see cref="DXGIFunctions.IsDebugLayerAvailable"/>. The setter forces enabling.
    /// </remarks>
    public static bool UseDebugLayer { get => _useDebugLayer && DXGIFunctions.IsDebugLayerAvailable; set => _useDebugLayer = true; }

    /// <summary>
    /// Gets the HMODULE of the current process.
    /// </summary>
    public static HMODULE ModuleHandle => WiceCommons.GetModuleHandleW(PWSTR.Null);

    /// <summary>
    /// Gets a value indicating whether a fatal error dialog is currently being shown.
    /// </summary>
    public static bool IsFatalErrorShowing { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether to exit all applications when the last non-background window is removed.
    /// </summary>
    /// <remarks>Default is <see langword="true"/>.</remarks>
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
    /// <remarks>This property reflects the state of the debugger at initialization time but can be changed.</remarks>
    public static bool IsDebuggerAttached { get; set; } = Debugger.IsAttached;

    /// <summary>
    /// Gets or sets a delegate that displays a fatal error message for a specified window handle.
    /// </summary>
    /// <remarks>This property allows customization of how fatal errors are displayed in the application.</remarks>
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
    /// <remarks>This method is thread-safe. If <paramref name="clear"/> is set to <see langword="true"/>, 
    /// the internal error list will be cleared after the errors are retrieved.</remarks>
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
    /// Adds an error to the internal error collection and optionally initiates a quit operation.
    /// </summary>
    /// <remarks>If the specified error already exists (by same exact text) in the internal error collection, it will not be added
    /// again.  When <paramref name="quit"/> is <see langword="true"/>, a quit operation is initiated by posting  a
    /// message to the main thread.</remarks>
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
    /// Displays a fatal error dialog to the user if there are any recorded fatal errors.
    /// </summary>
    /// <remarks>This method checks for any recorded fatal errors and displays them in a dialog box.  If
    /// multiple errors are present, they are aggregated and displayed together. Once the dialog is acknowledged, the
    /// recorded errors are cleared to prevent them from being shown again. If such a dialog is already being shown, this method will return <see
    /// langword="false"/> without displaying another dialog.</remarks>
    /// <param name="hwnd">The handle to the parent window for the dialog.</param>
    /// <returns><see langword="true"/> if the dialog was successfully shown and acknowledged by the user;  otherwise, <see
    /// langword="false"/> if no errors were present or the dialog could not be or was not displayed.</returns>
    public static bool ShowFatalError(HWND hwnd)
    {
        if (hwnd != 0 && !WiceCommons.IsWindow(hwnd))
        {
            hwnd.Value = 0;
        }

        var func = ShowFatalErrorFunc;
        if (func != null)
            return func(hwnd);

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

            // note if something goes wrong (dialog wasn't show), Show will possibly return cancel (TaskDialogIndirect won't fail).
            var result = td.Show(hwnd);
#if NETFRAMEWORK
            var shown = result != DialogResult.Cancel;
#else
            var shown = result != MESSAGEBOX_RESULT.IDCANCEL;
#endif
            if (shown)
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
