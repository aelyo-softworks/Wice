namespace Wice;

/// <summary>
/// Identifies the mouse button involved in a pointer or mouse input event.
/// </summary>
/// <remarks>
/// X1 and X2 typically correspond to auxiliary buttons found on some mice,
/// often mapped to "Back" and "Forward" actions in browsers and applications.
/// </remarks>
public enum MouseButton
{
    /// <summary>
    /// The primary mouse button, commonly the left button.
    /// </summary>
    Left,

    /// <summary>
    /// The secondary mouse button, commonly the right button.
    /// </summary>
    Right,

    /// <summary>
    /// The middle mouse button, typically integrated into the scroll wheel.
    /// </summary>
    Middle,

    /// <summary>
    /// The first extended mouse button (often mapped to "Back").
    /// </summary>
    X1,

    /// <summary>
    /// The second extended mouse button (often mapped to "Forward").
    /// </summary>
    X2
}
