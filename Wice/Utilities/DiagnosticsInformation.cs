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

#if !NET
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
        [DisplayName("mscorlib Version")]
        public string CorLibVersion => typeof(int).Assembly.GetInformationalVersion();

        [Category(".NET")]
        public string SystemVersion => typeof(Uri).Assembly.GetInformationalVersion();

        [Category(".NET")]
        public string FrameworkDescription => RuntimeInformation.FrameworkDescription;

        [Category(".NET")]
        public Version ClrVersion => Environment.Version;

        [Category("Process")]
        public TokenElevationType TokenElevationType => GetTokenElevationType();

        [Category("Process")]
        public string Bitness => GetBitness();

        [Category("Process")]
        public string CurrentCulture => CultureInfo.CurrentCulture.Name + Environment.NewLine + "UI: " + CultureInfo.CurrentUICulture.Name;

        [Category("Process")]
        public string Now => DateTime.Now + Environment.NewLine + "Utc: " + DateTime.UtcNow;

        [Category("Process")]
        public string InstalledUICulture => CultureInfo.InstalledUICulture.Name;

        [Category("Graphics")]
        public string DesktopDpi
        {
            get
            {
                var dpi = GetDpiSettings();
                return dpi.Width + " x " + dpi.Height;
            }
        }

        private static IEnumerable<IComObject<IDXGIAdapter1>> GetAdapters()
        {
            using (var fac = DXGIFunctions.CreateDXGIFactory1())
            {
                var adapter = fac.EnumAdapters1().FirstOrDefault(a => !((DXGI_ADAPTER_FLAG)a.GetDesc1().Flags).HasFlag(DXGI_ADAPTER_FLAG.DXGI_ADAPTER_FLAG_SOFTWARE) && a.EnumOutputs<IDXGIOutput1>().Count() > 0);
                if (adapter == null)
                {
                    adapter = fac.EnumAdapters1().FirstOrDefault();
                }

                yield return adapter;
            }
        }

        private static IEnumerable<string> DisplayConfigQuery()
        {
            var dd = DISPLAY_DEVICE.All.ToList();
            foreach (var path in DisplayConfig.Query())
            {
                var tar = DisplayConfig.GetDeviceInfoTargetName(path);
                var src = DisplayConfig.GetDeviceInfoSourceName(path);
                var display = dd.FirstOrDefault(m => m.DeviceName.EqualsIgnoreCase(src.viewGdiDeviceName));
                if (display.DeviceName == null)
                    yield return tar + " " + src.viewGdiDeviceName;

                yield return tar + " " + display.CurrentSettings;
            }
        }

        [Category("Graphics")]
        [DisplayName("Graphic Adapter(s)")]
        public string Adapters => string.Join(Environment.NewLine, GetAdapters().Select(a => a.GetDesc().Description));

        [Category("Graphics")]
        [DisplayName("Display(s)")]
        public string Displays => string.Join(Environment.NewLine, DisplayConfigQuery());

        [Category("Software")]
        public Version AssemblyInformationalVersion
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
        public string AssemblyConfiguration => Assembly.GetConfiguration();

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

#if !NET
        public static T GetManagementInfo<T>(string className, string propertyName, T defaultValue)
        {
            if (className == null)
                throw new ArgumentNullException(nameof(className));

            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            try
            {
                foreach (System.Management.ManagementObject mo in new System.Management.ManagementObjectSearcher(new System.Management.WqlObjectQuery("select * from " + className)).Get())
                {
                    foreach (System.Management.PropertyData data in mo.Properties)
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
