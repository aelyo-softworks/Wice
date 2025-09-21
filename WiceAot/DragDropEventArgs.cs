namespace Wice;

/// <summary>
/// Provides data for drag-and-drop notifications, including the event type, payload,
/// key modifiers, pointer position, and the resulting drop effect.
/// </summary>
/// <param name="type">The drag-and-drop notification type that caused the event.</param>
public class DragDropEventArgs(DragDropEventType type)
    : HandledEventArgs
{
    /// <summary>
    /// Gets the kind of drag-and-drop notification being raised.
    /// </summary>
    public DragDropEventType Type { get; internal set; } = type;

    /// <summary>
    /// Gets or sets the data payload associated with the drag operation.
    /// </summary>
    public IDataObject? DataObject { get; set; }

    /// <summary>
    /// Gets or sets the mouse button and keyboard modifier state at the time of the event.
    /// </summary>
    /// <seealso cref="MODIFIERKEYS_FLAGS"/>
    public MODIFIERKEYS_FLAGS KeyFlags { get; set; }

    /// <summary>
    /// Gets or sets the pointer location in pixels at the time of the event.
    /// </summary>
    public POINT Point { get; set; }

    /// <summary>
    /// Gets or sets the drop effect for the current drag operation.
    /// </summary>
    public DROPEFFECT Effect { get; set; }
}
