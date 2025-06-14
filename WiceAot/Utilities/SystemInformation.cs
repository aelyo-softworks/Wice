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
    public new uint WindowDpiFromDpiAwareness => (window ?? Application.AllWindows.FirstOrDefault())?.NativeIfCreated?.DpiFromDpiAwareness ?? 96;

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
}
#pragma warning restore CA1826 // Do not use Enumerable methods on indexable collections
#pragma warning restore CA1822 // Mark members as static
