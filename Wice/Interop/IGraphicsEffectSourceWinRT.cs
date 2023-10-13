namespace Wice.Interop
{
#if NET
    using System.Runtime.InteropServices;
    using DirectN;

    [ComImport, Guid("2d8f9ddc-4339-4eb9-9216-f9deb75658a2"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IGraphicsEffectSourceWinRT : IInspectable
    {
    }
#endif
}
