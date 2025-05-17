namespace Wice.Interop;

[ComImport, Guid("0000012B-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IDropSourceNotify
{
    [PreserveSig]
    HRESULT DragEnterTarget(HWND hwndTarget);

    [PreserveSig]
    HRESULT DragLeaveTarget();
}
