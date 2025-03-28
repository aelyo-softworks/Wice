using WinRT.Interop;

// see here https://github.com/microsoft/CsWinRT/issues/1722

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace ABI.Windows.Graphics
{
    // all names here are hardcoded in WinRT's generator (ABI., Methods, IID, AbiToProjectionVftablePtr)
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class IGeometrySource2DInteropMethods
    {
        public static Guid IID { get; } = typeof(global::Windows.Graphics.IGeometrySource2DInterop).GUID;
        public static nint AbiToProjectionVftablePtr { get; } = global::Windows.Graphics.IGeometrySource2DInterop.Vftbl.InitVtbl();
    }
}

namespace Windows.Graphics
{
    [Guid("0657af73-53fd-47cf-84ff-c8492d2a80a3")]
    [WindowsRuntimeType]
    [WindowsRuntimeHelperType(typeof(IGeometrySource2DInterop))]
    public partial interface IGeometrySource2DInterop
    {
        HRESULT GetGeometry(out ID2D1Geometry? value);
        HRESULT TryGetGeometryUsingFactory(ID2D1Factory factory, out ID2D1Geometry? value);

        // v-table
        internal unsafe struct Vftbl
        {
            public static nint InitVtbl()
            {
                var lpVtbl = (Vftbl*)ComWrappersSupport.AllocateVtableMemory(typeof(Vftbl), sizeof(Vftbl));

                lpVtbl->IUnknownVftbl = IUnknownVftbl.AbiToProjectionVftbl;
                lpVtbl->GetGeometry = &GetGeometryFromAbi;
                lpVtbl->TryGetGeometryUsingFactory = &TryGetGeometryUsingFactoryFromAbi;

                return (nint)lpVtbl;
            }

            private IUnknownVftbl IUnknownVftbl;

            // interface delegates
            private delegate* unmanaged[MemberFunction]<nint, nint*, int> GetGeometry;
            private delegate* unmanaged[MemberFunction]<nint, nint, nint*, int> TryGetGeometryUsingFactory;

            // interface implementation
            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int GetGeometryFromAbi(nint thisPtr, nint* value)
            {
                try
                {
                    if (value != null)
                    {
                        *value = 0;
                    }

                    var hr = ComWrappersSupport.FindObject<IGeometrySource2DInterop>(thisPtr).GetGeometry(out var v);
                    if (hr >= 0)
                    {
                        var unk = ComObject.ToComInstanceOfType<ID2D1Geometry>(v);
                        *value = unk;
                    }
                    return hr;
                }
                catch (Exception e)
                {
                    ExceptionHelpers.SetErrorInfo(e);
                    return Marshal.GetHRForException(e);
                }
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int TryGetGeometryUsingFactoryFromAbi(nint thisPtr, nint factory, nint* value)
            {
                try
                {
                    if (value != null)
                    {
                        *value = 0;
                    }

                    if (factory == 0)
                        throw new ArgumentException(null, nameof(factory));

                    var fac = ComObject.FromPointer<ID2D1Factory>(factory);
                    if (fac == null)
                        throw new ArgumentException(null, nameof(factory));

                    var hr = ComWrappersSupport.FindObject<IGeometrySource2DInterop>(thisPtr).TryGetGeometryUsingFactory(fac.Object, out var v);
                    if (hr >= 0)
                    {
                        var unk = ComObject.ToComInstanceOfType<ID2D1Geometry>(v);
                        *value = unk;
                    }
                    return hr;
                }
                catch (Exception e)
                {
                    ExceptionHelpers.SetErrorInfo(e);
                    return Marshal.GetHRForException(e);
                }
            }
        }
    }
}
#pragma warning restore IDE0130 // Namespace does not match folder structure
