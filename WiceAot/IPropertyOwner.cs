namespace Wice;

public interface IPropertyOwner
{
    bool TryGetPropertyValue(BaseObjectProperty property, out object? value);
    bool SetPropertyValue(BaseObjectProperty property, object value, BaseObjectSetOptions? options = null);
    object? GetPropertyValue(BaseObjectProperty property);
    bool IsPropertyValueSet(BaseObjectProperty property);
    bool ResetPropertyValue(BaseObjectProperty property, out object? value);
}
