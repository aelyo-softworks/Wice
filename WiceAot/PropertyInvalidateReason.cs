namespace Wice;

public class PropertyInvalidateReason : InvalidateReason
{
    public PropertyInvalidateReason(BaseObjectProperty property, InvalidateReason? innerReason = null)
        : base(property.DeclaringType!, innerReason)
    {
        ExceptionExtensions.ThrowIfNull(property, nameof(property));
        Property = property;
    }

    public BaseObjectProperty Property { get; }

    protected override string GetBaseString() => base.GetBaseString() + "[" + Property.Name + "]";
}
