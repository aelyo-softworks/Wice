namespace Wice.Interop;

[ComImport, Guid("00000121-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IDropSource
{
    [PreserveSig]
    HRESULT QueryContinueDrag(BOOL fEscapePressed, MODIFIERKEYS_FLAGS grfKeyState);

    [PreserveSig]
    HRESULT GiveFeedback(DROPEFFECT dwEffect);
}
