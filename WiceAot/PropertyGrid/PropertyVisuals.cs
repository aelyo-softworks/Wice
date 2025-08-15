namespace Wice.PropertyGrid;

/// <summary>
/// Aggregates the visuals used to display and edit a single property entry in a PropertyGrid.
/// </summary>
/// <typeparam name="T">
/// The CLR type of the selected object the property belongs to. Public properties of <typeparamref name="T"/> are
/// preserved for trimming via <see cref="DynamicallyAccessedMemberTypes.PublicProperties"/>.
/// </typeparam>
/// <remarks>
/// Simple data holder used by the PropertyGrid to keep the label and value editor together. Lifetime of the visuals
/// is managed by the grid.
/// </remarks>
public class PropertyVisuals<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>
{
    /// <summary>
    /// Gets or sets the visual used to display the property's label or descriptive text.
    /// </summary>
    /// <value>
    /// A <see cref="Visual"/> representing the textual header of the property; null when not created.
    /// </value>
    public Visual? Text { get; set; }

    /// <summary>
    /// Gets or sets the visual used to display and edit the property's value.
    /// </summary>
    /// <value>
    /// A <see cref="PropertyValueVisual{T}"/> hosting the editor for the value; null when not created.
    /// </value>
    public PropertyValueVisual<T>? ValueVisual { get; set; }
}
