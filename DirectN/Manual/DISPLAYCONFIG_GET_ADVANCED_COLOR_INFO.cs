using System.Runtime.InteropServices;

namespace DirectN
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
        public uint value;
        public DISPLAYCONFIG_COLOR_ENCODING colorEncoding;
        public int bitsPerColorChannel;

        public bool AdvancedColorSupported => (value & 0x1) == 0x1;
        public bool AdvancedColorEnabled => (value & 0x2) == 0x2;
        public bool WideColorEnforced => (value & 0x4) == 0x4;
        public bool AdvancedColorForceDisabled => (value & 0x8) == 0x8;
    }
}
