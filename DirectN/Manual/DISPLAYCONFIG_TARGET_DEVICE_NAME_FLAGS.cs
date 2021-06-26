using System.Runtime.InteropServices;

namespace DirectN
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS
    {
        public uint value;

        public bool FriendlyNameFromEdid => (value & 0x1) == 0x1;
        public bool FriendlyNameForced => (value & 0x2) == 0x2;
        public bool EdidIdsValid => (value & 0x4) == 0x4;
    }
}
