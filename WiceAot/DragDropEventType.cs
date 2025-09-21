namespace Wice;

/// <summary>
/// Represents the type of a drag-and-drop event within the drag-and-drop interaction lifecycle.
/// </summary>
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
