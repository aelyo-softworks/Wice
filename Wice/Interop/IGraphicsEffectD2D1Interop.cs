namespace Wice.Interop;

[ComImport, Guid("2fc57384-a068-44d7-a331-30982fcf7177"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IGraphicsEffectD2D1Interop
{
    [PreserveSig]
    HRESULT GetEffectId(out Guid id);

    [PreserveSig]
    HRESULT GetNamedPropertyMapping(PWSTR name, out uint index, out GRAPHICS_EFFECT_PROPERTY_MAPPING mapping);

    [PreserveSig]
    HRESULT GetPropertyCount(out uint count);

    [PreserveSig]
    HRESULT GetProperty(uint index, out nint value);

    [PreserveSig]
    HRESULT GetSource(uint index, out IGraphicsEffectSource? source);

    [PreserveSig]
    HRESULT GetSourceCount(out uint count);
}
