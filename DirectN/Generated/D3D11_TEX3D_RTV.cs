﻿// c:\program files (x86)\windows kits\10\include\10.0.22000.0\um\d3d11.h(3904,9)
using System;
using System.Runtime.InteropServices;

namespace DirectN
{
    [StructLayout(LayoutKind.Sequential)]
    public partial struct D3D11_TEX3D_RTV
    {
        public uint MipSlice;
        public uint FirstWSlice;
        public uint WSize;
    }
}
