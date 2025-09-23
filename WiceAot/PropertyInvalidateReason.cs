﻿namespace Wice;

/// <summary>
/// Represents an <see cref="InvalidateReason"/> that is tied to a specific
/// <see cref="BaseObjectProperty"/> which triggered the invalidation.
/// </summary>
public class PropertyInvalidateReason
    : InvalidateReason
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyInvalidateReason"/> class.
    /// </summary>
    /// <param name="property">The property that triggered the invalidation.</param>
    /// <param name="innerReason">An optional inner reason composing a causal chain.</param>
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

    /// <inheritdoc/>
    protected override string GetBaseString() => base.GetBaseString() + "[" + Property.Name + "]";
}
