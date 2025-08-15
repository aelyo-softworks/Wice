namespace Wice;

/// <summary>
/// Specifies how a window's non-client frame (title bar, borders, and system chrome) is handled on Windows.
/// </summary>
/// <remarks>
/// - Standard: Use the OS-provided frame and title bar without custom drawing.
/// - Merged: Extend the frame into the client area to allow custom chrome while preserving system interactions (e.g., hit testing, snapping).
/// - None: Create a borderless window; the application must implement moving, resizing, hit testing, and all chrome rendering.
/// </remarks>
public enum WindowsFrameMode
{
    /// <summary>
    /// Use the default OS-managed window frame and title bar.
    /// Suitable when no custom chrome is required.
    /// </summary>
    Standard,

    /// <summary>
    /// Merge the non-client frame into the client area to enable custom-drawn chrome.
    /// System behaviors (such as caption button hit testing and snapping) can be preserved with appropriate handling.
    /// </summary>
    Merged,

    /// <summary>
    /// Do not show any standard frame or title bar (borderless window).
    /// The application is responsible for window moving, resizing, hit testing, and visual chrome.
    /// </summary>
    None,
}
