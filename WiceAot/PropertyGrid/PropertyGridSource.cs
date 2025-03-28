namespace Wice.PropertyGrid;

public partial class PropertyGridSource<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : BaseObject
{
    public PropertyGridSource(PropertyGrid<T> grid, T? value)
    {
        ArgumentNullException.ThrowIfNull(grid);

        Grid = grid;
        Value = value;
        ComponentName = Conversions.Decamelize(Value?.GetType().Name);
        AddProperties();
    }

    public PropertyGrid<T> Grid { get; }
    public T? Value { get; }
    public virtual ObservableCollection<PropertyGridProperty<T>> Properties { get; } = [];
    public bool IsValid => !Properties.Cast<PropertyGridProperty<T>>().Any(p => !p.IsValid);
    public bool IsInvalid => !IsValid;
    public bool IsAnyPropertyReadWrite => Properties.Any(p => p.IsReadWrite);
    public bool AreAllPropertiesReadOnly => !IsAnyPropertyReadWrite;
    public bool IsInvalidOrAllPropertiesReadOnly => IsInvalid || AreAllPropertiesReadOnly;
    public bool IsValidAndAnyPropertyReadWrite => !IsInvalidOrAllPropertiesReadOnly;
    public string? ComponentName { get; }

    public virtual void UpdatePropertiesValues()
    {
        foreach (var prop in Properties)
        {
            prop.UpdateValueFromSource();
        }
    }

    public virtual void UpdatePropertyValue(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        var prop = GetProperty(name);
        if (prop == null)
            return;

        prop.UpdateValueFromSource();
    }

    public PropertyGridProperty<T>? GetProperty(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return Properties.FirstOrDefault(p => string.Compare(p.Name, name, StringComparison.Ordinal) == 0);
    }

    public virtual object? SetPropertyValue(string name, object value)
    {
        var prop = GetProperty(name);
        if (prop == null)
            return null;

        var ret = prop.Value;
        prop.Value = value;
        return ret;
    }

    public virtual TValue? GetPropertyValue<TValue>(string name, TValue? defaultValue = default)
    {
        var prop = GetProperty(name);
        if (prop == null)
            return defaultValue;

        if (!Conversions.TryChangeType(prop.Value, null, out TValue? value))
            return defaultValue;

        return value;
    }

    public virtual object? GetPropertyValue(string name, object? defaultValue = null)
    {
        var prop = GetProperty(name);
        if (prop != null)
            return prop.Value;

        return defaultValue;
    }

    protected IReadOnlyList<PropertyInfo> EnumerateProperties()
    {
        var list = new List<PropertyInfo>(typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly));
        foreach (var info in typeof(T).GetProperties())
        {
            if (list.Any(p => p.Name == info.Name))
                continue;

            list.Add(info);
        }
        return list.AsReadOnly();
    }

    public virtual void AddProperties()
    {
        Properties.Clear();
        if (Value == null)
            return;

        var props = new List<PropertyGridProperty<T>>();
        foreach (var info in EnumerateProperties())
        {
            var browsable = info.GetCustomAttribute<BrowsableAttribute>();
            if (browsable != null && !browsable.Browsable)
                continue;

            var property = CreateProperty(info);
            if (property != null)
            {
                props.Add(property);
            }
        }

        props.Sort();
        Properties.AddRange(props);
    }

    public virtual void OnPropertyChanged(PropertyGridProperty<T> property)
    {
        ArgumentNullException.ThrowIfNull(property);
        OnPropertyChanged(this, new PropertyChangedEventArgs(property.Name));
        OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(IsValid)));
        OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(IsInvalid)));
        OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(IsAnyPropertyReadWrite)));
        OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(AreAllPropertiesReadOnly)));
        OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(IsInvalidOrAllPropertiesReadOnly)));
        OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(IsValidAndAnyPropertyReadWrite)));
    }

    protected virtual PropertyGridProperty<T> CreateProperty(PropertyInfo info) => new(this, info);
}
