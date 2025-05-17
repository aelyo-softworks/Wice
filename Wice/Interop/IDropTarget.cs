namespace Wice.Interop;

[ComImport, Guid("00000122-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IDropTarget
{
    [PreserveSig]
    HRESULT DragEnter(IDataObject pDataObj, MODIFIERKEYS_FLAGS grfKeyState, POINTL pt, ref DROPEFFECT pdwEffect);

    [PreserveSig]
    HRESULT DragOver(MODIFIERKEYS_FLAGS grfKeyState, POINTL pt, ref DROPEFFECT pdwEffect);

    [PreserveSig]
    HRESULT DragLeave();

    [PreserveSig]
    HRESULT Drop(IDataObject pDataObj, MODIFIERKEYS_FLAGS grfKeyState, POINTL pt, ref DROPEFFECT pdwEffect);
}
