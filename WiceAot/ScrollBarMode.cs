namespace Wice;

/// <summary>
/// Specifies how scrolling is presented and interacted with in scrollable UI elements.
/// </summary>
/// <remarks>
/// - Standard: Traditional visible scroll bars; supports mouse wheel, keyboard, and thumb dragging.
/// - Panning: Emphasizes gesture-based panning (touch/trackpad); scroll bars may be hidden or minimal.
/// </remarks>
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
