namespace Wice;

/// <summary>
/// Represents the type of a standard window title bar button.
/// </summary>
/// <remarks>
/// Use to identify a caption button such as Close, Minimize, Maximize, or Restore.
/// </remarks>
public enum TitleBarButtonType
{
    /// <summary>
    /// No button type. Use when no button is present or not applicable.
    /// </summary>
    None,

    /// <summary>
    /// Closes the window.
    /// </summary>
    Close,

    /// <summary>
    /// Minimizes the window to the taskbar or dock.
    /// </summary>
    Minimize,

    /// <summary>
    /// Maximizes the window to fill the available screen space.
    /// </summary>
    Maximize,

    /// <summary>
    /// Restores the window to its previous size and position.
    /// </summary>
    Restore,
}
