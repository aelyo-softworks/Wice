namespace Wice;

/// <summary>
/// Defines the contract for a parent that exposes title bar related state to consumers,
/// such as the current window title and whether the window is maximized.
/// </summary>
public interface ITitleBarParent
{
    /// <summary>
    /// Gets the current title text to display in the title bar.
    /// </summary>
    /// <value>
    /// A non-null string representing the current window title. May be empty.
    /// </value>
    string Title { get; }

    /// <summary>
    /// Gets a value indicating whether the parent window is maximized (zoomed).
    /// </summary>
    /// <value>
    /// true if the window is maximized; otherwise, false.
    /// </value>
    bool IsZoomed { get; }
}
