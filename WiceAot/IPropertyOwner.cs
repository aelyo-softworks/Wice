namespace Wice;

/// <summary>
/// Defines a contract for objects that can store and retrieve values for
/// <see cref="BaseObjectProperty"/> descriptors.
/// </summary>
/// <remarks>
/// Implementations typically back these calls with an internal property bag and may perform
/// conversion using <see cref="BaseObjectProperty.ConvertToTargetType(object?)"/> as needed.
/// In this codebase, <see cref="BaseObject"/> is a canonical implementation.
/// </remarks>
/// <seealso cref="BaseObject"/>
/// <seealso cref="BaseObjectProperty"/>
public interface IPropertyOwner
{
    /// <summary>
    /// Attempts to get the current effective value of the specified property.
    /// </summary>
    /// <param name="property">The property descriptor that identifies the value to retrieve.</param>
    /// <param name="value">
    /// When this method returns, contains the effective value if one could be produced; otherwise, <see langword="null"/>.
    /// The effective value may be an explicitly set value or a converted default from <see cref="BaseObjectProperty.DefaultValue"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a value could be obtained for the given property; otherwise, <see langword="false"/>
    /// (for example, when the property is not supported by the owner).
    /// </returns>
    bool TryGetPropertyValue(BaseObjectProperty property, out object? value);

    /// <summary>
    /// Sets or updates the value of the specified property.
    /// </summary>
    /// <param name="property">The property descriptor that identifies the value to set.</param>
    /// <param name="value">The new value to assign. Implementations may convert this to the property's target type.</param>
    /// <param name="options">
    /// Optional flags that control behavior such as change notifications and equality checks.
    /// See <see cref="BaseObjectSetOptions"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the stored value changed as a result of this call (subject to <paramref name="options"/>); otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Implementations should honor <see cref="BaseObjectSetOptions.DontTestValuesForEquality"/> and related options when deciding
    /// whether to raise change notifications.
    /// </remarks>
    bool SetPropertyValue(BaseObjectProperty property, object value, BaseObjectSetOptions? options = null);

    /// <summary>
    /// Gets the current effective value of the specified property.
    /// </summary>
    /// <param name="property">The property descriptor that identifies the value to retrieve.</param>
    /// <returns>
    /// The effective value for the property. If no explicit value is set, this is typically the converted
    /// <see cref="BaseObjectProperty.DefaultValue"/> (which may be <see langword="null"/>).
    /// </returns>
    /// <remarks>
    /// Unlike <see cref="TryGetPropertyValue(BaseObjectProperty, out object?)"/>, this method may throw if the property
    /// is not supported by the owner, depending on the implementation.
    /// </remarks>
    object? GetPropertyValue(BaseObjectProperty property);

    /// <summary>
    /// Determines whether the specified property currently has an explicitly set value.
    /// </summary>
    /// <param name="property">The property descriptor to check.</param>
    /// <returns>
    /// <see langword="true"/> if a value has been explicitly set for the property (i.e., not just the default);
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsPropertyValueSet(BaseObjectProperty property);

    /// <summary>
    /// Clears any explicitly set value for the specified property, restoring its default.
    /// </summary>
    /// <param name="property">The property descriptor to reset.</param>
    /// <param name="value">
    /// When this method returns, contains the previous effective value before the reset (which may be <see langword="null"/>).
    /// </param>
    /// <returns>
    /// <see langword="true"/> if an explicitly set value was cleared; otherwise, <see langword="false"/> if no change occurred.
    /// </returns>
    bool ResetPropertyValue(BaseObjectProperty property, out object? value);
}
