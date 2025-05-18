namespace Wice.Interop;

[ComImport, Guid("3e68d4bd-7135-4d10-8018-9fb6d9f33fa1"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IInitializeWithWindow
{
    [PreserveSig]
    HRESULT Initialize(HWND hwnd);
}
