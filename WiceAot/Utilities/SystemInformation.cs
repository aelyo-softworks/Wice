namespace Wice.Utilities;

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable CA1826 // Do not use Enumerable methods on indexable collections

/// <summary>
/// Provides runtime system, process, and graphics-related diagnostics for Wice applications,
/// extending <see cref="DiagnosticsInformation"/> with window-aware values (DPI, monitor, etc.).
/// </summary>
/// <param name="assembly">
/// Optional assembly used by the base diagnostics class for version/serialization information.
/// </param>
/// <param name="window">
/// Optional window used as the source for DPI and monitor queries. When null, the first window in
/// <see cref="Application.AllWindows"/> is used, if any.
/// </param>
/// <param name="separator">
/// Optional separator string passed to the base class to format multi-value diagnostics output.
/// </param>
public class SystemInformation(Assembly? assembly = null, Window? window = null, string? separator = null)
    : DiagnosticsInformation(assembly, separator: separator)
{
    /// <summary>
    /// Gets the default Text Services Framework (TSF) generator version as exposed by <see cref="RichTextBox"/>.
    /// </summary>
    /// <value>A string describing the TSF generator version.</value>
    [Category("Graphics")]
    public string DefaultTextServicesGeneratorVersion { get; } = RichTextBox.DefaultTextServicesGeneratorVersion;

    /// <summary>
    /// Gets the DPI awareness description of the active window.
    /// </summary>
    /// <remarks>
    /// Returns a textual description (e.g., "Per Monitor Aware") when the window and its native handle exist;
    /// otherwise returns null.
    /// </remarks>
    [Category("Graphics")]
    public new string? WindowDpiAwareness => (window ?? Application.AllWindows.FirstOrDefault())?.NativeIfCreated?.DpiAwarenessDescription;

    /// <summary>
    /// Gets the effective DPI inferred from the window's current DPI awareness context.
    /// </summary>
    /// <remarks>
    /// Hidden from browsable editors. Falls back to <see cref="WiceCommons.USER_DEFAULT_SCREEN_DPI"/> (typically 96)
    /// when no active window is available or DPI awareness is unknown.
    /// </remarks>
    [Category("Graphics")]
    [Browsable(false)]
    public new uint WindowDpiFromDpiAwareness => (window ?? Application.AllWindows.FirstOrDefault())?.NativeIfCreated?.DpiFromDpiAwareness ?? WiceCommons.USER_DEFAULT_SCREEN_DPI;

    /// <summary>
    /// Gets the actual DPI of the active window.
    /// </summary>
    /// <remarks>
    /// Falls back to <see cref="WiceCommons.USER_DEFAULT_SCREEN_DPI"/> (typically 96) when no window is available.
    /// </remarks>
    [Category("Graphics")]
    public uint WindowDpi => (window ?? Application.AllWindows.FirstOrDefault())?.Dpi ?? WiceCommons.USER_DEFAULT_SCREEN_DPI;

    /// <summary>
    /// Gets a human-readable description of the monitor hosting the active window.
    /// </summary>
    /// <value>
    /// A string such as "\\.\DISPLAY1 (Monitor Friendly Name)" when available; otherwise <see langword="null"/>.
    /// </value>
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

    /// <summary>
    /// Gets the current process token elevation type as a string.
    /// </summary>
    /// <remarks>
    /// Converted to string for AOT friendliness to avoid potential enum metadata trimming/reflection issues.
    /// See <c>SystemUtilities.GetTokenElevationType()</c>.
    /// </remarks>
    [Category("Process")]
    public new string TokenElevationType => SystemUtilities.GetTokenElevationType().ToString();

    /// <summary>
    /// Gets the processor architecture as a string (e.g., X86, X64, Arm64).
    /// </summary>
    /// <remarks>
    /// Converted to string for consistent formatting and potential AOT friendliness.
    /// </remarks>
    [Category("System")]
    public new string ProcessorArchitecture => SystemUtilities.GetProcessorArchitecture().ToString();

    /// <summary>
    /// Gets a value indicating whether Tablet PC Pen Extensions are installed on this system.
    /// </summary>
    /// <remarks>
    /// Queries <c>GetSystemMetrics(SM_TABLETPC)</c>. Non-zero indicates presence.
    /// </remarks>
    [Category("System")]
    public bool IsPenExtensionsInstalled => Functions.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_TABLETPC) != 0;

    /// <summary>
    /// Gets a value indicating whether the system exposes at least one non-WARP (hardware) DXGI adapter.
    /// </summary>
    [Category("Graphics")]
    public static bool HasNonWarpAdapter
    {
        get
        {
            using var factory = DXGIFunctions.CreateDXGIFactory1();
            var adapters = factory.EnumAdapters().ToArray();
            try
            {
                // https://learn.microsoft.com/windows/win32/direct3ddxgi/d3d10-graphics-programming-guide-dxgi#new-info-about-enumerating-adapters-for-windows-8
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
