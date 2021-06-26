using System.Runtime.InteropServices;

namespace DirectN
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_DESKTOP_IMAGE_INFO
    {
        public tagPOINT PathSourceSize;
        public tagRECT DesktopImageRegion;
        public tagRECT DesktopImageClip;
    }
}
