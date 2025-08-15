namespace Wice;

/// <summary>
/// Provides data for handling drag-and-drop GiveFeedback events, exposing the current drop effect
/// and allowing a handler to control the HRESULT returned to the drag-and-drop subsystem.
/// </summary>
/// <param name="effect">
/// The drop effect suggested for the current drag operation. See <see cref="DROPEFFECT"/>.
/// </param>
/// <seealso cref="DROPEFFECT"/>
/// <seealso cref="EventArgs"/>
public class DragDropGiveFeedback(DROPEFFECT effect) : EventArgs
{
    /// <summary>
    /// Gets the drop effect for the current drag operation (None, Copy, Move, Link, or Scroll).
    /// </summary>
    public DROPEFFECT Effect { get; } = effect;

    /// <summary>
    /// Gets or sets the HRESULT to return to the drag-and-drop framework.
    /// Use <c>DRAGDROP_S_USEDEFAULTCURSORS</c> to request the system's default cursors.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="WiceCommons.DRAGDROP_S_USEDEFAULTCURSORS"/>.
    /// </remarks>
    public HRESULT Result { get; set; } = WiceCommons.DRAGDROP_S_USEDEFAULTCURSORS;
}
