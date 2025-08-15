namespace Wice;

/// <summary>
/// Specifies the side of a container to which a child element should be docked.
/// </summary>
/// <remarks>
/// Commonly used by docking/container layouts to align children along the specified edge,
/// with remaining space available for subsequent content.
/// </remarks>
public enum DockType
{
    /// <summary>
    /// Dock the element to the left edge of the container.
    /// </summary>
    Left,

    /// <summary>
    /// Dock the element to the top edge of the container.
    /// </summary>
    Top,

    /// <summary>
    /// Dock the element to the right edge of the container.
    /// </summary>
    Right,

    /// <summary>
    /// Dock the element to the bottom edge of the container.
    /// </summary>
    Bottom
}
