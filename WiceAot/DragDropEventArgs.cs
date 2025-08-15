namespace Wice;

/// <summary>
/// Provides data for drag-and-drop notifications, including the event type, payload,
/// key modifiers, pointer position, and the resulting drop effect.
/// </summary>
/// <remarks>
/// The <see cref="Type"/> indicates which stage of the drag operation is being reported
/// (e.g., Enter, Over, Leave, Drop). Targets may read or set <see cref="Effect"/> to
/// advertise or choose an operation such as <see cref="DROPEFFECT.DROPEFFECT_COPY"/> or
/// <see cref="DROPEFFECT.DROPEFFECT_MOVE"/>.
/// </remarks>
/// <param name="type">The drag-and-drop notification type that caused the event.</param>
public class DragDropEventArgs(DragDropEventType type)
    : HandledEventArgs
{
    /// <summary>
    /// Gets the kind of drag-and-drop notification being raised.
    /// </summary>
    /// <remarks>
    /// Set internally by the framework when the event is constructed.
    /// </remarks>
    public DragDropEventType Type { get; internal set; } = type;

    /// <summary>
    /// Gets or sets the data payload associated with the drag operation.
    /// </summary>
    /// <remarks>
    /// May be null when no data is available (for example, during <see cref="DragDropEventType.Leave"/>).
    /// </remarks>
    public IDataObject? DataObject { get; set; }

    /// <summary>
    /// Gets or sets the mouse button and keyboard modifier state at the time of the event.
    /// </summary>
    /// <seealso cref="MODIFIERKEYS_FLAGS"/>
    public MODIFIERKEYS_FLAGS KeyFlags { get; set; }

    /// <summary>
    /// Gets or sets the pointer location in pixels at the time of the event.
    /// </summary>
    /// <remarks>
    /// The coordinate space is defined by the event source (typically client coordinates of the target).
    /// </remarks>
    public POINT Point { get; set; }

    /// <summary>
    /// Gets or sets the drop effect for the current drag operation.
    /// </summary>
    /// <remarks>
    /// During Enter/Over, handlers can set this to indicate the allowed operation. During Drop, it
    /// reflects the chosen operation. Use values from <see cref="DROPEFFECT"/>.
    /// </remarks>
    /// <seealso cref="DROPEFFECT"/>
    public DROPEFFECT Effect { get; set; }
}
