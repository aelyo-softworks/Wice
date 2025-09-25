namespace Wice.PropertyGrid;

/// <summary>
/// Defines a named, dynamic metadata entry that can be attached multiple times to a target
/// (typically a property) and consumed by the <c>PropertyGrid</c> infrastructure.
/// </summary>
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
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the expected CLR type of <see cref="Value"/> for tooling/conversion hints.
    /// </summary>
    public Type? Type { get; set; }

    /// <inheritdoc/>
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
#if NETFRAMEWORK
    public static string? GetNullifiedValueFromProperty(PropertyGridProperty property, string name)
#else
    public static string? GetNullifiedValueFromProperty<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(PropertyGridProperty<T> property, string name)
#endif
    {
        var att = FromProperty(property, name);
        if (att == null)
            return null;

        if (att.Value is string str)
            return str.Nullify();

        return null;
    }

    /// <summary>
    /// Gets the value of a named dynamic property on the specified grid property, converted to <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The component type inspected by the <see cref="PropertyGridProperty{T}"/>, and the desired return type.
    /// Marked to preserve public properties under trimming/AOT.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The expected value or the default value.
    /// </typeparam>
    /// <param name="property">The property wrapper to inspect for this attribute.</param>
    /// <param name="name">The dynamic property name (case-insensitive).</param>
    /// <param name="defaultValue">The value to return when the attribute is missing or conversion fails.</param>
    /// <returns>The converted value when available; otherwise <paramref name="defaultValue"/>.</returns>
#if NETFRAMEWORK
    public static TValue? GetValueFromProperty<TValue>(PropertyGridProperty property, string name, TValue? defaultValue = default)
#else
    public static TValue? GetValueFromProperty<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T, TValue>(PropertyGridProperty<T> property, string name, TValue? defaultValue = default)
#endif
    {
        var att = FromProperty(property, name);
        if (att == null)
            return defaultValue;

        if (!Conversions.TryChangeType<TValue>(att.Value, out var value))
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
#if NETFRAMEWORK
    public static PropertyGridDynamicPropertyAttribute? FromProperty(PropertyGridProperty property, string name)
#else
    public static PropertyGridDynamicPropertyAttribute? FromProperty<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(PropertyGridProperty<T> property, string name)
#endif
    {
        ExceptionExtensions.ThrowIfNull(property, nameof(property));
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
