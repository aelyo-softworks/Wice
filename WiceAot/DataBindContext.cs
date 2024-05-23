namespace Wice;

public class DataBindContext(object? data)
{
    private readonly Dictionary<string, object> _properties = new(StringComparer.OrdinalIgnoreCase);

    public object? Data { get; } = data;
    public virtual ItemVisual? ItemVisual { get; set; }
    public virtual Visual? DataVisual { get; set; }
    public virtual IDictionary<string, object> Properties => _properties;

    public virtual string GetDisplayName(object? context = null)
    {
        context ??= this;
        if (Data is string str)
            return str;

        if (Data is IBindingDisplayName name)
        {
            str = name.GetName(context);
            if (str != null)
                return str;
        }

        return string.Format("{0}", Data);
    }
}
