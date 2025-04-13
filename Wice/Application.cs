using System;
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
        private static Application _current;

        public static event EventHandler<ValueEventArgs<Window>> WindowRemoved;
        public static event EventHandler<ValueEventArgs<Window>> WindowAdded;
        public static event EventHandler ApplicationExit;

        static Application()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnCurrentDomainAssemblyResolve;
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
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

        public Application()
        {
            if (Interlocked.CompareExchange(ref _current, this, null) != null)
                throw new WiceException("0006: There can be only one Application instance.");

            MainThreadId = Thread.CurrentThread.ManagedThreadId;
            // no, we can't use Windows.System.DispatcherQueueController.CreateOnDedicatedThread();
            // or compositor will raise an access denied error
            DispatcherQueueController.Create();
            ResourceManager = CreateResourceManager;
        }

        public ResourceManager ResourceManager { get; }

        public virtual void Run()
        {
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

                    // Trace("msg: " + msg.Decode());
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

        protected virtual ResourceManager CreateResourceManager => new ResourceManager(this);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                _disposedValue = true;
            }
        }

        ~Application() { Dispose(disposing: false); }
        public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }

        private static readonly object _lock = new object();
        private static bool _exiting;
        private readonly static List<Window> _windows = new List<Window>();
        private readonly static List<Exception> _errors = new List<Exception>();

        internal static void AddWindow(Window window)
        {
            lock (_lock)
            {
                _windows.Add(window);
                Current.ResourceManager.AddWindow(window);

                // default is for new windows to be background (don't prevent to quit)
                if (_windows.Count > 1)
                {
                    window.IsBackground = true;
                }

                WindowAdded?.Invoke(Current, new ValueEventArgs<Window>(window));
            }
        }

        internal static void RemoveWindow(Window window)
        {
            lock (_lock)
            {
                _windows.Remove(window);
                Current.ResourceManager.RemoveWindow(window);
                WindowRemoved?.Invoke(Current, new ValueEventArgs<Window>(window));

                if (QuitOnLastWindowRemoved && !_exiting && _windows.Count(w => !w.IsBackground) == 0)
                {
                    _exiting = true;
                    var backgroundWindows = _windows.Where(w => w.IsBackground).ToArray();
                    foreach (var bw in backgroundWindows)
                    {
                        bw.Destroy();
                    }
                    Exit();
                }
            }
        }

#if DEBUG
        private static bool _useDebugLayer = true;
#else
        private static bool _useDebugLayer;
#endif

        public static Application Current
        {
            get
            {
                if (_current == null)
                {
                    _ = new Application();
                }
                return _current;
            }
        }
        public static Theme CurrentTheme => Current.ResourceManager.Theme;
        public static bool UseDebugLayer { get => _useDebugLayer && DXGIFunctions.IsDebugLayerAvailable; set => _useDebugLayer = true; }
        public static IEnumerable<Window> Windows => _windows;
        public static IntPtr ModuleHandle => WindowsFunctions.GetModuleHandle(null);
        public static bool IsFatalErrorShowing { get; private set; }
        public static bool QuitOnLastWindowRemoved { get; set; } = true;
        public static ILogger Logger { get; set; }

        public static int MainThreadId { get; private set; }
        public static bool IsRunningAsMainThread => MainThreadId == Thread.CurrentThread.ManagedThreadId;
        public static void CheckRunningAsMainThread() { if (!IsRunningAsMainThread) throw new WiceException("0008: This method must be called on the render thread."); }

        public static void Exit()
        {
            ApplicationExit?.Invoke(Current, EventArgs.Empty);
            WindowsFunctions.PostQuitMessage(0);
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
            lock (_lock)
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

#pragma warning disable IDE1006 // Naming Styles
        internal const int WM_HOSTQUIT = MessageDecoder.WM_APP + 0x3FFF; // last possible app message
#pragma warning restore IDE1006 // Naming Styles
    }
}
