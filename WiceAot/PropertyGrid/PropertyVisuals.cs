namespace Wice.PropertyGrid;

/// <summary>
/// Represents the visual elements associated with a property in a PropertyGrid control.    
/// </summary>
/// <param name="property">The property associated with the PropertyGrid control.</param>
/// <param name="rowIndex">The zero-based index of the row.</param>
#if NETFRAMEWORK
public class PropertyVisuals(PropertyGridProperty property, int rowIndex)
#else
public class PropertyVisuals<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(PropertyGridProperty<T> property, int rowIndex)
#endif
{
    /// <summary>
    /// Gets the property associated with the PropertyGrid control.
    /// </summary>
#if NETFRAMEWORK
    public PropertyGridProperty Property { get; } = property;
#else
    public PropertyGridProperty<T> Property { get; } = property;
#endif

    /// <summary>
    /// Gets the zero-based index of the row.
    /// </summary>
    public int RowIndex { get; } = rowIndex;

    /// <summary>
    /// Gets or sets the visual used to display the property's label or descriptive text.
    /// </summary>
    /// <value>
    /// A <see cref="Visual"/> representing the textual header of the property; null when not created.
    /// </value>
    public virtual Visual? Text { get; set; }

    /// <summary>
    /// Gets or sets the visual used to display and edit the property's value.
    /// </summary>
    /// <value>
    /// A <see cref="PropertyValueVisual{T}"/> hosting the editor for the value; null when not created.
    /// </value>
#if NETFRAMEWORK
    public virtual PropertyValueVisual ValueVisual { get; set; }
#else
    public virtual PropertyValueVisual<T>? ValueVisual { get; set; }
#endif
}
