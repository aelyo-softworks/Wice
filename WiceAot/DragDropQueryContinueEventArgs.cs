namespace Wice;

/// <summary>
/// Provides event data for a drag-and-drop "query continue" operation, analogous to the Win32 IDropSource::QueryContinueDrag notification.
/// </summary>
/// <param name="escapedPressed">
/// True if the ESC key was pressed to cancel the drag operation; otherwise, false.
/// </param>
/// <param name="keyFlags">
/// The current state of mouse buttons and modifier keys, expressed as <see cref="MODIFIERKEYS_FLAGS"/> bit flags.
/// </param>
public class DragDropQueryContinueEventArgs(bool escapedPressed, MODIFIERKEYS_FLAGS keyFlags)
    : EventArgs
{
    /// <summary>
    /// Gets a value indicating whether the ESC key was pressed during the drag-and-drop operation.
    /// </summary>
    public bool EscapedPressed { get; } = escapedPressed;

    /// <summary>
    /// Gets the state of mouse buttons and modifier keys at the time of the query, as a combination of <see cref="MODIFIERKEYS_FLAGS"/>.
    /// </summary>
    public MODIFIERKEYS_FLAGS KeyFlags { get; } = keyFlags;

    /// <summary>
    /// Gets or sets the HRESULT that determines whether the drag-and-drop operation continues, drops, or cancels.
    /// </summary>
    public HRESULT Result { get; set; }
}
