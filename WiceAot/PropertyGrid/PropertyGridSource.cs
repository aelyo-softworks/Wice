namespace Wice.PropertyGrid;

/// <summary>
/// Provides a view-model-like source for a <see cref="PropertyGrid{T}"/>, wrapping a selected object
/// and exposing its public properties (filtered by <see cref="BrowsableAttribute"/>) as
/// <see cref="PropertyGridProperty{T}"/> entries.
/// </summary>
/// <typeparam name="T">
/// The component type to inspect. Marked with <see cref="DynamicallyAccessedMemberTypes.PublicProperties"/>
/// to preserve public properties under trimming/AOT.
/// </typeparam>
public partial class PropertyGridSource<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : BaseObject
{
    /// <summary>
    /// Initializes a new instance of <see cref="PropertyGridSource{T}"/> for the specified grid and value.
    /// </summary>
    /// <param name="grid">The owning property grid.</param>
    /// <param name="value">The selected object instance whose properties will be exposed.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="grid"/> is null.</exception>
    public PropertyGridSource(PropertyGrid<T> grid, T? value)
    {
        ArgumentNullException.ThrowIfNull(grid);

        Grid = grid;
        Value = value;
        ComponentName = Conversions.Decamelize(Value?.GetType().Name);
        AddProperties();
    }

    /// <summary>
    /// Gets the owning <see cref="PropertyGrid{T}"/>.
    /// </summary>
    public PropertyGrid<T> Grid { get; }

    /// <summary>
    /// Gets the selected object instance being inspected.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Gets the collection of grid-exposed properties built from <see cref="Value"/> and its type.
    /// The list is rebuilt by <see cref="AddProperties"/> and kept in sorted order.
    /// </summary>
    public virtual ObservableCollection<PropertyGridProperty<T>> Properties { get; } = [];

    /// <summary>
    /// Gets a value indicating whether all properties are valid.
    /// </summary>
    public bool IsValid => !Properties.Cast<PropertyGridProperty<T>>().Any(p => !p.IsValid);

    /// <summary>
    /// Gets a value indicating whether at least one property is invalid.
    /// </summary>
    public bool IsInvalid => !IsValid;

    /// <summary>
    /// Gets a value indicating whether any property is read-write.
    /// </summary>
    public bool IsAnyPropertyReadWrite => Properties.Any(p => p.IsReadWrite);

    /// <summary>
    /// Gets a value indicating whether all properties are read-only.
    /// </summary>
    public bool AreAllPropertiesReadOnly => !IsAnyPropertyReadWrite;

    /// <summary>
    /// Gets a value indicating whether the source is invalid or all properties are read-only.
    /// </summary>
    public bool IsInvalidOrAllPropertiesReadOnly => IsInvalid || AreAllPropertiesReadOnly;

    /// <summary>
    /// Gets a value indicating whether the source is valid and at least one property is read-write.
    /// </summary>
    public bool IsValidAndAnyPropertyReadWrite => !IsInvalidOrAllPropertiesReadOnly;

    /// <summary>
    /// Gets a user-friendly component name derived from the type name of <see cref="Value"/>.
    /// </summary>
    public string? ComponentName { get; }

    /// <summary>
    /// Refreshes the current values for all properties from the underlying <see cref="Value"/>.
    /// </summary>
    public virtual void UpdatePropertiesValues()
    {
        foreach (var prop in Properties)
        {
            prop.UpdateValueFromSource();
        }
    }

    /// <summary>
    /// Refreshes the value of a single property by name from the underlying <see cref="Value"/>.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
    public virtual void UpdatePropertyValue(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        var prop = GetProperty(name);
        if (prop == null)
            return;

        prop.UpdateValueFromSource();
    }

    /// <summary>
    /// Gets a property wrapper by name.
    /// </summary>
    /// <param name="name">The property name to look up.</param>
    /// <returns>The <see cref="PropertyGridProperty{T}"/> if found; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
    public PropertyGridProperty<T>? GetProperty(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return Properties.FirstOrDefault(p => string.Compare(p.Name, name, StringComparison.Ordinal) == 0);
    }

    /// <summary>
    /// Sets the value of a property by name, returning its previous value.
    /// </summary>
    /// <param name="name">The property name to set.</param>
    /// <param name="value">The new value to assign.</param>
    /// <returns>The previous value when the property exists; otherwise, null.</returns>
    public virtual object? SetPropertyValue(string name, object value)
    {
        var prop = GetProperty(name);
        if (prop == null)
            return null;

        var ret = prop.Value;
        prop.Value = value;
        return ret;
    }

    /// <summary>
    /// Gets the value of a property by name with type conversion support.
    /// </summary>
    /// <typeparam name="TValue">The requested value type.</typeparam>
    /// <param name="name">The property name.</param>
    /// <param name="defaultValue">The default value to return when not found or conversion fails.</param>
    /// <returns>The converted value, or <paramref name="defaultValue"/> on failure.</returns>
    public virtual TValue? GetPropertyValue<TValue>(string name, TValue? defaultValue = default)
    {
        var prop = GetProperty(name);
        if (prop == null)
            return defaultValue;

        if (!Conversions.TryChangeType(prop.Value, null, out TValue? value))
            return defaultValue;

        return value;
    }

    /// <summary>
    /// Gets the raw value of a property by name.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="defaultValue">The default value to return when not found.</param>
    /// <returns>The stored value, or <paramref name="defaultValue"/> when not found.</returns>
    public virtual object? GetPropertyValue(string name, object? defaultValue = null)
    {
        var prop = GetProperty(name);
        if (prop != null)
            return prop.Value;

        return defaultValue;
    }

    /// <summary>
    /// Enumerates public instance properties of <typeparamref name="T"/>. Ensures properties declared
    /// on <typeparamref name="T"/> are listed first, followed by inherited ones without duplicates.
    /// </summary>
    /// <returns>A read-only list of <see cref="PropertyInfo"/>.</returns>
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

    /// <summary>
    /// Rebuilds <see cref="Properties"/> from <see cref="Value"/> and its type metadata.
    /// Respects <see cref="BrowsableAttribute"/> and sorts the resulting collection.
    /// </summary>
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

    /// <summary>
    /// Raises change notifications for the specified property and all dependent computed flags.
    /// </summary>
    /// <param name="property">The changed property wrapper.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="property"/> is null.</exception>
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

    /// <summary>
    /// Creates a grid property wrapper for the given reflection property.
    /// Override to customize property creation or filtering.
    /// </summary>
    /// <param name="info">The reflection property metadata.</param>
    /// <returns>A new <see cref="PropertyGridProperty{T}"/> instance.</returns>
    protected virtual PropertyGridProperty<T> CreateProperty(PropertyInfo info) => new(this, info);
}
