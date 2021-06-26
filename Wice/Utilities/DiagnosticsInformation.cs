using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
#if NETFRAMEWORK
using System.Management;
#endif
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using DirectN;

namespace Wice.Utilities
{
    public class DiagnosticsInformation
    {
        public DiagnosticsInformation(Assembly assembly = null)
        {
            Assembly = assembly ?? Assembly.GetExecutingAssembly();
        }

        [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern int GetSystemMetrics(SM nIndex);

        [Browsable(false)]
        public Assembly Assembly { get; }

        [Category("Windows")]
        public string OSVersion => Environment.OSVersion.VersionString;

        [Category("Windows")]
        public string KernelVersion => WindowsVersionUtilities.KernelVersion?.ToString();

#if NETFRAMEWORK
        [Category("Windows")]
        public string WindowsVersion => GetManagementInfo<string>("Win32_OperatingSystem", "Version", null);

        [Category("Windows")]
        public string Caption => GetManagementInfo<string>("Win32_OperatingSystem", "Caption", null);

        [Category("System")]
        public string BoardModel => GetManagementInfo<string>("Win32_ComputerSystem", "Model", null);

        [Category("System")]
        public string BoardManufacturer => GetManagementInfo<string>("Win32_ComputerSystem", "Manufacturer", null);

        [Category("System")]
        public string Processor => GetManagementInfo<string>("Win32_Processor", "Name", null);
#endif

        [Category("System")]
        public bool IsTabletPC => GetSystemMetrics(SM.SM_TABLETPC) != 0;

        [Category("System")]
        public bool IsRemotelyControlled => GetSystemMetrics(SM.SM_REMOTECONTROL) != 0;

        [Category("System")]
        public bool IsRemoteSession => GetSystemMetrics(SM.SM_REMOTESESSION) != 0;

        [Category("System")]
        public string VirtualMachineType => GetVirtualMachineType();

        [Category("System")]
        public int ProcessorCount => Environment.ProcessorCount;

        [Category("System")]
        public ProcessorArchitecture ProcessorArchitecture => GetProcessorArchitecture();

        [Category(".NET")]
        public IReadOnlyList<Version> InstalledFrameworkVersions => GetInstalledFrameworkVersions();

        [Category(".NET")]
        public string CorLibVersion => typeof(int).Assembly.GetInformationalVersion();

        [Category(".NET")]
        public string SystemVersion => typeof(Uri).Assembly.GetInformationalVersion();

        [Category(".NET")]
        public Version ClrVersion => Environment.Version;

        [Category("Process")]
        public TokenElevationType TokenElevationType => GetTokenElevationType();

        [Category("Process")]
        public string Bitness => GetBitness();

        [Category("Process")]
        public string CurrentCulture => CultureInfo.CurrentCulture.Name;

        [Category("Process")]
        public string CurrentUICulture => CultureInfo.CurrentUICulture.Name;

        [Category("Process")]
        public DateTime UtcNow => DateTime.UtcNow;

        [Category("Process")]
        public DateTime Now => DateTime.Now;

        [Category("Process")]
        public string InstalledUICulture => CultureInfo.InstalledUICulture.Name;

        [Category("Shell")]
        public float DesktopDpiX => GetDpiSettings().Width;

        [Category("Shell")]
        public float DesktopDpiY => GetDpiSettings().Height;

        [Category("Shell")]
        public string Screens => string.Join(" | ", System.Windows.Forms.Screen.AllScreens.Select(s
            => "Name: " + s.DeviceName + (s.Primary ? "(Primary)" : null) + " Bpp: " + s.BitsPerPixel + " Bounds: " + s.Bounds + " WorkingArea: " + s.WorkingArea));

        [Category("Software")]
        public Version AssemblyVersion
        {
            get
            {
                var v = Assembly.GetInformationalVersion();
                if (v != null)
                {
                    try
                    {
                        return new Version(v);
                    }
                    catch
                    {
                        // do nothing
                    }
                }
                return new Version(0, 0);
            }
        }

        [Category("Software")]
        public DateTime? AssemblyCompileDate => Assembly.GetLinkerTimestampUtc()?.ToLocalTime();

        [Category("Software")]
        public string AssemblyConfiguration => Assembly.GetConfiguration();

        [Category("Software")]
        public string AssemblyDisplayName
        {
            get
            {
                var conf = AssemblyConfiguration.Nullify();
                if (conf != null)
                {
                    conf += " - ";
                }
                var name = "Version " + AssemblyVersion + " - " + conf + Bitness;
                var dt = AssemblyCompileDate;
                if (dt.HasValue)
                {
                    name += " - Compiled " + dt.Value;
                }
                return name;
            }
        }

        public static SizeF GetDpiSettings()
        {
            try
            {
                using (var g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    return new SizeF(g.DpiX, g.DpiY);
                }
            }
            catch
            {
                return new SizeF();
            }
        }

        public static TokenElevationType GetTokenElevationType()
        {
            var type = TokenElevationType.Unknown;
            var size = IntPtr.Size;
            if (!OpenProcessToken(GetCurrentProcess(), TOKEN_QUERY, out IntPtr handle))
                return type;

            try
            {
                GetTokenInformation(handle, TokenElevationTypeInformation, out type, size, out int returnLength);
                return type;
            }
            finally
            {
                CloseHandle(handle);
            }
        }

        public static string GetBitness()
        {
            if (IntPtr.Size == 8)
                return "64-bit";

            if (Environment.Is64BitOperatingSystem)
                return "32-bit on a 64-bit OS";

            return "32-bit";
        }

        public static ProcessorArchitecture GetProcessorArchitecture()
        {
            var si = new SYSTEM_INFO();
            GetNativeSystemInfo(ref si);
            switch (si.wProcessorArchitecture)
            {
                case PROCESSOR_ARCHITECTURE_AMD64:
                    return ProcessorArchitecture.Amd64;

                case PROCESSOR_ARCHITECTURE_IA64:
                    return ProcessorArchitecture.IA64;

                case PROCESSOR_ARCHITECTURE_INTEL:
                    return ProcessorArchitecture.X86;

                default:
                    return ProcessorArchitecture.None;
            }
        }

        public static IReadOnlyList<Version> GetInstalledFrameworkVersions()
        {
            var versions = new List<Version>();
            //+ http://astebner.sts.winisp.net/Tools/detectFX.cpp.txt
            using (var ndpKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP"))
            {
                if (ndpKey != null)
                {
                    foreach (var keyName in ndpKey.GetSubKeyNames())
                    {
                        if (keyName != null && !keyName.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                            continue;

                        using (var key = ndpKey.OpenSubKey(keyName))
                        {
                            if (key == null || "deprecated".Equals(key.GetValue(null)))
                                continue;

                            var s = key.GetValue("Version", null) as string;
                            if (string.IsNullOrEmpty(s)) // FX4+?
                            {
                                foreach (var skeyName in key.GetSubKeyNames())
                                {
                                    using (var sk = key.OpenSubKey(skeyName))
                                    {
                                        if (sk == null)
                                            continue;

                                        s = sk.GetValue("Version", null) as string;
                                        if (!string.IsNullOrEmpty(s))
                                            break;
                                    }
                                }
                            }

                            if (string.IsNullOrEmpty(s))
                            {
                                //+ FX1
                                s = keyName.Substring(1);
                            }

                            if (Version.TryParse(s, out Version v))
                            {
                                versions.Add(v);
                            }
                        }
                    }

                }
            }
            versions.Sort(); // Version is IComparable
            return versions;
        }

#if NETFRAMEWORK
        public static T GetManagementInfo<T>(string className, string propertyName, T defaultValue)
        {
            if (className == null)
                throw new ArgumentNullException(nameof(className));

            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            try
            {
                foreach (ManagementObject mo in new ManagementObjectSearcher(new WqlObjectQuery("select * from " + className)).Get())
                {
                    foreach (PropertyData data in mo.Properties)
                    {
                        if (data == null || data.Name == null)
                            continue;

                        if (data.Name.EqualsIgnoreCase(propertyName))
                            return Conversions.ChangeType(data.Value, defaultValue);
                    }
                }
            }
            catch
            {
                // do nothing
            }
            return defaultValue;
        }
#endif

        private static bool SearchASCIICaseInsensitive(byte[] bytes, string asciiString)
        {
            if (bytes == null || bytes.Length == 0)
                return false;

            var s = Encoding.ASCII.GetBytes(asciiString);
            if (s.Length > bytes.Length)
                return false;

            for (var i = 0; i < bytes.Length; i++)
            {
                var equals = true;
                for (var j = 0; j < s.Length; j++)
                {
                    var c1 = (char)bytes[i + j];
                    var c2 = (char)s[j];
                    if (char.ToLowerInvariant(c1) != char.ToLowerInvariant(c2))
                    {
                        equals = false;
                        break;
                    }
                }

                if (equals)
                    return true;
            }
            return false;
        }

        public static string GetVirtualMachineType()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\services\mssmbios\Data", false))
                {
                    if (key?.GetValue("SMBiosData") is byte[] bytes)
                    {
                        //+ detect known emulators
                        if (SearchASCIICaseInsensitive(bytes, "Microsoft Corporation - Virtual Machine"))
                            return "Hyper-V";

                        if (SearchASCIICaseInsensitive(bytes, "Microsoft"))
                            return "Virtual PC";

                        if (SearchASCIICaseInsensitive(bytes, "VMWare"))
                            return "VMWare";

                        if (SearchASCIICaseInsensitive(bytes, "VBox"))
                            return "Virtual Box";

                        if (SearchASCIICaseInsensitive(bytes, "Bochs"))
                            return "Bochs";

                        if (SearchASCIICaseInsensitive(bytes, "QEMU"))
                            return "QEMU";

                        if (SearchASCIICaseInsensitive(bytes, "Plex86"))
                            return "Plex86";

                        if (SearchASCIICaseInsensitive(bytes, "Parallels"))
                            return "Parallels";

                        if (SearchASCIICaseInsensitive(bytes, "Xen"))
                            return "Xen";

                        if (SearchASCIICaseInsensitive(bytes, "Virtual"))
                            return "Generic Virtual Machine";
                    }
                }
            }
            catch
            {
            }
            return null;
        }

        public static string Serialize(Assembly assembly = null)
        {
            var di = new DiagnosticsInformation(assembly);
            var dic = new Dictionary<string, List<PropertyInfo>>();
            foreach (var prop in di.GetType().GetProperties())
            {
                var browsable = prop.GetCustomAttribute<BrowsableAttribute>();
                if (browsable != null && !browsable.Browsable)
                    continue;

                var cat = prop.GetCustomAttribute<CategoryAttribute>();
                var catName = cat?.Category ?? "General";
                if (!dic.TryGetValue(catName, out List<PropertyInfo> props))
                {
                    props = new List<PropertyInfo>();
                    dic.Add(catName, props);
                }
                props.Add(prop);
            }

            var sb = new StringBuilder();
            foreach (var kv in dic.OrderBy(k => k.Key))
            {
                sb.AppendLine("[" + kv.Key + "]");
                foreach (var prop in kv.Value.OrderBy(i => i.Name))
                {
                    var value = prop.GetValue(di);
                    if (value is IEnumerable e && !(value is string))
                    {
                        value = string.Join(" | ", e.Cast<object>());
                    }
                    sb.AppendLine(" " + prop.Name + " = " + value);
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEM_INFO
        {
            public short wProcessorArchitecture;
            public short wReserved;
            public int dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public IntPtr dwActiveProcessorMask;
            public int dwNumberOfProcessors;
            public int dwProcessorType;
            public int dwAllocationGranularity;
            public short wProcessorLevel;
            public short wProcessorRevision;
        }

#pragma warning disable IDE1006 // Naming Styles
        private const int PROCESSOR_ARCHITECTURE_AMD64 = 9;
        private const int PROCESSOR_ARCHITECTURE_IA64 = 6;
        private const int PROCESSOR_ARCHITECTURE_INTEL = 0;
        private const int TOKEN_QUERY = 8;
        private const int TokenElevationTypeInformation = 18;
#pragma warning restore IDE1006 // Naming Styles

        [DllImport("kernel32")]
        private static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32", SetLastError = true)]
        private static extern bool GetTokenInformation(IntPtr TokenHandle, int TokenInformationClass, out TokenElevationType TokenInformation, int TokenInformationLength, out int ReturnLength);

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);
    }
}
