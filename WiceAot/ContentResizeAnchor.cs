namespace Wice;

/// <summary>
/// Specifies which corner of a window stays in place when the window resizes itself to its content,
/// see <see cref="Window.ContentResizeAnchor"/>.
/// </summary>
public enum ContentResizeAnchor
{
    /// <summary>
    /// The top left corner stays in place, the window grows to the right and to the bottom. This is how Windows resizes windows.
    /// </summary>
    TopLeft,

    /// <summary>
    /// The top right corner stays in place, the window grows to the left and to the bottom.
    /// </summary>
    TopRight,

    /// <summary>
    /// The bottom left corner stays in place, the window grows to the right and to the top.
    /// </summary>
    BottomLeft,

    /// <summary>
    /// The bottom right corner stays in place, the window grows to the left and to the top.
    /// </summary>
    BottomRight,

    /// <summary>
    /// The corner of the monitor work area quarter that holds the window's center stays in place,
    /// so a window placed near a corner of the screen keeps to that corner.
    /// </summary>
    NearestCorner,
}
