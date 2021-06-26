using System.Runtime.InteropServices;

namespace DirectN
{
    [StructLayout(LayoutKind.Sequential)]
    public struct tagPOINTS
    {
        public short x;
        public short y;

        public override string ToString() => "X=" + x + ",Y=" + y;
    }
}
