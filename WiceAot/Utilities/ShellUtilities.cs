#if !NETFRAMEWORK
using System.Runtime.InteropServices.Marshalling;
#endif

namespace Wice.Utilities;

/// <summary>
/// Windows Shell interop helpers.
/// Provides APIs to create an <see cref="IDataObject"/> that represents one or more shell items (e.g., file system paths),
/// using Win32 shell APIs. This type supports both .NET Framework 4.7.2 and .NET 9+ via conditional compilation.
/// </summary>
public static partial class ShellUtilities
{
#if NETFRAMEWORK

    [DllImport("shell32", CharSet = CharSet.Unicode)]
    private static extern HRESULT SHParseDisplayName(string pszName, IBindCtx pbc, out IntPtr ppidl, uint sfgaoIn, out uint psfgaoOut);

    [DllImport("shell32")]
    private static extern HRESULT SHGetFolderLocation(IntPtr hwnd, uint csidl, IntPtr hToken, int dwReserved, out IntPtr ppidl);

    [DllImport("ole32")]
    private static extern HRESULT CreateBindCtx(int reserved, out IBindCtx ppbc);

    /// <summary>
    /// Wraps the Win32 <c>SHCreateDataObject</c> function, creating a shell data object for one or more items.
    /// </summary>
    /// <param name="pidlFolder">PIDL of the parent folder (e.g., Desktop). Can be <see cref="IntPtr.Zero"/> for an empty object.</param>
    /// <param name="cidl">Number of child item PIDLs in <paramref name="apidl"/>.</param>
    /// <param name="apidl">Array of relative child item PIDLs; can be <see langword="null"/> when <paramref name="cidl"/> is 0.</param>
    /// <param name="pdtInner">Optional inner data object for aggregation; can be <see langword="null"/>.</param>
    /// <param name="riid">Requested interface IID, typically <see cref="IDataObject"/>.</param>
    /// <param name="ppv">On success, receives the requested interface pointer.</param>
    /// <returns>Standard <see cref="HRESULT"/> indicating success or failure.</returns>
    [DllImport("shell32")]
    public static extern HRESULT SHCreateDataObject(IntPtr pidlFolder, int cidl, IntPtr[] apidl, IDataObject pdtInner, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);

    /// <summary>
    /// Creates a Windows Shell <see cref="IDataObject"/> that represents the specified file system items.
    /// </summary>
    /// <param name="filePaths">File system paths to include in the data object. If <see langword="null"/> or empty, an empty shell data object is created (supports only SetData with fRelease = true).</param>
    /// <returns>
    /// An <see cref="IComObject{T}"/> for <see cref="IDataObject"/> that callers can use for drag-and-drop or clipboard scenarios.
    /// </returns>
    public static IComObject<IDataObject> CreateDataObject(IEnumerable<string> filePaths = null)
    {
        if (filePaths == null || !filePaths.Any())
        {
            // note this data object only supports SetData(..., ..., true);
            SHCreateDataObject(IntPtr.Zero, 0, null, null, typeof(IDataObject).GUID, out var obj).ThrowOnError();
            return new ComObject<IDataObject>((IDataObject)obj);
        }

        CreateBindCtx(0, out var ctxUnk).ThrowOnError();
        using var ctx = new ComObject<IBindCtx>(ctxUnk);
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

#else

    /// <summary>
    /// Wraps the Win32 <c>SHParseDisplayName</c> function.
    /// Parses a display name (e.g., a file system path) into a PIDL.
    /// </summary>
    /// <param name="pszName">The display name to parse (e.g., a file path).</param>
    /// <param name="pbc">Optional bind context to control parsing; can be <see langword="null"/>.</param>
    /// <param name="ppidl">On success, receives an absolute PIDL. Caller must free with <see cref="Marshal.FreeCoTaskMem(nint)"/>.</param>
    /// <param name="sfgaoIn">Attribute flags to query; can be 0.</param>
    /// <param name="psfgaoOut">Optional pointer that receives attributes for the parsed item; can be 0.</param>
    /// <returns>Standard <see cref="HRESULT"/> indicating success or failure.</returns>
    [LibraryImport("SHELL32")]
    [SupportedOSPlatform("windows5.1.2600")]
    [PreserveSig]
    public static partial HRESULT SHParseDisplayName(PWSTR pszName, [MarshalUsing(typeof(UniqueComInterfaceMarshaller<IBindCtx?>))] IBindCtx? pbc, out nint ppidl, uint sfgaoIn, nint psfgaoOut);

    /// <summary>
    /// Wraps the Win32 <c>SHGetFolderLocation</c> function.
    /// Retrieves the PIDL of a special folder (e.g., Desktop).
    /// </summary>
    /// <param name="hwnd">Reserved; typically <see cref="HWND.Null"/>.</param>
    /// <param name="csidl">The CSIDL of the special folder (e.g., 0 for Desktop).</param>
    /// <param name="hToken">An access token for another user; typically <see cref="HANDLE.Null"/>.</param>
    /// <param name="dwFlags">Reserved; must be 0.</param>
    /// <param name="ppidl">On success, receives the folder PIDL. Caller must free with <see cref="Marshal.FreeCoTaskMem(nint)"/>.</param>
    /// <returns>Standard <see cref="HRESULT"/> indicating success or failure.</returns>
    [LibraryImport("SHELL32")]
    [SupportedOSPlatform("windows5.0")]
    [PreserveSig]
    public static partial HRESULT SHGetFolderLocation(HWND hwnd, int csidl, HANDLE hToken, uint dwFlags, out nint ppidl);

    /// <summary>
    /// Creates a Windows Shell data object that represents the specified file system items.
    /// </summary>
    /// <param name="filePaths">File system paths to include in the data object. If <see langword="null"/> or empty, an empty shell data object is created (supports only SetData with fRelease = true).</param>
    /// <param name="owned">When <see langword="true"/>, the resulting wrapper takes ownership of the underlying COM object and will release it when disposed.</param>
    /// <returns>
    /// A <c>DataObject</c> that wraps a shell <see cref="IDataObject"/> suitable for drag-and-drop or clipboard scenarios.
    /// </returns>
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
