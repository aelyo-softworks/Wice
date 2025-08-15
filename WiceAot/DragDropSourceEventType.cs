namespace Wice;

/// <summary>
/// Represents the lifecycle events reported by a drag-and-drop source.
/// </summary>
/// <remarks>
/// Use this enumeration to distinguish when a drag operation begins (<see cref="Start"/>) and when it ends (<see cref="Stop"/>).
/// </remarks>
public enum DragDropSourceEventType
{
    /// <summary>
    /// A drag operation has begun and the source is initiating the drag.
    /// </summary>
    Start,

    /// <summary>
    /// The drag operation has ended, either successfully completed or canceled.
    /// </summary>
    Stop,
}
