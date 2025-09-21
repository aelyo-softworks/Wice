namespace Wice;

/// <summary>
/// Specifies how scrolling is presented and interacted with in scrollable UI elements.
/// </summary>
public enum ScrollBarMode
{
    /// <summary>
    /// Traditional scroll bars are displayed and can be used for navigation.
    /// </summary>
    Standard,

    /// <summary>
    /// Scrolling is primarily performed via panning gestures; scroll bars may be hidden or minimized.
    /// </summary>
    Panning,
}
