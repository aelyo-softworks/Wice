namespace Wice.Utilities;

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable CA1826 // Do not use Enumerable methods on indexable collections
public class SystemInformation(Assembly? assembly = null, Window? window = null, string? separator = null) : DiagnosticsInformation(assembly, separator: separator)
{
    [Category("Graphics")]
    public string DefaultTextServicesGeneratorVersion { get; } = RichTextBox.DefaultTextServicesGeneratorVersion;

    [Category("Graphics")]
    public new string? WindowDpiAwareness => (window ?? Application.AllWindows.FirstOrDefault())?.NativeIfCreated?.DpiAwarenessDescription;

    [Category("Graphics")]
    [Browsable(false)]
    public new uint WindowDpiFromDpiAwareness => (window ?? Application.AllWindows.FirstOrDefault())?.NativeIfCreated?.DpiFromDpiAwareness ?? WiceCommons.USER_DEFAULT_SCREEN_DPI;

    [Category("Graphics")]
    public uint WindowDpi => (window ?? Application.AllWindows.FirstOrDefault())?.Dpi ?? WiceCommons.USER_DEFAULT_SCREEN_DPI;

    [Category("Graphics")]
    public new string? WindowMonitor
    {
        get
        {
            var monitor = (window ?? Application.AllWindows.FirstOrDefault())?.GetMonitor();
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

    // convert to string because of AOT
    [Category("Process")]
    public new string TokenElevationType => SystemUtilities.GetTokenElevationType().ToString();

    [Category("System")]
    public new string ProcessorArchitecture => SystemUtilities.GetProcessorArchitecture().ToString();

    [Category("System")]
    public bool IsPenExtensionsInstalled => Functions.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_TABLETPC) != 0;

    public static bool HasNonWarpAdapter
    {
        get
        {
            using var factory = DXGIFunctions.CreateDXGIFactory1();
            var adapters = factory.EnumAdapters().ToArray();
            try
            {
                // https://learn.microsoft.com/en-us/windows/win32/direct3ddxgi/d3d10-graphics-programming-guide-dxgi#new-info-about-enumerating-adapters-for-windows-8
                const int WarpDeviceId = 0x8c;
                return adapters.Any(adapter => adapter.GetDesc().DeviceId != WarpDeviceId);
            }
            finally
            {
                adapters.Dispose();
            }
        }
    }
}
#pragma warning restore CA1826 // Do not use Enumerable methods on indexable collections
#pragma warning restore CA1822 // Mark members as static
