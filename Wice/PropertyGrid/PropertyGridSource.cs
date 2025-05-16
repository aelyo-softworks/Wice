namespace Wice.PropertyGrid;

public class PropertyGridSource : BaseObject
{
    public PropertyGridSource(PropertyGrid grid, object value)
    {
        if (grid == null)
            throw new ArgumentNullException(nameof(grid));

        Grid = grid;
        Value = value;
        ComponentName = Conversions.Decamelize(Value?.GetType().Name);
        AddProperties();
    }

    public PropertyGrid Grid { get; }
    public object Value { get; }
    public virtual ObservableCollection<PropertyGridProperty> Properties { get; } = new ObservableCollection<PropertyGridProperty>();
    public bool IsValid => !Properties.Cast<PropertyGridProperty>().Any(p => !p.IsValid);
    public bool IsInvalid => !IsValid;
    public bool IsAnyPropertyReadWrite => Properties.Any(p => p.IsReadWrite);
    public bool AreAllPropertiesReadOnly => !IsAnyPropertyReadWrite;
    public bool IsInvalidOrAllPropertiesReadOnly => IsInvalid || AreAllPropertiesReadOnly;
    public bool IsValidAndAnyPropertyReadWrite => !IsInvalidOrAllPropertiesReadOnly;
    public string ComponentName { get; }

    public virtual void UpdatePropertiesValues()
    {
        foreach (var prop in Properties)
        {
            prop.UpdateValueFromSource();
        }
    }

    public virtual void UpdatePropertyValue(string name)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));

        var prop = GetProperty(name);
        if (prop == null)
            return;

        prop.UpdateValueFromSource();
    }

    public PropertyGridProperty GetProperty(string name)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));

        return Properties.FirstOrDefault(p => string.Compare(p.Name, name, StringComparison.Ordinal) == 0);
    }

    public virtual object SetPropertyValue(string name, object value)
    {
        var prop = GetProperty(name);
        if (prop == null)
            return null;

        var ret = prop.Value;
        prop.Value = value;
        return ret;
    }

    public virtual T GetPropertyValue<T>(string name, T defaultValue = default)
    {
        var prop = GetProperty(name);
        if (prop == null)
            return defaultValue;

        if (!Conversions.TryChangeType(prop.Value, null, out T value))
            return defaultValue;

        return value;
    }

    public virtual object GetPropertyValue(string name, object defaultValue = null)
    {
        var prop = GetProperty(name);
        if (prop != null)
            return prop.Value;

        return defaultValue;
    }

    //public virtual void CommitChanges()
    //{
    //    foreach (var prop in Properties.Where(p => p.HasChanged))
    //    {
    //        prop.CommitChanges();
    //    }
    //}

    //public virtual void RollbackChanges()
    //{
    //    foreach (var prop in Properties.Where(p => p.HasChanged))
    //    {
    //        prop.RollbackChanges();
    //    }
    //}

    public virtual void AddProperties()
    {
        Properties.Clear();
        if (Value == null)
            return;

        var props = new List<PropertyGridProperty>();
        foreach (var descriptor in TypeDescriptor.GetProperties(Value).Cast<PropertyDescriptor>())
        {
            if (!descriptor.IsBrowsable)
                continue;

            var property = CreateProperty(descriptor);
            if (property != null)
            {
                props.Add(property);
            }
        }

        props.Sort();
        Properties.AddRange(props);
    }

    public virtual void OnPropertyChanged(PropertyGridProperty property)
    {
        if (property == null)
            throw new ArgumentNullException(nameof(property));

        OnPropertyChanged(this, new PropertyChangedEventArgs(property.Name));
        OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(IsValid)));
        OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(IsInvalid)));
        OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(IsAnyPropertyReadWrite)));
        OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(AreAllPropertiesReadOnly)));
        OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(IsInvalidOrAllPropertiesReadOnly)));
        OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(IsValidAndAnyPropertyReadWrite)));
        //OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(HasChanged)));
        //OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(IsValidAndHasChanged)));
    }

    protected virtual PropertyGridProperty CreateProperty(PropertyDescriptor descriptor) => new PropertyGridProperty(this, descriptor);
}
