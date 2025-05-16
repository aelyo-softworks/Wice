namespace Wice;

public class BaseObjectProperty : IEquatable<BaseObjectProperty>
{
    public delegate object? ConvertDelegate(BaseObject obj, object? value);
    public delegate void ChangedDelegate(BaseObject obj, object? newValue, object? oldValue);
    public delegate bool ChangingDelegate(BaseObject obj, object? newValue, object? oldValue);

    private static int _lastPropertyId;
    private static readonly ConcurrentDictionary<int, BaseObjectProperty> _allProperties = new();
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, BaseObjectProperty>> _propertiesPerType = new();
    public static IEnumerable<BaseObjectProperty> AllProperties => _allProperties.Values;

    public static BaseObjectProperty? GetById(int id)
    {
        _allProperties.TryGetValue(id, out var prop);
        return prop;
    }

    public static BaseObjectProperty? GetByName(string name, Type type)
    {
        ExceptionExtensions.ThrowIfNull(name, nameof(name));
        ExceptionExtensions.ThrowIfNull(type, nameof(type));
        foreach (var kv in _allProperties)
        {
            if (kv.Value.Name == name && kv.Value.DeclaringType.IsAssignableFrom(type))
                return kv.Value;
        }
        return null;
    }

#if !NETFRAMEWORK
    [return: NotNullIfNotNull(nameof(property))]
#endif
    public static BaseObjectProperty? GetFinal(Type declaringType, BaseObjectProperty property)
    {
        if (property == null)
            return null;

        if (!property.IsOverriden)
            return property;

        ExceptionExtensions.ThrowIfNull(declaringType, nameof(declaringType));
        if (!typeof(BaseObject).IsAssignableFrom(declaringType))
            throw new ArgumentException(null, nameof(declaringType));

        if (!_propertiesPerType.TryGetValue(declaringType, out var dic))
            return property;

        if (!dic.TryGetValue(property.Name, out var overriden))
            return property;

        return overriden;
    }

    public static IReadOnlyCollection<BaseObjectProperty> GetProperties(Type declaringType)
    {
        ExceptionExtensions.ThrowIfNull(declaringType, nameof(declaringType));
        if (!typeof(BaseObject).IsAssignableFrom(declaringType))
            throw new ArgumentException(null, nameof(declaringType));

        _propertiesPerType.TryGetValue(declaringType, out var dic);
        if (dic != null)
            return (IReadOnlyCollection<BaseObjectProperty>)dic.Values;

        return [];
    }

    public static BaseObjectProperty Add<
#if !NETFRAMEWORK
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    T>(Type declaringType, string name, T? defaultValue = default, ConvertDelegate? convert = null, ChangingDelegate? changing = null, ChangedDelegate? changed = null) => Add(declaringType, name, typeof(T), defaultValue, convert, changing, changed);

    public static BaseObjectProperty Add(
        Type declaringType,
        string name,
#if !NETFRAMEWORK
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    Type type,
        object? defaultValue = null,
        ConvertDelegate? convert = null,
        ChangingDelegate? changing = null,
        ChangedDelegate? changed = null) => Add(new BaseObjectProperty(declaringType, name, type, defaultValue, convert, changing, changed));

    public static BaseObjectProperty Add(BaseObjectProperty property)
    {
        ExceptionExtensions.ThrowIfNull(property, nameof(property));
        property.Id = Interlocked.Increment(ref _lastPropertyId);

        if (property.DefaultValue == null)
        {
            if (property.Type.IsValueType)
            {
                property.DefaultValue = Activator.CreateInstance(property.Type);
            }
        }
        else
        {
            if (!property.Type.GetType().IsAssignableFrom(property.DefaultValue.GetType()))
            {
#if NETFRAMEWORK
                property.DefaultValue = Conversions.ChangeType(property.DefaultValue, property.Type);
#else
                property.DefaultValue = Conversions.ChangeObjectType(property.DefaultValue, property.Type);
#endif
            }
        }

        property.IsFrozen = true;

        _allProperties[property.Id] = property;
        if (!_propertiesPerType.TryGetValue(property.DeclaringType, out var dic))
        {
            dic = new ConcurrentDictionary<string, BaseObjectProperty>();
            _propertiesPerType.AddOrUpdate(property.DeclaringType, dic, (k, o) => o);
        }

        HandleOverrides(property.DeclaringType, property);

        dic[property.Name] = property;
        return property;
    }

    private static void HandleOverrides(Type declaringType, BaseObjectProperty property)
    {
        var baseType = declaringType.BaseType;
        if (baseType == typeof(BaseObject) || !typeof(BaseObject).IsAssignableFrom(baseType))
            return;

        if (_propertiesPerType.TryGetValue(baseType, out var props) && props.TryGetValue(property.Name, out var baseProp))
        {
            baseProp.IsOverriden = true;
        }
        HandleOverrides(baseType, property);
    }

    public virtual bool SetValue(BaseObject target, object value, BaseObjectSetOptions? options = null)
    {
        ExceptionExtensions.ThrowIfNull(target, nameof(target));
        return ((IPropertyOwner)target).SetPropertyValue(this, value, options);
    }

    public virtual object? GetValue(BaseObject target)
    {
        ExceptionExtensions.ThrowIfNull(target, nameof(target));
        return ((IPropertyOwner)target).GetPropertyValue(this);
    }

    public virtual bool TryGetPropertyValue(BaseObject target, out object? value)
    {
        ExceptionExtensions.ThrowIfNull(target, nameof(target));
        return ((IPropertyOwner)target).TryGetPropertyValue(this, out value);
    }

    public virtual bool IsPropertyValueSet(BaseObject target)
    {
        ExceptionExtensions.ThrowIfNull(target, nameof(target));
        return ((IPropertyOwner)target).IsPropertyValueSet(this);
    }

    public bool ResetPropertyValue(BaseObject target) => ResetPropertyValue(target, out _);
    public virtual bool ResetPropertyValue(BaseObject target, out object? value)
    {
        ExceptionExtensions.ThrowIfNull(target, nameof(target));
        return ((IPropertyOwner)target).ResetPropertyValue(this, out value);
    }

    private object? _defaultValue;
    private BaseObjectPropertyOptions _options;

    public BaseObjectProperty(
        Type declaringType,
        string name,
#if !NETFRAMEWORK
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] 
#endif
        Type type,
        object? defaultValue = null,
        ConvertDelegate? convert = null,
        ChangingDelegate? changing = null,
        ChangedDelegate? changed = null,
        BaseObjectPropertyOptions options = BaseObjectPropertyOptions.None)
    {
        ExceptionExtensions.ThrowIfNull(declaringType, nameof(declaringType));
        if (!typeof(BaseObject).IsAssignableFrom(declaringType))
            throw new ArgumentException("Type '" + declaringType.FullName + "' does not derive from " + nameof(BaseObject) + ".", nameof(declaringType));

        ExceptionExtensions.ThrowIfNull(name, nameof(name));
        ExceptionExtensions.ThrowIfNull(type, nameof(type));

        DeclaringType = declaringType;
        Name = name;
        Type = type;
        DefaultValue = defaultValue;
#if NETFRAMEWORK
        ConvertedDefaultValue = Conversions.ChangeType(defaultValue, type, null, CultureInfo.InvariantCulture);
#else
        ConvertedDefaultValue = Conversions.ChangeObjectType(defaultValue, type, null, CultureInfo.InvariantCulture);
#endif
        Convert = convert;
        Changing = changing;
        Changed = changed;
        Options = options;
    }

    protected bool IsFrozen { get; private set; }
    public bool IsOverriden { get; private set; }
    public int Id { get; private set; }
    public string Name { get; }
    public Type DeclaringType { get; } // is a BaseObject

#if !NETFRAMEWORK
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    public Type Type { get; }
    public ConvertDelegate? Convert { get; }
    public ChangedDelegate? Changed { get; }
    public ChangingDelegate? Changing { get; }
    public object? DefaultValue { get => _defaultValue; set { if (IsFrozen) throw new InvalidOperationException(); _defaultValue = value; } }
    public object? ConvertedDefaultValue { get; }
    public BaseObjectPropertyOptions Options { get => _options; set { if (IsFrozen) throw new InvalidOperationException(); _options = value; } }
    public string[]? PropertyNameChanges { get; set; }

    public override int GetHashCode() => Id.GetHashCode();
    public override bool Equals(object? obj) => Equals(obj as BaseObjectProperty);
    public bool Equals(BaseObjectProperty? other) => other != null && Id > 0 && other.Id == Id;

    public override string ToString()
    {
        var s = Id + " " + Name;
        if (DefaultValue != null)
        {
            s += " def: " + DefaultValue + " (" + DefaultValue.GetType().Name + ")";
        }
        return s;
    }

    // TODO: add acrylic brush?
    public virtual bool TryConvertToTargetType(object? value, out object? convertedValue) =>
#if NETFRAMEWORK
        Conversions.TryChangeType(value, Type, CultureInfo.InvariantCulture, out convertedValue);
#else
        Conversions.TryChangeObjectType(value, Type, CultureInfo.InvariantCulture, out convertedValue);
#endif
    public virtual object? ConvertToTargetType(object? value) =>
#if NETFRAMEWORK
        Conversions.ChangeType(value, Type, ConvertedDefaultValue, CultureInfo.InvariantCulture);
#else
        Conversions.ChangeObjectType(value, Type, ConvertedDefaultValue, CultureInfo.InvariantCulture);
#endif
}
