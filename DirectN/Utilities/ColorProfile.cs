﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DirectN
{
    public sealed class ColorProfile
    {
        private static readonly Lazy<IReadOnlyList<ColorProfile>> _localProfiles = new Lazy<IReadOnlyList<ColorProfile>>(() => GetColorProfiles(null), true);
        public static IReadOnlyList<ColorProfile> LocalProfiles => _localProfiles.Value;

        private static readonly Lazy<string> _colorDirectoryPath = new Lazy<string>(() => GetColorDirectoryPath(null), true);
        public static string LocalColorDirectoryPath => _colorDirectoryPath.Value;

        private ColorProfile(IntPtr handle)
        {
            var header = new PROFILEHEADER();
            if (!GetColorProfileHeader(handle, ref header))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            Size = header.phSize;
            CmmType = header.phCMMType;
            Version = header.phVersion;
            Class = header.phClass;
            DataColorSpace = header.phDataColorSpace;
            ConnectionSpace = header.phConnectionSpace;
            Signature = header.phSignature;
            Platform = header.phPlatform;
            IsEmbeddedProfile = (header.phProfileFlags & FLAG_EMBEDDEDPROFILE) != 0;
            IsDependentOnData = (header.phProfileFlags & FLAG_DEPENDENTONDATA) != 0;
            Manufacturer = header.phManufacturer;
            Model = header.phModel;
            Attributes = header.phAttributes;
            Creator = header.phCreator;
            RenderingIntent = header.phRenderingIntent;
            Illuminant = header.phIlluminant;

            var size = 0;
            if (!GetColorProfileFromHandle(handle, null, ref size))
            {
                var gle = Marshal.GetLastWin32Error();
                if (gle != ERROR_INSUFFICIENT_BUFFER)
                    throw new Win32Exception(gle);
            }

            Profile = new byte[size];
            if (!GetColorProfileFromHandle(handle, Profile, ref size))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            var localizedStrings = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            GetCountColorProfileElements(handle, out var count);
            for (var i = 0; i < count; i++)
            {
                if (GetColorProfileElementTag(handle, i + 1, out var tag))
                {
                    size = 0;
                    GetColorProfileElement(handle, tag, 0, ref size, null, out _);
                    if (size > 0)
                    {
                        var bytes = new byte[size];
                        if (GetColorProfileElement(handle, tag, 0, ref size, bytes, out _))
                        {
                            int offset;
                            // https://www.color.org/specification/ICC.2-2019.pdf
                            var type = Get4BytesString(BitConverter.ToInt32(bytes, 0));
                            switch (type)
                            {
                                case "text":
                                    offset = 8;
                                    switch (tag)
                                    {
                                        case 0x63707274:
                                            Copyright = getAscii(bytes.Length - 8);
                                            break;

                                        case 0x74617267:
                                            RegisteredCharacterization = getAscii(bytes.Length - 8);
                                            break;

                                        //default:
                                        //    var s = getAscii(bytes.Length - 8);
                                        //    Console.WriteLine(tag.ToString("x8") + " => " + s);
                                        //    break;
                                    }
                                    break;

                                case "desc":
                                    offset = 8;
                                    var n = getInt32();
                                    if (n > 0)
                                    {
                                        Description = getAscii(n);
                                    }

                                    UnicodeLanguageCode = getInt32();
                                    var m = getInt32();
                                    if (m > 0)
                                    {
                                        Description = getUnicode(m);
                                    }
                                    break;

                                case "mluc": // multiLocalizedUnicodeType
                                    offset = 8;
                                    var records = getInt32();
                                    _ = getInt32(); // record size
                                    var languageCode = Get2BytesString(BitConverter.ToInt16(bytes, offset));
                                    offset += 2;
                                    var countryCode = Get2BytesString(BitConverter.ToInt16(bytes, offset));

                                    var lcid = languageCode + "-" + countryCode;
                                    offset += 2;
                                    for (var ir = 0; ir < records; ir++)
                                    {
                                        var sl = getInt32();
                                        var o = offset; // save
                                        offset = getInt32();
                                        var s = getBEUnicode(sl / 2);
                                        offset = o; // restore

                                        if (!localizedStrings.TryGetValue(lcid, out var list))
                                        {
                                            list = new List<string>();
                                            localizedStrings.Add(lcid, list);
                                        }

                                        list.Add(s);
                                    }
                                    break;
                            }

                            int getInt32() => (bytes[offset++] << 24) | (bytes[offset++] << 16) | (bytes[offset++] << 8) | bytes[offset++];
                            string getAscii(int len)
                            {
                                var s = TrimTerminatingZeros(Encoding.ASCII.GetString(bytes, offset, len));
                                offset += len;
                                return s;
                            }

                            string getBEUnicode(int len)
                            {
                                var s = TrimTerminatingZeros(Encoding.BigEndianUnicode.GetString(bytes, offset, len * 2));
                                offset += len;
                                return s;
                            }

                            string getUnicode(int len)
                            {
                                var bom = BitConverter.ToInt16(bytes, offset);
                                if (bom == -2)
                                {
                                    offset += 3;
                                    len -= 2;
                                }

                                var s = TrimTerminatingZeros(Encoding.Unicode.GetString(bytes, offset, len * 2));
                                offset += len;
                                return s;
                            }
                        }
                    }
                }
            }

            LocalizedStrings = localizedStrings.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<string>)kv.Value.AsReadOnly());
        }

        private ColorProfile(IntPtr handle, string filePath)
            : this(handle)
        {
            FilePath = filePath;
        }

        public string FilePath { get; } // null if loaded from memory
        public byte[] Profile { get; }
        public int Size { get; }
        public int Version { get; }
        public int CmmType { get; }
        public int Class { get; }
        public int DataColorSpace { get; }
        public int ConnectionSpace { get; }
        public int Signature { get; }
        public int Platform { get; }
        public int Manufacturer { get; }
        public int Model { get; }
        public int Creator { get; }
        public int[] Attributes { get; }
        public int RenderingIntent { get; }
        public CIEXYZ Illuminant { get; }
        public bool IsEmbeddedProfile { get; }
        public bool IsDependentOnData { get; }
        public string CmmTypeString => Get4BytesString(CmmType);
        public string ClassString => Get4BytesString(Class);
        public string DataColorSpaceString => Get4BytesString(DataColorSpace);
        public string ConnectionSpaceString => Get4BytesString(ConnectionSpace);
        public string SignatureString => Get4BytesString(Signature);
        public string PlatformString => Get4BytesString(Platform);
        public string ManufacturerString => Get4BytesString(Manufacturer);
        public string ModelString => Get4BytesString(Model);
        public string CreatorString => Get4BytesString(Creator);

        public int UnicodeLanguageCode { get; }
        public string Description { get; }
        public IReadOnlyDictionary<string, IReadOnlyList<string>> LocalizedStrings { get; }
        public string Copyright { get; }
        public string RegisteredCharacterization { get; }

        public override string ToString() => Description;

        private static string TrimTerminatingZeros(string str)
        {
            if (str == null || str.Length == 0)
                return null;

            var i = str.Length - 1;
            for (; i >= 0; i--)
            {
                if (str[i] != 0 && !char.IsWhiteSpace(str[i]))
                    break;
            }
            if (i == str.Length - 1)
                return str;

            return str.Substring(0, i + 1);
        }

        private static string Get4BytesString(int value)
        {
            try
            {
                return TrimTerminatingZeros(Encoding.ASCII.GetString(BitConverter.GetBytes(value)));
            }
            catch
            {
                return null;
            }
        }

        private static string Get2BytesString(short value)
        {
            try
            {
                return TrimTerminatingZeros(Encoding.ASCII.GetString(BitConverter.GetBytes(value)));
            }
            catch
            {
                return null;
            }
        }

        public static ColorProfile FromMemory(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentException(nameof(buffer));

            using (var mem = new ComMemory(buffer))
            {
                var prof = new PROFILE();
                prof.dwType = PROFILE_MEMBUFFER;
                prof.pProfileData = mem.Pointer;
                prof.cbDataSize = mem.Size;
                var handle = OpenColorProfile(ref prof, PROFILE_READ, FILE_SHARE_READ, OPEN_EXISTING);
                if (handle == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                try
                {
                    return new ColorProfile(handle);
                }
                finally
                {
                    CloseColorProfile(handle);
                }
            }
        }

        public static ColorProfile FromFileName(string fileName, string machineName = null)
        {
            if (fileName == null)
                throw new ArgumentException(nameof(fileName));

            string path;
            if (fileName.IndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }) < 0)
            {
                path = Path.Combine(GetColorDirectoryPath(machineName), fileName);
            }
            else
            {
                if (machineName != null)
                    throw new ArgumentException(null, nameof(machineName));

                path = Path.GetFullPath(fileName);
            }

            var name = Path.GetFileName(path);

            // note this is ANSI only
            var ptr = Marshal.StringToCoTaskMemAnsi(name);
            try
            {
                var prof = new PROFILE();
                prof.dwType = PROFILE_FILENAME;
                prof.pProfileData = ptr;
                prof.cbDataSize = name.Length + 1;
                var handle = OpenColorProfile(ref prof, PROFILE_READ, FILE_SHARE_READ, OPEN_EXISTING);
                if (handle == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                try
                {
                    return new ColorProfile(handle, path);
                }
                finally
                {
                    CloseColorProfile(handle);
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem(ptr);
            }
        }

        public static string GetColorDirectoryPath(string machineName = null)
        {
            var size = 0;
            if (!GetColorDirectory(machineName, null, ref size))
            {
                var gle = Marshal.GetLastWin32Error();
                if (gle != ERROR_INSUFFICIENT_BUFFER)
                    throw new Win32Exception(gle);
            }

            var str = new string('\0', (size - 1) / 2);
            if (!GetColorDirectory(machineName, str, ref size))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return str;
        }

        public static IReadOnlyList<ColorProfile> GetColorProfiles(string machineName = null)
        {
            var list = new List<ColorProfile>();
            var size = 0;
            var enumType = new ENUMTYPE();
            enumType.dwSize = Marshal.SizeOf(enumType);
            enumType.dwVersion = ENUM_TYPE_VERSION;
            if (!EnumColorProfiles(machineName, ref enumType, IntPtr.Zero, ref size, out var count))
            {
                var gle = Marshal.GetLastWin32Error();
                if (gle != ERROR_INSUFFICIENT_BUFFER)
                    throw new Win32Exception(gle);
            }

            if (count > 0)
            {
                using (var mem = new ComMemory(size))
                {
                    if (!EnumColorProfiles(machineName, ref enumType, mem.Pointer, ref size, out count))
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    var ptr = mem.Pointer;
                    do
                    {
                        var str = Marshal.PtrToStringUni(ptr);
                        if (string.IsNullOrEmpty(str))
                            break;

                        try
                        {
                            var profile = FromFileName(str, machineName);
                            if (profile != null)
                            {
                                list.Add(profile);
                            }
                        }
                        catch
                        {
                            // do nothing
                        }
                        ptr += (str.Length + 1) * 2;
                    }
                    while (true);
                }
            }
            return list;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct ENUMTYPE
        {
            public int dwSize;
            public int dwVersion;
            public int dwFields;
            public string pDeviceName;
            public int dwMediaType;
            public int dwDitheringMode;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] dwResolution;
            public int dwCMMType;
            public int dwClass;
            public int dwDataColorSpace;
            public int dwConnectionSpace;
            public int dwSignature;
            public int dwPlatform;
            public int dwProfileFlags;
            public int dwManufacturer;
            public int dwModel;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] dwAttributes;
            public int dwRenderingIntent;
            public int dwCreator;
            public int dwDeviceClass;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct PROFILE
        {
            public int dwType;
            public IntPtr pProfileData;
            public int cbDataSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROFILEHEADER
        {
            public int phSize;
            public int phCMMType;
            public int phVersion;
            public int phClass;
            public int phDataColorSpace;
            public int phConnectionSpace;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public int[] phDateTime;
            public int phSignature;
            public int phPlatform;
            public int phProfileFlags;
            public int phManufacturer;
            public int phModel;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] phAttributes;
            public int phRenderingIntent;
            public CIEXYZ phIlluminant;
            public int phCreator;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 44)]
            public byte[] phReserved;
        }

        private const int ENUM_TYPE_VERSION = 0x300;
        private const int ERROR_INSUFFICIENT_BUFFER = 122;
        private const int PROFILE_FILENAME = 1;
        private const int PROFILE_MEMBUFFER = 2;
        private const int PROFILE_READ = 1;
        private const int FILE_SHARE_READ = 1;
        private const int OPEN_EXISTING = 3;
        //private const int PROFILE_READWRITE = 2;
        private const int FLAG_EMBEDDEDPROFILE = 0x00000001;
        private const int FLAG_DEPENDENTONDATA = 0x00000002;

        [DllImport("mscms", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool EnumColorProfiles(string pMachineName, ref ENUMTYPE pEnumRecord, IntPtr pEnumerationBuffer, ref int pdwSizeOfEnumerationBuffer, out int pnProfiles);

        [DllImport("mscms", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool GetColorDirectory(string pMachineName, string pBuffer, ref int pdwSize);

        [DllImport("mscms", SetLastError = true)]
        private static extern IntPtr OpenColorProfile(ref PROFILE pProfile, uint dwDesiredAccess, uint dwShareMode, uint dwCreationMode);

        [DllImport("mscms", SetLastError = true)]
        private static extern bool CloseColorProfile(IntPtr hProfile);

        [DllImport("mscms", SetLastError = true)]
        private static extern bool GetColorProfileHeader(IntPtr hProfile, ref PROFILEHEADER pHeader);

        [DllImport("mscms", SetLastError = true)]
        private static extern bool GetColorProfileFromHandle(IntPtr hProfile, [In, Out] byte[] pProfile, ref int pcbProfile);

        [DllImport("mscms", SetLastError = true)]
        private static extern bool GetCountColorProfileElements(IntPtr hProfile, out int pnElementCount);

        [DllImport("mscms", SetLastError = true)]
        private static extern bool GetColorProfileElementTag(IntPtr hProfile, int dwIndex, out int pTag);

        [DllImport("mscms", SetLastError = true)]
        private static extern bool GetColorProfileElement(IntPtr hProfile, int tag, int dwOffset, ref int pcbElement, [In, Out] byte[] pElement, out bool pbReference);
    }
}
