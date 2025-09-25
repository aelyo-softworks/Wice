namespace Wice.PropertyGrid;

/// <summary>
/// Defines a validator capable of producing validation errors for a <see cref="PropertyGridProperty{T}"/>.
/// </summary>
/// <typeparam name="T">
/// The CLR type of the property value.
/// Annotated with <see cref="DynamicallyAccessedMembersAttribute"/> requesting
/// <see cref="DynamicallyAccessedMemberTypes.PublicProperties"/> to be preserved for trimming/AOT.
/// </typeparam>
#if NETFRAMEWORK
public interface IPropertyGridPropertyValidator
#else
public interface IPropertyGridPropertyValidator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>
#endif
{
    /// <summary>
    /// Validates the current value/state of the specified property and yields any errors.
    /// </summary>
    /// <param name="property">The property to validate.</param>
    /// <returns>
    /// An enumeration of validation errors (for example, strings or error objects).
    /// An empty enumeration indicates the value is valid.
    /// </returns>
#if NETFRAMEWORK
    IEnumerable ValidateValue(PropertyGridProperty property);
#else
    IEnumerable ValidateValue(PropertyGridProperty<T> property);
#endif
}
