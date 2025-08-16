namespace Wice.PropertyGrid;

/// <summary>
/// Defines a named, dynamic metadata entry that can be attached multiple times to a target
/// (typically a property) and consumed by the <c>PropertyGrid</c> infrastructure.
/// </summary>
/// <remarks>
/// - Multiple instances are allowed per target (<see cref="AttributeUsageAttribute.AllowMultiple"/> is true).
/// - Each instance is identified by <see cref="Name"/>; <see cref="TypeId"/> is overridden to return <see cref="Name"/>
///   so multiple attributes can coexist and be looked up by name.
/// - <see cref="Value"/> stores the payload as <see cref="object"/>; use the static helper methods to read and convert
///   values safely from a <see cref="PropertyGridProperty{T}"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public sealed class PropertyGridDynamicPropertyAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the attribute with a default <see cref="Type"/> of <see cref="object"/>.
    /// </summary>
    public PropertyGridDynamicPropertyAttribute()
    {
        Type = typeof(object);
    }

    /// <summary>
    /// Gets or sets the stored value for this dynamic property.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// Gets or sets the logical name (key) of this dynamic property.
    /// </summary>
    /// <remarks>
    /// Name comparisons performed by this type are case-insensitive.
    /// </remarks>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the expected CLR type of <see cref="Value"/> for tooling/conversion hints.
    /// </summary>
    /// <remarks>
    /// This property is informational; callers should still attempt conversion at read time.
    /// Defaults to <see cref="object"/>.
    /// </remarks>
    public Type? Type { get; set; }

    /// <summary>
    /// Gets the unique identifier for this attribute instance used by the attribute system.
    /// </summary>
    /// <remarks>
    /// Overridden to return <see cref="Name"/> (or an empty string) to allow multiple attributes of this type
    /// to be applied and disambiguated by name.
    /// </remarks>
    public override object TypeId => Name ?? string.Empty;

    /// <summary>
    /// Gets the value of a named dynamic property on the specified grid property, returning a nullified string.
    /// </summary>
    /// <typeparam name="T">
    /// The component type inspected by the <see cref="PropertyGridProperty{T}"/>.
    /// Marked to preserve public properties under trimming/AOT.
    /// </typeparam>
    /// <param name="property">The property wrapper to inspect for this attribute.</param>
    /// <param name="name">The dynamic property name (case-insensitive).</param>
    /// <returns>
    /// The nullified string (typically converts empty/whitespace to <see langword="null"/>) when present and a string;
    /// otherwise <see langword="null"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="property"/> is <see langword="null"/>.</exception>
    public static string? GetNullifiedValueFromProperty<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(PropertyGridProperty<T> property, string name)
    {
        var att = FromProperty(property, name);
        if (att == null)
            return null;

        if (att.Value is string str)
            return str.Nullify();

        return null;
    }

    /// <summary>
    /// Gets the value of a named dynamic property on the specified grid property, converted to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The component type inspected by the <see cref="PropertyGridProperty{T}"/>, and the desired return type.
    /// Marked to preserve public properties under trimming/AOT.
    /// </typeparam>
    /// <param name="property">The property wrapper to inspect for this attribute.</param>
    /// <param name="name">The dynamic property name (case-insensitive).</param>
    /// <param name="defaultValue">The value to return when the attribute is missing or conversion fails.</param>
    /// <returns>The converted value when available; otherwise <paramref name="defaultValue"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="property"/> is <see langword="null"/>.</exception>
    public static T? GetValueFromProperty<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(PropertyGridProperty<T> property, string name, T? defaultValue = default)
    {
        var att = FromProperty(property, name);
        if (att == null)
            return defaultValue;

        if (!Conversions.TryChangeType<T>(att.Value, out var value))
            return defaultValue;

        return value;
    }

    /// <summary>
    /// Retrieves the <see cref="PropertyGridDynamicPropertyAttribute"/> with the specified <paramref name="name"/>
    /// from the given <paramref name="property"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The component type inspected by the <see cref="PropertyGridProperty{T}"/>.
    /// Marked to preserve public properties under trimming/AOT.
    /// </typeparam>
    /// <param name="property">The property wrapper whose reflected member is searched.</param>
    /// <param name="name">The dynamic property name to look up (case-insensitive).</param>
    /// <returns>The matching attribute instance, or <see langword="null"/> if not found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="property"/> is <see langword="null"/>.</exception>
    public static PropertyGridDynamicPropertyAttribute? FromProperty<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(PropertyGridProperty<T> property, string name)
    {
        ArgumentNullException.ThrowIfNull(property);
        if (property.Info != null)
        {
            var att = property.Info
                .GetCustomAttributes<PropertyGridDynamicPropertyAttribute>()
                .FirstOrDefault(a => a.Name.EqualsIgnoreCase(name));
            if (att != null)
                return att;
        }
        return null;
    }
}
