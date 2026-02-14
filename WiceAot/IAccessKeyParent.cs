namespace Wice;

/// <summary>
/// Defines a contract for visuals or containers that can receive and react to access key invocations.
/// Implement this interface to participate in access key processing (keyboard shortcuts/accelerators).
/// </summary>
public interface IAccessKeyParent
{
    /// <summary>
    /// Notifies the parent that an access key has been invoked.
    /// Implementations may handle the request or route it to children.
    /// Set <see cref="System.ComponentModel.HandledEventArgs.Handled"/> on <paramref name="e"/> to true to stop further processing.
    /// </summary>
    /// <param name="e">
    /// Event data describing the key event, including the virtual key (<see cref="KeyEventArgs.Key"/>),
    /// modifiers (<see cref="KeyEventArgs.WithShift"/>, <see cref="KeyEventArgs.WithControl"/>, <see cref="KeyEventArgs.WithMenu"/>),
    /// and other keyboard details.
    /// </param>
    void OnAccessKey(KeyEventArgs e);
}
