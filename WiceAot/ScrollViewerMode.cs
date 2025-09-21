namespace Wice;

/// <summary>
/// Defines how scrollbars are presented by a scroll viewer.
/// </summary>
public enum ScrollViewerMode
{
    /// <summary>
    /// Scrollbars are docked alongside the content, reserving layout space.
    /// Typically visible and do not overlap the content area.
    /// </summary>
    Dock,

    /// <summary>
    /// Scrollbars overlay the content and may auto-hide.
    /// Useful when preserving content space is preferred over persistent scrollbar visibility.
    /// </summary>
    Overlay
}
