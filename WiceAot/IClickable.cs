namespace Wice;

/// <summary>
/// Defines a contract for types that can be activated via a user interaction (e.g., a click).
/// </summary>
public interface IClickable
{
    /// <summary>
    /// Occurs when the implementer is clicked or otherwise activated by the user.
    /// </summary>
    event EventHandler<EventArgs>? Click;
}
