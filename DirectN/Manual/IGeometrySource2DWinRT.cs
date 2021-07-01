using System;
using System.Runtime.InteropServices;

namespace DirectN
{
#if NET
    [ComImport, Guid("CAFF7902-670C-4181-A624-DA977203B845"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IGeometrySource2DWinRT : IInspectable
    {
    }
#endif
}
