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
    public static readonly Guid DXGI_DEBUG_ALL = new("e48ae283-da80-490b-87e6-43e9a9cfda08");

    public static int GetSystemMetrics(SYSTEM_METRICS_INDEX nIndex) => WindowsFunctions.GetSystemMetrics(nIndex);
    public static uint GetDoubleClickTime() => (uint)WindowsFunctions.GetDoubleClickTime();
    public static uint GetCurrentThreadId() => (uint)WindowsUtilities.GetCurrentThreadId();
    public static BOOL PostThreadMessageW(uint idThread, uint Msg, WPARAM wParam, LPARAM lParam) => PostThreadMessage(idThread, Msg, wParam, lParam);
    public static BOOL PostMessageW(HWND hWnd, uint Msg, WPARAM wParam, LPARAM lParam) => WindowsFunctions.PostMessage(hWnd, (int)Msg, wParam, lParam);
    public static LRESULT SendMessageW(HWND hWnd, uint Msg, WPARAM wParam, LPARAM lParam) => WindowsFunctions.SendMessage(hWnd, (int)Msg, wParam, lParam);
    public static HMODULE GetModuleHandleW(string lpModuleName) => WindowsFunctions.GetModuleHandle(lpModuleName);
    public static uint GetDpiForWindow(HWND hwnd) => (uint)WindowsFunctions.GetDpiForWindow(hwnd);
    public static BOOL AdjustWindowRectExForDpi(ref RECT lpRect, WINDOW_STYLE dwStyle, BOOL bMenu, WINDOW_EX_STYLE dwExStyle, uint dpi) => WindowsFunctions.AdjustWindowRectExForDpi(ref lpRect, dwStyle, bMenu, dwExStyle, (int)dpi);
    public static void PostQuitMessage(int nExitCode) => WindowsFunctions.PostQuitMessage(nExitCode);
    public static BOOL TrackMouseEvent(ref TRACKMOUSEEVENT lpEventTrack) => WindowsFunctions.TrackMouseEvent(ref lpEventTrack);
    public static BOOL KillTimer(HWND hWnd, nuint uIDEvent) => WindowsFunctions.KillTimer(hWnd, (nint)uIDEvent);
    public static void SetTimer(HWND hWnd, nuint nIDEvent, uint uElapse, object? lpTimerFunc) => WindowsFunctions.SetTimer(hWnd, (nint)nIDEvent, (int)uElapse, IntPtr.Zero);
    public static HRESULT SetWindowCompositionAttribute(HWND hwnd, ref WINDOWCOMPOSITIONATTRIBDATA data) => WindowsFunctions.SetWindowCompositionAttribute(hwnd, ref data);
    public static BOOL DeleteObject(HGDIOBJ ho) => WindowsFunctions.DeleteObject(ho);
    public static BOOL GetIconInfo(HICON hIcon, out ICONINFO piconinfo) => WindowsFunctions.GetIconInfo(hIcon, out piconinfo);
    public static BOOL GetMonitorInfoW(HMONITOR hMonitor, ref MONITORINFO lpmi) => WindowsFunctions.GetMonitorInfo(hMonitor, ref lpmi);
    public static HMONITOR MonitorFromWindow(HWND hwnd, MONITOR_FROM_FLAGS dwFlags) => WindowsFunctions.MonitorFromWindow(hwnd, dwFlags);
    public static BOOL GetWindowRect(HWND hWnd, out RECT lpRect) => WindowsFunctions.GetWindowRect(hWnd, out lpRect);
    public static BOOL GetClientRect(HWND hWnd, out RECT lpRect) => WindowsFunctions.GetClientRect(hWnd, out lpRect);
    public static BOOL UnregisterClassW(string lpClassName, HINSTANCE hInstance) => WindowsFunctions.UnregisterClass(lpClassName, hInstance);
    public static HWND WindowFromPoint(POINT Point) => WindowsFunctions.WindowFromPoint(Point);
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
    public static int ShellAboutW(HWND hWnd, string szApp, string szOtherStuff, HICON hIcon) => WindowsFunctions.ShellAbout(hWnd, szApp, szOtherStuff, hIcon);
    public static BOOL ScreenToClient(HWND hWnd, ref POINT lpPoint) => WindowsFunctions.ScreenToClient(hWnd, ref lpPoint);
    public static BOOL ClientToScreen(HWND hWnd, ref POINT lpPoint) => WindowsFunctions.ClientToScreen(hWnd, ref lpPoint);
    public static BOOL SetCaretPos(int X, int Y) => WindowsFunctions.SetCaretPos(X, Y);
    public static BOOL CreateCaret(HWND hWnd, HBITMAP hBitmap, int nWidth, int nHeight) => WindowsFunctions.CreateCaret(hWnd, hBitmap, nWidth, nHeight);
    public static HWND SetCapture(HWND hWnd) => WindowsFunctions.SetCapture(hWnd);
    public static BOOL SetForegroundWindow(HWND hWnd) => WindowsFunctions.SetForegroundWindow(hWnd);
    public static BOOL SetWindowPos(HWND hWnd, HWND hWndInsertAfter, int X, int Y, int cx, int cy, SET_WINDOW_POS_FLAGS uFlags) => WindowsFunctions.SetWindowPos(hWnd, hWndInsertAfter, X, Y, cx, cy, uFlags); 
    public static BOOL ShowWindow(HWND hWnd, SHOW_WINDOW_CMD nCmdShow) => WindowsFunctions.ShowWindow(hWnd, nCmdShow);
    public static BOOL DestroyWindow(HWND hWnd) => WindowsFunctions.DestroyWindow(hWnd);
    public static BOOL IsZoomed(HWND hWnd) => WindowsFunctions.IsZoomed(hWnd);
    public static BOOL SetWindowDisplayAffinity(HWND hWnd, WINDOW_DISPLAY_AFFINITY dwAffinity) => WindowsFunctions.SetWindowDisplayAffinity(hWnd, dwAffinity);
    public static BOOL SetWindowTextW(HWND hWnd, string lpString) => WindowsFunctions.SetWindowText(hWnd, lpString);
    public static BOOL IsWindowEnabled(HWND hWnd) => WindowsFunctions.IsWindowEnabled(hWnd);
    public static BOOL EnableWindow(HWND hWnd, BOOL bEnable) => WindowsFunctions.EnableWindow(hWnd, bEnable);
    public static uint GetDpiFromDpiAwarenessContext(DPI_AWARENESS_CONTEXT value) => (uint)DpiUtilities.GetDpiFromDpiAwarenessContext(value);
    public static HWND GetWindow(HWND hWnd, GET_WINDOW_CMD uCmd) => WindowsFunctions.GetWindow(hWnd, (int)uCmd);
    public static HWND GetParent(HWND hWnd) => WindowsFunctions.GetParent(hWnd);
    public static HWND SetParent(HWND hWndChild, HWND hWndNewParent) => WindowsFunctions.SetParent(hWndChild, hWndNewParent);
    public static uint GetCaretBlinkTime() => WindowsFunctions.GetCaretBlinkTime();
    public static BOOL DestroyCaret() => WindowsFunctions.DestroyCaret();
    public static string? GetMessageBoxString(MESSAGEBOX_RESULT button, bool removeMnemonics = true) => WindowsFunctions.GetMessageBoxString(button, removeMnemonics);

    public static BOOL GetCursorPos(out POINT lpPoint)
    {
        var point = new POINT();
        var ret = WindowsFunctions.GetCursorPos(ref point);
        lpPoint = point;
        return ret;
    }

    public static HRESULT DwmExtendFrameIntoClientArea(HWND hWnd, in MARGINS pMarInset)
    {
        var margins = pMarInset;
        return WindowsFunctions.DwmExtendFrameIntoClientArea(hWnd, ref margins);
    }

    public static BOOL GetClassInfoW(HINSTANCE hInstance, string lpClassName, out WNDCLASSW lpWndClass)
    {
        lpWndClass = new WNDCLASSW(); // unused
        return WindowsFunctions.GetClassInfo(hInstance, lpClassName, out var cls);
    }

    public static ushort RegisterClassW(in WNDCLASSW lpWndClass)
    {
        var cls = lpWndClass;
        return (ushort)WindowsFunctions.RegisterClass(ref cls);
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

    public static HWND CreateWindowExW(WINDOW_EX_STYLE dwExStyle, string lpClassName, string  lpWindowName, WINDOW_STYLE dwStyle, int X, int Y, int nWidth, int nHeight, HWND hWndParent, HMENU hMenu, HINSTANCE hInstance, nint lpParam)
        => WindowsFunctions.CreateWindowEx(dwExStyle, lpClassName, lpWindowName, dwStyle, X, Y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam);

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
    public static extern BOOL SystemParametersInfoW(SYSTEM_PARAMETERS_INFO_ACTION uiAction, uint uiParam, nint pvParam, SYSTEM_PARAMETERS_INFO_UPDATE_FLAGS fWinIni);

    [DllImport("kernel32")]
    public static extern nint GetProcAddress(HMODULE hModule, string lpProcName);

    [DllImport("user32")]
    public static extern uint GetWindowThreadProcessId(HWND hWnd, nint lpdwProcessId);

    [DllImport("user32")]
    public static extern bool GetWindowDisplayAffinity(HWND hwnd, out WINDOW_DISPLAY_AFFINITY pdwAffinity);

    [DllImport("user32")]
    public static extern HANDLE GetPropW(HWND hWnd, string lpString);

    [DllImport("user32")]
    public static extern int EnumPropsW(HWND hWnd, PROPENUMPROCW lpEnumFunc);

    [DllImport("shell32")]
    public static extern bool SHGetPropertyStoreForWindow(HWND hwnd, in Guid riid, out nint ppv);
    
    [DllImport("user32")]
    private static extern bool PostThreadMessage(uint idThread, uint msg, WPARAM wParam, LPARAM lParam);

    [DllImport("ole32")]
    public static extern HRESULT RegisterDragDrop(HWND hwnd, IDropTarget pDropTarget);

    [DllImport("ole32")]
    public  static extern HRESULT OleInitialize(nint pvReserved);

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

#else
    public const int ICON_BIG = Constants.ICON_BIG;
    public const int CW_USEDEFAULT = Constants.CW_USEDEFAULT;
    public const uint SIZE_MINIMIZED = Constants.SIZE_MINIMIZED;
    public static readonly HRESULT S_OK = Constants.S_OK;
    public static readonly Guid DXGI_DEBUG_ALL = Constants.DXGI_DEBUG_ALL;

    public static int GetSystemMetrics(SYSTEM_METRICS_INDEX nIndex) => Functions.GetSystemMetrics(nIndex);
    public static uint GetDoubleClickTime() => Functions.GetDoubleClickTime();
    public static uint GetCurrentThreadId() => Functions.GetCurrentThreadId();
    public static BOOL PostThreadMessageW(uint idThread, uint Msg, WPARAM wParam, LPARAM lParam) => Functions.PostThreadMessageW(idThread, Msg, wParam, lParam);
    public static BOOL PostMessageW(HWND hWnd, uint Msg, WPARAM wParam, LPARAM lParam) => Functions.PostMessageW(hWnd, Msg, wParam, lParam);
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

#endif
}
