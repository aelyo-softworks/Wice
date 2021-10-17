using System.Runtime.InteropServices;

namespace DirectN
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINTER_PEN_INFO
    {
        public POINTER_INFO pointerInfo;
        public PEN_FLAGS penFlags;
        public PEN_MASK penMask;
        public int pressure;
        public int rotation;
        public int tiltX;
        public int tiltY;
    }
}
