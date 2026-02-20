namespace Wice;

/// <summary>
/// Provides data for the Activated and Deactivated events.
/// </summary>
/// <param name="windowHandle">Gets the handle of the associated window.</param>
/// <param name="otherWindowHandle">Gets the handle to the window being deactivated. Can be null.</param>
/// <param name="byMouseClick">Indicates whether the activation was triggered by a mouse click.</param>
public class ActivationEventArgs(HWND windowHandle, HWND otherWindowHandle, bool byMouseClick) : EventArgs
{
    /// <summary>
    /// Gets the handle of the associated window.
    /// </summary>
    public HWND WindowHandle { get; } = windowHandle;

    /// <summary>
    /// Gets the handle to the window being deactivated. Can be null.
    /// </summary>
    public HWND OtherWindowHandle { get; } = otherWindowHandle;

    /// <summary>
    /// Gets a value indicating whether the action was triggered by a mouse click.
    /// </summary>
    public bool ByMouseClick { get; } = byMouseClick;
}
