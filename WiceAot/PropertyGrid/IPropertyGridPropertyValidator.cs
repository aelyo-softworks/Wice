namespace Wice.PropertyGrid;

public interface IPropertyGridPropertyValidator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>
{
    IEnumerable ValidateValue(PropertyGridProperty<T> property);
}
