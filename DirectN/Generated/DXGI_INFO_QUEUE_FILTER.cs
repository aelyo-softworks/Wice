﻿// c:\program files (x86)\windows kits\10\include\10.0.22000.0\um\dxgidebug.h(156,9)
using System.Runtime.InteropServices;

namespace DirectN
{
    [StructLayout(LayoutKind.Sequential)]
    public partial struct DXGI_INFO_QUEUE_FILTER
    {
        public DXGI_INFO_QUEUE_FILTER_DESC AllowList;
        public DXGI_INFO_QUEUE_FILTER_DESC DenyList;
    }
}
