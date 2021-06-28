﻿// c:\program files (x86)\windows kits\10\include\10.0.19041.0\um\d3d11.h(2945,9)
using System;
using System.Runtime.InteropServices;

namespace DirectN
{
    [StructLayout(LayoutKind.Sequential)]
    public partial struct D3D11_TEXTURE3D_DESC
    {
        public uint Width;
        public uint Height;
        public uint Depth;
        public uint MipLevels;
        public DXGI_FORMAT Format;
        public D3D11_USAGE Usage;
        public uint BindFlags;
        public uint CPUAccessFlags;
        public uint MiscFlags;
    }
}
