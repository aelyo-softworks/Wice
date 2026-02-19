namespace Wice.PropertyGrid;

/// <summary>
/// Defines an interface for objects that can be displayed in a property grid.
/// </summary>
/// <typeparam name="T">Specifies the type of the property represented by the <see cref="PropertyGridProperty{T}"/>.</typeparam>
public interface IPropertyGridObject<T>
{
    /// <summary>
    /// Determines whether the specified property should be displayed in the property grid.
    /// </summary>
    /// <param name="property">The property to evaluate for display in the property grid. This parameter influences the visibility of the
    /// property based on its attributes and current state.</param>
    /// <returns>true if the property should be shown; otherwise, false.</returns>
    bool ShowProperty(PropertyGridProperty<T> property);
}
