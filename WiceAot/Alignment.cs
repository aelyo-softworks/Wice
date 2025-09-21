namespace Wice;

/// <summary>
/// Specifies how content is positioned along an axis or within the available layout space.
/// </summary>
public enum Alignment
{
    /// <summary>
    /// Positions content near the start of the axis (left for horizontal layout, top for vertical layout).
    /// </summary>
    Near,

    /// <summary>
    /// Centers the content along the axis.
    /// </summary>
    Center,

    /// <summary>
    /// Positions content near the end of the axis (right for horizontal layout, bottom for vertical layout).
    /// </summary>
    Far,

    /// <summary>
    /// Stretches the content to fill the available space along the axis.
    /// </summary>
    Stretch
}
