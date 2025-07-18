namespace Wice;

#if !NETFRAMEWORK
[System.Runtime.InteropServices.Marshalling.GeneratedComClass]
#endif
public sealed partial class NativeWindow : IEquatable<NativeWindow>, IDropTarget, IDropSource, IDropSourceNotify
#if NETFRAMEWORK
    , System.Windows.Forms.IWin32Window
#endif
{
    private static readonly ConcurrentHashSet<string> _classesNames = [];

    public static WNDPROC DefWindowProc { get; } = GetDefWindowProc();
    private static WNDPROC GetDefWindowProc() => Marshal.GetDelegateForFunctionPointer<WNDPROC>(WiceCommons.GetProcAddress(WiceCommons.GetModuleHandleW(PWSTR.From("user32.dll")), PSTR.From("DefWindowProcW")));

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

#if NETFRAMEWORK
    IntPtr System.Windows.Forms.IWin32Window.Handle => Handle;
#endif

    public HWND Handle { get; }
    public string? ClassName => GetClassName(Handle);
    public HWND ParentHandle { get => WiceCommons.GetParent(Handle); set => WiceCommons.SetParent(Handle, value); }
    public HWND OwnerHandle => WiceCommons.GetWindow(Handle, GET_WINDOW_CMD.GW_OWNER);
    public NativeWindow? Parent => FromHandle(ParentHandle);
    public NativeWindow? Owner => FromHandle(OwnerHandle);
    public RECT WindowRect { get { WiceCommons.GetWindowRect(Handle, out var rc); return rc; } set => WiceCommons.SetWindowPos(Handle, HWND.Null, value.left, value.top, value.Width, value.Height, SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_NOREDRAW | SET_WINDOW_POS_FLAGS.SWP_NOZORDER); }
    public RECT ClientRect { get { WiceCommons.GetClientRect(Handle, out var rc); return rc; } }
    public HICON IconHandle { get => new() { Value = WiceCommons.SendMessageW(Handle, MessageDecoder.WM_GETICON, new WPARAM { Value = WiceCommons.ICON_BIG }, LPARAM.Null).Value }; set { var ptr = WiceCommons.SendMessageW(Handle, MessageDecoder.WM_SETICON, new WPARAM { Value = WiceCommons.ICON_BIG }, new LPARAM { Value = value.Value }); if (ptr.Value != 0) { WiceCommons.DestroyIcon(new HICON { Value = ptr.Value }); } } }
    public uint Dpi { get { var dpi = WiceCommons.GetDpiForWindow(Handle); if (dpi <= 0) return WiceCommons.USER_DEFAULT_SCREEN_DPI; return dpi; } }
    public DPI_AWARENESS_CONTEXT DpiAwareness => DpiUtilities.GetWindowDpiAwarenessContext(Handle);
    public string DpiAwarenessDescription => DpiUtilities.GetDpiAwarenessDescription(DpiAwareness);
    public uint DpiFromDpiAwareness => WiceCommons.GetDpiFromDpiAwarenessContext(DpiAwareness);
    public WINDOW_STYLE Style { get => (WINDOW_STYLE)GetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_STYLE).ToInt64(); set => SetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_STYLE, new IntPtr((int)value)); }
    public WINDOW_EX_STYLE ExtendedStyle { get => (WINDOW_EX_STYLE)GetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE).ToInt64(); set => SetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, new IntPtr((int)value)); }
#if !NETFRAMEWORK
    public WINDOWPLACEMENT Placement { get { return WINDOWPLACEMENT.GetPlacement(Handle); } set => value.SetPlacement(Handle); }
#endif
    public bool IsEnabled { get => WiceCommons.IsWindowEnabled(Handle); set => WiceCommons.EnableWindow(Handle, value); }
    public uint ThreadId => WiceCommons.GetWindowThreadProcessId(Handle, 0);
    public int ManagedThreadId { get; internal set; }
    public unsafe int ProcessId
    {
        get
        {
            int pid;
            _ = WiceCommons.GetWindowThreadProcessId(Handle, (nint)(&pid));
            return pid;
        }
    }

    public string Text
    {
        get => GetWindowText(Handle);
        set
        {
            value ??= string.Empty;
            WiceCommons.SetWindowTextW(Handle, PWSTR.From(value));
        }
    }

    public string? ModuleFileName
    {
        get
        {
#if NETFRAMEWORK
            return WindowsFunctions.GetWindowModuleFileName(Handle);
#else
            using var name = new AllocPwstr(1024);
            _ = Functions.GetWindowModuleFileNameW(Handle, name, name.SizeInChars);
            return name.ToString();
#endif
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
            WiceCommons.GetWindowDisplayAffinity(Handle, out var affinity);
            return (WINDOW_DISPLAY_AFFINITY)affinity;
        }
        set => WiceCommons.SetWindowDisplayAffinity(Handle, value);
    }

    public unsafe DWM_CLOAKED CloakedState
    {
        get
        {
            var state = 0;
            WiceCommons.DwmGetWindowAttribute(Handle, (uint)DWMWINDOWATTRIBUTE.DWMWA_CLOAKED, (nint)(&state), sizeof(int));
            return (DWM_CLOAKED)state;
        }
    }

    public unsafe RECT ExtendedFrameBounds
    {
        get
        {
            var bounds = new RECT();
            WiceCommons.DwmGetWindowAttribute(Handle, (uint)DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, (nint)(&bounds), (uint)sizeof(RECT));
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
                var value = WiceCommons.GetPropW(h, p1);
                var s = p1.ToString();
                if (s != null)
                {
                    dic[s] = value;
                }
                return true;
            }
            WiceCommons.EnumPropsW(Handle, enumProc);
            return dic;
        }
    }

    public IReadOnlyDictionary<PROPERTYKEY, object?> Properties
    {
        get
        {
            var dic = new Dictionary<PROPERTYKEY, object?>();
            WiceCommons.SHGetPropertyStoreForWindow(Handle, typeof(IPropertyStore).GUID, out var unk);
            using var ps = WiceCommons.GetComObjectFromPointer<IPropertyStore>(unk);
            if (ps != null)
            {
                ps.Object.GetCount(out var count);
                for (uint i = 0; i < count; i++)
                {
                    var hr = ps.Object.GetAt(i, out var pk);
                    if (hr.IsError)
                        continue;

#if NETFRAMEWORK
                    using var pv = new PROPVARIANT();
                    hr = ps.Object.GetValue(ref pk, pv);
                    if (hr == 0)
                    {
                        dic[pk] = pv.Value;
                    }
#else
                    hr = ps.Object.GetValue(pk, out var pv);
                    if (hr == 0)
                    {
                        using var managed = PropVariant.Attach(ref pv);
                        dic[pk] = managed.Value;
                    }
#endif
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
                WiceCommons.OleInitialize(0); // don't check error
                var hr = WiceCommons.RegisterDragDrop(Handle, this);
                if (hr.IsError && hr != WiceCommons.DRAGDROP_E_ALREADYREGISTERED)
                    throw new WiceException("0027: Cannot enable drag & drop operations. Make sure the thread is initialized as an STA thread.", Marshal.GetExceptionForHR((int)hr)!);

                _isDropTarget = true;
            }
            else
            {
                var hr = WiceCommons.RevokeDragDrop(Handle);
                hr.ThrowOnErrorExcept(WiceCommons.DRAGDROP_E_NOTREGISTERED);
                _isDropTarget = false;
            }
        }
    }

    public override int GetHashCode() => Handle.GetHashCode();
    public override bool Equals(object? obj) => Equals(obj as NativeWindow);
    public bool Equals(NativeWindow? other) => other != null && Handle.Value == other.Handle.Value;
    public bool IsZoomed() => WiceCommons.IsZoomed(Handle);
    public bool Destroy() => WiceCommons.DestroyWindow(Handle);
    public bool Show(SHOW_WINDOW_CMD command = SHOW_WINDOW_CMD.SW_SHOW) => WiceCommons.ShowWindow(Handle, command);
    public bool Move(int x, int y) => WiceCommons.SetWindowPos(Handle, HWND.Null, x, y, -1, -1, SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);
    public bool Resize(int width, int height) => WiceCommons.SetWindowPos(Handle, HWND.Null, 0, 0, width, height, SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);
    public bool MoveAndResize(D2D_RECT_F rect, HWND hWndInsertAfter, SET_WINDOW_POS_FLAGS flags) => WiceCommons.SetWindowPos(Handle, hWndInsertAfter, (int)rect.left, (int)rect.top, (int)rect.Width, (int)rect.Height, flags);
    public bool MoveAndResize(int x, int y, int width, int height, HWND hWndInsertAfter, SET_WINDOW_POS_FLAGS flags) => WiceCommons.SetWindowPos(Handle, hWndInsertAfter, x, y, width, height, flags);
    public bool MoveAndResize(int x, int y, int width, int height) => WiceCommons.SetWindowPos(Handle, HWND.Null, x, y, width, height, SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);
#if NETFRAMEWORK
    public bool MoveAndResize(D2D_RECT_U rect) => MoveAndResize((int)rect.left, (int)rect.top, (int)(rect.right - rect.left), (int)(rect.bottom - rect.top));
    public bool MoveAndResize(D2D_RECT_U rect, HWND hWndInsertAfter, SET_WINDOW_POS_FLAGS flags) => WiceCommons.SetWindowPos(Handle, hWndInsertAfter, (int)rect.left, (int)rect.top, (int)(rect.right - rect.left), (int)(rect.bottom - rect.top), flags);
#else
    public bool MoveAndResize(D2D_RECT_U rect) => MoveAndResize((int)rect.left, (int)rect.top, (int)rect.Width, (int)rect.Height);
    public bool MoveAndResize(D2D_RECT_U rect, HWND hWndInsertAfter, SET_WINDOW_POS_FLAGS flags) => WiceCommons.SetWindowPos(Handle, hWndInsertAfter, (int)rect.left, (int)rect.top, (int)rect.Width, (int)rect.Height, flags);
    public bool BringWindowToTop() => Functions.BringWindowToTop(Handle);
#endif
    public bool MoveAndResize(D2D_RECT_F rect) => MoveAndResize((int)rect.left, (int)rect.top, (int)rect.Width, (int)rect.Height);
    public bool MoveAndResize(RECT rect) => MoveAndResize(rect.left, rect.top, rect.Width, rect.Height);
    public bool FrameChanged() => WiceCommons.SetWindowPos(Handle, HWND.Null, 0, 0, 0, 0, SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);
    public bool Center() => Center(HWND.Null);
    public bool SetForeground() => WiceCommons.SetForegroundWindow(Handle);
    public nint GetWindowLong(WINDOW_LONG_PTR_INDEX index) => GetWindowLong(Handle, index);
    public nint SetWindowLong(WINDOW_LONG_PTR_INDEX index, nint data) => SetWindowLong(Handle, index, data);
    public nint GetUserData() => GetUserData(Handle);
    public nint SetUserData(nint data) => SetUserData(Handle, data);
    public LRESULT SendMessage(uint msg) => WiceCommons.SendMessageW(Handle, msg, WPARAM.Null, LPARAM.Null);
    public LRESULT SendMessage(uint msg, WPARAM wParam) => WiceCommons.SendMessageW(Handle, msg, wParam, LPARAM.Null);
    public LRESULT SendMessage(uint msg, WPARAM wParam, LPARAM lParam) => WiceCommons.SendMessageW(Handle, msg, wParam, lParam);
    public bool PostMessage(uint msg) => WiceCommons.PostMessageW(Handle, msg, WPARAM.Null, LPARAM.Null);
    public bool PostMessage(uint msg, WPARAM wParam) => WiceCommons.PostMessageW(Handle, msg, wParam, LPARAM.Null);
    public bool PostMessage(uint msg, WPARAM wParam, LPARAM lParam) => WiceCommons.PostMessageW(Handle, msg, wParam, lParam);
    public HMONITOR GetMonitorHandle(MONITOR_FROM_FLAGS flags) => WiceCommons.MonitorFromWindow(Handle, flags);
    public HRESULT ExtendFrameIntoClientArea(int left, int right, int top, int bottom) => DwmExtendFrameIntoClientArea(Handle, left, right, top, bottom);
    public HWND CaptureMouse() => WiceCommons.SetCapture(Handle);
    public bool CreateCaret(int width, int height) => CreateCaret(HBITMAP.Null, width, height);
    public bool CreateCaret(HBITMAP bitmap, int width, int height) => WiceCommons.CreateCaret(Handle, bitmap, width, height);
    public static bool SetCaretPosition(int x, int y) => WiceCommons.SetCaretPos(x, y);
    public POINT ScreenToClient(POINT pt) { WiceCommons.ScreenToClient(Handle, ref pt); return pt; }
    public POINT ClientToScreen(POINT pt) { WiceCommons.ClientToScreen(Handle, ref pt); return pt; }
    public POINT GetClientCursorPosition() => ScreenToClient(GetCursorPosition());
    public Monitor? GetMonitor(MONITOR_FROM_FLAGS flags = MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONULL) => WiceCommons.GetMonitorFromWindow(Handle, flags);
    public bool IsChild(HWND parentHandle) => WiceCommons.IsChild(parentHandle, Handle);
    public bool IsVisible() => WiceCommons.IsWindowVisible(Handle);
    public bool UpdateWindow() => WiceCommons.UpdateWindow(Handle);
    public unsafe bool InvalidateRect(RECT? rc = null, bool erase = false)
    {
        if (rc == null)
            return WiceCommons.InvalidateRect(Handle, 0, erase);

        var r = rc.Value;
        return WiceCommons.InvalidateRect(Handle, (nint)(&r), erase);
    }

    public unsafe bool ValidateRect(RECT? rc = null)
    {
        if (rc == null)
            return WiceCommons.ValidateRect(Handle, 0);

        var r = rc.Value;
        return WiceCommons.ValidateRect(Handle, (nint)(&r));
    }

    public unsafe bool RedrawWindow(RECT? updateRc = null, REDRAW_WINDOW_FLAGS flags = 0) => RedrawWindow(HRGN.Null, updateRc, flags);
    public unsafe bool RedrawWindow(HRGN region, RECT? updateRc = null, REDRAW_WINDOW_FLAGS flags = 0)
    {
        if (updateRc == null)
            return WiceCommons.RedrawWindow(Handle, 0, region, flags);

        var r = updateRc.Value;
        return WiceCommons.RedrawWindow(Handle, (nint)(&r), region, flags);
    }

    public bool IsRunningAsMainThread => ManagedThreadId == Environment.CurrentManagedThreadId;
    public void CheckRunningAsMainThread() { if (!IsRunningAsMainThread) throw new WiceException("0029: This method must be called on the UI thread."); }

    public int ShellAbout(string? text = null, string? otherStuff = null)
    {
        var txt = text.Nullify() ?? Assembly.GetEntryAssembly()?.GetTitle();
        return WiceCommons.ShellAboutW(Handle, PWSTR.From(txt), PWSTR.From(otherStuff), IconHandle);
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
        return WiceCommons.S_OK;
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
        return WiceCommons.S_OK;
    }

    HRESULT IDropTarget.DragLeave()
    {
        var e = new DragDropEventArgs(DragDropEventType.Leave);
        OnDragDrop(e);
        return WiceCommons.S_OK;
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
        return WiceCommons.S_OK;
    }

    HRESULT IDropSourceNotify.DragEnterTarget(HWND hwndTarget)
    {
        var win = new NativeWindow(hwndTarget);
        Application.Trace("DragEnterTarget hwndTarget:" + win);

        var e = new DragDropTargetEventArgs(DragDropTargetEventType.Enter, hwndTarget);
        _dragDropTarget = hwndTarget;
        DragDropTarget?.Invoke(this, e);
        return WiceCommons.S_OK;
    }

    HRESULT IDropSourceNotify.DragLeaveTarget()
    {
        Application.Trace("DragLeaveTarget");
        var e = new DragDropTargetEventArgs(DragDropTargetEventType.Leave, _dragDropTarget);
        DragDropTarget?.Invoke(this, e);
        return WiceCommons.S_OK;
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
            e.Result = WiceCommons.DRAGDROP_S_CANCEL;
        }
        else if (mouseButtons == 0)
        {
            e.Result = WiceCommons.DRAGDROP_S_DROP;
        }

        DragDropQueryContinue?.Invoke(this, e);
        return e.Result;
    }

    HRESULT IDropSource.GiveFeedback(DROPEFFECT effect)
    {
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

    public static HWND ConsoleHandle => WiceCommons.GetConsoleWindow();
    public static NativeWindow? Console => FromHandle(ConsoleHandle);

    public static HWND ActiveHandle => WiceCommons.GetActiveWindow();
    public static NativeWindow? Active => FromHandle(ActiveHandle);

    public static HWND ForegroundHandle => WiceCommons.GetForegroundWindow();
    public static NativeWindow? Foreground => FromHandle(ForegroundHandle);

    public static HWND DesktopHandle => WiceCommons.GetDesktopWindow();
    public static NativeWindow? Desktop => FromHandle(DesktopHandle);

    public static HWND ShellHandle => WiceCommons.GetShellWindow();
    public static NativeWindow? Shell => FromHandle(ShellHandle);

    public static NativeWindow? FromHandle(HWND handle) { if (handle.Value == 0) return null; return new NativeWindow(handle); }
    public static bool IsChildWindow(HWND parentHandle, HWND handle) => WiceCommons.IsChild(parentHandle, handle);
    public static HWND GetWindowFromPoint(POINT point) => WiceCommons.WindowFromPoint(point);
    public static char VirtualKeyToCharacter(VIRTUAL_KEY vk) => (char)WiceCommons.MapVirtualKeyW((uint)vk, MAP_VIRTUAL_KEY_TYPE.MAPVK_VK_TO_CHAR);
    public static bool IsKeyPressed(VIRTUAL_KEY vk, bool async = true) => (async ? WiceCommons.GetAsyncKeyState((int)vk) : WiceCommons.GetKeyState((int)vk)) < 0;
    public static bool ReleaseMouse() => WiceCommons.ReleaseCapture();
    public static POINT GetCursorPosition() { WiceCommons.GetCursorPos(out var pt); return pt; }

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
            var pos = WiceCommons.GetMessagePos();
            return new POINT(pos.SignedLOWORD(), pos.SignedHIWORD());
        }
    }

    // https://stackoverflow.com/a/51037982/403671
    internal static bool DidMouseLeaveWindow(HWND handle, HWND other)
    {
        var pt = LastMessagePosision;
        var win = WiceCommons.WindowFromPoint(pt);
        var child = WiceCommons.IsChild(handle, win);
        var result = !child && (win.Value != other.Value || other.Value == 0);
        return result;
    }

    internal static HRESULT DwmExtendFrameIntoClientArea(HWND hwnd, int left, int right, int top, int bottom)
    {
        var margin = new MARGINS
        {
            cxLeftWidth = left,
            cxRightWidth = right,
            cyBottomHeight = bottom,
            cyTopHeight = top
        };
        return WiceCommons.DwmExtendFrameIntoClientArea(hwnd, margin);
    }

    internal static bool RegisterWindowClass(string className, WNDCLASS_STYLES styles, nint windowProc, HBRUSH background)
    {
        ExceptionExtensions.ThrowIfNull(className, nameof(className));
        if (windowProc == 0)
            throw new ArgumentException(null, nameof(windowProc));

        if (!WiceCommons.GetClassInfoW(new HINSTANCE { Value = Application.ModuleHandle.Value }, PWSTR.From(className), out _))
        {
            var cls = new WNDCLASSW
            {
                style = styles,
                lpfnWndProc = windowProc,
                hInstance = new HINSTANCE { Value = Application.ModuleHandle.Value },
                lpszClassName = PWSTR.From(className),
                //cls.hCursor = DirectN.Cursor.Arrow.Handle; // we set the cursor ourselves, otherwise the cursor will blink
                hbrBackground = background
            };
            if (WiceCommons.RegisterClassW(cls) == 0)
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
            WiceCommons.UnregisterClassW(PWSTR.From(name), new HINSTANCE { Value = Application.ModuleHandle.Value });
            _classesNames.TryRemove(name);
        }
    }

    public static void OnPaint(HWND hwnd, Action? action = null)
    {
        WiceCommons.BeginPaint(hwnd, out var ps);
        try
        {
            action?.Invoke();
        }
        finally
        {
            WiceCommons.EndPaint(hwnd, ps);
        }
    }

    internal static HT NonClientHitTest(HWND hwnd, LPARAM lParam, ref RECT extend)
    {
        var ptMouse = new POINT(lParam.Value.SignedLOWORD(), lParam.Value.SignedHIWORD());
        WiceCommons.GetWindowRect(hwnd, out var rcWindow);

        var rcFrame = new RECT();
        var dpi = WiceCommons.GetDpiForWindow(hwnd);
        WiceCommons.AdjustWindowRectExForDpi(ref rcFrame, WINDOW_STYLE.WS_OVERLAPPEDWINDOW & ~WINDOW_STYLE.WS_CAPTION, false, 0, dpi);

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

    public static nint SetWindowLong(HWND hwnd, WINDOW_LONG_PTR_INDEX nIndex, nint dwNewLong)
#if NETFRAMEWORK
        => IntPtr.Size == 8 ? WindowsFunctions.SetWindowLongPtr64(hwnd, (int)nIndex, dwNewLong) : WindowsFunctions.SetWindowLongPtr32(hwnd, (int)nIndex, dwNewLong);
#else
        => nint.Size == 8 ? Functions.SetWindowLongPtrW(hwnd, nIndex, dwNewLong) : Functions.SetWindowLongW(hwnd, nIndex, (int)dwNewLong);
#endif

    public static nint GetWindowLong(HWND hwnd, WINDOW_LONG_PTR_INDEX nIndex)
#if NETFRAMEWORK
        => IntPtr.Size == 8 ? WindowsFunctions.GetWindowLongPtr64(hwnd, (int)nIndex) : WindowsFunctions.GetWindowLong32(hwnd, (int)nIndex);
#else
        => nint.Size == 8 ? Functions.GetWindowLongPtrW(hwnd, nIndex) : Functions.GetWindowLongW(hwnd, nIndex);
#endif

    public static string GetWindowText(HWND hwnd)
    {
#if NETFRAMEWORK
        var sb = new StringBuilder(256);
        if (WindowsFunctions.GetWindowText(hwnd, sb, sb.Capacity - 1) == 0)
            return string.Empty;

        return sb.ToString();
#else
        using var p = new AllocPwstr(1024);
        if (Functions.GetWindowTextW(hwnd, p, (int)p.SizeInChars) == 0)
            return string.Empty;

        return p.ToString() ?? string.Empty;
#endif
    }

    internal static string? GetClassName(HWND hwnd)
    {
#if NETFRAMEWORK
        var sb = new StringBuilder(256);
        if (WindowsFunctions.GetClassName(hwnd, sb, sb.Capacity - 1) == 0)
            return null;

        return sb.ToString();
#else
        using var p = new AllocPwstr(1024);
        if (Functions.GetClassNameW(hwnd, p, (int)p.SizeInChars) == 0)
            return null;

        return p.ToString();
#endif
    }

    public static RECT AdjustWindowRect(RECT rect, WINDOW_STYLE style, bool hasMenu, WINDOW_EX_STYLE extendedStyle, uint dpi)
    {
        var rc = rect;
        WiceCommons.AdjustWindowRectExForDpi(ref rc, style, hasMenu, extendedStyle, dpi);
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
                WiceCommons.GetMonitorInfoW(WiceCommons.MonitorFromWindow(hwDefault, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY), ref mi);
                center = mi.rcWork;
                area = mi.rcWork;
            }
            else
            {
                center = WindowRect;
                WiceCommons.GetMonitorInfoW(WiceCommons.MonitorFromWindow(hwndCenter, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST), ref mi);
                area = mi.rcWork;
            }
        }
        else
        {
            // center within parent client coordinates
            var hWndParent = ParentHandle;
            area = ClientRect;
            center = ClientRect;
#if NETFRAMEWORK
            WindowsFunctions.MapWindowPoints(hwndCenter, hWndParent, ref center, 2);
#else
            var pts = Unsafe.AsRef<POINT[]>(&center);
            Functions.MapWindowPoints(hwndCenter, hWndParent, pts, 2);
#endif
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

        WiceCommons.SendMessageW(hwnd, MessageDecoder.WM_GETTITLEBARINFOEX, WPARAM.Null, new LPARAM { Value = (nint)(&info) });
        return info;
    }

    // https://devblogs.microsoft.com/oldnewthing/20101020-00/?p=12493
    public static SIZE? GetIconDimension(HICON handle) => GetIconDimension(handle, out _);
    public static unsafe SIZE? GetIconDimension(HICON handle, out SIZE? hotspot)
    {
        hotspot = null;
        if (handle.Value == 0)
            return null;

        if (!WiceCommons.GetIconInfo(handle, out var ii))
            return null;

        var bmpSize = sizeof(BITMAP);
        var bm = new BITMAP();
        var res = WiceCommons.GetObjectW(new HGDIOBJ { Value = ii.hbmMask.Value }, bmpSize, (nint)(&bm));

        SIZE? size = null;
        if (res == bmpSize)
        {
            size = new SIZE(bm.bmWidth, ii.hbmColor.Value != 0 ? bm.bmHeight : bm.bmHeight / 2);
            hotspot = new SIZE(ii.xHotspot, ii.yHotspot);
        }

        if (ii.hbmMask.Value != 0)
        {
            WiceCommons.DeleteObject(new HGDIOBJ { Value = ii.hbmMask.Value });
        }

        if (ii.hbmColor.Value != 0)
        {
            WiceCommons.DeleteObject(new HGDIOBJ { Value = ii.hbmColor.Value });
        }
        return size;
    }

    public void SetWindowAttribute(DWMWINDOWATTRIBUTE attribute, bool value) => SetWindowAttribute(Handle, attribute, value);
    public unsafe static void SetWindowAttribute(HWND hwnd, DWMWINDOWATTRIBUTE attribute, bool value)
    {
        var i = value ? 1 : 0;
        WiceCommons.DwmSetWindowAttribute(hwnd, (uint)attribute, (nint)(&i), 4).ThrowOnError();
    }

    public void SetNonClientRenderingPolicy(DWMNCRENDERINGPOLICY policy) => SetNonClientRenderingPolicy(Handle, policy);
    public unsafe static void SetNonClientRenderingPolicy(HWND hwnd, DWMNCRENDERINGPOLICY policy)
    {
        var i = (int)policy;
        WiceCommons.DwmSetWindowAttribute(hwnd, (uint)DWMWINDOWATTRIBUTE.DWMWA_NCRENDERING_POLICY, (nint)(&i), 4).ThrowOnError();
    }

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
        WiceCommons.SetWindowCompositionAttribute(hwnd, ref data).ThrowOnError();
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
        WiceCommons.SetWindowCompositionAttribute(hwnd, ref data).ThrowOnError();
    }

    private delegate bool EnumWindowsProc(IntPtr handle, IntPtr lParam);
    private delegate bool EnumChildProc(IntPtr handle, IntPtr lParam);

    public static IReadOnlyList<HWND> EnumerateChildWindows(HWND handle)
    {
        var list = new List<HWND>();
        WiceCommons.EnumChildWindows(handle, (h, l) =>
        {
            list.Add(h);
            return true;
        }, LPARAM.Null);
        return list.AsReadOnly();
    }

    public static IReadOnlyList<HWND> EnumerateTopLevelWindows()
    {
        var list = new List<HWND>();
        WiceCommons.EnumWindows((h, l) =>
        {
            list.Add(h);
            return true;
        }, LPARAM.Null);
        return list.AsReadOnly();
    }
}