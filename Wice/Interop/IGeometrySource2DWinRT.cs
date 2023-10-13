namespace Wice.Interop
{
#if NET
    using System.Runtime.InteropServices;
    using DirectN;

    [ComImport, Guid("CAFF7902-670C-4181-A624-DA977203B845"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IGeometrySource2DWinRT : IInspectable
    {
    }
#endif
}
