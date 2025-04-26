using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using DirectN;
using Wice.Utilities;

namespace Wice
{
    public class Application : IDisposable
    {
        private bool _disposedValue;

        public event EventHandler ApplicationExit;

        public Application()
        {
            var apps = _applications;
            if (apps == null)
                throw new WiceException("0030: It's not possible to create applications any more.");

            // no, we can't use Windows.System.DispatcherQueueController.CreateOnDedicatedThread();
            // or compositor will raise an access denied error
            DispatcherQueueController.Create();

            apps.AddOrUpdate(Thread.CurrentThread.ManagedThreadId, this, (k, o) =>
            {
                throw new WiceException("0006: There can be only one Application instance for a given thread.");
            });

            MainThreadId = Thread.CurrentThread.ManagedThreadId;
            ThreadId = WindowsUtilities.GetCurrentThreadId();
            ResourceManager = CreateResourceManager();
        }

        public bool ExitOnLastWindowRemoved { get; set; } = true;
        public bool Exiting { get; private set; }
        public int MainThreadId { get; }
        public int ThreadId { get; }
        public bool IsRunningAsMainThread => MainThreadId == Thread.CurrentThread.ManagedThreadId;
        public bool IsDisposed => _disposedValue;
        public ResourceManager ResourceManager { get; }

        public IReadOnlyList<Window> Windows
        {
            get
            {
                lock (_windowsLock)
                {
                    return _windows.Where(w => w.ManagedThreadId == MainThreadId).ToArray();
                }
            }
        }

        public IReadOnlyList<Window> BackgroundWindows
        {
            get
            {
                lock (_windowsLock)
                {
                    return _windows.Where(w => w.ManagedThreadId == MainThreadId && w.IsBackground).ToArray();
                }
            }
        }

        public IReadOnlyList<Window> NonBackgroundWindows
        {
            get
            {
                lock (_windowsLock)
                {
                    return _windows.Where(w => w.ManagedThreadId == MainThreadId && !w.IsBackground).ToArray();
                }
            }
        }

        public void CheckRunningAsMainThread() { if (!IsRunningAsMainThread) throw new WiceException("0008: This method must be called on the render thread."); }
        public override string ToString() => MainThreadId + " (" + ThreadId + ")";

        public virtual void Run()
        {
            CheckRunningAsMainThread();
            if (_errors.Count == 0)
            {
                var msg = new MSG();
                while (WindowsFunctions.GetMessage(ref msg, IntPtr.Zero, 0, 0))
                {
                    if (msg.message == WM_HOSTQUIT)
                    {
                        Trace("WM_HOSTQUIT was received.");
                        break;
                    }

                    // call this just at init time
                    if (msg.hwnd == IntPtr.Zero)
                    {
                        if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
                        {
                            foreach (var window in _windows)
                            {
                                window.RegisterForDragDrop();
                            }
                        }
                    }

                    if (MainThreadId != 1)
                    {
                        Trace("msg: " + msg.Decode());
                    }
                    WindowsFunctions.TranslateMessage(ref msg);
                    WindowsFunctions.DispatchMessage(ref msg);
                }
#if DEBUG
                if (msg.message == MessageDecoder.WM_QUIT)
                {
                    Trace("WM_QUIT was received.");
                }
#endif
            }

            ShowFatalError(IntPtr.Zero);
        }

        public virtual void Exit()
        {
            CheckRunningAsMainThread();
            OnApplicationExit(this, EventArgs.Empty);
            WindowsFunctions.PostQuitMessage(0);
        }

        ~Application() { Dispose(disposing: false); }
        public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }

        protected virtual void OnApplicationExit(object sender, EventArgs e) => ApplicationExit?.Invoke(sender, e);
        protected virtual ResourceManager CreateResourceManager()
        {
            CheckRunningAsMainThread();
            return new ResourceManager(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                // note: we don't check thread id here
                _applications?.TryRemove(Thread.CurrentThread.ManagedThreadId, out _);
                OnApplicationExit(this, EventArgs.Empty);
                _disposedValue = true;
            }
        }

#if DEBUG
        private static bool _useDebugLayer = true;
#else
        private static bool _useDebugLayer;
#endif

#pragma warning disable IDE1006 // Naming Styles
        internal const int WM_HOSTQUIT = MessageDecoder.WM_APP + 0x3FFF; // last possible app message
#pragma warning restore IDE1006 // Naming Styles

        private static readonly object _windowsLock = new object();
        private static readonly object _errorsLock = new object();
        private readonly static List<Window> _windows = new List<Window>();
        private readonly static List<Exception> _errors = new List<Exception>();
        private static ConcurrentDictionary<int, Application> _applications = new ConcurrentDictionary<int, Application>();

        public static event EventHandler<ValueEventArgs<Window>> WindowRemoved;
        public static event EventHandler<ValueEventArgs<Window>> WindowAdded;
        public static event EventHandler AllApplicationsExit;

        public static Application Current
        {
            get
            {
                var apps = _applications;
                if (apps == null)
                    return null;

                apps.TryGetValue(Thread.CurrentThread.ManagedThreadId, out var app);
                return app;
            }
        }

        public static IReadOnlyList<Window> AllWindows
        {
            get
            {
                lock (_windowsLock)
                {
                    return _windows.ToArray();
                }
            }
        }

        public static ResourceManager CurrentResourceManager => Current?.ResourceManager ?? throw new WiceException("0031: Resource Manager is not available at this time.");
        public static Theme CurrentTheme => CurrentResourceManager.Theme ?? throw new WiceException("0031: Theme is not available at this time.");
        public static bool UseDebugLayer { get => _useDebugLayer && DXGIFunctions.IsDebugLayerAvailable; set => _useDebugLayer = true; }
        public static IntPtr ModuleHandle => WindowsFunctions.GetModuleHandle(null);
        public static bool IsFatalErrorShowing { get; private set; }
        public static ILogger Logger { get; set; }
        public static bool ExitAllOnLastWindowRemoved { get; set; } = true;
        public static bool AllExiting { get; private set; }

        static Application()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnCurrentDomainAssemblyResolve;
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
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
                NativeWindow.PostThreadMessage(kv.Value.ThreadId, MessageDecoder.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
            }

            AllApplicationsExit?.Invoke(null, EventArgs.Empty);
        }

        public static bool HasErrors => _errors.Count > 0;
        public static void AddError(Exception error, [CallerMemberName] string methodName = null)
        {
            if (error == null)
                return;

#if !DEBUG
            Audit.AddRecord(AuditRecordType.Error, error);
            Audit.FlushRecords();
#endif
            lock (_errorsLock)
            {
                var ts = error.ToString();
                if (_errors.Any(e => e.ToString() == ts))
                    return;

                Trace("ERROR[" + methodName + "]:" + error);
                _errors.Add(error);
            }

            // we don't use PostQuitMessage because it prevents any other windows from showing (like messagebox, taskdialog, etc.)
            WindowsFunctions.PostMessage(IntPtr.Zero, WM_HOSTQUIT);
        }

        public static void ShowFatalError(IntPtr hwnd, bool useWinforms = false)
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

                if (useWinforms)
                {
                    try
                    {
                        System.Windows.Forms.Application.EnableVisualStyles();
                        System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
                    }
                    catch
                    {
                        //  do nothing
                    }

                    var dlg = new ErrorForm(errors);
                    dlg.Text = GetTitle(hwnd);
                    dlg.ShowDialog(NativeWindow.FromHandle(hwnd));
                    return;
                }

                var td = new TaskDialog();
                td.Flags |= TASKDIALOG_FLAGS.TDF_SIZE_TO_CONTENT;
                td.MainIcon = TaskDialog.TD_ERROR_ICON;
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

        public static void Trace(string message = null, [CallerMemberName] string methodName = null) => Logger?.Log(TraceLevel.Info, message, methodName);

        public static string GetTitle() => GetTitle(IntPtr.Zero);
        public static string GetTitle(IntPtr hwnd)
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

            return Assembly.GetEntryAssembly().GetName().Name;
        }

        private static Assembly OnCurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            // don't distribute System.Numerics.Vectors
            if (args.Name.StartsWith("System.Numerics.Vectors,"))
            {
                var path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(int).Assembly.Location), "System.Numerics.Vectors.dll");
                Audit.AddRecord(AuditRecordType.Info, "Loading System.Numerics.Vectors from '" + path + "'.");
                var asm = Assembly.LoadFrom(path);
                return asm;
            }

            return null;
        }

        private static void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception error)
            {
                AddError(error);
                var win = _windows.FirstOrDefault()?.Native?.Handle;
                ShowFatalError(win.GetValueOrDefault());
            }
        }

        internal static void AddWindow(Window window)
        {
            lock (_windowsLock)
            {
                _windows.Add(window);

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

        internal static void RemoveWindow(Window window)
        {
            lock (_windowsLock)
            {
                _windows.Remove(window);

                var apps = _applications;
                if (apps != null)
                {
                    apps.TryGetValue(window.ManagedThreadId, out var app);
                    if (app != null)
                    {
                        WindowRemoved?.Invoke(app, new ValueEventArgs<Window>(window));
                        app.ResourceManager.RemoveWindow(window);
                        if (app.ExitOnLastWindowRemoved && !app.Exiting && app.NonBackgroundWindows.Count == 0)
                        {
                            app.Exiting = true;
                            foreach (var bw in app.BackgroundWindows)
                            {
                                bw.Destroy();
                            }
                            app.Exit();
                        }
                    }
                }

                if (ExitAllOnLastWindowRemoved && !AllExiting && !_windows.Any(w => !w.IsBackground))
                {
                    AllExiting = true;
                    var backgroundWindows = _windows.Where(w => w.IsBackground).ToArray();
                    foreach (var bw in backgroundWindows)
                    {
                        bw.Destroy();
                    }
                    AllExit();
                }
            }
        }
    }
}
