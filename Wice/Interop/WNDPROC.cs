namespace Wice.Interop;

[UnmanagedFunctionPointer(CallingConvention.Winapi)]
public delegate LRESULT WNDPROC(HWND param0, uint param1, WPARAM param2, LPARAM param3);
