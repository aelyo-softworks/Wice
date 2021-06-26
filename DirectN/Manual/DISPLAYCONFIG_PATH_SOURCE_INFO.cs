using System.Runtime.InteropServices;

namespace DirectN
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_PATH_SOURCE_INFO
    {
        public _LUID adapterId;
        public uint id;
        public uint modeInfoIdx;
        public DISPLAYCONFIG_SOURCE_FLAGS statusFlags;
    }
}
