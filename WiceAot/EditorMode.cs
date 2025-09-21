namespace Wice;

/// <summary>
/// Specifies how an editor should be displayed within the UI.
/// </summary>
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
