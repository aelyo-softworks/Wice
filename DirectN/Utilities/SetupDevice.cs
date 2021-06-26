using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DirectN
{
    public class SetupDevice
    {
        //private static readonly PROPERTYKEY _parentPk = new PROPERTYKEY(new Guid("4340a6c5-93fa-4706-972c-7b648008a5a7"), 8);
        //private static readonly PROPERTYKEY _childrenPk = new PROPERTYKEY(new Guid("4340a6c5-93fa-4706-972c-7b648008a5a7"), 9);
        //private readonly Lazy<SetupDevice> _parent;
        //private readonly Lazy<IReadOnlyList<SetupDevice>> _children;

        private readonly Dictionary<PROPERTYKEY, object> _properties = new Dictionary<PROPERTYKEY, object>();

        private SetupDevice()
        {
            //_parent = new Lazy<SetupDevice>(GetParent);
            //_children = new Lazy<IReadOnlyList<SetupDevice>>(GetChildren);
        }

        public IReadOnlyDictionary<PROPERTYKEY, object> Properties => _properties;
        public string DriverVersion => GetPropertyValue<string>(PROPERTYKEY.DEVPKEY_Device_DriverVersion);
        public string DriverDesc => GetPropertyValue<string>(PROPERTYKEY.DEVPKEY_Device_DriverDesc);
        public string FriendlyName => GetPropertyValue<string>(PROPERTYKEY.DEVPKEY_Device_FriendlyName);
        public string Manufacturer => GetPropertyValue<string>(PROPERTYKEY.DEVPKEY_Device_Manufacturer);
        public string DriverProvider => GetPropertyValue<string>(PROPERTYKEY.DEVPKEY_Device_DriverProvider);
        public string Name => GetPropertyValue<string>(PROPERTYKEY.DEVPKEY_NAME);

        public T GetPropertyValue<T>(PROPERTYKEY pk, T defaultValue = default)
        {
            if (!_properties.TryGetValue(pk, out var value))
                return defaultValue;

            if (value == null)
                return default;

            if (!typeof(T).IsAssignableFrom(value.GetType()))
                return defaultValue;

            return (T)value;
        }

        //public SetupDevice Parent => _parent.Value;
        //public IReadOnlyList<SetupDevice> Children => _children.Value;

        //private SetupDevice GetParent()
        //{
        //    _properties.TryGetValue(_parentPk, out var obj);
        //    if (!(obj is string id))
        //        return null;

        //    return LoadFromId(id);
        //}

        //private IReadOnlyList<SetupDevice> GetChildren()
        //{
        //    var list = new List<SetupDevice>();
        //    _properties.TryGetValue(_childrenPk, out var obj);
        //    if (obj is string[] ids)
        //    {
        //        foreach (var id in ids)
        //        {
        //            var dev = LoadFromId(id);
        //            if (dev != null)
        //            {
        //                list.Add(dev);
        //            }
        //        }
        //    }
        //    return list.AsReadOnly();
        //}

        public static SetupDevice LoadFromPath(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var INVALID_HANDLE_VALUE = (IntPtr)(-1);
            var list = SetupDiCreateDeviceInfoList(IntPtr.Zero, IntPtr.Zero);
            if (list == INVALID_HANDLE_VALUE)
                return null;

            try
            {
                var idata = new SP_DEVICE_INTERFACE_DATA();
                idata.cbSize = Marshal.SizeOf<SP_DEVICE_INTERFACE_DATA>();
                if (SetupDiOpenDeviceInterface(list, path, 0, out idata))
                {
                    var size = 0;
                    SetupDiGetDeviceInterfaceDetail(list, ref idata, IntPtr.Zero, 0, ref size, IntPtr.Zero);
                    if (size == 0)
                        return null;

                    var ptr = Marshal.AllocHGlobal(size);
                    Marshal.WriteInt32(ptr, Marshal.SizeOf<SP_DEVINFO_DATA>());
                    try
                    {
                        var newSize = size;
                        SetupDiGetDeviceInterfaceDetail(list, ref idata, IntPtr.Zero, 0, ref size, ptr); // seems buggy, returns 122 on second call too
                        if (size != newSize)
                            return null;

                        var count = 0;
                        SetupDiGetDevicePropertyKeys(list, ptr, null, 0, ref count, 0);
                        if (count == 0)
                            return null;

                        var pks = new PROPERTYKEY[count];
                        if (SetupDiGetDevicePropertyKeys(list, ptr, pks, count, ref count, 0))
                        {
                            var device = new SetupDevice();
                            foreach (var pk in pks)
                            {
                                var propSize = 0;
                                var pk2 = pk;
                                SetupDiGetDeviceProperty(list, ptr, ref pk2, out var propType, IntPtr.Zero, 0, ref propSize, 0);
                                if (propSize > 0)
                                {
                                    var propPtr = Marshal.AllocHGlobal(propSize);
                                    try
                                    {
                                        SetupDiGetDeviceProperty(list, ptr, ref pk2, out propType, propPtr, propSize, ref propSize, 0);
                                        var value = GetPropertyValue(propType, propPtr, propSize);
                                        device._properties[pk] = value;
                                    }
                                    finally
                                    {
                                        Marshal.FreeHGlobal(propPtr);
                                    }
                                }
                            }
                            return device;
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                }
            }
            finally
            {
                SetupDiDestroyDeviceInfoList(list);
            }
            return null;
        }

        //public static SetupDevice LoadFromId(string id)
        //{
        //    if (id == null)
        //        throw new ArgumentNullException(nameof(id));

        //    var INVALID_HANDLE_VALUE = (IntPtr)(-1);
        //    var list = SetupDiCreateDeviceInfoList(IntPtr.Zero, IntPtr.Zero);
        //    if (list == INVALID_HANDLE_VALUE)
        //        return null;

        //    try
        //    {
        //        if (SetupDiOpenDeviceInfo(list, id, IntPtr.Zero, 0, out var data))
        //        {
        //            var count = 0;
        //            SetupDiGetDevicePropertyKeys(list, ref data, null, 0, ref count, 0);
        //            if (count == 0)
        //                return null;

        //            var pks = new PROPERTYKEY[count];
        //            if (SetupDiGetDevicePropertyKeys(list, ref data, pks, count, ref count, 0))
        //            {
        //                var device = new SetupDevice();
        //                foreach (var pk in pks)
        //                {
        //                    var propSize = 0;
        //                    var pk2 = pk;
        //                    SetupDiGetDeviceProperty(list, ref data, ref pk2, out var propType, IntPtr.Zero, 0, ref propSize, 0);
        //                    if (propSize > 0)
        //                    {
        //                        var propPtr = Marshal.AllocHGlobal(propSize);
        //                        try
        //                        {
        //                            SetupDiGetDeviceProperty(list, ref data, ref pk2, out propType, propPtr, propSize, ref propSize, 0);
        //                            var value = GetPropertyValue(propType, propPtr, propSize);
        //                            device._properties[pk] = value;
        //                        }
        //                        finally
        //                        {
        //                            Marshal.FreeHGlobal(propPtr);
        //                        }
        //                    }
        //                }
        //                return device;
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        SetupDiDestroyDeviceInfoList(list);
        //    }
        //    return null;
        //}

        private static object GetPropertyValue(DEVPROPTYPE type, IntPtr ptr, int size)
        {
            switch (type & ~DEVPROPTYPE.DEVPROP_MASK_TYPEMOD)
            {
                case DEVPROPTYPE.DEVPROP_TYPE_STRING:
                case DEVPROPTYPE.DEVPROP_TYPE_STRING_INDIRECT:
                case DEVPROPTYPE.DEVPROP_TYPE_SECURITY_DESCRIPTOR_STRING:
                    if (type.HasFlag(DEVPROPTYPE.DEVPROP_TYPEMOD_LIST))
                        return Marshal.PtrToStringUni(ptr, size / 2).Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);

                    return Marshal.PtrToStringUni(ptr);

                case DEVPROPTYPE.DEVPROP_TYPE_BOOLEAN:
                    return Marshal.ReadByte(ptr) != 0;

                case DEVPROPTYPE.DEVPROP_TYPE_UINT32:
                    return (uint)Marshal.ReadInt32(ptr);

                case DEVPROPTYPE.DEVPROP_TYPE_UINT16:
                    return (ushort)Marshal.ReadInt16(ptr);

                case DEVPROPTYPE.DEVPROP_TYPE_UINT64:
                    return (ulong)Marshal.ReadInt64(ptr);

                case DEVPROPTYPE.DEVPROP_TYPE_INT32:
                case DEVPROPTYPE.DEVPROP_TYPE_ERROR:
                case DEVPROPTYPE.DEVPROP_TYPE_NTSTATUS:
                    return Marshal.ReadInt32(ptr);

                case DEVPROPTYPE.DEVPROP_TYPE_INT16:
                    return Marshal.ReadInt16(ptr);

                case DEVPROPTYPE.DEVPROP_TYPE_INT64:
                case DEVPROPTYPE.DEVPROP_TYPE_CURRENCY:
                    return Marshal.ReadInt64(ptr);

                case DEVPROPTYPE.DEVPROP_TYPE_FILETIME:
                    var ft = Marshal.ReadInt64(ptr);
                    return DateTime.FromFileTime(ft);

                case DEVPROPTYPE.DEVPROP_TYPE_GUID:
                    return new Guid(readBytes(16));

                case DEVPROPTYPE.DEVPROP_TYPE_EMPTY:
                case DEVPROPTYPE.DEVPROP_TYPE_NULL:
                    return null;

                case DEVPROPTYPE.DEVPROP_TYPE_SBYTE:
                    return (sbyte)Marshal.ReadByte(ptr);

                case DEVPROPTYPE.DEVPROP_TYPE_BYTE:
                    return Marshal.ReadByte(ptr);

                case DEVPROPTYPE.DEVPROP_TYPE_FLOAT:
                    return BitConverter.ToSingle(readBytes(4), 0);

                case DEVPROPTYPE.DEVPROP_TYPE_DOUBLE:
                    return BitConverter.ToDouble(readBytes(8), 0);

                case DEVPROPTYPE.DEVPROP_TYPE_DATE:
                    return DateTime.FromOADate(BitConverter.ToDouble(readBytes(8), 0));

                case DEVPROPTYPE.DEVPROP_TYPE_DEVPROPKEY:
                    var dpk = new PROPERTYKEY();
                    dpk.FormatId = new Guid(readBytes(16));
                    dpk.Id = Marshal.ReadInt32(ptr, 16);
                    return dpk;

                case DEVPROPTYPE.DEVPROP_TYPE_DEVPROPTYPE:
                    return (DEVPROPTYPE)Marshal.ReadInt32(ptr);

                case DEVPROPTYPE.DEVPROP_TYPE_SECURITY_DESCRIPTOR:
                case DEVPROPTYPE.DEVPROP_TYPE_BINARY:
                case DEVPROPTYPE.DEVPROP_TYPE_DECIMAL:
                default:
                    // note we don't handle DEVPROP_TYPEMOD_ARRAY
                    return readBytes(size);
            }

            byte[] readBytes(int bs)
            {
                var bytes = new byte[bs];
                for (var i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = Marshal.ReadByte(ptr + i);
                }
                return bytes;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid InterfaceClassGuid;
            public int Flags;
            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_DEVINFO_DATA
        {
            public int cbSize;
            public Guid ClassGuid;
            public int DevInst;
            public IntPtr Reserved;
        }

        [Flags]
        private enum DEVPROPTYPE
        {
            DEVPROP_TYPEMOD_ARRAY = 0x00001000,// array of fixed-sized data elements
            DEVPROP_TYPEMOD_LIST = 0x00002000,// list of variable-sized data elements
            DEVPROP_TYPE_EMPTY = 0x00000000,// nothing, no property data
            DEVPROP_TYPE_NULL = 0x00000001,// null property data
            DEVPROP_TYPE_SBYTE = 0x00000002,// 8-bit signed int (SBYTE)
            DEVPROP_TYPE_BYTE = 0x00000003,// 8-bit unsigned int (BYTE)
            DEVPROP_TYPE_INT16 = 0x00000004,// 16-bit signed int (SHORT)
            DEVPROP_TYPE_UINT16 = 0x00000005,// 16-bit unsigned int (USHORT)
            DEVPROP_TYPE_INT32 = 0x00000006,// 32-bit signed int (LONG)
            DEVPROP_TYPE_UINT32 = 0x00000007,// 32-bit unsigned int (ULONG)
            DEVPROP_TYPE_INT64 = 0x00000008,// 64-bit signed int (LONG64)
            DEVPROP_TYPE_UINT64 = 0x00000009,// 64-bit unsigned int (ULONG64)
            DEVPROP_TYPE_FLOAT = 0x0000000A,// 32-bit floating-point (FLOAT)
            DEVPROP_TYPE_DOUBLE = 0x0000000B,// 64-bit floating-point (DOUBLE)
            DEVPROP_TYPE_DECIMAL = 0x0000000C,// 128-bit data (DECIMAL)
            DEVPROP_TYPE_GUID = 0x0000000D,// 128-bit unique identifier (GUID)
            DEVPROP_TYPE_CURRENCY = 0x0000000E,// 64 bit signed int currency value (CURRENCY)
            DEVPROP_TYPE_DATE = 0x0000000F,// date (DATE)
            DEVPROP_TYPE_FILETIME = 0x00000010,// file time (FILETIME)
            DEVPROP_TYPE_BOOLEAN = 0x00000011,// 8-bit boolean (DEVPROP_BOOLEAN)
            DEVPROP_TYPE_STRING = 0x00000012,// null-terminated string
            DEVPROP_TYPE_STRING_LIST = (DEVPROP_TYPE_STRING | DEVPROP_TYPEMOD_LIST),// multi-sz string list
            DEVPROP_TYPE_SECURITY_DESCRIPTOR = 0x00000013, // self-relative binary SECURITY_DESCRIPTOR
            DEVPROP_TYPE_SECURITY_DESCRIPTOR_STRING = 0x00000014, // security descriptor string (SDDL format)
            DEVPROP_TYPE_DEVPROPKEY = 0x00000015,  // device property key (DEVPROPKEY)
            DEVPROP_TYPE_DEVPROPTYPE = 0x00000016, // device property type (DEVPROPTYPE)
            DEVPROP_TYPE_BINARY = (DEVPROP_TYPE_BYTE | DEVPROP_TYPEMOD_ARRAY),  // custom binary data
            DEVPROP_TYPE_ERROR = 0x00000017, // 32-bit Win32 system error code
            DEVPROP_TYPE_NTSTATUS = 0x00000018, // 32-bit NTSTATUS code
            DEVPROP_TYPE_STRING_INDIRECT = 0x00000019, // string resource (@[path\]<dllname>,-<strId>)
            DEVPROP_MASK_TYPE = 0x00000FFF, // range for base DEVPROP_TYPE_ values
            DEVPROP_MASK_TYPEMOD = 0x0000F000, // mask for DEVPROP_TYPEMOD_ type modifiers
        }

        [DllImport("setupapi")]
        private static extern IntPtr SetupDiCreateDeviceInfoList(IntPtr ClassGuid, IntPtr hwndParent);

        [DllImport("setupapi", CharSet = CharSet.Unicode)]
        private static extern bool SetupDiOpenDeviceInterface(IntPtr DeviceInfoSet, string DevicePath, int OpenFlags, out SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

        [DllImport("setupapi", CharSet = CharSet.Unicode)]
        private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, int DeviceInterfaceDetailDataSize, ref int RequiredSize, IntPtr DeviceInfoData);

        [DllImport("setupapi")]
        private static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport("setupapi")]
        private static extern bool SetupDiGetDevicePropertyKeys(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] PROPERTYKEY[] PropertyKeyArray, int PropertyKeyCount, ref int RequiredPropertyKeyCount, int Flags);

        [DllImport("setupapi")]
        private static extern bool SetupDiGetDevicePropertyKeys(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] PROPERTYKEY[] PropertyKeyArray, int PropertyKeyCount, ref int RequiredPropertyKeyCount, int Flags);

        [DllImport("setupapi", CharSet = CharSet.Unicode)]
        private static extern bool SetupDiGetDeviceProperty(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, ref PROPERTYKEY PropertyKey, out DEVPROPTYPE PropertyType, IntPtr PropertyBuffer, int PropertyBufferSize, ref int RequiredSize, int Flags);

        [DllImport("setupapi", CharSet = CharSet.Unicode)]
        private static extern bool SetupDiGetDeviceProperty(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, ref PROPERTYKEY PropertyKey, out DEVPROPTYPE PropertyType, IntPtr PropertyBuffer, int PropertyBufferSize, ref int RequiredSize, int Flags);

        [DllImport("setupapi", CharSet = CharSet.Unicode)]
        private static extern bool SetupDiOpenDeviceInfo(IntPtr DeviceInfoSet, string DeviceInstanceId, IntPtr hwndParent, int OpenFlags, out SP_DEVINFO_DATA DeviceInfoData);
    }
}
