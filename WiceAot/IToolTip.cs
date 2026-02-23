namespace Wice;

/// <summary>
/// Defines a contract for displaying contextual tooltips within a user interface.
/// </summary>
public interface IToolTip
{
    /// <summary>
    /// Gets or sets the visual element that serves as the target for placement of the associated element.
    /// </summary>
    Visual? PlacementTarget { get; set; }
}
