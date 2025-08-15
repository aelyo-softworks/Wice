namespace Wice;

/// <summary>
/// Defines a visual element that can participate in modal interactions.
/// </summary>
/// <remarks>
/// A modal visual captures user interaction until it is dismissed, typically blocking
/// interaction with other parts of the UI. Implementations should update
/// <see cref="IsModal"/> to reflect their current state.
/// </remarks>
public interface IModalVisual
{
    /// <summary>
    /// Gets a value indicating whether this visual is currently modal.
    /// </summary>
    /// <value>
    /// True if the visual is modal and blocks interaction with other visuals; otherwise, false.
    /// </value>
    bool IsModal { get; }
}
