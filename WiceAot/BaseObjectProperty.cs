namespace Wice;

/// <summary>
/// Describes a dynamic property that can be stored on <see cref="BaseObject"/> instances,
/// including metadata (name, type, defaults), conversion hooks, and change notification hooks.
/// </summary>
public class BaseObjectProperty : IEquatable<BaseObjectProperty>
{
    /// <summary>
    /// Converts an incoming value to a value appropriate for storage on the target object.
    /// </summary>
    /// <param name="obj">The owning object for which the conversion is occurring.</param>
    /// <param name="value">The incoming value to convert.</param>
    /// <returns>The converted value, which may be the original value if no conversion is necessary.</returns>
    public delegate object? ConvertDelegate(BaseObject obj, object? value);

    /// <summary>
    /// Callback invoked after a value has been stored.
    /// </summary>
    /// <param name="obj">The owning object whose value changed.</param>
    /// <param name="newValue">The new value that was set.</param>
    /// <param name="oldValue">The previous value that was replaced.</param>
    public delegate void ChangedDelegate(BaseObject obj, object? newValue, object? oldValue);

    /// <summary>
    /// Callback invoked before a value is stored; return <see langword="false"/> to veto the change.
    /// </summary>
    /// <param name="obj">The owning object whose value would change.</param>
    /// <param name="newValue">The proposed new value.</param>
    /// <param name="oldValue">The current value.</param>
    /// <returns><see langword="true"/> to allow the change; <see langword="false"/> to veto it.</returns>
    public delegate bool ChangingDelegate(BaseObject obj, object? newValue, object? oldValue);

    private static int _lastPropertyId;

    /// <summary>
    /// Global registry of all properties by <see cref="Id"/>.
    /// </summary>
    private static readonly ConcurrentDictionary<int, BaseObjectProperty> _allProperties = new();

    /// <summary>
    /// Registry of properties per declaring <see cref="Type"/>, keyed by property <see cref="Name"/>.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, BaseObjectProperty>> _propertiesPerType = new();

    /// <summary>
    /// Enumerates all properties currently registered in the process.
    /// </summary>
    public static IEnumerable<BaseObjectProperty> AllProperties => _allProperties.Values;

    /// <summary>
    /// Looks up a property by its unique <see cref="Id"/>.
    /// </summary>
    /// <param name="id">The property identifier.</param>
    /// <returns>The matching property or <see langword="null"/> if not found.</returns>
    public static BaseObjectProperty? GetById(int id)
    {
        _allProperties.TryGetValue(id, out var prop);
        return prop;
    }

    /// <summary>
    /// Looks up a property by <see cref="Name"/> and compatible <paramref name="type"/> in the inheritance chain.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="type">A type assignable to the property's <see cref="DeclaringType"/>.</param>
    /// <returns>The matching property or <see langword="null"/> if not found.</returns>
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
    /// <summary>
    /// If <paramref name="property"/> is an overridden base descriptor, returns the final descriptor as visible
    /// on <paramref name="declaringType"/>; otherwise returns <paramref name="property"/>.
    /// </summary>
    /// <param name="declaringType">The type on which the property is being requested.</param>
    /// <param name="property">The property descriptor to resolve.</param>
    /// <returns>The final descriptor visible on <paramref name="declaringType"/>, or the original when not overridden.</returns>
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

    /// <summary>
    /// Gets all properties declared directly on the specified <paramref name="declaringType"/>.
    /// </summary>
    /// <param name="declaringType">The declaring type (must derive from <see cref="BaseObject"/>).</param>
    /// <returns>An immutable view of the properties declared on that type, or an empty set.</returns>
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

    /// <summary>
    /// Registers a new property for <paramref name="declaringType"/> using <typeparamref name="T"/> as the value type.
    /// </summary>
    /// <typeparam name="T">The CLR type of the property.</typeparam>
    /// <param name="declaringType">The type declaring the property (must derive from <see cref="BaseObject"/>).</param>
    /// <param name="name">The property name (unique per declaring type).</param>
    /// <param name="defaultValue">The default value (optional).</param>
    /// <param name="convert">Optional conversion hook invoked before storage.</param>
    /// <param name="changing">Optional veto hook invoked before storage.</param>
    /// <param name="changed">Optional callback invoked after storage.</param>
    /// <returns>The registered property descriptor.</returns>
    public static BaseObjectProperty Add<
#if !NETFRAMEWORK
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    T>(Type declaringType, string name, T? defaultValue = default, ConvertDelegate? convert = null, ChangingDelegate? changing = null, ChangedDelegate? changed = null) => Add(declaringType, name, typeof(T), defaultValue, convert, changing, changed);

    /// <summary>
    /// Registers a new property for <paramref name="declaringType"/> using the provided <paramref name="type"/>.
    /// </summary>
    /// <param name="declaringType">The type declaring the property (must derive from <see cref="BaseObject"/>).</param>
    /// <param name="name">The property name (unique per declaring type).</param>
    /// <param name="type">The CLR type of the property.</param>
    /// <param name="defaultValue">The default value (optional).</param>
    /// <param name="convert">Optional conversion hook invoked before storage.</param>
    /// <param name="changing">Optional veto hook invoked before storage.</param>
    /// <param name="changed">Optional callback invoked after storage.</param>
    /// <returns>The registered property descriptor.</returns>
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

    /// <summary>
    /// Registers the given <paramref name="property"/>, assigning an <see cref="Id"/>, normalizing defaults,
    /// freezing metadata, tracking overrides, and indexing it globally.
    /// </summary>
    /// <param name="property">The property descriptor to add.</param>
    /// <returns>The same instance for chaining.</returns>
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
            // Normalize default value to the declared type.
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

    /// <summary>
    /// Walks up the type hierarchy and marks matching base properties as overridden.
    /// </summary>
    /// <param name="declaringType">The type declaring the new/overriding property.</param>
    /// <param name="property">The property being registered.</param>
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

    /// <summary>
    /// Sets the value for this property on the specified <paramref name="target"/>.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="options">Optional set behavior flags.</param>
    /// <returns><see langword="true"/> if the stored value changed (subject to <paramref name="options"/>).</returns>
    public virtual bool SetValue(BaseObject target, object? value, BaseObjectSetOptions? options = null)
    {
        ExceptionExtensions.ThrowIfNull(target, nameof(target));
        return ((IPropertyOwner)target).SetPropertyValue(this, value, options);
    }

    /// <summary>
    /// Copy the effective value for this property from the specified <paramref name="source"/> to the specified <paramref name="target"/>.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target object.</param>
    /// <param name="options">Optional set behavior flags.</param>
    /// <returns><see langword="true"/> if the value was copied and/or changed (subject to <paramref name="options"/>).</returns>
    public virtual bool CopyValue(BaseObject source, BaseObject target, BaseObjectSetOptions? options = null)
    {
        ExceptionExtensions.ThrowIfNull(source, nameof(source));
        ExceptionExtensions.ThrowIfNull(target, nameof(target));
        if (source.Equals(target))
            return false;

        if (options == null || options.OnlyIfExistsInSource)
        {
            if (!TryGetPropertyValue(source, out var value))
                return false;

            return SetValue(target, value, options);
        }

        return SetValue(target, GetValue(source), options);
    }

    /// <summary>
    /// Gets the effective value for this property on the specified <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The target object.</param>
    /// <returns>The stored value or, if unset, the converted <see cref="DefaultValue"/>.</returns>
    public virtual object? GetValue(BaseObject source)
    {
        ExceptionExtensions.ThrowIfNull(source, nameof(source));
        return ((IPropertyOwner)source).GetPropertyValue(this);
    }

    /// <summary>
    /// Attempts to get the explicitly stored value for this property on <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="value">When this method returns, contains the stored value if present.</param>
    /// <returns><see langword="true"/> if an explicit value is present; otherwise, <see langword="false"/>.</returns>
    public virtual bool TryGetPropertyValue(BaseObject source, out object? value)
    {
        ExceptionExtensions.ThrowIfNull(source, nameof(source));
        return ((IPropertyOwner)source).TryGetPropertyValue(this, out value);
    }

    /// <summary>
    /// Determines whether this property has an explicitly set value on <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The target object.</param>
    /// <returns><see langword="true"/> if explicitly set; otherwise, <see langword="false"/>.</returns>
    public virtual bool IsPropertyValueSet(BaseObject source)
    {
        ExceptionExtensions.ThrowIfNull(source, nameof(source));
        return ((IPropertyOwner)source).IsPropertyValueSet(this);
    }

    /// <summary>
    /// Clears an explicitly set value for this property on <paramref name="target"/>.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <returns><see langword="true"/> if a value was removed; otherwise, <see langword="false"/>.</returns>
    public bool ResetPropertyValue(BaseObject target) => ResetPropertyValue(target, out _);

    /// <summary>
    /// Clears an explicitly set value for this property on <paramref name="target"/>, returning the previous value.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="value">When this method returns, contains the removed value if present.</param>
    /// <returns><see langword="true"/> if a value was removed; otherwise, <see langword="false"/>.</returns>
    public virtual bool ResetPropertyValue(BaseObject target, out object? value)
    {
        ExceptionExtensions.ThrowIfNull(target, nameof(target));
        return ((IPropertyOwner)target).ResetPropertyValue(this, out value);
    }

    private object? _defaultValue;
    private BaseObjectPropertyOptions _options;

    /// <summary>
    /// Initializes a new property descriptor.
    /// </summary>
    /// <param name="declaringType">The type declaring the property (must derive from <see cref="BaseObject"/>).</param>
    /// <param name="name">The property name (unique per declaring type).</param>
    /// <param name="type">The CLR type of the property.</param>
    /// <param name="defaultValue">The default value (optional; will be normalized to <paramref name="type"/>).</param>
    /// <param name="convert">Optional conversion hook invoked before storage.</param>
    /// <param name="changing">Optional veto hook invoked before storage.</param>
    /// <param name="changed">Optional callback invoked after storage.</param>
    /// <param name="options">Behavior flags that can influence setting on owners.</param>
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

    /// <summary>
    /// Indicates whether metadata has been frozen (set during registration).
    /// </summary>
    protected bool IsFrozen { get; private set; }

    /// <summary>
    /// Indicates that a derived type has registered a property with the same <see cref="Name"/>,
    /// thereby overriding this one in lookups via <see cref="GetFinal(Type, BaseObjectProperty)"/>.
    /// </summary>
    public bool IsOverriden { get; private set; }

    /// <summary>
    /// Unique identifier for this descriptor within the current process.
    /// Assigned during <see cref="Add(BaseObjectProperty)"/>.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// The logical name of the property (unique per <see cref="DeclaringType"/>).
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The type that declared/registered this property (must derive from <see cref="BaseObject"/>).
    /// </summary>
    public Type DeclaringType { get; } // is a BaseObject

#if !NETFRAMEWORK
    /// <summary>
    /// CLR type of the property's value.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    public Type Type { get; }

    /// <summary>
    /// Optional conversion callback invoked before storing a value on an owner.
    /// </summary>
    public ConvertDelegate? Convert { get; }

    /// <summary>
    /// Optional callback invoked after a value has been stored on an owner.
    /// </summary>
    public ChangedDelegate? Changed { get; }

    /// <summary>
    /// Optional veto callback invoked before storing a value on an owner.
    /// </summary>
    public ChangingDelegate? Changing { get; }

    /// <summary>
    /// Unconverted default value. Can be set only before the descriptor is registered (then frozen).
    /// </summary>
    public object? DefaultValue { get => _defaultValue; set { if (IsFrozen) throw new InvalidOperationException(); _defaultValue = value; } }

    /// <summary>
    /// Default value converted to the target <see cref="Type"/> using invariant culture.
    /// </summary>
    public object? ConvertedDefaultValue { get; }

    /// <summary>
    /// Behavioral flags that can influence how owners set values (e.g., threading requirements).
    /// </summary>
    public BaseObjectPropertyOptions Options { get => _options; set { if (IsFrozen) throw new InvalidOperationException(); _options = value; } }

    /// <summary>
    /// Additional property names to raise notifications for when this property changes.
    /// </summary>
    public string[]? PropertyNameChanges { get; set; }

    /// <inheritdoc/>
    public override int GetHashCode() => Id.GetHashCode();

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as BaseObjectProperty);

    /// <summary>
    /// Equality is based on non-zero <see cref="Id"/> identity assigned at registration time.
    /// </summary>
    /// <param name="other">Another property descriptor.</param>
    /// <returns><see langword="true"/> if both refer to the same registered descriptor; otherwise, <see langword="false"/>.</returns>
    public bool Equals(BaseObjectProperty? other) => other != null && Id > 0 && other.Id == Id;

    /// <inheritdoc/>
    public override string ToString()
    {
        var s = Id + " " + Name;
        if (DefaultValue != null)
        {
            s += " def: " + DefaultValue + " (" + DefaultValue.GetType().Name + ")";
        }
        return s;
    }

    /// <summary>
    /// Attempts to convert <paramref name="value"/> to the property's <see cref="Type"/> using invariant culture.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="convertedValue">When this method returns, contains the converted value if successful.</param>
    /// <returns><see langword="true"/> if the conversion succeeded; otherwise, <see langword="false"/>.</returns>
    // TODO: add acrylic brush?
    public virtual bool TryConvertToTargetType(object? value, out object? convertedValue) =>
#if NETFRAMEWORK
        Conversions.TryChangeType(value, Type, CultureInfo.InvariantCulture, out convertedValue);
#else
        Conversions.TryChangeObjectType(value, Type, CultureInfo.InvariantCulture, out convertedValue);
#endif

    /// <summary>
    /// Converts <paramref name="value"/> to the property's <see cref="Type"/> using invariant culture,
    /// falling back to <see cref="ConvertedDefaultValue"/> when appropriate.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted value.</returns>
    public virtual object? ConvertToTargetType(object? value) =>
#if NETFRAMEWORK
        Conversions.ChangeType(value, Type, ConvertedDefaultValue, CultureInfo.InvariantCulture);
#else
        Conversions.ChangeObjectType(value, Type, ConvertedDefaultValue, CultureInfo.InvariantCulture);
#endif
}
