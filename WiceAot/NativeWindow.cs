namespace Wice;

/// <summary>
/// Provides a thin, safe wrapper around a Win32 window handle (HWND) and common window operations.
/// </summary>
#if !NETFRAMEWORK
[System.Runtime.InteropServices.Marshalling.GeneratedComClass]
#endif
public sealed partial class NativeWindow : IEquatable<NativeWindow>, IDropTarget, IDropSource, IDropSourceNotify
#if NETFRAMEWORK
    , System.Windows.Forms.IWin32Window
#endif
{
    private static readonly ConcurrentHashSet<string> _classesNames = [];

    /// <summary>
    /// Gets a delegate to the default window procedure (DefWindowProcW).
    /// </summary>
    public static WNDPROC DefWindowProc { get; } = GetDefWindowProc();

    private static WNDPROC GetDefWindowProc() => Marshal.GetDelegateForFunctionPointer<WNDPROC>(WiceCommons.GetProcAddress(WiceCommons.GetModuleHandleW(PWSTR.From("user32.dll")), PSTR.From("DefWindowProcW")));

    /// <summary>
    /// Enumerates all top-level windows and returns a sequence of <see cref="NativeWindow"/> objects.
    /// </summary>
    public static IEnumerable<NativeWindow> TopLevelWindows => EnumerateTopLevelWindows().Select(FromHandle).Where(w => w != null)!;

    /// <summary>
    /// Occurs for IDropTarget drag-and-drop notifications (enter/over/leave/drop).
    /// </summary>
    public event EventHandler<DragDropEventArgs>? DragDrop;

    /// <summary>
    /// Occurs for IDropSource query-continue to decide continue/cancel/drop.
    /// </summary>
    public event EventHandler<DragDropQueryContinueEventArgs>? DragDropQueryContinue;

    /// <summary>
    /// Occurs for IDropSource feedback, allowing custom cursor behavior/results.
    /// </summary>
    public event EventHandler<DragDropGiveFeedback>? DragDropGiveFeedback;

    /// <summary>
    /// Occurs when entering or leaving a drag target while acting as a drop source.
    /// </summary>
    public event EventHandler<DragDropTargetEventArgs>? DragDropTarget;

    private HWND _dragDropTarget;
    private bool _isDropTarget;
    private IDataObject? _currentDataObject;

    private NativeWindow(HWND handle)
    {
        Handle = handle;
    }

#if NETFRAMEWORK
    /// <inheritdoc />
    IntPtr System.Windows.Forms.IWin32Window.Handle => Handle;
#endif

    /// <summary>
    /// Gets the underlying native window handle.
    /// </summary>
    public HWND Handle { get; }

    /// <summary>
    /// Gets the native window class name for this window, if available.
    /// </summary>
    public string? ClassName => GetClassName(Handle);

    /// <summary>
    /// Gets or sets the parent window handle (SetParent/GetParent).
    /// </summary>
    public HWND ParentHandle { get => WiceCommons.GetParent(Handle); set => WiceCommons.SetParent(Handle, value); }

    /// <summary>
    /// Gets the owner window handle (GW_OWNER).
    /// </summary>
    public HWND OwnerHandle => WiceCommons.GetWindow(Handle, GET_WINDOW_CMD.GW_OWNER);

    /// <summary>
    /// Gets the parent window as a <see cref="NativeWindow"/> instance, if any.
    /// </summary>
    public NativeWindow? Parent => FromHandle(ParentHandle);

    /// <summary>
    /// Gets the owner window as a <see cref="NativeWindow"/> instance, if any.
    /// </summary>
    public NativeWindow? Owner => FromHandle(OwnerHandle);

    /// <summary>
    /// Gets or sets the window rectangle in screen coordinates.
    /// </summary>
    public RECT WindowRect { get { WiceCommons.GetWindowRect(Handle, out var rc); return rc; } set => WiceCommons.SetWindowPos(Handle, HWND.Null, value.left, value.top, value.Width, value.Height, SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_NOREDRAW | SET_WINDOW_POS_FLAGS.SWP_NOZORDER); }

    /// <summary>
    /// Gets the client rectangle in client coordinates.
    /// </summary>
    public RECT ClientRect { get { WiceCommons.GetClientRect(Handle, out var rc); return rc; } }

    /// <summary>
    /// Gets or sets the big icon (WM_GETICON/WM_SETICON). Disposes previous icon if replaced.
    /// </summary>
    public HICON IconHandle { get => new() { Value = WiceCommons.SendMessageW(Handle, MessageDecoder.WM_GETICON, new WPARAM { Value = WiceCommons.ICON_BIG }, LPARAM.Null).Value }; set { var ptr = WiceCommons.SendMessageW(Handle, MessageDecoder.WM_SETICON, new WPARAM { Value = WiceCommons.ICON_BIG }, new LPARAM { Value = value.Value }); if (ptr.Value != 0) { WiceCommons.DestroyIcon(new HICON { Value = ptr.Value }); } } }

    /// <summary>
    /// Gets the current DPI for the window or the system default if unavailable.
    /// </summary>
    public uint Dpi { get { var dpi = WiceCommons.GetDpiForWindow(Handle); if (dpi <= 0) return WiceCommons.USER_DEFAULT_SCREEN_DPI; return dpi; } }

    /// <summary>
    /// Gets the DPI awareness context of the window.
    /// </summary>
    public DPI_AWARENESS_CONTEXT DpiAwareness => DpiUtilities.GetWindowDpiAwarenessContext(Handle);

    /// <summary>
    /// Gets a descriptive string for the current <see cref="DpiAwareness"/>.
    /// </summary>
    public string DpiAwarenessDescription => DpiUtilities.GetDpiAwarenessDescription(DpiAwareness);

    /// <summary>
    /// Gets the DPI integer associated with the current <see cref="DpiAwareness"/>.
    /// </summary>
    public uint DpiFromDpiAwareness => WiceCommons.GetDpiFromDpiAwarenessContext(DpiAwareness);

    /// <summary>
    /// Gets or sets the window style (GWL_STYLE).
    /// </summary>
    public WINDOW_STYLE Style { get => (WINDOW_STYLE)GetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_STYLE).ToInt64(); set => SetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_STYLE, new IntPtr((int)value)); }

    /// <summary>
    /// Gets or sets the extended window style (GWL_EXSTYLE).
    /// </summary>
    public WINDOW_EX_STYLE ExtendedStyle { get => (WINDOW_EX_STYLE)GetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE).ToInt64(); set => SetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, new IntPtr((int)value)); }

#if !NETFRAMEWORK
    /// <summary>
    /// Gets or sets the window placement (show state, normal/min/max position).
    /// </summary>
    public WINDOWPLACEMENT Placement { get { return WINDOWPLACEMENT.GetPlacement(Handle); } set => value.SetPlacement(Handle); }
#endif

    /// <summary>
    /// Gets or sets whether the window is enabled.
    /// </summary>
    public bool IsEnabled { get => WiceCommons.IsWindowEnabled(Handle); set => WiceCommons.EnableWindow(Handle, value); }

    /// <summary>
    /// Gets the thread ID that created the window.
    /// </summary>
    public uint ThreadId => WiceCommons.GetWindowThreadProcessId(Handle, 0);

    /// <summary>
    /// Gets the associated managed UI thread ID if tracked by the framework.
    /// </summary>
    public int ManagedThreadId { get; internal set; }

    /// <summary>
    /// Gets the process ID for the window.
    /// </summary>
    public unsafe int ProcessId
    {
        get
        {
            int pid;
            _ = WiceCommons.GetWindowThreadProcessId(Handle, (nint)(&pid));
            return pid;
        }
    }

    /// <summary>
    /// Gets or sets the window text (title).
    /// </summary>
    public string Text
    {
        get => GetWindowText(Handle);
        set
        {
            value ??= string.Empty;
            WiceCommons.SetWindowTextW(Handle, PWSTR.From(value));
        }
    }

    /// <summary>
    /// Gets the module file name for the window's process, if available.
    /// </summary>
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

    /// <summary>
    /// Gets the managed <see cref="System.Diagnostics.Process"/> for this window, or null if it cannot be obtained.
    /// </summary>
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

    /// <summary>
    /// Enumerates parent windows from nearest to the root.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the window display affinity (monitoring/capture behavior).
    /// </summary>
    public WINDOW_DISPLAY_AFFINITY DisplayAffinity
    {
        get
        {
            WiceCommons.GetWindowDisplayAffinity(Handle, out var affinity);
            return (WINDOW_DISPLAY_AFFINITY)affinity;
        }
        set => WiceCommons.SetWindowDisplayAffinity(Handle, value);
    }

    /// <summary>
    /// Gets the DWM cloaked state of the window (if it is cloaked/hidden by the system).
    /// </summary>
    public unsafe DWM_CLOAKED CloakedState
    {
        get
        {
            var state = 0;
            WiceCommons.DwmGetWindowAttribute(Handle, (uint)DWMWINDOWATTRIBUTE.DWMWA_CLOAKED, (nint)(&state), sizeof(int));
            return (DWM_CLOAKED)state;
        }
    }

    /// <summary>
    /// Gets the extended frame bounds as reported by DWM.
    /// </summary>
    public unsafe RECT ExtendedFrameBounds
    {
        get
        {
            var bounds = new RECT();
            WiceCommons.DwmGetWindowAttribute(Handle, (uint)DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, (nint)(&bounds), (uint)sizeof(RECT));
            return bounds;
        }
    }

    /// <summary>
    /// Enumerates direct child windows.
    /// </summary>
    public IEnumerable<NativeWindow> ChildWindows => EnumerateChildWindows(Handle).Select(h => FromHandle(h)).Where(w => w != null)!;

    /// <summary>
    /// Enumerates all descendant windows (depth-first).
    /// </summary>
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

    /// <summary>
    /// Gets all Win32 string properties associated with the window (EnumPropsW/GetPropW).
    /// </summary>
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

    /// <summary>
    /// Gets the IPropertyStore values, if available, keyed by PROPERTYKEY.
    /// </summary>
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

    /// <summary>
    /// Gets or sets whether this window is registered as a drop target (IDropTarget).
    /// </summary>
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

    /// <inheritdoc/>
    public override int GetHashCode() => Handle.GetHashCode();

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as NativeWindow);

    /// <inheritdoc/>
    public bool Equals(NativeWindow? other) => other != null && Handle.Value == other.Handle.Value;

    /// <summary>
    /// Returns whether the window is maximized.
    /// </summary>
    public bool IsZoomed() => WiceCommons.IsZoomed(Handle);

    /// <summary>
    /// Returns whether the window is minimized.
    /// </summary>
    public bool IsIconic() => WiceCommons.IsIconic(Handle);

    /// <summary>
    /// Destroys the window (DestroyWindow).
    /// </summary>
    public bool Destroy() => WiceCommons.DestroyWindow(Handle);

    /// <summary>
    /// Shows the window with the specified command (ShowWindow).
    /// </summary>
    public bool Show(SHOW_WINDOW_CMD command = SHOW_WINDOW_CMD.SW_SHOW) => WiceCommons.ShowWindow(Handle, command);

    /// <summary>
    /// Moves the window to the specified screen coordinate without resizing.
    /// </summary>
    public bool Move(int x, int y) => WiceCommons.SetWindowPos(Handle, HWND.Null, x, y, -1, -1, SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);

    /// <summary>
    /// Resizes the window to the specified width and height without moving it.
    /// </summary>
    public bool Resize(int width, int height) => WiceCommons.SetWindowPos(Handle, HWND.Null, 0, 0, width, height, SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);

    /// <summary>
    /// Moves and resizes the window using a floating-point rectangle and flags.
    /// </summary>
    public bool MoveAndResize(D2D_RECT_F rect, HWND hWndInsertAfter, SET_WINDOW_POS_FLAGS flags) => WiceCommons.SetWindowPos(Handle, hWndInsertAfter, (int)rect.left, (int)rect.top, (int)rect.Width, (int)rect.Height, flags);

    /// <summary>
    /// Moves and resizes the window to the specified bounds and z-order with flags.
    /// </summary>
    public bool MoveAndResize(int x, int y, int width, int height, HWND hWndInsertAfter, SET_WINDOW_POS_FLAGS flags) => WiceCommons.SetWindowPos(Handle, hWndInsertAfter, x, y, width, height, flags);

    /// <summary>
    /// Moves and resizes the window to the specified bounds.
    /// </summary>
    public bool MoveAndResize(int x, int y, int width, int height) => WiceCommons.SetWindowPos(Handle, HWND.Null, x, y, width, height, SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);

#if NETFRAMEWORK
    /// <summary>
    /// Moves and resizes using an unsigned rectangle (D2D_RECT_U).
    /// </summary>
    public bool MoveAndResize(D2D_RECT_U rect) => MoveAndResize((int)rect.left, (int)rect.top, (int)(rect.right - rect.left), (int)(rect.bottom - rect.top));

    /// <summary>
    /// Moves and resizes using an unsigned rectangle (D2D_RECT_U) and flags.
    /// </summary>
    public bool MoveAndResize(D2D_RECT_U rect, HWND hWndInsertAfter, SET_WINDOW_POS_FLAGS flags) => WiceCommons.SetWindowPos(Handle, hWndInsertAfter, (int)rect.left, (int)rect.top, (int)(rect.right - rect.left), (int)(rect.bottom - rect.top), flags);
#else
    /// <summary>
    /// Moves and resizes using an unsigned rectangle (D2D_RECT_U).
    /// </summary>
    public bool MoveAndResize(D2D_RECT_U rect) => MoveAndResize((int)rect.left, (int)rect.top, (int)rect.Width, (int)rect.Height);

    /// <summary>
    /// Moves and resizes using an unsigned rectangle (D2D_RECT_U) and flags.
    /// </summary>
    public bool MoveAndResize(D2D_RECT_U rect, HWND hWndInsertAfter, SET_WINDOW_POS_FLAGS flags) => WiceCommons.SetWindowPos(Handle, hWndInsertAfter, (int)rect.left, (int)rect.top, (int)rect.Width, (int)rect.Height, flags);

    /// <summary>
    /// Brings the window to the top of the Z order.
    /// </summary>
    public bool BringWindowToTop() => Functions.BringWindowToTop(Handle);
#endif

    /// <summary>
    /// Moves and resizes using a floating-point rectangle (D2D_RECT_F).
    /// </summary>
    public bool MoveAndResize(D2D_RECT_F rect) => MoveAndResize((int)rect.left, (int)rect.top, (int)rect.Width, (int)rect.Height);

    /// <summary>
    /// Moves and resizes using a RECT.
    /// </summary>
    public bool MoveAndResize(RECT rect) => MoveAndResize(rect.left, rect.top, rect.Width, rect.Height);

    /// <summary>
    /// Notifies that the frame has changed so that the non-client area is recalculated.
    /// </summary>
    public bool FrameChanged() => WiceCommons.SetWindowPos(Handle, HWND.Null, 0, 0, 0, 0, SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);

    /// <summary>
    /// Centers the window relative to its owner or parent.
    /// </summary>
    public bool Center() => Center(HWND.Null);

    /// <summary>
    /// Sets this window to the foreground.
    /// </summary>
    public bool SetForeground() => WiceCommons.SetForegroundWindow(Handle);

    /// <summary>
    /// Gets window long data for this window.
    /// </summary>
    public nint GetWindowLong(WINDOW_LONG_PTR_INDEX index) => GetWindowLong(Handle, index);

    /// <summary>
    /// Sets window long data for this window.
    /// </summary>
    public nint SetWindowLong(WINDOW_LONG_PTR_INDEX index, nint data) => SetWindowLong(Handle, index, data);

    /// <summary>
    /// Gets the GWLP_USERDATA value for this window.
    /// </summary>
    public nint GetUserData() => GetUserData(Handle);

    /// <summary>
    /// Sets the GWLP_USERDATA value for this window.
    /// </summary>
    public nint SetUserData(nint data) => SetUserData(Handle, data);

    /// <summary>
    /// Sends a message with no parameters.
    /// </summary>
    public LRESULT SendMessage(uint msg) => WiceCommons.SendMessageW(Handle, msg, WPARAM.Null, LPARAM.Null);

    /// <summary>
    /// Sends a message with a wParam.
    /// </summary>
    public LRESULT SendMessage(uint msg, WPARAM wParam) => WiceCommons.SendMessageW(Handle, msg, wParam, LPARAM.Null);

    /// <summary>
    /// Sends a message with wParam and lParam.
    /// </summary>
    public LRESULT SendMessage(uint msg, WPARAM wParam, LPARAM lParam) => WiceCommons.SendMessageW(Handle, msg, wParam, lParam);

    /// <summary>
    /// Posts a message with no parameters.
    /// </summary>
    public bool PostMessage(uint msg) => WiceCommons.PostMessageW(Handle, msg, WPARAM.Null, LPARAM.Null);

    /// <summary>
    /// Posts a message with wParam.
    /// </summary>
    public bool PostMessage(uint msg, WPARAM wParam) => WiceCommons.PostMessageW(Handle, msg, wParam, LPARAM.Null);

    /// <summary>
    /// Posts a message with wParam and lParam.
    /// </summary>
    public bool PostMessage(uint msg, WPARAM wParam, LPARAM lParam) => WiceCommons.PostMessageW(Handle, msg, wParam, lParam);

    /// <summary>
    /// Gets a monitor handle for this window using <paramref name="flags"/>.
    /// </summary>
    public HMONITOR GetMonitorHandle(MONITOR_FROM_FLAGS flags) => WiceCommons.MonitorFromWindow(Handle, flags);

    /// <summary>
    /// Extends the DWM glass frame into the client area.
    /// </summary>
    public HRESULT ExtendFrameIntoClientArea(int left, int right, int top, int bottom) => DwmExtendFrameIntoClientArea(Handle, left, right, top, bottom);

    /// <summary>
    /// Captures the mouse input to this window.
    /// </summary>
    public HWND CaptureMouse() => WiceCommons.SetCapture(Handle);

    /// <summary>
    /// Creates a solid caret with the given size.
    /// </summary>
    public bool CreateCaret(int width, int height) => CreateCaret(HBITMAP.Null, width, height);

    /// <summary>
    /// Creates a caret using a bitmap with the given size.
    /// </summary>
    public bool CreateCaret(HBITMAP bitmap, int width, int height) => WiceCommons.CreateCaret(Handle, bitmap, width, height);

    /// <summary>
    /// Sets the caret position in client coordinates.
    /// </summary>
    public static bool SetCaretPosition(int x, int y) => WiceCommons.SetCaretPos(x, y);

    /// <summary>
    /// Converts a screen point to client coordinates.
    /// </summary>
    public POINT ScreenToClient(POINT pt) { WiceCommons.ScreenToClient(Handle, ref pt); return pt; }

    /// <summary>
    /// Converts a client point to screen coordinates.
    /// </summary>
    public POINT ClientToScreen(POINT pt) { WiceCommons.ClientToScreen(Handle, ref pt); return pt; }

    /// <summary>
    /// Gets the cursor position relative to this window's client area.
    /// </summary>
    public POINT GetClientCursorPosition() => ScreenToClient(GetCursorPosition());

    /// <summary>
    /// Gets the monitor information for this window or null based on flags.
    /// </summary>
    public Monitor? GetMonitor(MONITOR_FROM_FLAGS flags = MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONULL) => WiceCommons.GetMonitorFromWindow(Handle, flags);

    /// <summary>
    /// Determines whether this window is a child of the specified parent handle.
    /// </summary>
    public bool IsChild(HWND parentHandle) => WiceCommons.IsChild(parentHandle, Handle);

    /// <summary>
    /// Gets whether the window is visible.
    /// </summary>
    public bool IsVisible() => WiceCommons.IsWindowVisible(Handle);

    /// <summary>
    /// Forces a paint of the window if any update region is present.
    /// </summary>
    public bool UpdateWindow() => WiceCommons.UpdateWindow(Handle);

    /// <summary>
    /// Invalidates a rectangle region (or the entire client area), optionally erasing the background.
    /// </summary>
    public unsafe bool InvalidateRect(RECT? rc = null, bool erase = false)
    {
        if (rc == null)
            return WiceCommons.InvalidateRect(Handle, 0, erase);

        var r = rc.Value;
        return WiceCommons.InvalidateRect(Handle, (nint)(&r), erase);
    }

    /// <summary>
    /// Validates a rectangle region (or the entire client area), removing it from the update region.
    /// </summary>
    public unsafe bool ValidateRect(RECT? rc = null)
    {
        if (rc == null)
            return WiceCommons.ValidateRect(Handle, 0);

        var r = rc.Value;
        return WiceCommons.ValidateRect(Handle, (nint)(&r));
    }

    /// <summary>
    /// Requests a redraw of a region with optional flags and region handle.
    /// </summary>
    public unsafe bool RedrawWindow(RECT? updateRc = null, REDRAW_WINDOW_FLAGS flags = 0) => RedrawWindow(HRGN.Null, updateRc, flags);

    /// <summary>
    /// Requests a redraw of a region with optional flags.
    /// </summary>
    public unsafe bool RedrawWindow(HRGN region, RECT? updateRc = null, REDRAW_WINDOW_FLAGS flags = 0)
    {
        if (updateRc == null)
            return WiceCommons.RedrawWindow(Handle, 0, region, flags);

        var r = updateRc.Value;
        return WiceCommons.RedrawWindow(Handle, (nint)(&r), region, flags);
    }

    /// <summary>
    /// Gets whether the current call is on the managed UI thread tracked by this window.
    /// </summary>
    public bool IsRunningAsMainThread => ManagedThreadId == Environment.CurrentManagedThreadId;

    /// <summary>
    /// Throws if the current thread is not the tracked UI thread for this window.
    /// </summary>
    public void CheckRunningAsMainThread() { if (!IsRunningAsMainThread) throw new WiceException("0029: This method must be called on the UI thread."); }

    /// <summary>
    /// Shows the ShellAbout dialog for this window using optional text and details.
    /// </summary>
    public int ShellAbout(string? text = null, string? otherStuff = null)
    {
        var txt = text.Nullify() ?? Assembly.GetEntryAssembly()?.GetTitle();
        return WiceCommons.ShellAboutW(Handle, PWSTR.From(txt), PWSTR.From(otherStuff), IconHandle);
    }

    /// <summary>
    /// Acquires the IME context and invokes an action with it, ensuring release.
    /// </summary>
    public void WithImmContext(Action<HIMC> action)
    {
        ExceptionExtensions.ThrowIfNull(action, nameof(action));
        var ctx = WiceCommons.ImmGetContext(Handle);
        if (ctx == 0)
            return;

        try
        {
            action(ctx);
        }
        finally
        {
            WiceCommons.ImmReleaseContext(Handle, ctx);
        }
    }

    /// <summary>
    /// Acquires the IME context and invokes a function with it, ensuring release.
    /// </summary>
    public T? WithImmContext<T>(Func<HIMC, T> func)
    {
        ExceptionExtensions.ThrowIfNull(func, nameof(func));
        var ctx = WiceCommons.ImmGetContext(Handle);
        if (ctx == 0)
            return default;

        try
        {
            return func(ctx);
        }
        finally
        {
            WiceCommons.ImmReleaseContext(Handle, ctx);
        }
    }

    /// <summary>
    /// Gets the IME conversion and sentence modes.
    /// </summary>
    public bool GetImmConversionStatus(out IME_CMODE conversion, out IME_SMODE sentence)
    {
        var ret = WithImmContext(ctx =>
        {
            var r = WiceCommons.ImmGetConversionStatus(ctx, out var c, out var s);
            return (r, c, s);
        });
        conversion = ret.c;
        sentence = ret.s;
        return ret.r;
    }

    /// <summary>
    /// Gets the default IME window handle associated with this window.
    /// </summary>
    public HWND GetDefaultIMEWnd() => WiceCommons.ImmGetDefaultIMEWnd(Handle);

    /// <summary>
    /// Gets the IME open status.
    /// </summary>
    public bool GetImmOpenStatus() => WithImmContext(WiceCommons.ImmGetOpenStatus);

    /// <summary>
    /// Sets the IME open status.
    /// </summary>
    public bool SetImmOpenStatus(bool open) => WithImmContext(ctx => WiceCommons.ImmSetOpenStatus(ctx, open));

    /// <summary>
    /// Sends an IME notification.
    /// </summary>
    public bool NotifyImmIME(IME_NI action, IME_CPS index = 0, uint value = 0) => WithImmContext(ctx => WiceCommons.ImmNotifyIME(ctx, action, index, value));

    /// <summary>
    /// Sets the IME conversion and sentence modes.
    /// </summary>
    public bool SetImmConversionStatus(IME_CMODE conversion, IME_SMODE sentence) => WithImmContext(ctx => WiceCommons.ImmSetConversionStatus(ctx, conversion, sentence));

    /// <summary>
    /// Sets the IME status window position.
    /// </summary>
    public bool SetImmStatusWindowPosition(POINT position) => WithImmContext(ctx => WiceCommons.ImmSetStatusWindowPos(ctx, position));

    /// <summary>
    /// Sets the IME composition window position or form.
    /// </summary>
    public bool SetImmCompositionWindowPosition(POINT position, COMPOSITIONFORM? form = null) => WithImmContext(ctx =>
    {
        form ??= new COMPOSITIONFORM
        {
            dwStyle = CFS.CFS_POINT,
            ptCurrentPos = position,
        };

        return WiceCommons.ImmSetCompositionWindow(ctx, form.Value);
    });

    /// <summary>
    /// Sets the IME candidate window position or form.
    /// </summary>
    public bool SetImmCandidateWindowPosition(POINT position, CANDIDATEFORM? form = null) => WithImmContext(ctx =>
    {
        form ??= new CANDIDATEFORM
        {
            dwStyle = CFS.CFS_POINT,
            ptCurrentPos = position,
        };

        return WiceCommons.ImmSetCandidateWindow(ctx, form.Value);
    });

    private void OnDragDrop(DragDropEventArgs e) => DragDrop?.Invoke(this, e);

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    HRESULT IDropTarget.DragLeave()
    {
        var e = new DragDropEventArgs(DragDropEventType.Leave);
        OnDragDrop(e);
        return WiceCommons.S_OK;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    HRESULT IDropSourceNotify.DragEnterTarget(HWND hwndTarget)
    {
        var win = new NativeWindow(hwndTarget);

        var e = new DragDropTargetEventArgs(DragDropTargetEventType.Enter, hwndTarget);
        _dragDropTarget = hwndTarget;
        DragDropTarget?.Invoke(this, e);
        return WiceCommons.S_OK;
    }

    /// <inheritdoc />
    HRESULT IDropSourceNotify.DragLeaveTarget()
    {
        var e = new DragDropTargetEventArgs(DragDropTargetEventType.Leave, _dragDropTarget);
        DragDropTarget?.Invoke(this, e);
        return WiceCommons.S_OK;
    }

    /// <inheritdoc />
    HRESULT IDropSource.QueryContinueDrag(BOOL escapePressed, MODIFIERKEYS_FLAGS flags)
    {
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

    /// <inheritdoc />
    HRESULT IDropSource.GiveFeedback(DROPEFFECT effect)
    {
        var e = new DragDropGiveFeedback(effect);
        DragDropGiveFeedback?.Invoke(this, e);
        return e.Result;
    }

    /// <inheritdoc/>
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

    /// <summary>
    /// Gets the console window handle.
    /// </summary>
    public static HWND ConsoleHandle => WiceCommons.GetConsoleWindow();

    /// <summary>
    /// Gets the console window as a <see cref="NativeWindow"/>, if any.
    /// </summary>
    public static NativeWindow? Console => FromHandle(ConsoleHandle);

    /// <summary>
    /// Gets the active window handle.
    /// </summary>
    public static HWND ActiveHandle => WiceCommons.GetActiveWindow();

    /// <summary>
    /// Gets the active window as a <see cref="NativeWindow"/>, if any.
    /// </summary>
    public static NativeWindow? Active => FromHandle(ActiveHandle);

    /// <summary>
    /// Gets the foreground window handle.
    /// </summary>
    public static HWND ForegroundHandle => WiceCommons.GetForegroundWindow();

    /// <summary>
    /// Gets the foreground window as a <see cref="NativeWindow"/>, if any.
    /// </summary>
    public static NativeWindow? Foreground => FromHandle(ForegroundHandle);

    /// <summary>
    /// Gets the desktop window handle.
    /// </summary>
    public static HWND DesktopHandle => WiceCommons.GetDesktopWindow();

    /// <summary>
    /// Gets the desktop window as a <see cref="NativeWindow"/>.
    /// </summary>
    public static NativeWindow? Desktop => FromHandle(DesktopHandle);

    /// <summary>
    /// Gets the shell window handle.
    /// </summary>
    public static HWND ShellHandle => WiceCommons.GetShellWindow();

    /// <summary>
    /// Gets the shell window as a <see cref="NativeWindow"/>.
    /// </summary>
    public static NativeWindow? Shell => FromHandle(ShellHandle);

    /// <summary>
    /// Wraps a non-null <see cref="HWND"/> into a <see cref="NativeWindow"/>, or returns null for zero handles.
    /// </summary>
    public static NativeWindow? FromHandle(HWND handle) { if (handle.Value == 0) return null; return new NativeWindow(handle); }

    /// <summary>
    /// Determines if a window is a child of a specified parent.
    /// </summary>
    public static bool IsChildWindow(HWND parentHandle, HWND handle) => WiceCommons.IsChild(parentHandle, handle);

    /// <summary>
    /// Returns the window at the specified screen point.
    /// </summary>
    public static HWND GetWindowFromPoint(POINT point) => WiceCommons.WindowFromPoint(point);

    /// <summary>
    /// Converts a virtual-key code to a character.
    /// </summary>
    public static char VirtualKeyToCharacter(VIRTUAL_KEY vk) => (char)WiceCommons.MapVirtualKeyW((uint)vk, MAP_VIRTUAL_KEY_TYPE.MAPVK_VK_TO_CHAR);

    /// <summary>
    /// Returns whether a key is pressed. Uses GetAsyncKeyState by default, else GetKeyState.
    /// </summary>
    public static bool IsKeyPressed(VIRTUAL_KEY vk, bool async = true) => (async ? WiceCommons.GetAsyncKeyState((int)vk) : WiceCommons.GetKeyState((int)vk)) < 0;

    /// <summary>
    /// Releases mouse capture.
    /// </summary>
    public static bool ReleaseMouse() => WiceCommons.ReleaseCapture();

    /// <summary>
    /// Gets the current cursor position in screen coordinates.
    /// </summary>
    public static POINT GetCursorPosition() { WiceCommons.GetCursorPos(out var pt); return pt; }

    /// <summary>
    /// Reads the accessibility cursor size preference from the registry (1..15).
    /// </summary>
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

    /// <summary>
    /// Gets the last message cursor position extracted from GetMessagePos.
    /// </summary>
    public static POINT LastMessagePosision
    {
        get
        {
            var pos = WiceCommons.GetMessagePos();
            return new POINT(pos.SignedLOWORD(), pos.SignedHIWORD());
        }
    }

    /// <summary>
    /// Determines whether the mouse left the window based on the last message position and child checks.
    /// </summary>
    internal static bool DidMouseLeaveWindow(HWND handle, HWND other)
    {
        var pt = LastMessagePosision;
        var win = WiceCommons.WindowFromPoint(pt);
        var child = WiceCommons.IsChild(handle, win);
        var result = !child && (win.Value != other.Value || other.Value == 0);
        return result;
    }

    /// <summary>
    /// Calls DwmExtendFrameIntoClientArea with margin values.
    /// </summary>
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

    /// <summary>
    /// Registers a window class if it does not already exist for the module.
    /// </summary>
    /// <param name="className">The class name.</param>
    /// <param name="styles">Class styles.</param>
    /// <param name="windowProc">The WNDPROC pointer.</param>
    /// <param name="background">Background brush.</param>
    /// <returns>True if registered now; false if already registered.</returns>
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

    /// <summary>
    /// Unregisters all window classes previously tracked and registered by this type.
    /// </summary>
    internal static void UnregisterWindowClasses()
    {
        foreach (var name in _classesNames)
        {
            WiceCommons.UnregisterClassW(PWSTR.From(name), new HINSTANCE { Value = Application.ModuleHandle.Value });
            _classesNames.TryRemove(name);
        }
    }

    /// <summary>
    /// Performs a BeginPaint/EndPaint pair around an optional action.
    /// </summary>
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

    /// <summary>
    /// Performs hit-testing for the non-client area allowing custom caption/resize areas.
    /// </summary>
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
            fOnResizeBorder = ptMouse.y < (rcWindow.top - rcFrame.top);
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

    /// <summary>
    /// Sets GWLP_USERDATA.
    /// </summary>
    internal static nint SetUserData(HWND hwnd, nint data) => SetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWLP_USERDATA, data);

    /// <summary>
    /// Gets GWLP_USERDATA.
    /// </summary>
    internal static nint GetUserData(HWND hwnd) => GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWLP_USERDATA);

    /// <summary>
    /// Sets a window long value, selecting 32/64-bit API as appropriate.
    /// </summary>
    public static nint SetWindowLong(HWND hwnd, WINDOW_LONG_PTR_INDEX nIndex, nint dwNewLong)
#if NETFRAMEWORK
        => IntPtr.Size == 8 ? WindowsFunctions.SetWindowLongPtr64(hwnd, (int)nIndex, dwNewLong) : WindowsFunctions.SetWindowLongPtr32(hwnd, (int)nIndex, dwNewLong);
#else
        => nint.Size == 8 ? Functions.SetWindowLongPtrW(hwnd, nIndex, dwNewLong) : Functions.SetWindowLongW(hwnd, nIndex, (int)dwNewLong);
#endif

    /// <summary>
    /// Gets a window long value, selecting 32/64-bit API as appropriate.
    /// </summary>
    public static nint GetWindowLong(HWND hwnd, WINDOW_LONG_PTR_INDEX nIndex)
#if NETFRAMEWORK
        => IntPtr.Size == 8 ? WindowsFunctions.GetWindowLongPtr64(hwnd, (int)nIndex) : WindowsFunctions.GetWindowLong32(hwnd, (int)nIndex);
#else
        => nint.Size == 8 ? Functions.GetWindowLongPtrW(hwnd, nIndex) : Functions.GetWindowLongW(hwnd, nIndex);
#endif

    /// <summary>
    /// Gets the window text, or an empty string if none.
    /// </summary>
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

    /// <summary>
    /// Gets the class name of a window, or null if it cannot be obtained.
    /// </summary>
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

    /// <summary>
    /// Adjusts a RECT for the specified style/menu/ex-style at a given DPI.
    /// </summary>
    public static RECT AdjustWindowRect(RECT rect, WINDOW_STYLE style, bool hasMenu, WINDOW_EX_STYLE extendedStyle, uint dpi)
    {
        var rc = rect;
        WiceCommons.AdjustWindowRectExForDpi(ref rc, style, hasMenu, extendedStyle, dpi);
        return rc;
    }

    /// <summary>
    /// Centers this window relative to an owner or parent depending on style and visibility.
    /// </summary>
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

    /// <summary>
    /// Gets the TITLEBARINFOEX structure for a window.
    /// </summary>
    public TITLEBARINFOEX GetTITLEBARINFOEX() => GetTITLEBARINFOEX(Handle);

    /// <summary>
    /// Gets the TITLEBARINFOEX structure for the specified window handle.
    /// </summary>
    public static unsafe TITLEBARINFOEX GetTITLEBARINFOEX(HWND hwnd)
    {
        var info = new TITLEBARINFOEX { cbSize = (uint)sizeof(TITLEBARINFOEX) };
        if (hwnd.Value == 0)
            return info;

        WiceCommons.SendMessageW(hwnd, MessageDecoder.WM_GETTITLEBARINFOEX, WPARAM.Null, new LPARAM { Value = (nint)(&info) });
        return info;
    }

    /// <summary>
    /// Gets an icon's dimensions and optional hotspot.
    /// </summary>
    public static SIZE? GetIconDimension(HICON handle) => GetIconDimension(handle, out _);

    /// <summary>
    /// Gets an icon's dimensions and outputs the hotspot if available.
    /// </summary>
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

    /// <summary>
    /// Sets a DWM window attribute as a boolean.
    /// </summary>
    public void SetWindowAttribute(DWMWINDOWATTRIBUTE attribute, bool value) => SetWindowAttribute(Handle, attribute, value);

    /// <summary>
    /// Sets a DWM window attribute as a boolean on an HWND.
    /// </summary>
    public unsafe static void SetWindowAttribute(HWND hwnd, DWMWINDOWATTRIBUTE attribute, bool value)
    {
        var i = value ? 1 : 0;
        WiceCommons.DwmSetWindowAttribute(hwnd, (uint)attribute, (nint)(&i), 4).ThrowOnError();
    }

    /// <summary>
    /// Sets the non-client rendering policy (DWM).
    /// </summary>
    public void SetNonClientRenderingPolicy(DWMNCRENDERINGPOLICY policy) => SetNonClientRenderingPolicy(Handle, policy);

    /// <summary>
    /// Sets the non-client rendering policy (DWM) on an HWND.
    /// </summary>
    public unsafe static void SetNonClientRenderingPolicy(HWND hwnd, DWMNCRENDERINGPOLICY policy)
    {
        var i = (int)policy;
        WiceCommons.DwmSetWindowAttribute(hwnd, (uint)DWMWINDOWATTRIBUTE.DWMWA_NCRENDERING_POLICY, (nint)(&i), 4).ThrowOnError();
    }

    /// <summary>
    /// Enables acrylic blur-behind using SetWindowCompositionAttribute.
    /// </summary>
    public void EnableAcrylicBlurBehind() => EnableAcrylicBlurBehind(Handle);

    /// <summary>
    /// Enables acrylic blur-behind using SetWindowCompositionAttribute on an HWND.
    /// </summary>
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

    /// <summary>
    /// Enables blur-behind using SetWindowCompositionAttribute.
    /// </summary>
    public void EnableBlurBehind() => EnableBlurBehind(Handle);

    /// <summary>
    /// Enables blur-behind using SetWindowCompositionAttribute on an HWND.
    /// </summary>
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

    /// <summary>
    /// Enumerates the child window handles of a specified parent.
    /// </summary>
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

    /// <summary>
    /// Enumerates all top-level window handles.
    /// </summary>
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