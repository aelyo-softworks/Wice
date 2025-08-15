namespace Wice;

/// <summary>
/// Represents the type of a drag-and-drop event within the drag-and-drop interaction lifecycle.
/// </summary>
/// <remarks>
/// The sequence typically follows:
/// 1) <see cref="Enter"/> when the drag cursor enters a potential drop target,
/// 2) <see cref="Over"/> while the cursor moves within the target,
/// 3) <see cref="Leave"/> if the cursor exits the target without dropping,
/// 4) <see cref="Drop"/> when data is released onto the target.
/// Consumers can use this type to distinguish the current phase when handling drag-and-drop events.
/// </remarks>
public enum DragDropEventType
{
    /// <summary>
    /// The drag cursor entered the bounds of the drop target.
    /// </summary>
    Enter,

    /// <summary>
    /// The drag cursor is moving within the bounds of the drop target.
    /// </summary>
    Over,

    /// <summary>
    /// The drag cursor left the bounds of the drop target without dropping.
    /// </summary>
    Leave,

    /// <summary>
    /// The dragged data was released (dropped) on the drop target.
    /// </summary>
    Drop
}
