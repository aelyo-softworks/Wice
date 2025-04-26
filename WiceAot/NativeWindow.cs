namespace Wice;

[System.Runtime.InteropServices.Marshalling.GeneratedComClass]
public sealed partial class NativeWindow : IEquatable<NativeWindow>, IDropTarget, IDropSource, IDropSourceNotify
{
    private static readonly ConcurrentHashSet<string> _classesNames = [];

    public static WNDPROC DefWindowProc { get; } = GetDefWindowProc();
    private static WNDPROC GetDefWindowProc() => Marshal.GetDelegateForFunctionPointer<WNDPROC>(Functions.GetProcAddress(Functions.GetModuleHandleW(PWSTR.From("user32.dll")), PSTR.From("DefWindowProcW")));

    public static IEnumerable<NativeWindow> TopLevelWindows => EnumerateTopLevelWindows().Select(FromHandle).Where(w => w != null)!;

    public event EventHandler<DragDropEventArgs>? DragDrop;
    public event EventHandler<DragDropQueryContinueEventArgs>? DragDropQueryContinue;
    public event EventHandler<DragDropGiveFeedback>? DragDropGiveFeedback;
    public event EventHandler<DragDropTargetEventArgs>? DragDropTarget;

    private HWND _dragDropTarget;
    private bool _isDropTarget;
    private IDataObject? _currentDataObject;

    private NativeWindow(HWND handle)
    {
        Handle = handle;
    }

    public HWND Handle { get; }
    public string? ClassName => GetClassName(Handle);
    public HWND ParentHandle { get => Functions.GetParent(Handle); set => Functions.SetParent(Handle, value); }
    public HWND OwnerHandle => Functions.GetWindow(Handle, GET_WINDOW_CMD.GW_OWNER);
    public NativeWindow? Parent => FromHandle(ParentHandle);
    public NativeWindow? Owner => FromHandle(OwnerHandle);
    public RECT WindowRect { get { Functions.GetWindowRect(Handle, out var rc); return rc; } set => Functions.SetWindowPos(Handle, HWND.Null, value.left, value.top, value.Width, value.Height, SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_NOREDRAW | SET_WINDOW_POS_FLAGS.SWP_NOZORDER); }
    public RECT ClientRect { get { Functions.GetClientRect(Handle, out var rc); return rc; } }
    public HICON IconHandle { get => new() { Value = Functions.SendMessageW(Handle, MessageDecoder.WM_GETICON, new WPARAM { Value = Constants.ICON_BIG }, LPARAM.Null).Value }; set { var ptr = Functions.SendMessageW(Handle, MessageDecoder.WM_SETICON, new WPARAM { Value = Constants.ICON_BIG }, new LPARAM { Value = value.Value }); if (ptr.Value != 0) { Functions.DestroyIcon(new HICON { Value = ptr.Value }); } } }
    public uint Dpi { get { var dpi = Functions.GetDpiForWindow(Handle); if (dpi <= 0) return 96; return dpi; } }
    public DPI_AWARENESS_CONTEXT DpiAwareness => DpiUtilities.GetWindowDpiAwarenessContext(Handle);
    public string DpiAwarenessDescription => DpiUtilities.GetDpiAwarenessDescription(DpiAwareness);
    public uint DpiFromDpiAwareness => Functions.GetDpiFromDpiAwarenessContext(DpiAwareness);
    public WINDOW_STYLE Style { get => (WINDOW_STYLE)GetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_STYLE).ToInt64(); set => SetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_STYLE, new nint((int)value)); }
    public WINDOW_EX_STYLE ExtendedStyle { get => (WINDOW_EX_STYLE)GetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE).ToInt64(); set => SetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, new nint((int)value)); }
    public bool IsEnabled { get => Functions.IsWindowEnabled(Handle); set => Functions.EnableWindow(Handle, value); }
    public uint ThreadId => Functions.GetWindowThreadProcessId(Handle, 0);
    public int ManagedThreadId { get; internal set; }
    public unsafe int ProcessId
    {
        get
        {
            int pid;
            _ = Functions.GetWindowThreadProcessId(Handle, (nint)(&pid));
            return pid;
        }
    }

    public string Text
    {
        get => GetWindowText(Handle);
        set
        {
            value ??= string.Empty;
            Functions.SetWindowTextW(Handle, PWSTR.From(value));
        }
    }

    public string? ModuleFileName
    {
        get
        {
            using var name = new AllocPwstr(1024);
            _ = Functions.GetWindowModuleFileNameW(Handle, name, name.SizeInChars);
            return name.ToString();
        }
    }

    public Process? Process
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

    public WINDOW_DISPLAY_AFFINITY DisplayAffinity
    {
        get
        {
            Functions.GetWindowDisplayAffinity(Handle, out var affinity);
            return (WINDOW_DISPLAY_AFFINITY)affinity;
        }
        set => Functions.SetWindowDisplayAffinity(Handle, value);
    }

    public unsafe DWM_CLOAKED CloakedState
    {
        get
        {
            var state = 0;
            Functions.DwmGetWindowAttribute(Handle, (uint)DWMWINDOWATTRIBUTE.DWMWA_CLOAKED, (nint)(&state), sizeof(int));
            return (DWM_CLOAKED)state;
        }
    }

    public unsafe RECT ExtendedFrameBounds
    {
        get
        {
            var bounds = new RECT();
            Functions.DwmGetWindowAttribute(Handle, (uint)DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, (nint)(&bounds), (uint)sizeof(RECT));
            return bounds;
        }
    }

    public IEnumerable<NativeWindow> ChildWindows => EnumerateChildWindows(Handle).Select(h => FromHandle(h)).Where(w => w != null)!;
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

    public IReadOnlyDictionary<string, nint> Props
    {
        get
        {
            var dic = new Dictionary<string, nint>();
            BOOL enumProc(HWND h, PWSTR p1, HANDLE p2)
            {
                var value = Functions.GetPropW(h, p1);
                var s = p1.ToString();
                if (s != null)
                {
                    dic[s] = value;
                }
                return true;
            }
            Functions.EnumPropsW(Handle, enumProc);
            return dic.AsReadOnly();
        }
    }

    public IReadOnlyDictionary<PROPERTYKEY, object?> Properties
    {
        get
        {
            var dic = new Dictionary<PROPERTYKEY, object?>();
            Functions.SHGetPropertyStoreForWindow(Handle, typeof(IPropertyStore).GUID, out var unk);
            using var ps = ComObject.FromPointer<IPropertyStore>(unk);
            if (ps != null)
            {
                ps.Object.GetCount(out var count);
                for (uint i = 0; i < count; i++)
                {
                    var hr = ps.Object.GetAt(i, out var pk);
                    if (hr.IsError)
                        continue;

                    hr = ps.Object.GetValue(pk, out var pv);
                    if (hr == 0)
                    {
                        using var managed = PropVariant.Attach(ref pv);
                        dic[pk] = managed.Value;
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
                Functions.OleInitialize(0); // don't check error
                var hr = Functions.RegisterDragDrop(Handle, this);
                if (hr.IsError && hr != Constants.DRAGDROP_E_ALREADYREGISTERED)
                    throw new WiceException("0027: Cannot enable drag & drop operations. Make sure the thread is initialized as an STA thread.", Marshal.GetExceptionForHR(hr)!);

                _isDropTarget = true;
            }
            else
            {
                var hr = Functions.RevokeDragDrop(Handle);
                hr.ThrowOnErrorExcept(Constants.DRAGDROP_E_NOTREGISTERED);
                _isDropTarget = false;
            }
        }
    }

    public override int GetHashCode() => Handle.GetHashCode();
    public override bool Equals(object? obj) => Equals(obj as NativeWindow);
    public bool Equals(NativeWindow? other) => other != null && Handle.Value == other.Handle.Value;
    public bool IsZoomed() => Functions.IsZoomed(Handle);
    public bool Destroy() => Functions.DestroyWindow(Handle);
    public bool Show(SHOW_WINDOW_CMD command = SHOW_WINDOW_CMD.SW_SHOW) => Functions.ShowWindow(Handle, command);
    public bool Move(int x, int y) => Functions.SetWindowPos(Handle, HWND.Null, x, y, -1, -1, SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);
    public bool Resize(int width, int height) => Functions.SetWindowPos(Handle, HWND.Null, 0, 0, width, height, SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);
    public bool FrameChanged() => Functions.SetWindowPos(Handle, HWND.Null, 0, 0, 0, 0, SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);
    public bool Center() => Center(HWND.Null);
    public bool SetForeground() => Functions.SetForegroundWindow(Handle);
    public nint GetWindowLong(WINDOW_LONG_PTR_INDEX index) => GetWindowLong(Handle, index);
    public nint SetWindowLong(WINDOW_LONG_PTR_INDEX index, nint data) => SetWindowLong(Handle, index, data);
    public nint GetUserData() => GetUserData(Handle);
    public nint SetUserData(nint data) => SetUserData(Handle, data);
    public bool PostMessage(uint msg) => Functions.PostMessageW(Handle, msg, WPARAM.Null, LPARAM.Null);
    public bool PostMessage(uint msg, WPARAM wParam) => Functions.PostMessageW(Handle, msg, wParam, LPARAM.Null);
    public bool PostMessage(uint msg, WPARAM wParam, LPARAM lParam) => Functions.PostMessageW(Handle, msg, wParam, lParam);
    public HMONITOR GetMonitorHandle(MONITOR_FROM_FLAGS flags) => Functions.MonitorFromWindow(Handle, flags);
    public void ExtendFrameIntoClientArea(int left, int right, int top, int bottom) => DwmExtendFrameIntoClientArea(Handle, left, right, top, bottom);
    public HWND CaptureMouse() => Functions.SetCapture(Handle);
    public bool CreateCaret(int width, int height) => CreateCaret(HBITMAP.Null, width, height);
    public bool CreateCaret(HBITMAP bitmap, int width, int height) => Functions.CreateCaret(Handle, bitmap, width, height);
    public static bool SetCaretPosition(int x, int y) => Functions.SetCaretPos(x, y);
    public POINT ScreenToClient(POINT pt) { Functions.ScreenToClient(Handle, ref pt); return pt; }
    public POINT ClientToScreen(POINT pt) { Functions.ClientToScreen(Handle, ref pt); return pt; }
    public POINT GetClientCursorPosition() => ScreenToClient(GetCursorPosition());
    public DirectN.Extensions.Utilities.Monitor? GetMonitor(MONITOR_FROM_FLAGS flags = MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONULL) => DirectN.Extensions.Utilities.Monitor.FromWindow(Handle, flags);
    public bool IsChild(HWND parentHandle) => Functions.IsChild(parentHandle, Handle);

    public bool IsRunningAsMainThread => ManagedThreadId == Environment.CurrentManagedThreadId;
    public void CheckRunningAsMainThread() { if (!IsRunningAsMainThread) throw new WiceException("0029: This method must be called on the UI thread."); }

    public int ShellAbout(string? text = null, string? otherStuff = null)
    {
        var txt = text.Nullify() ?? Assembly.GetEntryAssembly()?.GetTitle();
        return Functions.ShellAboutW(Handle, PWSTR.From(txt), PWSTR.From(otherStuff), IconHandle);
    }

    private void OnDragDrop(DragDropEventArgs e) => DragDrop?.Invoke(this, e);
    HRESULT IDropTarget.DragEnter(IDataObject dataObject, MODIFIERKEYS_FLAGS flags, POINTL pt, ref DROPEFFECT effect)
    {
        var e = new DragDropEventArgs(DragDropEventType.Enter)
        {
            DataObject = dataObject,
            KeyFlags = flags,
            Point = ScreenToClient(pt.ToPOINT()),
            Effect = effect
        };
        _currentDataObject = dataObject;

        OnDragDrop(e);
        effect = e.Effect;
        return Constants.S_OK;
    }

    HRESULT IDropTarget.DragOver(MODIFIERKEYS_FLAGS flags, POINTL pt, ref DROPEFFECT effect)
    {
        var e = new DragDropEventArgs(DragDropEventType.Over)
        {
            DataObject = _currentDataObject,
            KeyFlags = flags,
            Point = ScreenToClient(pt.ToPOINT()),
            Effect = effect
        };

        OnDragDrop(e);
        effect = e.Effect;
        return Constants.S_OK;
    }

    HRESULT IDropTarget.DragLeave()
    {
        var e = new DragDropEventArgs(DragDropEventType.Leave);
        OnDragDrop(e);
        return Constants.S_OK;
    }

    HRESULT IDropTarget.Drop(IDataObject dataObject, MODIFIERKEYS_FLAGS flags, POINTL pt, ref DROPEFFECT effect)
    {
        var e = new DragDropEventArgs(DragDropEventType.Drop)
        {
            DataObject = dataObject,
            KeyFlags = flags,
            Point = ScreenToClient(pt.ToPOINT()),
            Effect = effect
        };

        OnDragDrop(e);
        effect = e.Effect;
        return Constants.S_OK;
    }

    HRESULT IDropSourceNotify.DragEnterTarget(HWND hwndTarget)
    {
        var win = new NativeWindow(hwndTarget);
        Application.Trace("DragEnterTarget hwndTarget:" + win);

        var e = new DragDropTargetEventArgs(DragDropTargetEventType.Enter, hwndTarget);
        _dragDropTarget = hwndTarget;
        DragDropTarget?.Invoke(this, e);
        return Constants.S_OK;
    }

    HRESULT IDropSourceNotify.DragLeaveTarget()
    {
        Application.Trace("DragLeaveTarget");
        var e = new DragDropTargetEventArgs(DragDropTargetEventType.Leave, _dragDropTarget);
        DragDropTarget?.Invoke(this, e);
        return Constants.S_OK;
    }

    HRESULT IDropSource.QueryContinueDrag(BOOL escapePressed, MODIFIERKEYS_FLAGS flags)
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
            e.Result = Constants.DRAGDROP_S_CANCEL;
        }
        else if (mouseButtons == 0)
        {
            e.Result = Constants.DRAGDROP_S_DROP;
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
        var str = ClassName;
        var text = Text;
        if (text != null)
        {
            str += " '" + text + "'";
        }
        return str ?? string.Empty;
    }

    public static HWND ConsoleHandle => Functions.GetConsoleWindow();
    public static NativeWindow? Console => FromHandle(ConsoleHandle);

    public static HWND ActiveHandle => Functions.GetActiveWindow();
    public static NativeWindow? Active => FromHandle(ActiveHandle);

    public static HWND ForegroundHandle => Functions.GetForegroundWindow();
    public static NativeWindow? Foreground => FromHandle(ForegroundHandle);

    public static HWND DesktopHandle => Functions.GetDesktopWindow();
    public static NativeWindow? Desktop => FromHandle(DesktopHandle);

    public static HWND ShellHandle => Functions.GetShellWindow();
    public static NativeWindow? Shell => FromHandle(ShellHandle);

    public static NativeWindow? FromHandle(HWND handle) { if (handle.Value == 0) return null; return new NativeWindow(handle); }
    public static bool IsChildWindow(HWND parentHandle, HWND handle) => Functions.IsChild(parentHandle, handle);
    public static HWND GetWindowFromPoint(POINT point) => Functions.WindowFromPoint(point);
    public static char VirtualKeyToCharacter(VIRTUAL_KEY vk) => (char)Functions.MapVirtualKeyW((uint)vk, MAP_VIRTUAL_KEY_TYPE.MAPVK_VK_TO_CHAR);
    public static bool IsKeyPressed(VIRTUAL_KEY vk, bool async = true) => (async ? Functions.GetAsyncKeyState((int)vk) : Functions.GetKeyState((int)vk)) < 0;
    public static bool ReleaseMouse() => Functions.ReleaseCapture();
    public static POINT GetCursorPosition() { Functions.GetCursorPos(out var pt); return pt; }

    public static int GetAccessibilityCursorSize()
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Accessibility", false);
        if (key != null)
        {
            if (key.GetValue("CursorSize") is int i)
                return Math.Min(Math.Max(i, 1), 15);
        }
        return 1;
    }

    public static POINT LastMessagePosision
    {
        get
        {
            var pos = Functions.GetMessagePos();
            return new POINT(pos.SignedLOWORD(), pos.SignedHIWORD());
        }
    }

    // https://stackoverflow.com/a/51037982/403671
    internal static bool DidMouseLeaveWindow(HWND handle, HWND other)
    {
        var pt = LastMessagePosision;
        var win = Functions.WindowFromPoint(pt);
        var child = Functions.IsChild(handle, win);
        var result = !child && (win.Value != other.Value || other.Value == 0);
        //Application.Trace("pt: " + pt + " win: " + new NativeWindow(win) + " child: " + child + " => " + result);
        return result;
    }

    internal static void DwmExtendFrameIntoClientArea(HWND hwnd, int left, int right, int top, int bottom)
    {
        var margin = new MARGINS
        {
            cxLeftWidth = left,
            cxRightWidth = right,
            cyBottomHeight = bottom,
            cyTopHeight = top
        };
        Functions.DwmExtendFrameIntoClientArea(hwnd, margin).ThrowOnError();
    }

    internal static bool RegisterWindowClass(string className, nint windowProc)
    {
        ArgumentNullException.ThrowIfNull(className);
        if (windowProc == 0)
            throw new ArgumentException(null, nameof(windowProc));

        if (!Functions.GetClassInfoW(new HINSTANCE { Value = Application.ModuleHandle.Value }, PWSTR.From(className), out _))
        {
            var cls = new WNDCLASSW
            {
                style = WNDCLASS_STYLES.CS_HREDRAW | WNDCLASS_STYLES.CS_VREDRAW,
                lpfnWndProc = windowProc,
                hInstance = new HINSTANCE { Value = Application.ModuleHandle.Value },
                lpszClassName = PWSTR.From(className),
            };
            //cls.hCursor = DirectN.Cursor.Arrow.Handle; // we set the cursor ourselves, otherwise the cursor will blink
            //const int WHITE_BRUSH = 0;
            //const int LTGRAY_BRUSH = 1;
            //const int GRAY_BRUSH = 2;
            //const int DKGRAY_BRUSH = 3;
            //const int BLACK_BRUSH = 4;
            //cls.hbrBackground = GetStockObject(WHITE_BRUSH);
            if (Functions.RegisterClassW(cls) == 0)
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
            Functions.UnregisterClassW(PWSTR.From(name), new HINSTANCE { Value = Application.ModuleHandle.Value });
            _classesNames.TryRemove(name);
        }
    }

    public static void OnPaint(HWND hwnd, Action? action = null)
    {
        Functions.BeginPaint(hwnd, out var ps);
        action?.Invoke();
        Functions.EndPaint(hwnd, ps);
    }

    internal static HT NonClientHitTest(HWND hwnd, LPARAM lParam, ref RECT extend)
    {
        var ptMouse = new POINT(lParam.Value.SignedLOWORD(), lParam.Value.SignedHIWORD());
        Functions.GetWindowRect(hwnd, out var rcWindow);

        var rcFrame = new RECT();
        var dpi = Functions.GetDpiForWindow(hwnd);
        Functions.AdjustWindowRectExForDpi(ref rcFrame, WINDOW_STYLE.WS_OVERLAPPEDWINDOW & ~WINDOW_STYLE.WS_CAPTION, false, 0, dpi);

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

    internal static nint SetUserData(HWND hwnd, nint data) => SetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWLP_USERDATA, data);
    internal static nint GetUserData(HWND hwnd) => GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWLP_USERDATA);
    public static nint SetWindowLong(HWND hwnd, WINDOW_LONG_PTR_INDEX nIndex, nint dwNewLong) => nint.Size == 8 ? Functions.SetWindowLongPtrW(hwnd, nIndex, dwNewLong) : Functions.SetWindowLongW(hwnd, nIndex, (int)dwNewLong);
    public static nint GetWindowLong(HWND hwnd, WINDOW_LONG_PTR_INDEX nIndex) => nint.Size == 8 ? Functions.GetWindowLongPtrW(hwnd, nIndex) : Functions.GetWindowLongW(hwnd, nIndex);
    public static string GetWindowText(HWND hwnd)
    {
        using var p = new AllocPwstr(1024);
        Functions.GetWindowTextW(hwnd, p, (int)p.SizeInChars);
        return p.ToString() ?? string.Empty;
    }

    internal static string? GetClassName(HWND hwnd)
    {
        using var p = new AllocPwstr(1024);
        Functions.GetClassNameW(hwnd, p, (int)p.SizeInChars);
        return p.ToString();
    }

    public static RECT AdjustWindowRect(RECT rect, WINDOW_STYLE style, bool hasMenu, WINDOW_EX_STYLE extendedStyle, uint dpi)
    {
        var rc = rect;
        Functions.AdjustWindowRectExForDpi(ref rc, style, hasMenu, extendedStyle, dpi);
        return rc;
    }

    public unsafe bool Center(HWND alternateOwner)
    {
        var style = (WINDOW_STYLE)GetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_STYLE).ToInt64();
        var hwndCenter = alternateOwner;
        if (alternateOwner.Value == 0)
        {
            if (style.HasFlag(WINDOW_STYLE.WS_CHILD))
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
        RECT area;
        RECT center;
        if (!style.HasFlag(WINDOW_STYLE.WS_CHILD))
        {
            // don't center against invisible or minimized windows
            if (hwndCenter.Value != 0)
            {
                var alternateStyle = (WINDOW_STYLE)GetWindowLong(hwndCenter, WINDOW_LONG_PTR_INDEX.GWL_STYLE).ToInt64();
                if (!alternateStyle.HasFlag(WINDOW_STYLE.WS_VISIBLE) || alternateStyle.HasFlag(WINDOW_STYLE.WS_MINIMIZE))
                {
                    hwndCenter = HWND.Null;
                }
            }

            var mi = new MONITORINFO
            {
                cbSize = (uint)sizeof(MONITORINFO)
            };

            // center within appropriate monitor coordinates
            if (hwndCenter.Value == 0)
            {
                var hwDefault = Handle;
                Functions.GetMonitorInfoW(Functions.MonitorFromWindow(hwDefault, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY), ref mi);
                center = mi.rcWork;
                area = mi.rcWork;
            }
            else
            {
                center = WindowRect;
                Functions.GetMonitorInfoW(Functions.MonitorFromWindow(hwndCenter, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST), ref mi);
                area = mi.rcWork;
            }
        }
        else
        {
            // center within parent client coordinates
            var hWndParent = ParentHandle;
            area = ClientRect;
            center = ClientRect;
            var pts = Unsafe.AsRef<POINT[]>(&center);
            Functions.MapWindowPoints(hwndCenter, hWndParent, pts, 2);
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
    public static unsafe TITLEBARINFOEX GetTITLEBARINFOEX(HWND hwnd)
    {
        var info = new TITLEBARINFOEX { cbSize = (uint)sizeof(TITLEBARINFOEX) };
        if (hwnd.Value == 0)
            return info;

        Functions.SendMessageW(hwnd, MessageDecoder.WM_GETTITLEBARINFOEX, WPARAM.Null, new LPARAM { Value = (nint)(&info) });
        return info;
    }

    // https://devblogs.microsoft.com/oldnewthing/20101020-00/?p=12493
    public static SIZE? GetIconDimension(HICON handle) => GetIconDimension(handle, out _);
    public static unsafe SIZE? GetIconDimension(HICON handle, out SIZE? hotspot)
    {
        hotspot = null;
        if (handle.Value == 0)
            return null;

        if (!Functions.GetIconInfo(handle, out var ii))
            return null;

        var bmpSize = sizeof(BITMAP);
        var bm = new BITMAP();
        var res = Functions.GetObjectW(new HGDIOBJ { Value = ii.hbmMask.Value }, bmpSize, (nint)(&bm));

        SIZE? size = null;
        if (res == bmpSize)
        {
            size = new SIZE(bm.bmWidth, ii.hbmColor.Value != 0 ? bm.bmHeight : bm.bmHeight / 2);
            hotspot = new SIZE(ii.xHotspot, ii.yHotspot);
        }

        if (ii.hbmMask.Value != 0)
        {
            Functions.DeleteObject(new HGDIOBJ { Value = ii.hbmMask.Value });
        }

        if (ii.hbmColor.Value != 0)
        {
            Functions.DeleteObject(new HGDIOBJ { Value = ii.hbmColor.Value });
        }
        return size;
    }

    public void SetWindowAttribute(DWMWINDOWATTRIBUTE attribute, bool value) => SetWindowAttribute(Handle, attribute, value);
    public unsafe static void SetWindowAttribute(HWND hwnd, DWMWINDOWATTRIBUTE attribute, bool value)
    {
        var i = value ? 1 : 0;
        Functions.DwmSetWindowAttribute(hwnd, (uint)attribute, (nint)(&i), 4).ThrowOnError();
    }

    public void SetNonClientRenderingPolicy(DWMNCRENDERINGPOLICY policy) => SetNonClientRenderingPolicy(Handle, policy);
    public unsafe static void SetNonClientRenderingPolicy(HWND hwnd, DWMNCRENDERINGPOLICY policy)
    {
        var i = (int)policy;
        Functions.DwmSetWindowAttribute(hwnd, (uint)DWMWINDOWATTRIBUTE.DWMWA_NCRENDERING_POLICY, (nint)(&i), 4).ThrowOnError();
    }

    //public void EnableBlurBehindWindow(bool enable = true) => EnableBlurBehindWindow(Handle, enable);
    //public static void EnableBlurBehindWindow(nint hwnd, bool enable = true)
    //{
    //    var bb = new DWM_BLURBEHIND();
    //    bb.dwFlags = DWM_BB.DWM_BB_ENABLE;
    //    bb.fEnable = enable;
    //    DwmEnableBlurBehindWindow(hwnd, ref bb).ThrowOnError();
    //}

    public void EnableAcrylicBlurBehind() => EnableAcrylicBlurBehind(Handle);
    public unsafe static void EnableAcrylicBlurBehind(HWND hwnd)
    {
        var accent = new ACCENT_POLICY
        {
            AccentState = ACCENT_STATE.ACCENT_ENABLE_ACRYLICBLURBEHIND
        };

        //accent.GradientColor = 0xFF00FF00;
        var data = new WINDOWCOMPOSITIONATTRIBDATA
        {
            dwAttrib = WINDOWCOMPOSITIONATTRIB.WCA_ACCENT_POLICY,
            cbData = sizeof(ACCENT_POLICY),
            pvData = (nint)(&accent)
        };
        Functions.SetWindowCompositionAttribute(hwnd, ref data).ThrowOnError();
    }

    public void EnableBlurBehind() => EnableBlurBehind(Handle);
    public unsafe static void EnableBlurBehind(HWND hwnd)
    {
        var accent = new ACCENT_POLICY
        {
            AccentState = ACCENT_STATE.ACCENT_ENABLE_BLURBEHIND
        };

        //accent.GradientColor = 0xFF00FF00;
        var data = new WINDOWCOMPOSITIONATTRIBDATA
        {
            dwAttrib = WINDOWCOMPOSITIONATTRIB.WCA_ACCENT_POLICY,
            cbData = sizeof(ACCENT_POLICY),
            pvData = (nint)(&accent)
        };
        Functions.SetWindowCompositionAttribute(hwnd, ref data).ThrowOnError();
    }

    private delegate bool EnumWindowsProc(IntPtr handle, IntPtr lParam);
    private delegate bool EnumChildProc(IntPtr handle, IntPtr lParam);

    public static IReadOnlyList<HWND> EnumerateChildWindows(HWND handle)
    {
        var list = new List<HWND>();
        Functions.EnumChildWindows(handle, (h, l) =>
        {
            list.Add(h);
            return true;
        }, LPARAM.Null);
        return list.AsReadOnly();
    }

    public static IReadOnlyList<HWND> EnumerateTopLevelWindows()
    {
        var list = new List<HWND>();
        Functions.EnumWindows((h, l) =>
        {
            list.Add(h);
            return true;
        }, LPARAM.Null);
        return list.AsReadOnly();
    }
}