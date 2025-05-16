#if !NETFRAMEWORK
using System.Runtime.InteropServices.Marshalling;
#endif

namespace Wice.Utilities;

public static partial class ShellUtilities
{
#if NETFRAMEWORK

    [DllImport("shell32", CharSet = CharSet.Unicode)]
    private static extern HRESULT SHParseDisplayName(string pszName, IBindCtx pbc, out IntPtr ppidl, uint sfgaoIn, out uint psfgaoOut);

    [DllImport("shell32")]
    private static extern HRESULT SHGetFolderLocation(IntPtr hwnd, uint csidl, IntPtr hToken, int dwReserved, out IntPtr ppidl);

    [DllImport("ole32")]
    private static extern HRESULT CreateBindCtx(int reserved, out IBindCtx ppbc);

    [DllImport("shell32")]
    public static extern HRESULT SHCreateDataObject(IntPtr pidlFolder, int cidl, IntPtr[] apidl, IDataObject pdtInner, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);

    public static IComObject<IDataObject> CreateDataObject(IEnumerable<string> filePaths = null, bool owned = true)
    {
        if (filePaths == null || !filePaths.Any())
        {
            // note this data object only supports SetData(..., ..., true); (fRelease argument set to true)
            SHCreateDataObject(IntPtr.Zero, 0, null, null, typeof(IDataObject).GUID, out var obj).ThrowOnError();
            return new ComObject<IDataObject>((IDataObject)obj);
        }

        CreateBindCtx(0, out var ctxUnk).ThrowOnError();
        using (var ctx = new ComObject<IBindCtx>(ctxUnk))
        {
            var pidls = new List<IntPtr>();

            const int CSIDL_DESKTOP = 0;
            SHGetFolderLocation(IntPtr.Zero, CSIDL_DESKTOP, IntPtr.Zero, 0, out var folderPidl).ThrowOnError();
            pidls.Add(folderPidl);
            try
            {
                foreach (var filePath in filePaths)
                {
                    SHParseDisplayName(filePath, ctx.Object, out var pidl, 0, out _).ThrowOnError();
                    pidls.Add(pidl);
                }

                var arr = pidls.Skip(1).ToArray();
                SHCreateDataObject(folderPidl, arr.Length, arr, null, typeof(IDataObject).GUID, out var obj).ThrowOnError();
                return new ComObject<IDataObject>((IDataObject)obj);
            }
            finally
            {
                pidls.ForEach(Marshal.FreeCoTaskMem);
            }
        }
    }

#else

    [LibraryImport("SHELL32")]
    [SupportedOSPlatform("windows5.1.2600")]
    [PreserveSig]
    public static partial HRESULT SHParseDisplayName(PWSTR pszName, [MarshalUsing(typeof(UniqueComInterfaceMarshaller<IBindCtx?>))] IBindCtx? pbc, out nint ppidl, uint sfgaoIn, nint psfgaoOut);

    [LibraryImport("SHELL32")]
    [SupportedOSPlatform("windows5.0")]
    [PreserveSig]
    public static partial HRESULT SHGetFolderLocation(HWND hwnd, int csidl, HANDLE hToken, uint dwFlags, out nint ppidl);

    [SupportedOSPlatform("windows6.0.6000")]

    public static DataObject CreateDataObject(IEnumerable<string>? filePaths = null, bool owned = true)
    {
        if (filePaths == null || !filePaths.Any())
        {
            // note this data object only supports SetData(..., ..., true); (fRelease argument set to true)
            Functions.SHCreateDataObject(0, 0, 0, null, typeof(IDataObject).GUID, out var obj).ThrowOnError();
            return new DataObject(new ComObject<IDataObject>(obj));
        }

        Functions.CreateBindCtx(0, out var ctxUnk).ThrowOnError();
        using var ctx = new ComObject<IBindCtx>(ctxUnk);

        var pidls = new List<nint>();

        const int CSIDL_DESKTOP = 0;
        SHGetFolderLocation(0, CSIDL_DESKTOP, 0, 0, out var folderPidl).ThrowOnError();
        pidls.Add(folderPidl);
        try
        {
            foreach (var filePath in filePaths)
            {
                SHParseDisplayName(PWSTR.From(filePath), ctx.Object, out var pidl, 0, 0).ThrowOnError();
                pidls.Add(pidl);
            }

            var arr = pidls.Skip(1).ToArray();
            Functions.SHCreateDataObject(folderPidl, arr.Length(), arr.AsPointer(), null, typeof(IDataObject).GUID, out var unk).ThrowOnError();
            return new DataObject(DirectN.Extensions.Com.ComObject.FromPointer<IDataObject>(unk), owned);
        }
        finally
        {
            pidls.ForEach(Marshal.FreeCoTaskMem);
        }
    }
#endif
}
