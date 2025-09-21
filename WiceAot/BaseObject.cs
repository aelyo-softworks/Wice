namespace Wice;

/// <summary>
/// Base type providing a thread-safe property bag with change/error notification plumbing.
/// </summary>
public abstract class BaseObject : INotifyPropertyChanged, INotifyPropertyChanging, IDataErrorInfo, INotifyDataErrorInfo, IPropertyOwner
{
    /// <summary>Category name used for base properties in designers.</summary>
    public const string CategoryBase = "Base";
    /// <summary>Category name used for live/computed properties in designers.</summary>
    public const string CategoryLive = "Live"; // some computed

    private static int _id;
    private static readonly ConcurrentDictionary<int, BaseObject> _objectsById = new();

    /// <summary>
    /// Retrieves an instance by its <see cref="Id"/> if it is still registered.
    /// </summary>
    /// <param name="id">The unique identifier of the instance.</param>
    /// <returns>The matching instance, or <see langword="null"/> if not found.</returns>
    public static BaseObject? GetById(int id)
    {
        _objectsById.TryGetValue(id, out var value);
        return value;
    }

#if DEBUG
    /// <summary>
    /// Records property changes for diagnostics when compiled in DEBUG.
    /// </summary>
    public class Change
    {
        /// <summary>
        /// Special property used as a marker for invalidation notifications in diagnostics.
        /// </summary>
        public static BaseObjectProperty InvalidateMarker { get; } = BaseObjectProperty.Add<VisualPropertyInvalidateModes>(typeof(Window), "Invalidate");

        private static readonly ConcurrentList<Change> _changes = [];

        /// <summary>
        /// Initializes a new change record.
        /// </summary>
        /// <param name="type">The concrete type of the owning object.</param>
        /// <param name="objectId">The <see cref="BaseObject.Id"/> of the owning object.</param>
        /// <param name="property">The property that changed.</param>
        /// <param name="value">The new value that was set.</param>
        public Change(Type type, int objectId, BaseObjectProperty property, object? value)
        {
            Index = _changes.Count;
            Type = type;
            ObjectId = objectId;
            Property = property;
            Value = value;
            _changes.Add(this);
        }

        /// <summary>Monotonic index of this change within the session log.</summary>
        public int Index;
        /// <summary>The concrete type of the object whose property changed.</summary>
        public Type Type;
        /// <summary>The id of the object whose property changed.</summary>
        public int ObjectId;
        /// <summary>The property that changed.</summary>
        public BaseObjectProperty Property;
        /// <summary>The new value of the property.</summary>
        public object? Value;

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Property == InvalidateMarker)
                return Index + "/" + ObjectId + " | " + nameof(InvalidateMarker);

            return Index + "/" + ObjectId + " | " + Type + " | " + Property + " | " + Value;
        }

        /// <summary>
        /// Determines whether this change refers to the same property on the same type as another change,
        /// ignoring the value.
        /// </summary>
        public bool IsSameAs(Change other)
        {
            if (other == null)
                return false;

            return Type == other.Type && Property == other.Property;
        }

        /// <summary>Gets an immutable view of all recorded changes.</summary>
        public static IReadOnlyList<Change> Changes => _changes;
    }
#endif

    /// <summary>Raised when the set of validation errors for a property changes.</summary>
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    /// <summary>Raised just after a property value has been updated.</summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>Raised just before a property value is updated.</summary>
    public event PropertyChangingEventHandler? PropertyChanging;

    private string? _name;
    private Lazy<string?> _fullName;

    /// <summary>
    /// Initializes a new instance, assigning a unique <see cref="Id"/> and registering it for lookup.
    /// </summary>
    protected BaseObject()
    {
#if DEBUG
        _name = GetType().Name;
#endif
        _fullName = new Lazy<string?>(GetFullName, true);
        Values = new ConcurrentDictionary<int, object?>();
        Id = Interlocked.Increment(ref _id);
        _objectsById.AddOrUpdate(Id, this, (k, o) => this);
    }

    /// <summary>Unique identifier for this instance within the current process.</summary>
    [Browsable(false)]
    public int Id { get; }

    /// <summary>
    /// Full, possibly computed display name for this instance. Backed by a lazy that is reset when <see cref="Name"/> changes.
    /// </summary>
    [Browsable(false)]
    public string? FullName => _fullName.Value;

    /// <summary>
    /// Optional user-friendly name. Defaults to the type name in DEBUG builds.
    /// </summary>
    [Category(CategoryBase)]
    public virtual string? Name
    {
        get => _name;
        set
        {
            if (_name == value)
                return;

            _name = value;
            _fullName = new Lazy<string?>(GetFullName, true);
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Produces the full display name for this instance. Override to compose a richer name.
    /// </summary>
    protected virtual string? GetFullName() => Name;

    /// <summary>
    /// Internal property bag storing values keyed by <see cref="BaseObjectProperty.Id"/>.
    /// </summary>
    protected ConcurrentDictionary<int, object?> Values { get; }

    /// <summary>
    /// When true, <see cref="OnPropertyChanging(string?)"/> is raised for compatible operations in <see cref="SetPropertyValue"/>.
    /// </summary>
    protected virtual bool RaiseOnPropertyChanging { get; set; }

    /// <summary>
    /// When true, <see cref="OnPropertyChanged(string?)"/> is raised for compatible operations in <see cref="SetPropertyValue"/>.
    /// </summary>
    protected virtual bool RaiseOnPropertyChanged { get; set; }

    /// <summary>
    /// When true, <see cref="OnErrorsChanged(string?)"/> is raised when result of <see cref="GetErrors(string?)"/> changes after a set.
    /// </summary>
    protected virtual bool RaiseOnErrorsChanged { get; set; }

#if DEBUG
    /// <inheritdoc/>
    public override string ToString() => Name.Nullify() ?? GetType().Name;
#else
    /// <inheritdoc/>
    public override string ToString() => Name ?? GetType().Name;
#endif

    /// <summary>
    /// Raises <see cref="ErrorsChanged"/> with the supplied event arguments.
    /// </summary>
    protected virtual void OnErrorsChanged(object sender, DataErrorsChangedEventArgs e) => ErrorsChanged?.Invoke(sender, e);

    /// <summary>
    /// Raises <see cref="PropertyChanging"/> with the supplied event arguments.
    /// </summary>
    protected virtual void OnPropertyChanging(object sender, PropertyChangingEventArgs e) => PropertyChanging?.Invoke(sender, e);

    /// <summary>
    /// Raises <see cref="PropertyChanged"/> with the supplied event arguments.
    /// </summary>
    protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e) => PropertyChanged?.Invoke(sender, e);

    /// <summary>
    /// Convenience overload to raise <see cref="ErrorsChanged"/> for a property by name.
    /// </summary>
    /// <param name="propertyName">The property name. If omitted, uses the caller member name.</param>
    protected void OnErrorsChanged([CallerMemberName] string? propertyName = null) => OnErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));

    /// <summary>
    /// Convenience overload to raise <see cref="PropertyChanging"/> for a property by name.
    /// </summary>
    /// <param name="propertyName">The property name. If omitted, uses the caller member name.</param>
    protected void OnPropertyChanging([CallerMemberName] string? propertyName = null) => OnPropertyChanging(this, new PropertyChangingEventArgs(propertyName));

    /// <summary>
    /// Convenience overload to raise <see cref="PropertyChanged"/> for a property by name.
    /// </summary>
    /// <param name="propertyName">The property name. If omitted, uses the caller member name.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => OnPropertyChanged(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Provides validation errors for a given property name. Override to provide custom validation.
    /// </summary>
    /// <param name="propertyName">The property name or <see langword="null"/> for entity-level errors.</param>
    /// <returns>An enumerable of error objects (often strings). Default: none.</returns>
    protected virtual IEnumerable GetErrors(string? propertyName) { yield break; }

    /// <summary>
    /// Aggregated error string for the object (entity-level). Returns <see langword="null"/> if no errors.
    /// </summary>
    protected string? Error => GetError(null);

    /// <summary>
    /// Aggregated error string for a property or the entire object.
    /// </summary>
    /// <param name="propertyName">The property name or <see langword="null"/> for entity-level errors.</param>
    /// <returns>A newline-joined string of errors, or <see langword="null"/> if none.</returns>
    protected virtual string? GetError(string? propertyName)
    {
        var errors = GetErrors(propertyName);
        if (errors == null)
            return null;

        var error = string.Join(Environment.NewLine, errors.Cast<object>().Select(e => string.Format("{0}", e)));
        return !string.IsNullOrEmpty(error) ? error : null;
    }

    /// <summary>
    /// Determines value equality for the property bag. Override for custom semantics (e.g., tolerance).
    /// </summary>
    protected virtual bool AreValuesEqual(object? value1, object? value2)
    {
        if (value1 == null)
            return value2 == null;

        if (value2 == null)
            return false;

        return value1.Equals(value2);
    }

    /// <summary>
    /// Equality comparer that delegates to <see cref="AreValuesEqual(object?, object?)"/>.
    /// </summary>
    private sealed class ObjectComparer(BaseObject bo) : IEqualityComparer<object>
    {
        private readonly BaseObject _bo = bo;

        public new bool Equals(object? x, object? y) => _bo.AreValuesEqual(x, y);
        public int GetHashCode(object obj) => (obj?.GetHashCode()).GetValueOrDefault();
    }

    /// <summary>
    /// Compares two error enumerations for multiset equality using <see cref="AreValuesEqual(object?, object?)"/>.
    /// </summary>
    /// <param name="errors1">First error sequence (may be null).</param>
    /// <param name="errors2">Second error sequence (may be null).</param>
    /// <returns><see langword="true"/> if equal as multisets; otherwise, <see langword="false"/>.</returns>
    protected virtual bool AreErrorsEqual(IEnumerable? errors1, IEnumerable? errors2)
    {
        if (errors1 == null && errors2 == null)
            return true;

        var dic = new Dictionary<object, int>(new ObjectComparer(this));
        IEnumerable<object> left = errors1 != null ? errors1.Cast<object>() : [];
        foreach (var obj in left)
        {
            if (dic.TryGetValue(obj, out int value))
            {
                dic[obj] = ++value;
            }
            else
            {
                dic.Add(obj, 1);
            }
        }

        if (errors2 == null)
            return dic.Count == 0;

        foreach (var obj in errors2)
        {
            if (dic.TryGetValue(obj, out int value))
            {
                dic[obj] = --value;
            }
            else
                return false;
        }
        return dic.Values.All(c => c == 0);
    }

    /// <summary>
    /// Copies all explicitly set values and the <see cref="Name"/> from another instance into this one.
    /// </summary>
    /// <param name="source">The instance to copy from.</param>
    /// <param name="options">Optional set behavior flags applied to each copied property.</param>
    /// <returns><see langword="true"/> if any value changed; otherwise, <see langword="false"/>.</returns>
    protected virtual bool MergeProperties(BaseObject source, BaseObjectSetOptions? options = null)
    {
        ExceptionExtensions.ThrowIfNull(source, nameof(source));
        var changed = false;
        foreach (var kv in source.Values)
        {
            var prop = BaseObjectProperty.GetById(kv.Key);
            if (prop == null)
                continue;

            if (SetPropertyValue(prop, kv.Value, options))
            {
                changed = true;
            }
        }

        if (source.Name != null && Name != source.Name)
        {
            Name = source.Name;
            changed = true;
        }
        return changed;
    }

    /// <summary>
    /// Determines whether the given property currently has an explicitly stored value.
    /// </summary>
    /// <param name="property">The property descriptor.</param>
    /// <returns><see langword="true"/> if explicitly set; otherwise, <see langword="false"/>.</returns>
    protected virtual bool IsPropertyValueSet(BaseObjectProperty property)
    {
        ExceptionExtensions.ThrowIfNull(property, nameof(property));
        property = BaseObjectProperty.GetFinal(GetType(), property);
        return Values.ContainsKey(property.Id);
    }

    /// <summary>
    /// Gets the effective value for the given property, falling back to its default if not set.
    /// </summary>
    /// <param name="property">The property descriptor.</param>
    /// <returns>The stored value or the property's default.</returns>
    protected virtual object? GetPropertyValue(BaseObjectProperty property)
    {
        ExceptionExtensions.ThrowIfNull(property, nameof(property));
        if (!TryGetPropertyValue(property, out var value))
            return property.DefaultValue;

        return value;
    }

    /// <summary>
    /// Attempts to get the explicitly stored value for a property.
    /// </summary>
    /// <param name="property">The property descriptor.</param>
    /// <param name="value">When this method returns, contains the stored value if present.</param>
    /// <returns><see langword="true"/> if a value is present; otherwise, <see langword="false"/>.</returns>
    protected virtual bool TryGetPropertyValue(BaseObjectProperty property, out object? value)
    {
        ExceptionExtensions.ThrowIfNull(property, nameof(property));
        property = BaseObjectProperty.GetFinal(GetType(), property);
        return Values.TryGetValue(property.Id, out value);
    }

    /// <summary>
    /// Clears an explicitly set value for a property.
    /// </summary>
    /// <param name="property">The property descriptor.</param>
    /// <returns><see langword="true"/> if a value was removed; otherwise, <see langword="false"/>.</returns>
    protected bool ResetPropertyValue(BaseObjectProperty property) => ResetPropertyValue(property, out _);

    /// <summary>
    /// Clears an explicitly set value for a property and returns the previous value.
    /// </summary>
    /// <param name="property">The property descriptor.</param>
    /// <param name="value">When this method returns, contains the removed value if present.</param>
    /// <returns><see langword="true"/> if a value was removed; otherwise, <see langword="false"/>.</returns>
    protected virtual bool ResetPropertyValue(BaseObjectProperty property, out object? value
    )
    {
        ExceptionExtensions.ThrowIfNull(property, nameof(property));
        property = BaseObjectProperty.GetFinal(GetType(), property);
        return Values.TryRemove(property.Id, out value);
    }

    /// <summary>
    /// Sets the value for a property with conversion, equality checks, event notifications, and validation handling.
    /// </summary>
    /// <param name="property">The property descriptor.</param>
    /// <param name="value">The new value to set. May be converted using <see cref="BaseObjectProperty.Convert"/>.</param>
    /// <param name="options">Optional behavioral flags (notifications, equality checks, etc.).</param>
    /// <returns><see langword="true"/> if the stored value changed; otherwise, <see langword="false"/>.</returns>
    protected virtual bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        ExceptionExtensions.ThrowIfNull(property, nameof(property));
        property = BaseObjectProperty.GetFinal(GetType(), property);
        if (property.Convert != null)
        {
            value = property.Convert(this, value);
        }

        IEnumerable? oldErrors = null;
        var raiseOnErrorsChanged = RaiseOnErrorsChanged;
        if (options?.DontRaiseOnErrorsChanged == true)
        {
            raiseOnErrorsChanged = false;
        }

        if (raiseOnErrorsChanged)
        {
            oldErrors = GetErrors(property.Name);
        }

        var raiseOnPropertyChanged = RaiseOnPropertyChanged;
        var forceRaiseOnPropertyChanged = options?.ForceRaiseOnPropertyChanged == true;
        if (options?.DontRaiseOnPropertyChanged == true)
        {
            raiseOnPropertyChanged = false;
            forceRaiseOnPropertyChanged = false;
        }

        var changed = true;
        object? old = null;
        var finalProp = Values.AddOrUpdate(property.Id, value, (k, o) =>
        {
            old = o;
            var testEquality = !(options?.DontTestValuesForEquality == true);
            if (testEquality && AreValuesEqual(value, o))
            {
                changed = false;
                return o;
            }

            if (property.Changing != null)
            {
                if (!property.Changing(this, value, o))
                {
                    changed = false;
                    return o;
                }
            }

            var raiseOnPropertyChanging = RaiseOnPropertyChanging;
            if (options?.DontRaiseOnPropertyChanging == true)
            {
                raiseOnPropertyChanging = false;
            }

            if (raiseOnPropertyChanging)
            {
                var e = new PropertyChangingEventArgs(property.Name);
                OnPropertyChanging(this, e);
            }

            return value;
        });

#if DEBUG
        if (changed)
        {
            _ = new Change(GetType(), Id, property, value);
        }
#endif
        if (changed && property.Changed != null)
        {
            property.Changed(this, value, old);
        }

        if ((changed && raiseOnPropertyChanged) || forceRaiseOnPropertyChanged)
        {
            var e = new PropertyChangedEventArgs(property.Name);
            OnPropertyChanged(this, e);

            var otherChanges = property.PropertyNameChanges;
            if (otherChanges != null)
            {
                foreach (var otherName in otherChanges)
                {
                    if (!string.IsNullOrWhiteSpace(otherName))
                    {
                        OnPropertyChanged(this, new PropertyChangedEventArgs(otherName));
                    }
                }
            }

            if (options?.ForceRaiseOnErrorsChanged == true)
            {
                OnErrorsChanged(this, new DataErrorsChangedEventArgs(property.Name));
            }
            else if (raiseOnErrorsChanged)
            {
                var newErrors = GetErrors(property.Name);
                if (!AreErrorsEqual(oldErrors, newErrors))
                {
                    OnErrorsChanged(this, new DataErrorsChangedEventArgs(property.Name));
                }
            }
        }

        return changed;
    }

    /// <summary>
    /// IDataErrorInfo indexer proxy, returning a concatenated string of errors for a property.
    /// </summary>
    string IDataErrorInfo.this[string columnName] => GetError(columnName)!;

    /// <summary>
    /// Indicates whether the object currently has any validation errors.
    /// </summary>
    bool INotifyDataErrorInfo.HasErrors => ((IDataErrorInfo)this).Error != null;

    /// <summary>
    /// IDataErrorInfo aggregated error string for the object.
    /// </summary>
    string IDataErrorInfo.Error => Error!;

    /// <summary>
    /// INotifyDataErrorInfo enumerates validation errors for a property.
    /// </summary>
    IEnumerable INotifyDataErrorInfo.GetErrors(string? propertyName) => GetErrors(propertyName);

    /// <inheritdoc />
    bool IPropertyOwner.TryGetPropertyValue(BaseObjectProperty property, out object? value) => TryGetPropertyValue(property, out value);

    /// <inheritdoc />
    bool IPropertyOwner.SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options) => SetPropertyValue(property, value, options);

    /// <inheritdoc />
    object? IPropertyOwner.GetPropertyValue(BaseObjectProperty property) => GetPropertyValue(property);

    /// <inheritdoc />
    bool IPropertyOwner.IsPropertyValueSet(BaseObjectProperty property) => IsPropertyValueSet(property);

    /// <inheritdoc />
    bool IPropertyOwner.ResetPropertyValue(BaseObjectProperty property, out object? value) => ResetPropertyValue(property, out value);
}
