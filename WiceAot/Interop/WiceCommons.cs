namespace Wice.Interop;

public static class WiceCommons
{
    public static LRESULT SendMessageW(HWND hWnd, uint Msg, WPARAM wParam) => SendMessageW(hWnd, Msg, wParam, LPARAM.Null);
    public static LRESULT SendMessageW(HWND hWnd, uint Msg, LPARAM lParam) => SendMessageW(hWnd, Msg, WPARAM.Null, lParam);
    public static LRESULT SendMessageW(HWND hWnd, uint Msg) => SendMessageW(hWnd, Msg, WPARAM.Null, LPARAM.Null);
    public static BOOL PostMessageW(HWND hWnd, uint Msg, WPARAM wParam) => PostMessageW(hWnd, Msg, wParam, LPARAM.Null);
    public static BOOL PostMessageW(HWND hWnd, uint Msg, LPARAM lParam) => PostMessageW(hWnd, Msg, WPARAM.Null, lParam);
    public static BOOL PostMessageW(HWND hWnd, uint Msg) => PostMessageW(hWnd, Msg, WPARAM.Null, LPARAM.Null);

#if NETFRAMEWORK
    public const int ICON_BIG = 1;
    public const int CW_USEDEFAULT = unchecked((int)0x80000000);
    public const uint SIZE_MINIMIZED = 1;
    public static readonly HRESULT S_OK = HRESULTS.S_OK;
    public static readonly HRESULT DRAGDROP_E_NOTREGISTERED = 0x80040100;
    public static readonly HRESULT DRAGDROP_E_ALREADYREGISTERED = 0x80040101;
    public static readonly HRESULT DRAGDROP_S_DROP = 0x00040100;
    public static readonly HRESULT DRAGDROP_S_CANCEL = 0x00040101;
    public static readonly HRESULT DRAGDROP_S_USEDEFAULTCURSORS = 0x00040102;
    public static readonly HRESULT E_BOUNDS = 0x8000000B;
    public static readonly HRESULT E_FAIL = 0x80004005;
    public static readonly HRESULT E_INVALIDARG = 0x80070057;
    public static readonly HRESULT E_NOTIMPL = 0x80004001;

    public static readonly Guid DXGI_DEBUG_ALL = new("e48ae283-da80-490b-87e6-43e9a9cfda08");

    public const uint WM_MOUSEMOVE = MessageDecoder.WM_MOUSEMOVE;
    public const uint WM_MOUSEHOVER = MessageDecoder.WM_MOUSEHOVER;
    public const uint WM_XBUTTONUP = MessageDecoder.WM_XBUTTONUP;
    public const uint WM_XBUTTONDOWN = MessageDecoder.WM_XBUTTONDOWN;
    public const uint WM_MBUTTONUP = MessageDecoder.WM_MBUTTONUP;
    public const uint WM_MBUTTONDOWN = MessageDecoder.WM_MBUTTONDOWN;
    public const uint WM_RBUTTONUP = MessageDecoder.WM_RBUTTONUP;
    public const uint WM_RBUTTONDOWN = MessageDecoder.WM_RBUTTONDOWN;
    public const uint WM_LBUTTONUP = MessageDecoder.WM_LBUTTONUP;
    public const uint WM_LBUTTONDOWN = MessageDecoder.WM_LBUTTONDOWN;

    public static string? MsgToString(uint msg) => MessageDecoder.MsgToString((int)msg);
    public static Monitor? GetMonitorFromWindow(HWND hwnd, MONITOR_FROM_FLAGS dwFlags) => Monitor.FromWindow(hwnd, (MFW)dwFlags);
    public static IComObject<T>? GetComObjectFromPointer<T>(nint ptr) => ComObject.From<T>(ptr);
    public static IComObject<T>? Cast<T>(this IComObject co) where T : class => co.AsComObject<T>();
    public static IComObject<T>? AsComObject<T>(this object? winRTObject) => new ComObject<T>((T)winRTObject);

    public static int GetSystemMetrics(SYSTEM_METRICS_INDEX nIndex) => WindowsFunctions.GetSystemMetrics(nIndex);
    public static uint GetDoubleClickTime() => (uint)WindowsFunctions.GetDoubleClickTime();
    public static uint GetCurrentThreadId() => (uint)WindowsUtilities.GetCurrentThreadId();
    public static BOOL PostThreadMessageW(uint idThread, uint Msg, WPARAM wParam, LPARAM lParam) => PostThreadMessage(idThread, Msg, wParam, lParam);
    public static BOOL PostMessageW(HWND hWnd, uint Msg, WPARAM wParam, LPARAM lParam) => WindowsFunctions.PostMessage(hWnd, (int)Msg, wParam, lParam);
    public static LRESULT SendMessageW(HWND hWnd, uint Msg, WPARAM wParam, LPARAM lParam) => WindowsFunctions.SendMessage(hWnd, (int)Msg, wParam, lParam);
    public static uint GetDpiForWindow(HWND hwnd) => (uint)WindowsFunctions.GetDpiForWindow(hwnd);
    public static void PostQuitMessage(int nExitCode) => WindowsFunctions.PostQuitMessage(nExitCode);
    public static BOOL KillTimer(HWND hWnd, nuint uIDEvent) => WindowsFunctions.KillTimer(hWnd, (nint)uIDEvent);
#pragma warning disable IDE0060 // Remove unused parameter
    public static void SetTimer(HWND hWnd, nuint nIDEvent, uint uElapse, object? lpTimerFunc) => WindowsFunctions.SetTimer(hWnd, (nint)nIDEvent, (int)uElapse, IntPtr.Zero);
#pragma warning restore IDE0060 // Remove unused parameter
    public static BOOL DeleteObject(HGDIOBJ ho) => WindowsFunctions.DeleteObject(ho);
    public static BOOL IsChild(HWND hWndParent, HWND hWnd) => WindowsFunctions.IsChild(hWndParent, hWnd);
    public static uint GetMessagePos() => (uint)WindowsFunctions.GetMessagePos();
    public static BOOL ReleaseCapture() => WindowsFunctions.ReleaseCapture();
    public static short GetAsyncKeyState(int vKey) => WindowsFunctions.GetAsyncKeyState((VirtualKeys)vKey);
    public static short GetKeyState(int nVirtKey) => WindowsFunctions.GetKeyState((VirtualKeys)nVirtKey);
    public static uint MapVirtualKeyW(uint uCode, MAP_VIRTUAL_KEY_TYPE uMapType) => (uint)WindowsFunctions.MapVirtualKey(uCode, (int)uMapType);
    public static HWND GetShellWindow() => WindowsFunctions.GetShellWindow();
    public static HWND GetConsoleWindow() => WindowsFunctions.GetConsoleWindow();
    public static HWND GetActiveWindow() => WindowsFunctions.GetActiveWindow();
    public static HWND GetForegroundWindow() => WindowsFunctions.GetForegroundWindow();
    public static HWND GetDesktopWindow() => WindowsFunctions.GetDesktopWindow();
    public static BOOL SetCaretPos(int X, int Y) => WindowsFunctions.SetCaretPos(X, Y);
    public static BOOL CreateCaret(HWND hWnd, HBITMAP hBitmap, int nWidth, int nHeight) => WindowsFunctions.CreateCaret(hWnd, hBitmap, nWidth, nHeight);
    public static HWND SetCapture(HWND hWnd) => WindowsFunctions.SetCapture(hWnd);
    public static BOOL SetForegroundWindow(HWND hWnd) => WindowsFunctions.SetForegroundWindow(hWnd);
    public static BOOL SetWindowPos(HWND hWnd, HWND hWndInsertAfter, int X, int Y, int cx, int cy, SET_WINDOW_POS_FLAGS uFlags) => WindowsFunctions.SetWindowPos(hWnd, hWndInsertAfter, X, Y, cx, cy, uFlags);
    public static BOOL ShowWindow(HWND hWnd, SHOW_WINDOW_CMD nCmdShow) => WindowsFunctions.ShowWindow(hWnd, nCmdShow);
    public static BOOL DestroyWindow(HWND hWnd) => WindowsFunctions.DestroyWindow(hWnd);
    public static BOOL IsZoomed(HWND hWnd) => WindowsFunctions.IsZoomed(hWnd);
    public static BOOL SetWindowDisplayAffinity(HWND hWnd, WINDOW_DISPLAY_AFFINITY dwAffinity) => WindowsFunctions.SetWindowDisplayAffinity(hWnd, dwAffinity);
    public static BOOL IsWindowEnabled(HWND hWnd) => WindowsFunctions.IsWindowEnabled(hWnd);
    public static BOOL EnableWindow(HWND hWnd, BOOL bEnable) => WindowsFunctions.EnableWindow(hWnd, bEnable);
    public static uint GetDpiFromDpiAwarenessContext(DPI_AWARENESS_CONTEXT value) => (uint)DpiUtilities.GetDpiFromDpiAwarenessContext(value);
    public static HWND GetWindow(HWND hWnd, GET_WINDOW_CMD uCmd) => WindowsFunctions.GetWindow(hWnd, (int)uCmd);
    public static HWND GetParent(HWND hWnd) => WindowsFunctions.GetParent(hWnd);
    public static HWND SetParent(HWND hWndChild, HWND hWndNewParent) => WindowsFunctions.SetParent(hWndChild, hWndNewParent);
    public static uint GetCaretBlinkTime() => WindowsFunctions.GetCaretBlinkTime();
    public static BOOL DestroyCaret() => WindowsFunctions.DestroyCaret();
    public static string? GetMessageBoxString(MESSAGEBOX_RESULT button, bool removeMnemonics = true) => WindowsFunctions.GetMessageBoxString(button, removeMnemonics);

    public static HRESULT DwmExtendFrameIntoClientArea(HWND hWnd, in MARGINS pMarInset)
    {
        var margins = pMarInset;
        return WindowsFunctions.DwmExtendFrameIntoClientArea(hWnd, ref margins);
    }

    public static BOOL EndPaint(HWND hWnd, in PAINTSTRUCT lpPaint)
    {
        var paint = lpPaint;
        return WindowsFunctions.EndPaint(hWnd, ref paint);
    }

    public static HDC BeginPaint(HWND hWnd, out PAINTSTRUCT lpPaint)
    {
        var paint = new PAINTSTRUCT();
        var ret = WindowsFunctions.BeginPaint(hWnd, ref paint);
        lpPaint = paint;
        return ret;
    }

    public static BOOL DwmDefWindowProc(HWND hWnd, uint msg, WPARAM wParam, LPARAM lParam, out LRESULT plResult)
    {
        var ret = WindowsFunctions.DwmDefWindowProc(hWnd, (int)msg, wParam, lParam, out var result);
        plResult = result;
        return ret;
    }

    public static LRESULT DispatchMessageW(in MSG lpMsg)
    {
        var msg = lpMsg;
        return WindowsFunctions.DispatchMessage(ref msg);
    }

    public static BOOL TranslateMessage(in MSG lpMsg)
    {
        var msg = lpMsg;
        return WindowsFunctions.TranslateMessage(ref msg);
    }

    public static BOOL GetMessageW(out MSG lpMsg, HWND hWnd, uint wMsgFilterMin, uint wMsgFilterMax)
    {
        var msg = new MSG();
        var ret = WindowsFunctions.GetMessage(ref msg, hWnd, (int)wMsgFilterMin, (int)wMsgFilterMax);
        lpMsg = msg;
        return ret;
    }

    [DllImport("user32")]
    public static extern BOOL PeekMessageW(out MSG lpMsg, HWND hWnd, uint wMsgFilterMin, uint wMsgFilterMax, PEEK_MESSAGE_REMOVE_TYPE wRemoveMsg);

    [DllImport("user32")]
    public static extern BOOL GetCursorPos(out POINT lpPoint);

    [DllImport("user32")]
    public static extern BOOL IsWindowVisible(HWND hWnd);

    [DllImport("user32")]
    public static extern BOOL AdjustWindowRectExForDpi(ref RECT lpRect, WINDOW_STYLE dwStyle, BOOL bMenu, WINDOW_EX_STYLE dwExStyle, uint dpi);

    [DllImport("user32")]
    public static extern BOOL ScreenToClient(HWND hWnd, ref POINT lpPoint);

    [DllImport("user32")]
    public static extern BOOL ClientToScreen(HWND hWnd, ref POINT lpPoint);

    [DllImport("user32")]
    public static extern BOOL GetWindowRect(HWND hWnd, out RECT lpRect);

    [DllImport("user32")]
    public static extern BOOL GetClientRect(HWND hWnd, out RECT lpRect);

    [DllImport("user32")]
    public static extern HWND WindowFromPoint(POINT Point);

    [DllImport("user32")]
    public static extern HWND CreateWindowExW(WINDOW_EX_STYLE dwExStyle, PWSTR lpClassName, PWSTR lpWindowName, WINDOW_STYLE dwStyle, int X, int Y, int nWidth, int nHeight, HWND hWndParent, HMENU hMenu, HINSTANCE hInstance, nint /* optional void* */ lpParam);

    [DllImport("user32")]
    public static extern BOOL TrackMouseEvent(ref TRACKMOUSEEVENT lpEventTrack);

    [DllImport("user32")]
    public static extern int GetSystemMetricsForDpi(SYSTEM_METRICS_INDEX nIndex, uint dpi);

    [DllImport("user32")]
    public static extern BOOL SetWindowTextW(HWND hWnd, PWSTR lpString);

    [DllImport("user32")]
    public static extern BOOL GetClassInfoW(HINSTANCE hInstance, PWSTR lpClassName, out WNDCLASSW lpWndClass);

    [DllImport("user32")]
    public static extern BOOL UnregisterClassW(PWSTR lpClassName, HINSTANCE hInstance);

    [DllImport("shell32")]
    public static extern int ShellAboutW(HWND hWnd, PWSTR szApp, PWSTR szOtherStuff, HICON hIcon);

    [DllImport("kernel32")]
    public static extern HMODULE GetModuleHandleW(PWSTR lpModuleName);

    [DllImport("user32")]
    public static extern BOOL EnumChildWindows(HWND hWndParent, WNDENUMPROC lpEnumFunc, LPARAM lParam);

    [DllImport("user32")]
    public static extern BOOL EnumWindows(WNDENUMPROC lpEnumFunc, LPARAM lParam);

    [DllImport("user32")]
    public static extern HMONITOR MonitorFromWindow(HWND hwnd, MONITOR_FROM_FLAGS dwFlags);

    [DllImport("user32")]
    public static extern ushort RegisterClassW(in WNDCLASSW lpWndClass);

    [DllImport("user32")]
    public static extern BOOL GetMonitorInfoW(HMONITOR hMonitor, ref MONITORINFO lpmi);

    [DllImport("user32")]
    public static extern HRESULT SetWindowCompositionAttribute(HWND hwnd, ref WINDOWCOMPOSITIONATTRIBDATA data);

    [DllImport("user32")]
    public static extern HRESULT GetWindowCompositionAttribute(HWND hwnd, ref WINDOWCOMPOSITIONATTRIBDATA data);

    [DllImport("user32")]
    public static extern BOOL GetIconInfo(HICON hIcon, out ICONINFO piconinfo);

    [DllImport("user32")]
    public static extern BOOL SystemParametersInfoW(SYSTEM_PARAMETERS_INFO_ACTION uiAction, uint uiParam, nint pvParam, SYSTEM_PARAMETERS_INFO_UPDATE_FLAGS fWinIni);

    [DllImport("kernel32")]
    public static extern nint GetProcAddress(HMODULE hModule, PSTR lpProcName);

    [DllImport("user32")]
    public static extern uint GetWindowThreadProcessId(HWND hWnd, nint lpdwProcessId);

    [DllImport("user32")]
    public static extern bool GetWindowDisplayAffinity(HWND hwnd, out WINDOW_DISPLAY_AFFINITY pdwAffinity);

    [DllImport("user32")]
    public static extern HANDLE GetPropW(HWND hWnd, PWSTR lpString);

    [DllImport("user32")]
    public static extern int EnumPropsW(HWND hWnd, PROPENUMPROCW lpEnumFunc);

    [DllImport("shell32")]
    public static extern bool SHGetPropertyStoreForWindow(HWND hwnd, in Guid riid, out nint ppv);

    [DllImport("user32")]
    private static extern bool PostThreadMessage(uint idThread, uint msg, WPARAM wParam, LPARAM lParam);

    [DllImport("ole32")]
    public static extern HRESULT RegisterDragDrop(HWND hwnd, IDropTarget pDropTarget);

    [DllImport("ole32")]
    public static extern HRESULT OleInitialize(nint pvReserved);

    [DllImport("ole32")]
    public static extern HRESULT DoDragDrop(IDataObject pDataObj, IDropSource pDropSource, DROPEFFECT dwOKEffects, out DROPEFFECT pdwEffect);

    [DllImport("ole32")]
    public static extern HRESULT RevokeDragDrop(HWND hwnd);

    [DllImport("dwmapi")]
    public static extern HRESULT DwmGetWindowAttribute(HWND hwnd, uint dwAttribute, nint pvAttribute, uint cbAttribute);

    [DllImport("dwmapi")]
    public static extern HRESULT DwmSetWindowAttribute(HWND hwnd, uint dwAttribute, nint pvAttribute, uint cbAttribute);

    [DllImport("gdi32")]
    public static extern int GetObjectW(HGDIOBJ h, int c, nint pv);

    [DllImport("user32")]
    public static extern BOOL DestroyIcon(HICON hIcon);

    [DllImport("user32")]
    public static extern BOOL GetPointerType(uint pointerId, out POINTER_INPUT_TYPE pointerType);

    [DllImport("user32")]
    public static extern BOOL GetPointerInfo(uint pointerId, out POINTER_INFO pointerInfo);

    [DllImport("user32")]
    public static extern BOOL GetPointerPenInfo(uint pointerId, out POINTER_PEN_INFO penInfo);

    [DllImport("user32")]
    public static extern BOOL GetPointerTouchInfo(uint pointerId, out POINTER_TOUCH_INFO touchInfo);

    [DllImport("user32")]
    public static extern BOOL IsMouseInPointerEnabled();

    [DllImport("user32")]
    public static extern BOOL EnableMouseInPointer(BOOL fEnable);

    [DllImport("user32")]
    public static extern BOOL UpdateWindow(HWND hWnd);

    [DllImport("user32")]
    public static extern BOOL ValidateRect(HWND hWnd, nint lpRect);

    [DllImport("user32")]
    public static extern BOOL InvalidateRect(HWND hWnd, nint lpRect, BOOL bErase);

    [DllImport("user32")]
    public static extern BOOL RedrawWindow(HWND hWnd, nint lprcUpdate, HRGN hrgnUpdate, REDRAW_WINDOW_FLAGS flags);

    [DllImport("windows.data.pdf.dll", ExactSpelling = true)]
    public static extern HRESULT PdfCreateRenderer(IDXGIDevice pDevice, out IPdfRendererNative ppRenderer);

#else
    public const int ICON_BIG = Constants.ICON_BIG;
    public const int CW_USEDEFAULT = Constants.CW_USEDEFAULT;
    public const uint SIZE_MINIMIZED = Constants.SIZE_MINIMIZED;
    public static readonly HRESULT S_OK = Constants.S_OK;
    public static readonly HRESULT DRAGDROP_E_ALREADYREGISTERED = Constants.DRAGDROP_E_ALREADYREGISTERED;
    public static readonly HRESULT DRAGDROP_E_NOTREGISTERED = Constants.DRAGDROP_E_NOTREGISTERED;
    public static readonly HRESULT DRAGDROP_S_DROP = Constants.DRAGDROP_S_DROP;
    public static readonly HRESULT DRAGDROP_S_CANCEL = Constants.DRAGDROP_S_CANCEL;
    public static readonly HRESULT DRAGDROP_S_USEDEFAULTCURSORS = Constants.DRAGDROP_S_USEDEFAULTCURSORS;
    public static readonly HRESULT E_BOUNDS = Constants.E_BOUNDS;
    public static readonly HRESULT E_FAIL = Constants.E_FAIL;
    public static readonly HRESULT E_INVALIDARG = Constants.E_INVALIDARG;
    public static readonly HRESULT E_NOTIMPL = Constants.E_NOTIMPL;

    public static readonly Guid DXGI_DEBUG_ALL = Constants.DXGI_DEBUG_ALL;

    public const uint WM_MOUSEMOVE = MessageDecoder.WM_MOUSEMOVE;
    public const uint WM_MOUSEHOVER = MessageDecoder.WM_MOUSEHOVER;
    public const uint WM_XBUTTONUP = MessageDecoder.WM_XBUTTONUP;
    public const uint WM_XBUTTONDOWN = MessageDecoder.WM_XBUTTONDOWN;
    public const uint WM_MBUTTONUP = MessageDecoder.WM_MBUTTONUP;
    public const uint WM_MBUTTONDOWN = MessageDecoder.WM_MBUTTONDOWN;
    public const uint WM_RBUTTONUP = MessageDecoder.WM_RBUTTONUP;
    public const uint WM_RBUTTONDOWN = MessageDecoder.WM_RBUTTONDOWN;
    public const uint WM_LBUTTONUP = MessageDecoder.WM_LBUTTONUP;
    public const uint WM_LBUTTONDOWN = MessageDecoder.WM_LBUTTONDOWN;

    public static string? MsgToString(uint msg) => MessageDecoder.MsgToString(msg);
    public static Monitor? GetMonitorFromWindow(HWND hwnd, MONITOR_FROM_FLAGS dwFlags) => Monitor.FromWindow(hwnd, dwFlags);
    public static IComObject<T>? GetComObjectFromPointer<T>(nint ptr) => ComObject.FromPointer<T>(ptr);
    public static IComObject<T>? Cast<T>(this IComObject co) => co.As<T>();

    // this is to replace the As<T> on C#/WinRT object which doesn't work well under AOT...
    [return: NotNullIfNotNull(nameof(winRTObject))]
    public static IComObject<T>? AsComObject<T>(this object? winRTObject, CreateObjectFlags flags = CreateObjectFlags.UniqueInstance)
    {
        if (winRTObject == null)
            return null;

        var ptr = WinRT.MarshalInspectable<object>.FromManaged(winRTObject);
        var obj = ComObject.FromPointer<T>(ptr, flags);
        if (obj == null)
            throw new InvalidCastException($"Object of type '{winRTObject.GetType().FullName}' is not of type '{typeof(T).FullName}'.");

        return obj;
    }

    public static int GetSystemMetrics(SYSTEM_METRICS_INDEX nIndex) => Functions.GetSystemMetrics(nIndex);
    public static uint GetDoubleClickTime() => Functions.GetDoubleClickTime();
    public static uint GetCurrentThreadId() => Functions.GetCurrentThreadId();
    public static BOOL PostThreadMessageW(uint idThread, uint Msg, WPARAM wParam, LPARAM lParam) => Functions.PostThreadMessageW(idThread, Msg, wParam, lParam);
    public static BOOL PostMessageW(HWND hWnd, uint Msg, WPARAM wParam, LPARAM lParam) => Functions.PostMessageW(hWnd, Msg, wParam, lParam);
    public static BOOL PeekMessageW(out MSG lpMsg, HWND hWnd, uint wMsgFilterMin, uint wMsgFilterMax, PEEK_MESSAGE_REMOVE_TYPE wRemoveMsg) => Functions.PeekMessageW(out lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
    public static LRESULT SendMessageW(HWND hWnd, uint Msg, WPARAM wParam, LPARAM lParam) => Functions.SendMessageW(hWnd, Msg, wParam, lParam);
    public static HMODULE GetModuleHandleW(PWSTR lpModuleName) => Functions.GetModuleHandleW(lpModuleName);
    public static BOOL TranslateMessage(in MSG lpMsg) => Functions.TranslateMessage(in lpMsg);
    public static LRESULT DispatchMessageW(in MSG lpMsg) => Functions.DispatchMessageW(in lpMsg);
    public static uint GetDpiForWindow(HWND hwnd) => Functions.GetDpiForWindow(hwnd);
    public static BOOL AdjustWindowRectExForDpi(ref RECT lpRect, WINDOW_STYLE dwStyle, BOOL bMenu, WINDOW_EX_STYLE dwExStyle, uint dpi) => Functions.AdjustWindowRectExForDpi(ref lpRect, dwStyle, bMenu, dwExStyle, dpi);
    public static void PostQuitMessage(int nExitCode) => Functions.PostQuitMessage(nExitCode);
    public static HRESULT DoDragDrop(IDataObject pDataObj, IDropSource pDropSource, DROPEFFECT dwOKEffects, out DROPEFFECT pdwEffect) => Functions.DoDragDrop(pDataObj, pDropSource, dwOKEffects, out pdwEffect);
    public static BOOL GetMessageW(out MSG lpMsg, HWND hWnd, uint wMsgFilterMin, uint wMsgFilterMax) => Functions.GetMessageW(out lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax);
    public static BOOL TrackMouseEvent(ref TRACKMOUSEEVENT lpEventTrack) => Functions.TrackMouseEvent(ref lpEventTrack);
    public static BOOL KillTimer(HWND hWnd, nuint uIDEvent) => Functions.KillTimer(hWnd, uIDEvent);
    public static HWND CreateWindowExW(WINDOW_EX_STYLE dwExStyle, PWSTR lpClassName, PWSTR lpWindowName, WINDOW_STYLE dwStyle, int X, int Y, int nWidth, int nHeight, HWND hWndParent, HMENU hMenu, HINSTANCE hInstance, nint lpParam) => Functions.CreateWindowExW(dwExStyle, lpClassName, lpWindowName, dwStyle, X, Y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam);
    public static BOOL DwmDefWindowProc(HWND hWnd, uint msg, WPARAM wParam, LPARAM lParam, out LRESULT plResult) => Functions.DwmDefWindowProc(hWnd, msg, wParam, lParam, out plResult);
    public static nuint SetTimer(HWND hWnd, nuint nIDEvent, uint uElapse, TIMERPROC? lpTimerFunc) => Functions.SetTimer(hWnd, nIDEvent, uElapse, lpTimerFunc);
    public static HRESULT SetWindowCompositionAttribute(HWND hwnd, ref WINDOWCOMPOSITIONATTRIBDATA data) => Functions.SetWindowCompositionAttribute(hwnd, ref data);
    public static HRESULT DwmSetWindowAttribute(HWND hwnd, uint dwAttribute, nint pvAttribute, uint cbAttribute) => Functions.DwmSetWindowAttribute(hwnd, dwAttribute, pvAttribute, cbAttribute);
    public static BOOL DeleteObject(HGDIOBJ ho) => Functions.DeleteObject(ho);
    public static int GetObjectW(HGDIOBJ h, int c, nint pv) => Functions.GetObjectW(h, c, pv);
    public static BOOL GetIconInfo(HICON hIcon, out ICONINFO piconinfo) => Functions.GetIconInfo(hIcon, out piconinfo);
    public static BOOL GetMonitorInfoW(HMONITOR hMonitor, ref MONITORINFO lpmi) => Functions.GetMonitorInfoW(hMonitor, ref lpmi);
    public static HMONITOR MonitorFromWindow(HWND hwnd, MONITOR_FROM_FLAGS dwFlags) => Functions.MonitorFromWindow(hwnd, dwFlags);
    public static HDC BeginPaint(HWND hWnd, out PAINTSTRUCT lpPaint) => Functions.BeginPaint(hWnd, out lpPaint);
    public static BOOL EndPaint(HWND hWnd, in PAINTSTRUCT lpPaint) => Functions.EndPaint(hWnd, in lpPaint);
    public static BOOL GetWindowRect(HWND hWnd, out RECT lpRect) => Functions.GetWindowRect(hWnd, out lpRect);
    public static BOOL GetClientRect(HWND hWnd, out RECT lpRect) => Functions.GetClientRect(hWnd, out lpRect);
    public static BOOL UnregisterClassW(PWSTR lpClassName, HINSTANCE hInstance) => Functions.UnregisterClassW(lpClassName, hInstance);
    public static ushort RegisterClassW(in WNDCLASSW lpWndClass) => Functions.RegisterClassW(in lpWndClass);
    public static BOOL GetClassInfoW(HINSTANCE hInstance, PWSTR lpClassName, out WNDCLASSW lpWndClass) => Functions.GetClassInfoW(hInstance, lpClassName, out lpWndClass);
    public static HRESULT DwmExtendFrameIntoClientArea(HWND hWnd, in MARGINS pMarInset) => Functions.DwmExtendFrameIntoClientArea(hWnd, in pMarInset);
    public static HWND WindowFromPoint(POINT Point) => Functions.WindowFromPoint(Point);
    public static BOOL IsChild(HWND hWndParent, HWND hWnd) => Functions.IsChild(hWndParent, hWnd);
    public static uint GetMessagePos() => Functions.GetMessagePos();
    public static BOOL ReleaseCapture() => Functions.ReleaseCapture();
    public static BOOL GetCursorPos(out POINT lpPoint) => Functions.GetCursorPos(out lpPoint);
    public static short GetAsyncKeyState(int vKey) => Functions.GetAsyncKeyState(vKey);
    public static short GetKeyState(int nVirtKey) => Functions.GetKeyState(nVirtKey);
    public static uint MapVirtualKeyW(uint uCode, MAP_VIRTUAL_KEY_TYPE uMapType) => Functions.MapVirtualKeyW(uCode, uMapType);
    public static HWND GetShellWindow() => Functions.GetShellWindow();
    public static HWND GetConsoleWindow() => Functions.GetConsoleWindow();
    public static HWND GetActiveWindow() => Functions.GetActiveWindow();
    public static HWND GetForegroundWindow() => Functions.GetForegroundWindow();
    public static HWND GetDesktopWindow() => Functions.GetDesktopWindow();
    public static int ShellAboutW(HWND hWnd, PWSTR szApp, PWSTR szOtherStuff, HICON hIcon) => Functions.ShellAboutW(hWnd, szApp, szOtherStuff, hIcon);
    public static BOOL ScreenToClient(HWND hWnd, ref POINT lpPoint) => Functions.ScreenToClient(hWnd, ref lpPoint);
    public static BOOL ClientToScreen(HWND hWnd, ref POINT lpPoint) => Functions.ClientToScreen(hWnd, ref lpPoint);
    public static BOOL SetCaretPos(int X, int Y) => Functions.SetCaretPos(X, Y);
    public static BOOL CreateCaret(HWND hWnd, HBITMAP hBitmap, int nWidth, int nHeight) => Functions.CreateCaret(hWnd, hBitmap, nWidth, nHeight);
    public static HWND SetCapture(HWND hWnd) => Functions.SetCapture(hWnd);
    public static BOOL SetForegroundWindow(HWND hWnd) => Functions.SetForegroundWindow(hWnd);
    public static BOOL SetWindowPos(HWND hWnd, HWND hWndInsertAfter, int X, int Y, int cx, int cy, SET_WINDOW_POS_FLAGS uFlags) => Functions.SetWindowPos(hWnd, hWndInsertAfter, X, Y, cx, cy, uFlags);
    public static BOOL ShowWindow(HWND hWnd, SHOW_WINDOW_CMD nCmdShow) => Functions.ShowWindow(hWnd, nCmdShow);
    public static BOOL DestroyWindow(HWND hWnd) => Functions.DestroyWindow(hWnd);
    public static BOOL IsZoomed(HWND hWnd) => Functions.IsZoomed(hWnd);
    public static HRESULT RevokeDragDrop(HWND hwnd) => Functions.RevokeDragDrop(hwnd);
    public static HRESULT RegisterDragDrop(HWND hwnd, IDropTarget pDropTarget) => Functions.RegisterDragDrop(hwnd, pDropTarget);
    public static HRESULT OleInitialize(nint pvReserved) => Functions.OleInitialize(pvReserved);
    public static HRESULT SHGetPropertyStoreForWindow(HWND hwnd, in Guid riid, out nint ppv) => Functions.SHGetPropertyStoreForWindow(hwnd, in riid, out ppv);
    public static HANDLE GetPropW(HWND hWnd, PWSTR lpString) => Functions.GetPropW(hWnd, lpString);
    public static int EnumPropsW(HWND hWnd, PROPENUMPROCW lpEnumFunc) => Functions.EnumPropsW(hWnd, lpEnumFunc);
    public static HRESULT DwmGetWindowAttribute(HWND hwnd, uint dwAttribute, nint pvAttribute, uint cbAttribute) => Functions.DwmGetWindowAttribute(hwnd, dwAttribute, pvAttribute, cbAttribute);
    public static BOOL SetWindowDisplayAffinity(HWND hWnd, WINDOW_DISPLAY_AFFINITY dwAffinity) => Functions.SetWindowDisplayAffinity(hWnd, dwAffinity);
    public static BOOL GetWindowDisplayAffinity(HWND hWnd, out uint pdwAffinity) => Functions.GetWindowDisplayAffinity(hWnd, out pdwAffinity);
    public static BOOL SetWindowTextW(HWND hWnd, PWSTR lpString) => Functions.SetWindowTextW(hWnd, lpString);
    public static uint GetWindowThreadProcessId(HWND hWnd, nint lpdwProcessId) => Functions.GetWindowThreadProcessId(hWnd, lpdwProcessId);
    public static BOOL IsWindowEnabled(HWND hWnd) => Functions.IsWindowEnabled(hWnd);
    public static BOOL IsWindowVisible(HWND hWnd) => Functions.IsWindowVisible(hWnd);
    public static BOOL EnableWindow(HWND hWnd, BOOL bEnable) => Functions.EnableWindow(hWnd, bEnable);
    public static uint GetDpiFromDpiAwarenessContext(DPI_AWARENESS_CONTEXT value) => Functions.GetDpiFromDpiAwarenessContext(value);
    public static HWND GetWindow(HWND hWnd, GET_WINDOW_CMD uCmd) => Functions.GetWindow(hWnd, uCmd);
    public static HWND GetParent(HWND hWnd) => Functions.GetParent(hWnd);
    public static HWND SetParent(HWND hWndChild, HWND hWndNewParent) => Functions.SetParent(hWndChild, hWndNewParent);
    public static nint GetProcAddress(HMODULE hModule, PSTR lpProcName) => Functions.GetProcAddress(hModule, lpProcName);
    public static uint GetCaretBlinkTime() => Functions.GetCaretBlinkTime();
    public static BOOL DestroyCaret() => Functions.DestroyCaret();
    public static BOOL SystemParametersInfoW(SYSTEM_PARAMETERS_INFO_ACTION uiAction, uint uiParam, nint pvParam, SYSTEM_PARAMETERS_INFO_UPDATE_FLAGS fWinIni) => Functions.SystemParametersInfoW(uiAction, uiParam, pvParam, fWinIni);
    public static string? GetMessageBoxString(MESSAGEBOX_RESULT button, bool removeMnemonics = true) => Functions.GetMessageBoxString(button, removeMnemonics);
    public static BOOL EnumChildWindows(HWND hWndParent, WNDENUMPROC lpEnumFunc, LPARAM lParam) => Functions.EnumChildWindows(hWndParent, lpEnumFunc, lParam);
    public static BOOL EnumWindows(WNDENUMPROC lpEnumFunc, LPARAM lParam) => Functions.EnumWindows(lpEnumFunc, lParam);
    public static BOOL DestroyIcon(HICON hIcon) => Functions.DestroyIcon(hIcon);
    public static int GetSystemMetricsForDpi(SYSTEM_METRICS_INDEX nIndex, uint dpi) => Functions.GetSystemMetricsForDpi(nIndex, dpi);
    public static BOOL GetPointerType(uint pointerId, out POINTER_INPUT_TYPE pointerType) => Functions.GetPointerType(pointerId, out pointerType);
    public static BOOL GetPointerInfo(uint pointerId, out POINTER_INFO pointerInfo) => Functions.GetPointerInfo(pointerId, out pointerInfo);
    public static BOOL GetPointerPenInfo(uint pointerId, out POINTER_PEN_INFO penInfo) => Functions.GetPointerPenInfo(pointerId, out penInfo);
    public static BOOL GetPointerTouchInfo(uint pointerId, out POINTER_TOUCH_INFO touchInfo) => Functions.GetPointerTouchInfo(pointerId, out touchInfo);
    public static BOOL IsMouseInPointerEnabled() => Functions.IsMouseInPointerEnabled();
    public static BOOL EnableMouseInPointer(BOOL fEnable) => Functions.EnableMouseInPointer(fEnable);
    public static BOOL InvalidateRect(HWND hWnd, nint lpRect, BOOL bErase) => Functions.InvalidateRect(hWnd, lpRect, bErase);
    public static BOOL ValidateRect(HWND hWnd, nint lpRect) => Functions.ValidateRect(hWnd, lpRect);
    public static BOOL RedrawWindow(HWND hWnd, nint lprcUpdate, HRGN hrgnUpdate, REDRAW_WINDOW_FLAGS flags) => Functions.RedrawWindow(hWnd, lprcUpdate, hrgnUpdate, flags);
    public static BOOL UpdateWindow(HWND hWnd) => Functions.UpdateWindow(hWnd);
    public static HRESULT PdfCreateRenderer(IDXGIDevice pDevice, out IPdfRendererNative ppRenderer) => Functions.PdfCreateRenderer(pDevice, out ppRenderer);

#endif
}
