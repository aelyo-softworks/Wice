namespace Wice.Utilities;

#pragma warning disable CA1822 // Mark members as static
public class SystemInformation(Assembly? assembly = null, Window? window = null) : DiagnosticsInformation(assembly)
{
    [Category("Graphics")]
    public string DefaultTextServicesGeneratorVersion { get; } = RichTextBox.GetDefaultTextServicesGeneratorVersion();

    [Category("Graphics")]
    public new string? WindowDpiAwareness => (window ?? Application.Windows.FirstOrDefault())?.Native.DpiAwarenessDescription;

    [Category("Graphics")]
    public new uint WindowDpiFromDpiAwareness => (window ?? Application.Windows.FirstOrDefault())?.Native.DpiFromDpiAwareness ?? 96;

    [Category("Graphics")]
    public new string? WindowMonitor
    {
        get
        {
            var monitor = (window ?? Application.Windows.FirstOrDefault())?.GetMonitor();
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
#pragma warning restore CA1822 // Mark members as static
