namespace Wice;

/// <summary>
/// Specifies the type of drag-and-drop target event that occurred.
/// </summary>
/// <remarks>
/// Use this to distinguish whether a drag operation has entered or left a drop target.
/// </remarks>
public enum DragDropTargetEventType
{
    /// <summary>
    /// A drag operation entered the drop target's bounds.
    /// </summary>
    Enter,

    /// <summary>
    /// A drag operation left the drop target's bounds.
    /// </summary>
    Leave,
}
