namespace Wice.PropertyGrid;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public sealed class PropertyGridDynamicPropertyAttribute : Attribute
{
    public PropertyGridDynamicPropertyAttribute()
    {
        Type = typeof(object);
    }

    public object? Value { get; set; }
    public string? Name { get; set; }
    public Type? Type { get; set; }
    public override object TypeId => Name ?? string.Empty;

    public static string? GetNullifiedValueFromProperty<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(PropertyGridProperty<T> property, string name)
    {
        var att = FromProperty(property, name);
        if (att == null)
            return null;

        if (att.Value is string str)
            return str.Nullify();

        return null;
    }

    public static T? GetValueFromProperty<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(PropertyGridProperty<T> property, string name, T? defaultValue = default)
    {
        var att = FromProperty(property, name);
        if (att == null)
            return defaultValue;

        if (!Conversions.TryChangeType<T>(att.Value, out var value))
            return defaultValue;

        return value;
    }

    public static PropertyGridDynamicPropertyAttribute? FromProperty<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(PropertyGridProperty<T> property, string name)
    {
        ArgumentNullException.ThrowIfNull(property);
        if (property.Info != null)
        {
            var att = property.Info.GetCustomAttributes<PropertyGridDynamicPropertyAttribute>().FirstOrDefault(a => a.Name.EqualsIgnoreCase(name));
            if (att != null)
                return att;
        }
        return null;
    }
}
