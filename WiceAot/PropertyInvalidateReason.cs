namespace Wice;

/// <summary>
/// Represents an <see cref="InvalidateReason"/> that is tied to a specific
/// <see cref="BaseObjectProperty"/> which triggered the invalidation.
/// </summary>
/// <remarks>
/// The base <see cref="InvalidateReason"/> captures the owning <see cref="Type"/> and an optional
/// inner reason to form a causal chain. This subclass adds the <see cref="Property"/> that initiated
/// the invalidation and augments the diagnostic string to include the property's name.
/// </remarks>
public class PropertyInvalidateReason
    : InvalidateReason
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyInvalidateReason"/> class.
    /// </summary>
    /// <param name="property">The property that triggered the invalidation.</param>
    /// <param name="innerReason">An optional inner reason composing a causal chain.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="property"/> is <c>null</c>.</exception>
    public PropertyInvalidateReason(BaseObjectProperty property, InvalidateReason? innerReason = null)
        : base(property.DeclaringType!, innerReason)
    {
        ExceptionExtensions.ThrowIfNull(property, nameof(property));
        Property = property;
    }

    /// <summary>
    /// Gets the property that triggered the invalidation.
    /// </summary>
    public BaseObjectProperty Property { get; }

    /// <summary>
    /// Builds the base diagnostic text and appends the property's logical name in brackets.
    /// </summary>
    /// <returns>A concise, human-readable description of this reason.</returns>
    protected override string GetBaseString() => base.GetBaseString() + "[" + Property.Name + "]";
}
