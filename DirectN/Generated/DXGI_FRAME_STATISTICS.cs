﻿// c:\program files (x86)\windows kits\10\include\10.0.19041.0\shared\dxgi.h(169,9)
using System;
using System.Runtime.InteropServices;

namespace DirectN
{
    [StructLayout(LayoutKind.Sequential)]
    public partial struct DXGI_FRAME_STATISTICS
    {
        public uint PresentCount;
        public uint PresentRefreshCount;
        public uint SyncRefreshCount;
        public long SyncQPCTime;
        public long SyncGPUTime;
    }
}
