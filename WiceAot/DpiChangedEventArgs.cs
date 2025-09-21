namespace Wice;

/// <summary>
/// Provides data for DPI change notifications, typically raised in response to a system DPI change (e.g., WM_DPICHANGED).
/// </summary>
/// <param name="newDpi">
/// The new DPI for the window/client area after the change, with separate horizontal (X) and vertical (Y) components.
/// </param>
/// <param name="suggestedRect">
/// The system-recommended new window rectangle in screen coordinates that preserves the window's physical size at the new DPI.
/// </param>
public class DpiChangedEventArgs(D2D_SIZE_U newDpi, RECT suggestedRect)
    : HandledEventArgs
{
    /// <summary>
    /// Gets the new DPI to apply, represented as separate horizontal and vertical components.
    /// </summary>
    public D2D_SIZE_U NewDpi { get; } = newDpi;

    /// <summary>
    /// Gets the system-suggested window rectangle (screen coordinates) appropriate for the new DPI.
    /// </summary>
    public RECT SuggestedRect { get; } = suggestedRect;
}
