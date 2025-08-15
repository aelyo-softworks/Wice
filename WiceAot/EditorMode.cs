namespace Wice;

/// <summary>
/// Specifies how an editor should be displayed within the UI.
/// </summary>
/// <remarks>
/// - <see cref="Modal"/> blocks interaction with other windows until the editor is closed.
/// - <see cref="NonModal"/> allows interaction with other windows while the editor remains open.
/// </remarks>
public enum EditorMode
{
    /// <summary>
    /// The editor is shown as a modal dialog and blocks interaction with other windows until it is closed.
    /// </summary>
    Modal,

    /// <summary>
    /// The editor is shown as a non-modal window and allows interaction with other windows.
    /// </summary>
    NonModal,
}
