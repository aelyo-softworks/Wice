﻿// c:\program files (x86)\windows kits\10\include\10.0.22000.0\um\wingdi.h(3294,9)
using System;
using System.Runtime.InteropServices;

namespace DirectN
{
    [StructLayout(LayoutKind.Sequential)]
    public partial struct _RGNDATAHEADER
    {
        public uint dwSize;
        public uint iType;
        public uint nCount;
        public uint nRgnSize;
        public tagRECT rcBound;
    }
}
