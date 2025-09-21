namespace Wice;

/// <summary>
/// Event data for a pointer causing window activation (e.g., WM_POINTERACTIVATE/WM_MOUSEACTIVATE translations).
/// Carries the window handle being activated and the hit-test result for where activation occurred.
/// </summary>
/// <param name="pointerId">The system-assigned pointer identifier from the originating pointer message.</param>
/// <param name="windowBeingActivated">The window handle (HWND) being activated as a result of the pointer action.</param>
/// <param name="hitTest">The hit-test result indicating where the activation occurred within the non-client/client areas.</param>
public class PointerActivateEventArgs(uint pointerId, HWND windowBeingActivated, HT hitTest)
    : PointerEventArgs(pointerId)
{
    /// <summary>
    /// Gets the window handle being activated.
    /// </summary>
    public HWND WindowBeingActivated { get; } = windowBeingActivated;

    /// <summary>
    /// Gets the hit-test result associated with the activation.
    /// </summary>
    public HT HitTest { get; } = hitTest;

    /// <summary>
    /// Returns a concise string that includes the base pointer information plus the window and hit-test values.
    /// </summary>
    public override string ToString() => base.ToString() + ",W=" + WindowBeingActivated + ",HT=" + HitTest;
}
