namespace Wice.Utilities;

#pragma warning disable CA1822 // Mark members as static
public class DiagnosticsInformation(Assembly? assembly = null, Window? window = null)
{
    private readonly Window? _window = window;

    [Browsable(false)]
    public Assembly Assembly { get; } = assembly ?? Assembly.GetExecutingAssembly();

    [Category("Windows")]
    public string OSVersion => Environment.OSVersion.VersionString;

    [Category("Windows")]
    public string? KernelVersion => WindowsVersionUtilities.KernelVersion?.ToString();

    [Category("System")]
    public bool IsTabletPC => Functions.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_TABLETPC) != 0;

    [Category("System")]
    public bool IsRemotelyControlled => Functions.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_REMOTECONTROL) != 0;

    [Category("System")]
    public bool IsRemoteSession => Functions.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_REMOTESESSION) != 0;

    [Category("System")]
    public string? VirtualMachineType => GetVirtualMachineType();

    [Category("System")]
    public int ProcessorCount => Environment.ProcessorCount;

    [Category("System")]
    public ProcessorArchitecture ProcessorArchitecture => SystemUtilities.GetProcessorArchitecture();

    [Category(".NET")]
    public string? SystemVersion => typeof(Uri).Assembly.GetInformationalVersion();

    [Category(".NET")]
    public string FrameworkDescription => RuntimeInformation.FrameworkDescription;

    [Category(".NET")]
    public Version ClrVersion => Environment.Version;

    [Category("Process")]
    public TokenElevationType TokenElevationType => SystemUtilities.GetTokenElevationType();

    [Category("Process")]
    public string Bitness => GetBitness();

    [Category("Process")]
    public string Culture => CultureInfo.CurrentCulture.Name.Nullify() ?? CultureInfo.CurrentCulture.NativeName.Nullify() ?? CultureInfo.CurrentCulture.TwoLetterISOLanguageName + ", UI: " + CultureInfo.CurrentUICulture.Name.Nullify() ?? CultureInfo.CurrentUICulture.NativeName.Nullify() ?? CultureInfo.CurrentUICulture.TwoLetterISOLanguageName + ", Installed: " + CultureInfo.InstalledUICulture.Name;

    [Category("Process")]
    public string Now => DateTime.Now + ", Utc: " + DateTime.UtcNow;

    [Category("Graphics")]
    public int TextScaleFactor => DpiUtilities.TextScaleFactor;

    [Category("Graphics")]
    public string DefaultTextServicesGeneratorVersion { get; } = RichTextBox.GetDefaultTextServicesGeneratorVersion();

    [Category("Graphics")]
    public string? WindowDpiAwareness => (_window ?? Application.Windows.FirstOrDefault())?.Native.DpiAwarenessDescription;

    [Category("Graphics")]
    public uint WindowDpiFromDpiAwareness => (_window ?? Application.Windows.FirstOrDefault())?.Native.DpiFromDpiAwareness ?? 96;

    [Category("Graphics")]
    public string ThreadDpiAwareness { get; } = NativeWindow.GetDpiAwarenessDescription(Functions.GetThreadDpiAwarenessContext());

    [Category("Graphics")]
    public uint ThreadDpiFromDpiAwareness { get; } = Functions.GetDpiFromDpiAwarenessContext(Functions.GetThreadDpiAwarenessContext());

    [Category("Graphics")]
    public string? WindowMonitor
    {
        get
        {
            var monitor = (_window ?? Application.Windows.FirstOrDefault())?.Monitor;
            if (monitor == null)
                return null;

            var s = monitor.DeviceName;
            var dd = monitor.DisplayDevice;
            if (dd != null)
            {
                s += " (" + dd.MonitorName + ")";
            }
            return s;
        }
    }

    [Category("Graphics")]
    public string DesktopDpi
    {
        get
        {
            var dpi = DpiUtilities.GetDpiForDesktopF();
            return dpi.width + " x " + dpi.height;
        }
    }

    [Category("Graphics")]
    [DisplayName("Graphic Adapter(s)")]
    public string Adapters => GetAdapters();

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
    public string AssemblyConfiguration => Assembly.GetConfiguration() ?? string.Empty;

    private static string GetAdapters()
    {
        using var fac = DXGIFunctions.CreateDXGIFactory1();
        var adapters = fac.EnumAdapters1().ToArray();
        var str = string.Join(Environment.NewLine, adapters.Select(a => a.GetDesc().Description));
        adapters.Dispose();
        return str;
    }

    private static IEnumerable<string> DisplayConfigQuery()
    {
        var dd = DisplayDevice.All.ToList();
        foreach (var path in DisplayConfig.Query())
        {
            var tar = DisplayConfig.GetTargetName(path);
            var src = DisplayConfig.GetSourceName(path);
            var display = dd.FirstOrDefault(m => m.DeviceName.EqualsIgnoreCase(src.viewGdiDeviceName.ToString()));
            if (display == null)
                continue;

            string? dpi = null;
            var mon = display.Monitor;
            if (mon != null)
            {
                dpi = " Dpi(a:" + mon.AngularDpi.width + " e:" + mon.EffectiveDpi.width + " r:" + mon.RawDpi.width + ")";
            }

            if (display.DeviceName == null)
                yield return tar.monitorFriendlyDeviceName + " " + src.viewGdiDeviceName + dpi;

            yield return tar.monitorFriendlyDeviceName + " " + display.CurrentSettings + dpi;
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

    public static string? GetVirtualMachineType()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\services\mssmbios\Data", false);
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
        catch
        {
        }
        return null;
    }

    public static string Serialize(Assembly? assembly = null)
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
            if (!dic.TryGetValue(catName, out var props))
            {
                props = [];
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
                if (value is IEnumerable e && value is not string)
                {
                    value = string.Join(" | ", e.Cast<object>());
                }
                sb.AppendLine(" " + prop.Name + " = " + value);
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
#pragma warning restore CA1822 // Mark members as static
