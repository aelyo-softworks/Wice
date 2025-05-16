namespace Wice.PropertyGrid;

public interface IPropertyGridPropertyValidator
{
    IEnumerable ValidateValue(PropertyGridProperty property);
}
