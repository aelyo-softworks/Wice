using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using DirectN;
using Microsoft.Win32;
using Wice.Interop;
using Wice.Utilities;

namespace Wice
{
    public sealed class NativeWindow : IWin32Window, IEquatable<NativeWindow>, NativeWindow.IDropTarget, NativeWindow.IDropSource, NativeWindow.IDropSourceNotify
    {
        private static readonly ConcurrentHashSet<string> _classesNames = new ConcurrentHashSet<string>();

        public static IEnumerable<NativeWindow> TopLevelWindows => WindowsFunctions.EnumerateTopLevelWindows().Select(h => FromHandle(h)).Where(w => w != null);

        public event EventHandler<DragDropEventArgs> DragDrop;
        public event EventHandler<DragDropQueryContinueEventArgs> DragDropQueryContinue;
        public event EventHandler<DragDropGiveFeedback> DragDropGiveFeedback;
        public event EventHandler<DragDropTargetEventArgs> DragDropTarget;

        private IntPtr _dragDropTarget;
        private bool _isDropTarget;
        private System.Runtime.InteropServices.ComTypes.IDataObject _currentDataObject;

        private NativeWindow(IntPtr handle)
        {
            Handle = handle;
        }

        public IntPtr Handle { get; }
        public string ClassName => GetClassName(Handle);
        public IntPtr ParentHandle { get => WindowsFunctions.GetParent(Handle); set => WindowsFunctions.SetParent(Handle, value); }
        public IntPtr OwnerHandle => WindowsFunctions.GetWindow(Handle, WindowsConstants.GW_OWNER);
        public NativeWindow Parent => FromHandle(ParentHandle);
        public NativeWindow Owner => FromHandle(OwnerHandle);
        public tagRECT WindowRect { get { WindowsFunctions.GetWindowRect(Handle, out var rc); return rc; } set => WindowsFunctions.SetWindowPos(Handle, IntPtr.Zero, value.left, value.top, value.Width, value.Height, SWP.SWP_NOACTIVATE | SWP.SWP_NOREDRAW | SWP.SWP_NOZORDER); }
        public tagRECT ClientRect { get { WindowsFunctions.GetClientRect(Handle, out var rc); return rc; } }
        public IntPtr IconHandle { get => WindowsFunctions.SendMessage(Handle, MessageDecoder.WM_GETICON, new IntPtr(WindowsConstants.ICON_BIG)); set { var ptr = WindowsFunctions.SendMessage(Handle, MessageDecoder.WM_SETICON, new IntPtr(WindowsConstants.ICON_BIG), value); if (ptr != IntPtr.Zero) { WindowsFunctions.DestroyIcon(ptr); } } }
        public int Dpi { get { var dpi = WindowsFunctions.GetDpiForWindow(Handle); if (dpi <= 0) return 96; return dpi; } }
        public IntPtr DpiAwareness => DpiUtilities.GetWindowDpiAwarenessContext(Handle);
        public string DpiAwarenessDescription => GetDpiAwarenessDescription(DpiAwareness);
        public int DpiFromDpiAwareness => GetDpiFromDpiAwarenessContext(DpiAwareness);
        public string Text { get => GetWindowText(Handle); set => WindowsFunctions.SetWindowText(Handle, value ?? string.Empty); }
        public WS Style { get => (WS)GetWindowLong(WindowsConstants.GWL_STYLE).ToInt64(); set => SetWindowLong(WindowsConstants.GWL_STYLE, new IntPtr((int)value)); }
        public WS_EX ExtendedStyle { get => (WS_EX)GetWindowLong(WindowsConstants.GWL_EXSTYLE).ToInt64(); set => SetWindowLong(WindowsConstants.GWL_EXSTYLE, new IntPtr((int)value)); }
        public bool IsEnabled { get => WindowsFunctions.IsWindowEnabled(Handle); set => WindowsFunctions.EnableWindow(Handle, value); }
        public int ThreadId => WindowsFunctions.GetWindowThreadId(Handle);
        public int ProcessId => WindowsFunctions.GetWindowProcessId(Handle);
        public string ModuleFileName => WindowsFunctions.GetWindowModuleFileName(Handle);

        public Process Process
        {
            get
            {
                try
                {
                    return Process.GetProcessById(ProcessId);
                }
                catch
                {
                    return null;
                }
            }
        }

        public IEnumerable<NativeWindow> Parents
        {
            get
            {
                var current = Parent;
                do
                {
                    var parent = current;
                    if (parent == null)
                        yield break;

                    yield return parent;
                    current = parent;
                }
                while (true);
            }
        }

        public DWM_CLOAKED CloakedState
        {
            get
            {
                var state = 0;
                WindowsFunctions.DwmGetWindowAttribute(Handle, DWMWINDOWATTRIBUTE.DWMWA_CLOAKED, ref state, Marshal.SizeOf<int>());
                return (DWM_CLOAKED)state;
            }
        }

        public tagRECT ExtendedFrameBounds
        {
            get
            {
                var bounds = new tagRECT();
                WindowsFunctions.DwmGetWindowAttribute(Handle, DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, ref bounds, Marshal.SizeOf<tagRECT>());
                return bounds;
            }
        }

        public IEnumerable<NativeWindow> ChildWindows => WindowsFunctions.EnumerateChildWindows(Handle).Select(h => FromHandle(h)).Where(w => w != null);
        public IEnumerable<NativeWindow> AllChildWindows
        {
            get
            {
                foreach (var child in ChildWindows)
                {
                    yield return child;
                    foreach (var gchild in child.AllChildWindows)
                    {
                        yield return gchild;
                    }
                }
            }
        }

        public IReadOnlyDictionary<PROPERTYKEY, object> Properties
        {
            get
            {
                var dic = new Dictionary<PROPERTYKEY, object>();
                SHGetPropertyStoreForWindow(Handle, typeof(IPropertyStore).GUID, out var obj);
                if (obj is IPropertyStore ps)
                {
                    ps.GetCount(out var count);
                    for (var i = 0; i < count; i++)
                    {
                        var hr = ps.GetAt(i, out var pk);
                        if (hr < 0)
                            continue;

                        using (var pv = new PropVariant())
                        {
                            hr = ps.GetValue(ref pk, pv);
                            if (hr == 0)
                            {
                                dic[pk] = pv.Value;
                            }
                        }
                    }
                }
                return dic;
            }
        }

        public bool IsDropTarget
        {
            get => _isDropTarget;
            set
            {
                if (value == _isDropTarget)
                    return;

                if (value)
                {
                    // we need to ensure this as STAThread doesn't always call it for some reason
                    OleInitialize(IntPtr.Zero); // don't check error
                    var hr = RegisterDragDrop(Handle, this);
                    if (hr.IsError && hr != DRAGDROP_E_ALREADYREGISTERED)
                        throw new WiceException("0027: Cannot enable drag & drop operations. Make sure the thread is initialized as an STA thread.", Marshal.GetExceptionForHR(hr.Value));

                    _isDropTarget = true;
                }
                else
                {
                    var hr = RevokeDragDrop(Handle);
                    hr.ThrowOnErrorExcept(DRAGDROP_E_NOTREGISTERED);
                    _isDropTarget = false;
                }
            }
        }

        public override int GetHashCode() => Handle.GetHashCode();
        public override bool Equals(object obj) => Equals(obj as NativeWindow);
        public bool Equals(NativeWindow other) => other != null && Handle == other.Handle;
        public bool IsZoomed() => WindowsFunctions.IsZoomed(Handle);
        public bool Destroy() => WindowsFunctions.DestroyWindow(Handle);
        public bool Show(SW command = SW.SW_SHOW) => WindowsFunctions.ShowWindow(Handle, command);
        public bool Move(int x, int y) => WindowsFunctions.SetWindowPos(Handle, IntPtr.Zero, x, y, -1, -1, SWP.SWP_NOSIZE | SWP.SWP_NOZORDER | SWP.SWP_NOACTIVATE);
        public bool Resize(int width, int height) => WindowsFunctions.SetWindowPos(Handle, IntPtr.Zero, 0, 0, width, height, SWP.SWP_NOMOVE | SWP.SWP_NOZORDER | SWP.SWP_NOACTIVATE);
        public bool FrameChanged() => WindowsFunctions.SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0, SWP.SWP_FRAMECHANGED | SWP.SWP_NOMOVE | SWP.SWP_NOSIZE | SWP.SWP_NOACTIVATE);
        public bool Center() => Center(IntPtr.Zero);
        public bool SetForeground() => WindowsFunctions.SetForegroundWindow(Handle);
        public IntPtr GetWindowLong(int index) => GetWindowLong(Handle, index);
        public IntPtr SetWindowLong(int index, IntPtr data) => SetWindowLong(Handle, index, data);
        public IntPtr GetUserData() => GetUserData(Handle);
        public IntPtr SetUserData(IntPtr data) => SetUserData(Handle, data);
        public bool PostMessage(int msg) => WindowsFunctions.PostMessage(Handle, msg, IntPtr.Zero, IntPtr.Zero);
        public bool PostMessage(int msg, IntPtr wParam) => WindowsFunctions.PostMessage(Handle, msg, wParam, IntPtr.Zero);
        public bool PostMessage(int msg, IntPtr wParam, IntPtr lParam) => WindowsFunctions.PostMessage(Handle, msg, wParam, lParam);
        public void ShellAbout(string text = null, string otherStuff = null) => WindowsFunctions.ShellAbout(Handle, text.Nullify() ?? Assembly.GetEntryAssembly().GetTitle(), otherStuff, IconHandle);
        public IntPtr GetMonitorHandle(MONITOR_FLAGS flags) => WindowsFunctions.MonitorFromWindow(Handle, flags);
        public void ExtendFrameIntoClientArea(int left, int right, int top, int bottom) => DwmExtendFrameIntoClientArea(Handle, left, right, top, bottom);
        public IntPtr CaptureMouse() => WindowsFunctions.SetCapture(Handle);
        public bool CreateCaret(int width, int height) => CreateCaret(IntPtr.Zero, width, height);
        public bool CreateCaret(IntPtr bitmap, int width, int height) => WindowsFunctions.CreateCaret(Handle, bitmap, width, height);
        public static bool SetCaretPosition(int x, int y) => WindowsFunctions.SetCaretPos(x, y);
        public tagPOINT ScreenToClient(tagPOINT pt) { WindowsFunctions.ScreenToClient(Handle, ref pt); return pt; }
        public tagPOINT ClientToScreen(tagPOINT pt) { WindowsFunctions.ClientToScreen(Handle, ref pt); return pt; }
        public tagPOINT GetClientCursorPosition() => ScreenToClient(GetCursorPosition());
        public Monitor GetMonitor(MFW flags = MFW.MONITOR_DEFAULTTONULL) => Monitor.FromWindow(Handle, flags);
        public bool IsChild(IntPtr parentHandle) => WindowsFunctions.IsChild(parentHandle, Handle);
        public WDA DisplayAffinity { get => WindowsFunctions.GetWindowDisplayAffinity(Handle); set => WindowsFunctions.SetWindowDisplayAffinity(Handle, value); }

        public DROPEFFECT DoDragDrop(System.Runtime.InteropServices.ComTypes.IDataObject dataObject, DROPEFFECT allowedEffects)
        {
            if (dataObject == null)
                throw new ArgumentNullException(nameof(dataObject));

            try
            {
                DoDragDrop(dataObject, this, allowedEffects, out var effect).ThrowOnError();
                return effect;
            }
            catch (Exception ex)
            {
                throw new WiceException("0028: Cannot enable drag & drop operations. Make sure the thread is initialized as an STA thread.", ex);
            }
        }

        private void OnDragDrop(DragDropEventArgs e) => DragDrop?.Invoke(this, e);
        HRESULT IDropTarget.DragEnter(System.Runtime.InteropServices.ComTypes.IDataObject dataObject, MODIFIERKEYS_FLAGS flags, tagPOINT pt, ref DROPEFFECT effect)
        {
            var e = new DragDropEventArgs(DragDropEventType.Enter)
            {
                DataObject = dataObject,
                KeyFlags = flags,
                Point = ScreenToClient(pt),
                Effect = effect
            };
            _currentDataObject = dataObject;

            OnDragDrop(e);
            effect = e.Effect;
            return HRESULTS.S_OK;
        }

        HRESULT IDropTarget.DragOver(MODIFIERKEYS_FLAGS flags, tagPOINT pt, ref DROPEFFECT effect)
        {
            var e = new DragDropEventArgs(DragDropEventType.Over)
            {
                DataObject = _currentDataObject,
                KeyFlags = flags,
                Point = ScreenToClient(pt),
                Effect = effect
            };

            OnDragDrop(e);
            effect = e.Effect;
            return HRESULTS.S_OK;
        }

        HRESULT IDropTarget.DragLeave()
        {
            var e = new DragDropEventArgs(DragDropEventType.Leave);
            OnDragDrop(e);
            return HRESULTS.S_OK;
        }

        HRESULT IDropTarget.Drop(System.Runtime.InteropServices.ComTypes.IDataObject dataObject, MODIFIERKEYS_FLAGS flags, tagPOINT pt, ref DROPEFFECT effect)
        {
            var e = new DragDropEventArgs(DragDropEventType.Drop)
            {
                DataObject = dataObject,
                KeyFlags = flags,
                Point = ScreenToClient(pt),
                Effect = effect
            };

            OnDragDrop(e);
            effect = e.Effect;
            return HRESULTS.S_OK;
        }

        HRESULT IDropSourceNotify.DragEnterTarget(IntPtr hwndTarget)
        {
            var win = new NativeWindow(hwndTarget);
            Application.Trace("DragEnterTarget hwndTarget:" + win);

            var e = new DragDropTargetEventArgs(DragDropTargetEventType.Enter, hwndTarget);
            _dragDropTarget = hwndTarget;
            DragDropTarget?.Invoke(this, e);
            return HRESULTS.S_OK;
        }

        HRESULT IDropSourceNotify.DragLeaveTarget()
        {
            Application.Trace("DragLeaveTarget");
            var e = new DragDropTargetEventArgs(DragDropTargetEventType.Leave, _dragDropTarget);
            DragDropTarget?.Invoke(this, e);
            return HRESULTS.S_OK;
        }

        HRESULT IDropSource.QueryContinueDrag(bool escapePressed, MODIFIERKEYS_FLAGS flags)
        {
            //Application.Trace("QueryContinueDrag escapePressed:" + escapePressed + " flags:" + flags);

            var mouseButtons = 0;
            mouseButtons += flags.HasFlag(MODIFIERKEYS_FLAGS.MK_LBUTTON) ? 1 : 0;
            mouseButtons += flags.HasFlag(MODIFIERKEYS_FLAGS.MK_MBUTTON) ? 1 : 0;
            mouseButtons += flags.HasFlag(MODIFIERKEYS_FLAGS.MK_RBUTTON) ? 1 : 0;
            mouseButtons += flags.HasFlag(MODIFIERKEYS_FLAGS.MK_XBUTTON1) ? 1 : 0;
            mouseButtons += flags.HasFlag(MODIFIERKEYS_FLAGS.MK_XBUTTON2) ? 1 : 0;

            var e = new DragDropQueryContinueEventArgs(escapePressed, flags);
            if (escapePressed || mouseButtons > 1)
            {
                e.Result = Wice.DragDropGiveFeedback.DRAGDROP_S_CANCEL;
            }
            else if (mouseButtons == 0)
            {
                e.Result = Wice.DragDropGiveFeedback.DRAGDROP_S_DROP;
            }

            DragDropQueryContinue?.Invoke(this, e);
            return e.Result;
        }

        HRESULT IDropSource.GiveFeedback(DROPEFFECT effect)
        {
            //Application.Trace("GiveFeedback: " + effect);
            var e = new DragDropGiveFeedback(effect);
            DragDropGiveFeedback?.Invoke(this, e);
            return e.Result;
        }

        public override string ToString()
        {
            var s = ClassName;
            var text = Text;
            if (text != null)
            {
                s += " '" + text + "'";
            }
            return s;
        }

        public static IntPtr ConsoleHandle => WindowsFunctions.GetConsoleWindow();
        public static NativeWindow Console => FromHandle(ConsoleHandle);

        public static IntPtr ActiveHandle => WindowsFunctions.GetActiveWindow();
        public static NativeWindow Active => FromHandle(ActiveHandle);

        public static IntPtr ForegroundHandle => WindowsFunctions.GetForegroundWindow();
        public static NativeWindow Foreground => FromHandle(ForegroundHandle);

        public static IntPtr DesktopHandle => WindowsFunctions.GetDesktopWindow();
        public static NativeWindow Desktop => FromHandle(DesktopHandle);

        public static IntPtr ShellHandle => WindowsFunctions.GetShellWindow();
        public static NativeWindow Shell => FromHandle(ShellHandle);

        public static NativeWindow FromHandle(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                return null;

            return new NativeWindow(handle);
        }

        public static bool IsChildWindow(IntPtr parentHandle, IntPtr handle) => WindowsFunctions.IsChild(parentHandle, handle);
        public static IntPtr GetWindowFromPoint(tagPOINT point) => WindowsFunctions.WindowFromPoint(point);
        public static char VirtualKeyToCharacter(VirtualKeys vk) => (char)WindowsFunctions.MapVirtualKey((uint)vk, WindowsConstants.MAPVK_VK_TO_CHAR);
        public static bool IsKeyPressed(VirtualKeys vk, bool async = true) => (async ? WindowsFunctions.GetAsyncKeyState(vk) : WindowsFunctions.GetKeyState(vk)) < 0;
        public static bool ReleaseMouse() => WindowsFunctions.ReleaseCapture();
        public static tagPOINT GetCursorPosition()
        {
            var pt = new tagPOINT();
            WindowsFunctions.GetCursorPos(ref pt);
            return pt;
        }

        public static int GetAccessibilityCursorSize()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Accessibility", false))
            {
                if (key != null)
                {
                    if (key.GetValue("CursorSize") is int i)
                        return Math.Min(Math.Max(i, 1), 15);
                }
            }
            return 1;
        }

        public static tagPOINT LastMessagePosision
        {
            get
            {
                var pos = WindowsFunctions.GetMessagePos();
                return new tagPOINT(pos.SignedLOWORD(), pos.SignedHIWORD());
            }
        }

        // https://stackoverflow.com/a/51037982/403671
        internal static bool DidMouseLeaveWindow(IntPtr handle, IntPtr other)
        {
            var pt = LastMessagePosision;
            var win = WindowsFunctions.WindowFromPoint(pt);
            var child = WindowsFunctions.IsChild(handle, win);
            var result = !child && (win != other || other == IntPtr.Zero);
            //Application.Trace("pt: " + pt + " win: " + new NativeWindow(win) + " child: " + child + " => " + result);
            return result;
        }

        internal static void DwmExtendFrameIntoClientArea(IntPtr hwnd, int left, int right, int top, int bottom)
        {
            var margin = new MARGINS();
            margin.cxLeftWidth = left;
            margin.cxRightWidth = right;
            margin.cyBottomHeight = bottom;
            margin.cyTopHeight = top;
            WindowsFunctions.DwmExtendFrameIntoClientArea(hwnd, ref margin).ThrowOnError();
        }

        internal static bool RegisterWindowClass(string className, WndProc windowProc)
        {
            if (className == null)
                throw new ArgumentNullException(nameof(className));

            if (windowProc == null)
                throw new ArgumentNullException(nameof(windowProc));

            if (!WindowsFunctions.GetClassInfo(Application.ModuleHandle, className, out _))
            {
                var cls = new WNDCLASS();
                cls.style = WindowsConstants.CS_HREDRAW | WindowsConstants.CS_VREDRAW;
                cls.lpfnWndProc = windowProc;
                cls.hInstance = Application.ModuleHandle;
                cls.lpszClassName = className;
                //cls.hCursor = DirectN.Cursor.Arrow.Handle; // we set the cursor ourselves, otherwise the cursor will blink
                //const int WHITE_BRUSH = 0;
                //const int LTGRAY_BRUSH = 1;
                //const int GRAY_BRUSH = 2;
                //const int DKGRAY_BRUSH = 3;
                //const int BLACK_BRUSH = 4;
                //cls.hbrBackground = GetStockObject(WHITE_BRUSH);
                if (WindowsFunctions.RegisterClass(ref cls) == 0)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                //Trace("Class '" + className + "' registered.");
                _classesNames.Add(className);
                return true;
            }

            //Trace("Class '" + className + "' was already registered.");
            return false;
        }

        internal static void UnregisterWindowClasses()
        {
            foreach (var name in _classesNames)
            {
                WindowsFunctions.UnregisterClass(name, Application.ModuleHandle);
                _classesNames.TryRemove(name);
            }
        }

        public static void OnPaint(IntPtr hwnd, Action action = null)
        {
            var ps = new PAINTSTRUCT();
            WindowsFunctions.BeginPaint(hwnd, ref ps);
            if (action != null)
            {
                action();
            }
            WindowsFunctions.EndPaint(hwnd, ref ps);
        }

        internal static HT NonClientHitTest(IntPtr hwnd, IntPtr lParam, ref tagRECT extend)
        {
            var ptMouse = new tagPOINT(lParam.SignedLOWORD(), lParam.SignedHIWORD());
            WindowsFunctions.GetWindowRect(hwnd, out var rcWindow);

            var rcFrame = new tagRECT();
            var dpi = WindowsFunctions.GetDpiForWindow(hwnd);
            WindowsFunctions.AdjustWindowRectExForDpi(ref rcFrame, WS.WS_OVERLAPPEDWINDOW & ~WS.WS_CAPTION, false, 0, dpi);

            var uRow = 1;
            var uCol = 1;
            var fOnResizeBorder = false;

            if (ptMouse.y >= rcWindow.top && ptMouse.y < rcWindow.top + extend.top)
            {
                fOnResizeBorder = (ptMouse.y < (rcWindow.top - rcFrame.top));
                uRow = 0;
            }
            else if (ptMouse.y < rcWindow.bottom && ptMouse.y >= rcWindow.bottom - extend.bottom)
            {
                uRow = 2;
            }

            if (ptMouse.x >= rcWindow.left && ptMouse.x < rcWindow.left + extend.left)
            {
                uCol = 0;
            }
            else if (ptMouse.x < rcWindow.right && ptMouse.x >= rcWindow.right - extend.right)
            {
                uCol = 2;
            }

            var hitTests = new HT[,]{
                { HT.HTTOPLEFT, fOnResizeBorder? HT.HTTOP: HT.HTCAPTION,    HT.HTTOPRIGHT},
                { HT.HTLEFT, HT.HTNOWHERE, HT.HTRIGHT},
                { HT.HTBOTTOMLEFT, HT.HTBOTTOM, HT.HTBOTTOMRIGHT },
            };
            return hitTests[uRow, uCol];
        }

        internal static IntPtr SetUserData(IntPtr hwnd, IntPtr data) => SetWindowLong(hwnd, WindowsConstants.GWLP_USERDATA, data);
        internal static IntPtr GetUserData(IntPtr hwnd) => GetWindowLong(hwnd, WindowsConstants.GWLP_USERDATA);
        internal static IntPtr SetWindowLong(IntPtr hwnd, int nIndex, IntPtr dwNewLong) => IntPtr.Size == 8 ? WindowsFunctions.SetWindowLongPtr64(hwnd, nIndex, dwNewLong) : WindowsFunctions.SetWindowLongPtr32(hwnd, nIndex, dwNewLong);
        internal static IntPtr GetWindowLong(IntPtr hwnd, int nIndex) => IntPtr.Size == 8 ? WindowsFunctions.GetWindowLongPtr64(hwnd, nIndex) : WindowsFunctions.GetWindowLong32(hwnd, nIndex);
        internal static string GetWindowText(IntPtr hwnd)
        {
            var sb = new StringBuilder(256);
            WindowsFunctions.GetWindowText(hwnd, sb, sb.Capacity - 1);
            return sb.ToString();
        }

        internal static string GetClassName(IntPtr hwnd)
        {
            var sb = new StringBuilder(256);
            WindowsFunctions.GetClassName(hwnd, sb, sb.Capacity - 1);
            return sb.ToString();
        }

        public static tagRECT AdjustWindowRect(tagRECT rect, WS style, bool hasMenu, WS_EX extendedStyle, int dpi)
        {
            var rc = rect;
            WindowsFunctions.AdjustWindowRectExForDpi(ref rc, style, hasMenu, extendedStyle, dpi);
            return rc;
        }

        public bool Center(IntPtr alternateOwner)
        {
            var style = (WS)GetWindowLong(WindowsConstants.GWL_STYLE).ToInt64();
            var hwndCenter = alternateOwner;
            if (alternateOwner == IntPtr.Zero)
            {
                if (style.HasFlag(WS.WS_CHILD))
                {
                    hwndCenter = ParentHandle;
                }
                else
                {
                    hwndCenter = OwnerHandle;
                }
            }

            // get coordinates of the window relative to its parent
            var rc = WindowRect;
            tagRECT area;
            tagRECT center;
            if (!style.HasFlag(WS.WS_CHILD))
            {
                // don't center against invisible or minimized windows
                if (hwndCenter != IntPtr.Zero)
                {
                    var alternateStyle = (WS)GetWindowLong(hwndCenter, WindowsConstants.GWL_STYLE).ToInt64();
                    if (!alternateStyle.HasFlag(WS.WS_VISIBLE) || alternateStyle.HasFlag(WS.WS_MINIMIZE))
                    {
                        hwndCenter = IntPtr.Zero;
                    }
                }

                var mi = new MONITORINFO();
                mi.cbSize = Marshal.SizeOf(typeof(MONITORINFO));

                // center within appropriate monitor coordinates
                if (hwndCenter == IntPtr.Zero)
                {
                    var hwDefault = Handle;
                    WindowsFunctions.GetMonitorInfo(WindowsFunctions.MonitorFromWindow(hwDefault, MONITOR_FLAGS.MONITOR_DEFAULTTOPRIMARY), ref mi);
                    center = mi.rcWork;
                    area = mi.rcWork;
                }
                else
                {
                    center = WindowRect;
                    WindowsFunctions.GetMonitorInfo(WindowsFunctions.MonitorFromWindow(hwndCenter, MONITOR_FLAGS.MONITOR_DEFAULTTONEAREST), ref mi);
                    area = mi.rcWork;
                }
            }
            else
            {
                // center within parent client coordinates
                var hWndParent = ParentHandle;
                area = ClientRect;
                center = ClientRect;
                WindowsFunctions.MapWindowPoints(hwndCenter, hWndParent, ref center, 2);
            }

            // find dialog's upper left based on rcCenter
            var left = (center.left + center.right) / 2 - rc.Width / 2;
            var top = (center.top + center.bottom) / 2 - rc.Height / 2;

            // if the dialog is outside the screen, move it inside
            if (left + rc.Width > area.right)
            {
                left = area.right - rc.Width;
            }

            if (left < area.left)
            {
                left = area.left;
            }

            if (top + rc.Height > area.bottom)
            {
                top = area.bottom - rc.Height;
            }

            if (top < area.top)
            {
                top = area.top;
            }

            // map screen coordinates to child coordinates
            return Move(left, top);
        }

        public TITLEBARINFOEX GetTITLEBARINFOEX() => GetTITLEBARINFOEX(Handle);
        public static TITLEBARINFOEX GetTITLEBARINFOEX(IntPtr hwnd)
        {
            var info = new TITLEBARINFOEX { cbSize = Marshal.SizeOf<TITLEBARINFOEX>() };
            if (hwnd == IntPtr.Zero)
                return info;

            using (var mem = new ComMemory<TITLEBARINFOEX>(info))
            {
                WindowsFunctions.SendMessage(hwnd, MessageDecoder.WM_GETTITLEBARINFOEX, IntPtr.Zero, mem.Pointer);
                return mem.ToStructure();
            }
        }

        // https://devblogs.microsoft.com/oldnewthing/20101020-00/?p=12493
        public static tagSIZE? GetIconDimension(IntPtr handle) => GetIconDimension(handle, out _);
        public static tagSIZE? GetIconDimension(IntPtr handle, out tagSIZE? hotspot)
        {
            hotspot = null;
            if (handle == IntPtr.Zero)
                return null;

            if (!WindowsFunctions.GetIconInfo(handle, out var ii))
                return null;

            var bmpSize = Marshal.SizeOf<BITMAP>();
            var res = WindowsFunctions.GetObject(ii.hbmMask, bmpSize, out var bm);

            tagSIZE? size = null;
            if (res == bmpSize)
            {
                size = new tagSIZE(bm.bmWidth, ii.hbmColor != IntPtr.Zero ? bm.bmHeight : bm.bmHeight / 2);
                hotspot = new tagSIZE(ii.xHotspot, ii.yHotspot);
            }

            if (ii.hbmMask != IntPtr.Zero)
            {
                WindowsFunctions.DeleteObject(ii.hbmMask);
            }

            if (ii.hbmColor != IntPtr.Zero)
            {
                WindowsFunctions.DeleteObject(ii.hbmColor);
            }
            return size;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DWM_BLURBEHIND
        {
            public DWM_BB dwFlags;
            public bool fEnable;
            public IntPtr hRgnBlur;
            public bool fTransitionOnMaximized;
        }

        [DllImport("user32")]
        internal static extern bool PostThreadMessage(int idThread, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32")]
        internal static extern int GetSystemMetricsForDpi(SM index, int dpi);

        [DllImport("ole32")]
        private static extern HRESULT RegisterDragDrop(IntPtr hwnd, IDropTarget pDropTarget);

        [DllImport("ole32")]
        public static extern HRESULT OleInitialize(IntPtr pvReserved);

        [DllImport("ole32")]
        private static extern HRESULT DoDragDrop(System.Runtime.InteropServices.ComTypes.IDataObject pDataObj, IDropSource pDropSource, DROPEFFECT dwOKEffects, out DROPEFFECT pdwEffect);

        [DllImport("ole32")]
        private static extern HRESULT RevokeDragDrop(IntPtr hwnd);

        const int DRAGDROP_E_NOTREGISTERED = unchecked((int)0x80040100);
        const int DRAGDROP_E_ALREADYREGISTERED = unchecked((int)0x80040101);

        [ComImport, Guid("00000122-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IDropTarget
        {
            [PreserveSig]
            HRESULT DragEnter(System.Runtime.InteropServices.ComTypes.IDataObject pDataObj, MODIFIERKEYS_FLAGS grfKeyState, tagPOINT pt, ref DROPEFFECT pdwEffect);

            [PreserveSig]
            HRESULT DragOver(MODIFIERKEYS_FLAGS grfKeyState, tagPOINT pt, ref DROPEFFECT pdwEffect);

            [PreserveSig]
            HRESULT DragLeave();

            [PreserveSig]
            HRESULT Drop(System.Runtime.InteropServices.ComTypes.IDataObject pDataObj, MODIFIERKEYS_FLAGS grfKeyState, tagPOINT pt, ref DROPEFFECT pdwEffect);
        }

        [ComImport, Guid("00000121-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IDropSource
        {
            [PreserveSig]
            HRESULT QueryContinueDrag(bool fEscapePressed, MODIFIERKEYS_FLAGS grfKeyState);

            [PreserveSig]
            HRESULT GiveFeedback(DROPEFFECT dwEffect);
        }

        [ComImport, Guid("0000012B-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IDropSourceNotify
        {
            [PreserveSig]
            HRESULT DragEnterTarget(IntPtr hwndTarget);

            [PreserveSig]
            HRESULT DragLeaveTarget();
        }

        [DllImport("user32")]
        private static extern bool AreDpiAwarenessContextsEqual(IntPtr dpiContextA, IntPtr dpiContextB);

        [DllImport("user32")]
        internal static extern IntPtr GetThreadDpiAwarenessContext();

        [DllImport("user32")]
        internal static extern int GetDpiFromDpiAwarenessContext(IntPtr value);

        [DllImport("shell32", ExactSpelling = true)]
        private static extern bool SHGetPropertyStoreForWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);

        [ComImport, Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPropertyStore
        {
            [PreserveSig]
            int GetCount(out int cProps);

            [PreserveSig]
            int GetAt(int iProp, out PROPERTYKEY pkey);

            [PreserveSig]
            int GetValue(ref PROPERTYKEY key, [Out] PropVariant pv);

            [PreserveSig]
            int SetValue(ref PROPERTYKEY key, PropVariant propvar);

            [PreserveSig]
            int Commit();
        }

        public void SetWindowAttribute(DWMWINDOWATTRIBUTE attribute, bool value) => SetWindowAttribute(Handle, attribute, value);
        public static void SetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attribute, bool value)
        {
            var i = value ? 1 : 0;
            WindowsFunctions.DwmSetWindowAttribute(hwnd, attribute, ref i, 4).ThrowOnError();
        }

        public void SetNonClientRenderingPolicy(DWMNCRENDERINGPOLICY policy) => SetNonClientRenderingPolicy(Handle, policy);
        public static void SetNonClientRenderingPolicy(IntPtr hwnd, DWMNCRENDERINGPOLICY policy)
        {
            var i = (int)policy;
            WindowsFunctions.DwmSetWindowAttribute(hwnd, DWMWINDOWATTRIBUTE.DWMWA_NCRENDERING_POLICY, ref i, 4).ThrowOnError();
        }

        //public void EnableBlurBehindWindow(bool enable = true) => EnableBlurBehindWindow(Handle, enable);
        //public static void EnableBlurBehindWindow(IntPtr hwnd, bool enable = true)
        //{
        //    var bb = new DWM_BLURBEHIND();
        //    bb.dwFlags = DWM_BB.DWM_BB_ENABLE;
        //    bb.fEnable = enable;
        //    DwmEnableBlurBehindWindow(hwnd, ref bb).ThrowOnError();
        //}

        public static string GetDpiAwarenessDescription(IntPtr awareness)
        {
            if (awareness == IntPtr.Zero)
                return nameof(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE);

            if (WindowsVersionUtilities.KernelVersion >= new Version(10, 0, 14393))
            {
                if (AreDpiAwarenessContextsEqual(awareness, (IntPtr)DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_UNAWARE))
                    return nameof(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_UNAWARE);

                if (AreDpiAwarenessContextsEqual(awareness, (IntPtr)DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_SYSTEM_AWARE))
                    return nameof(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_SYSTEM_AWARE);

                if (AreDpiAwarenessContextsEqual(awareness, (IntPtr)DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE))
                    return nameof(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE);

                if (AreDpiAwarenessContextsEqual(awareness, (IntPtr)DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2))
                    return nameof(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);

                if (AreDpiAwarenessContextsEqual(awareness, (IntPtr)DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_UNAWARE_GDISCALED))
                    return nameof(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_UNAWARE_GDISCALED);
            }

            if (IntPtr.Size == 4)
                return "0x" + awareness.ToString("X8");

            return "0x" + awareness.ToString("X16");
        }


        public HRESULT EnableAcrylicBlurBehind() => EnableAcrylicBlurBehind(Handle);
        public static HRESULT EnableAcrylicBlurBehind(IntPtr hwnd)
        {
            var accent = new ACCENT_POLICY();
            accent.AccentState = ACCENT_STATE.ACCENT_ENABLE_ACRYLICBLURBEHIND;
            //accent.GradientColor = 0xFF00FF00;
            var data = new WINDOWCOMPOSITIONATTRIBDATA();
            data.dwAttrib = WINDOWCOMPOSITIONATTRIB.WCA_ACCENT_POLICY;
            using (var mem = new ComMemory(accent))
            {
                data.cbData = new IntPtr(mem.Size);
                data.pvData = mem.Pointer;
                return WindowsFunctions.SetWindowCompositionAttribute(hwnd, ref data).ThrowOnError();
            }
        }

        public HRESULT EnableBlurBehind() => EnableBlurBehind(Handle);
        public static HRESULT EnableBlurBehind(IntPtr hwnd)
        {
            var accent = new ACCENT_POLICY();
            accent.AccentState = ACCENT_STATE.ACCENT_ENABLE_BLURBEHIND;
            //accent.GradientColor = 0xFF00FF00;
            var data = new WINDOWCOMPOSITIONATTRIBDATA();
            data.dwAttrib = WINDOWCOMPOSITIONATTRIB.WCA_ACCENT_POLICY;
            using (var mem = new ComMemory(accent))
            {
                data.cbData = new IntPtr(mem.Size);
                data.pvData = mem.Pointer;
                return WindowsFunctions.SetWindowCompositionAttribute(hwnd, ref data).ThrowOnError();
            }
        }
    }
}