using System.Runtime.InteropServices;

namespace DirectN
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DISPLAYCONFIG_TARGET_DEVICE_NAME
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
        public DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS flags;
        public DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY outputTechnology;
        public ushort edidManufactureId;
        public ushort edidProductCodeId;
        public uint connectorInstance;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string monitorFriendlyDeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string monitorDevicePath;

        public string OutputTechnologyName
        {
            get
            {
                var name = outputTechnology.ToString();
                const string prefix = "DISPLAYCONFIG_OUTPUT_TECHNOLOGY_";
                if (!name.StartsWith(prefix))
                    return name;

                return Conversions.Decamelize(name.Substring(prefix.Length), DecamelizeOptions.ForceFirstUpper | DecamelizeOptions.ForceRestLower);
            }
        }

        public override string ToString() => monitorFriendlyDeviceName + " (" + OutputTechnologyName + ")";
    }
}
