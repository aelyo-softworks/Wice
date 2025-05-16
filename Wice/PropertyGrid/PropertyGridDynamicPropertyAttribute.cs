namespace Wice.PropertyGrid;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public sealed class PropertyGridDynamicPropertyAttribute : Attribute
{
    public PropertyGridDynamicPropertyAttribute()
    {
        Type = typeof(object);
    }

    public object Value { get; set; }
    public string Name { get; set; }
    public Type Type { get; set; }
    public override object TypeId => Name;

    public static string GetNullifiedValueFromProperty(PropertyGridProperty property, string name)
    {
        var att = FromProperty(property, name);
        if (att == null)
            return null;

        if (att.Value is string str)
            return str.Nullify();

        return null;
    }

    public static T GetValueFromProperty<T>(PropertyGridProperty property, string name, T defaultValue = default)
    {
        var att = FromProperty(property, name);
        if (att == null)
            return defaultValue;

        if (!Conversions.TryChangeType<T>(att.Value, out var value))
            return defaultValue;

        return value;
    }

    public static PropertyGridDynamicPropertyAttribute FromProperty(PropertyGridProperty property, string name)
    {
        if (property == null)
            throw new ArgumentNullException(nameof(property));

        if (property.Descriptor != null)
        {
            var att = property.Descriptor.Attributes.OfType<PropertyGridDynamicPropertyAttribute>().FirstOrDefault(a => a.Name.EqualsIgnoreCase(name));
            if (att != null)
                return att;
        }
        return null;
    }
}
