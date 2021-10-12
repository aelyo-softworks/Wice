﻿// c:\program files (x86)\windows kits\10\include\10.0.22000.0\um\d3d11.h(6869,5)
using System;
using System.Runtime.InteropServices;

namespace DirectN
{
    [ComImport, Guid("6e8c49fb-a371-4770-b440-29086022b741"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public partial interface ID3D11Counter : ID3D11Asynchronous
    {
        // ID3D11DeviceChild
        [PreserveSig]
        new void GetDevice(/* [annotation] _Outptr_ */ out ID3D11Device ppDevice);
        
        [PreserveSig]
        new HRESULT GetPrivateData(/* [annotation] _In_ */ [MarshalAs(UnmanagedType.LPStruct)] Guid guid, /* [annotation] _Inout_ */ ref uint pDataSize, /* optional(void) */ IntPtr pData);
        
        [PreserveSig]
        new HRESULT SetPrivateData(/* [annotation] _In_ */ [MarshalAs(UnmanagedType.LPStruct)] Guid guid, /* [annotation] _In_ */ uint DataSize, /* optional(void) */ IntPtr pData);
        
        [PreserveSig]
        new HRESULT SetPrivateDataInterface(/* [annotation] _In_ */ [MarshalAs(UnmanagedType.LPStruct)] Guid guid, /* optional(IUnknown) */ IntPtr pData);
        
        // ID3D11Asynchronous
        [PreserveSig]
        new uint GetDataSize();
        
        // ID3D11Counter
        [PreserveSig]
        void GetDesc(/* [annotation] _Out_ */ out D3D11_COUNTER_DESC pDesc);
    }
}
