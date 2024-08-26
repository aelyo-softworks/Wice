using WinRT.Interop;

// see here https://github.com/microsoft/CsWinRT/issues/1722

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace ABI.Windows.Graphics.Effects
{
    // all names here are hardcoded in WinRT's generator (ABI., Methods, IID, AbiToProjectionVftablePtr)
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class IGraphicsEffectD2D1InteropMethods
    {
        public static Guid IID { get; } = typeof(global::Windows.Graphics.Effects.IGraphicsEffectD2D1Interop).GUID;
        public static nint AbiToProjectionVftablePtr { get; } = global::Windows.Graphics.Effects.IGraphicsEffectD2D1Interop.Vftbl.InitVtbl();
    }
}

namespace Windows.Graphics.Effects
{
    [Guid("2fc57384-a068-44d7-a331-30982fcf7177")]
    [WindowsRuntimeType]
    [WindowsRuntimeHelperType(typeof(IGraphicsEffectD2D1Interop))]
    public partial interface IGraphicsEffectD2D1Interop
    {
        HRESULT GetEffectId(out Guid id);
        HRESULT GetNamedPropertyMapping(PWSTR name, out uint index, out GRAPHICS_EFFECT_PROPERTY_MAPPING mapping);
        HRESULT GetPropertyCount(out uint count);
        HRESULT GetProperty(uint index, out nint value);
        HRESULT GetSource(uint index, out IGraphicsEffectSource? source);
        HRESULT GetSourceCount(out uint count);

        // v-table
        internal unsafe struct Vftbl
        {
            public static nint InitVtbl()
            {
                var lpVtbl = (Vftbl*)ComWrappersSupport.AllocateVtableMemory(typeof(Vftbl), sizeof(Vftbl));

                lpVtbl->IUnknownVftbl = IUnknownVftbl.AbiToProjectionVftbl;
                lpVtbl->GetEffectId = &GetEffectIdFromAbi;
                lpVtbl->GetNamedPropertyMapping = &GetNamedPropertyMappingFromAbi;
                lpVtbl->GetPropertyCount = &GetPropertyCountFromAbi;
                lpVtbl->GetProperty = &GetPropertyFromAbi;
                lpVtbl->GetSource = &GetSourceFromAbi;
                lpVtbl->GetSourceCount = &GetSourceCountFromAbi;
                return (nint)lpVtbl;
            }

            private IUnknownVftbl IUnknownVftbl;

            // interface delegates
            private delegate* unmanaged[MemberFunction]<nint, Guid*, int> GetEffectId;
            private delegate* unmanaged[MemberFunction]<nint, PWSTR, uint*, GRAPHICS_EFFECT_PROPERTY_MAPPING*, int> GetNamedPropertyMapping;
            private delegate* unmanaged[MemberFunction]<nint, uint*, int> GetPropertyCount;
            private delegate* unmanaged[MemberFunction]<nint, uint, nint*, int> GetProperty;
            private delegate* unmanaged[MemberFunction]<nint, uint, nint*, int> GetSource;
            private delegate* unmanaged[MemberFunction]<nint, uint*, int> GetSourceCount;

            // interface implementation
            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
            private static int GetEffectIdFromAbi(nint thisPtr, Guid* value)
            {
                try
                {
                    if (value != null)
                    {
                        *value = Guid.Empty;
                    }

                    var hr = ComWrappersSupport.FindObject<IGraphicsEffectD2D1Interop>(thisPtr).GetEffectId(out var v);
                    if (hr >= 0)
                    {
                        if (value != null)
                        {
                            *value = v;
                        }
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
            private static int GetNamedPropertyMappingFromAbi(nint thisPtr, PWSTR name, uint* index, GRAPHICS_EFFECT_PROPERTY_MAPPING* mapping)
            {
                try
                {
                    if (index != null)
                    {
                        *index = 0;
                    }

                    if (mapping != null)
                    {
                        *mapping = 0;
                    }

                    var hr = ComWrappersSupport.FindObject<IGraphicsEffectD2D1Interop>(thisPtr).GetNamedPropertyMapping(name, out var i, out var m);
                    if (hr >= 0)
                    {
                        if (index != null)
                        {
                            *index = i;
                        }

                        if (mapping != null)
                        {
                            *mapping = m;
                        }
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
            private static int GetPropertyCountFromAbi(nint thisPtr, uint* value)
            {
                try
                {
                    if (value != null)
                    {
                        *value = 0;
                    }

                    var hr = ComWrappersSupport.FindObject<IGraphicsEffectD2D1Interop>(thisPtr).GetPropertyCount(out var v);
                    if (hr >= 0)
                    {
                        *value = v;
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
            private static int GetPropertyFromAbi(nint thisPtr, uint index, nint* value)
            {
                try
                {
                    if (value != null)
                    {
                        *value = 0;
                    }

                    var hr = ComWrappersSupport.FindObject<IGraphicsEffectD2D1Interop>(thisPtr).GetProperty(index, out var v);
                    if (hr >= 0)
                    {
                        *value = v;
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
            private static int GetSourceFromAbi(nint thisPtr, uint index, nint* value)
            {
                try
                {
                    if (value != null)
                    {
                        *value = 0;
                    }

                    var hr = ComWrappersSupport.FindObject<IGraphicsEffectD2D1Interop>(thisPtr).GetSource(index, out var v);
                    if (hr >= 0)
                    {
                        var unk = MarshalInspectable<IGraphicsEffectSource>.FromManaged(v!);
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
            private static int GetSourceCountFromAbi(nint thisPtr, uint* value)
            {
                try
                {
                    if (value != null)
                    {
                        *value = 0;
                    }

                    var hr = ComWrappersSupport.FindObject<IGraphicsEffectD2D1Interop>(thisPtr).GetSourceCount(out var v);
                    if (hr >= 0)
                    {
                        *value = v;
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
