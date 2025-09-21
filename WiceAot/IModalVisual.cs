namespace Wice;

/// <summary>
/// Defines a visual element that can participate in modal interactions.
/// </summary>
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
