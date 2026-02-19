namespace Wice.PropertyGrid;

/// <summary>
/// Wraps a reflected property (<see cref="PropertyInfo"/>) of the selected object and exposes
/// editable state, metadata, validation, and commit/rollback logic for <see cref="PropertyGrid{T}"/>.
/// </summary>
/// <typeparam name="T">
/// The selected object type hosted by the grid. Marked with
/// <see cref="DynamicallyAccessedMemberTypes.PublicProperties"/> to cooperate with trimming/AOT.
/// </typeparam>
#if NETFRAMEWORK
public partial class PropertyGridProperty : BaseObject, IComparable, IComparable<PropertyGridProperty>, IValueable
#else
public partial class PropertyGridProperty<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : BaseObject, IComparable, IValueable, IComparable<PropertyGridProperty<T>>
#endif
{
    /// <summary>
    /// Controls whether this property applies changes to the underlying object as the user edits.
    /// </summary>
#if NETFRAMEWORK
    public static VisualProperty LiveSyncProperty { get; } = VisualProperty.Add(typeof(PropertyGridProperty), nameof(LiveSync), VisualPropertyInvalidateModes.Render, false);
#else
    public static VisualProperty LiveSyncProperty { get; } = VisualProperty.Add(typeof(PropertyGridProperty<T>), nameof(LiveSync), VisualPropertyInvalidateModes.Render, false);
#endif

    /// <summary>
    /// Indicates whether the property is read-only in the editor (derived from <see cref="PropertyInfo.CanWrite"/>).
    /// Affects editor enabled state and opacity.
    /// </summary>
#if NETFRAMEWORK
    public static VisualProperty IsReadOnlyProperty { get; } = VisualProperty.Add(typeof(PropertyGridProperty), nameof(IsReadOnly), VisualPropertyInvalidateModes.Render, false);
#else
    public static VisualProperty IsReadOnlyProperty { get; } = VisualProperty.Add(typeof(PropertyGridProperty<T>), nameof(IsReadOnly), VisualPropertyInvalidateModes.Render, false);
#endif

    /// <summary>
    /// Holds a default value (when available from <see cref="DefaultValueAttribute"/>) used when
    /// <see cref="Value"/> cannot be converted to the target <see cref="Type"/>.
    /// </summary>
#if NETFRAMEWORK
    public static VisualProperty DefaultValueProperty { get; } = VisualProperty.Add<object?>(typeof(PropertyGridProperty), nameof(DefaultValue), VisualPropertyInvalidateModes.Measure, null);
#else
    public static VisualProperty DefaultValueProperty { get; } = VisualProperty.Add<object?>(typeof(PropertyGridProperty<T>), nameof(DefaultValue), VisualPropertyInvalidateModes.Measure, null);
#endif

    /// <summary>
    /// The current editable value for this property. Changes can trigger live sync and validation.
    /// </summary>
#if NETFRAMEWORK
    public static VisualProperty ValueProperty { get; } = VisualProperty.Add<object?>(typeof(PropertyGridProperty), nameof(Value), VisualPropertyInvalidateModes.Measure, null);
#else
    public static VisualProperty ValueProperty { get; } = VisualProperty.Add<object?>(typeof(PropertyGridProperty<T>), nameof(Value), VisualPropertyInvalidateModes.Measure, null);
#endif

    /// <summary>
    /// Sort weight used when ordering properties in the grid. Higher values appear first.
    /// </summary>
#if NETFRAMEWORK
    public static VisualProperty SortOrderProperty { get; } = VisualProperty.Add(typeof(PropertyGridProperty), nameof(SortOrder), VisualPropertyInvalidateModes.Render, 0);
#else
    public static VisualProperty SortOrderProperty { get; } = VisualProperty.Add(typeof(PropertyGridProperty<T>), nameof(SortOrder), VisualPropertyInvalidateModes.Render, 0);
#endif

    /// <summary>
    /// Indicates whether a default value is available for this property (typically from <see cref="DefaultValueAttribute"/>).
    /// </summary>
#if NETFRAMEWORK
    public static VisualProperty HasDefaultValueProperty { get; } = VisualProperty.Add(typeof(PropertyGridProperty), nameof(HasDefaultValue), VisualPropertyInvalidateModes.Render, false);
#else
    public static VisualProperty HasDefaultValueProperty { get; } = VisualProperty.Add(typeof(PropertyGridProperty<T>), nameof(HasDefaultValue), VisualPropertyInvalidateModes.Render, false);
#endif

    /// <summary>
    /// Gets the visual property that indicates whether the property has a default value.
    /// </summary>
#if NETFRAMEWORK
    public static VisualProperty DisplayNameProperty { get; } = VisualProperty.Add<string?>(typeof(PropertyGridProperty), nameof(DisplayName), VisualPropertyInvalidateModes.Render);
#else
    public static VisualProperty DisplayNameProperty { get; } = VisualProperty.Add<string?>(typeof(PropertyGridProperty<T>), nameof(DisplayName), VisualPropertyInvalidateModes.Render);
#endif

    /// <summary>
    /// Description shown as secondary text or tooltip when available (from <see cref="DescriptionAttribute"/>).
    /// </summary>
#if NETFRAMEWORK
    public static VisualProperty DescriptionProperty { get; } = VisualProperty.Add<string?>(typeof(PropertyGridProperty), nameof(Description), VisualPropertyInvalidateModes.Render);
#else
    public static VisualProperty DescriptionProperty { get; } = VisualProperty.Add<string?>(typeof(PropertyGridProperty<T>), nameof(Description), VisualPropertyInvalidateModes.Render);
#endif

    /// <summary>
    /// The CLR type of the reflected property (<see cref="PropertyInfo.PropertyType"/>).
    /// </summary>
#if NETFRAMEWORK
    public static VisualProperty TypeProperty { get; } = VisualProperty.Add<Type>(typeof(PropertyGridProperty), nameof(Type), VisualPropertyInvalidateModes.Render);
#else
    public static VisualProperty TypeProperty { get; } = VisualProperty.Add<Type>(typeof(PropertyGridProperty<T>), nameof(Type), VisualPropertyInvalidateModes.Render);
#endif

    /// <summary>
    /// Category name used to group properties in the grid (from <see cref="CategoryAttribute"/> when present).
    /// </summary>
#if NETFRAMEWORK
    public static VisualProperty CategoryProperty { get; } = VisualProperty.Add<string?>(typeof(PropertyGridProperty), nameof(Category), VisualPropertyInvalidateModes.Render);
#else
    public static VisualProperty CategoryProperty { get; } = VisualProperty.Add<string?>(typeof(PropertyGridProperty<T>), nameof(Category), VisualPropertyInvalidateModes.Render);
#endif

    /// <summary>
    /// Occurs when <see cref="Value"/> changes.
    /// </summary>
    public event EventHandler<ValueEventArgs>? ValueChanged;

    bool IValueable.CanChangeValue { get => IsReadWrite; set => throw new NotSupportedException(); }
    object? IValueable.Value => Value;
    bool IValueable.TrySetValue(object? value)
    {
        try
        {
            Value = value;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Initializes a new <see cref="PropertyGridProperty{T}"/> bound to a reflected property of the selected object.
    /// </summary>
    /// <param name="source">The property source owning this wrapper.</param>
    /// <param name="info">The reflected property to wrap.</param>
#if NETFRAMEWORK
    public PropertyGridProperty(PropertyGridSource source, PropertyInfo info)
#else
    public PropertyGridProperty(PropertyGridSource<T> source, PropertyInfo info)
#endif
    {
        ExceptionExtensions.ThrowIfNull(source, nameof(source));
        ExceptionExtensions.ThrowIfNull(info, nameof(info));

        Source = source;
        Info = info;
        LiveSync = source.Grid.LiveSync;
        var options = Info.GetCustomAttribute<PropertyGridPropertyOptionsAttribute>();
        if (options != null)
        {
            SortOrder = options.SortOrder;
            Options = options;
            ListSeparator = options.ListSeparator;
            Format = options.Format;
        }
        else
        {
            Options = new PropertyGridPropertyOptionsAttribute();
        }

        Name = info.Name;
        Category = info.GetCustomAttribute<CategoryAttribute>()?.Category.Nullify();
        DisplayName = info.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName.Nullify();
        DisplayName ??= Conversions.Decamelize(Name);
        Description = info.GetCustomAttribute<DescriptionAttribute>()?.Description.Nullify();
        IsReadOnly = !info.CanWrite || info.GetCustomAttribute<ReadOnlyAttribute>()?.IsReadOnly == true;

        var dva = info.GetCustomAttribute<DefaultValueAttribute>();
        if (dva != null)
        {
            HasDefaultValue = true;
            DefaultValue = dva.Value;
        }

        var tc = info.GetCustomAttribute<TypeConverterAttribute>()?.ConverterTypeName.Nullify();
        tc ??= info.PropertyType.GetCustomAttribute<TypeConverterAttribute>()?.ConverterTypeName.Nullify();

        if (!string.IsNullOrWhiteSpace(tc))
        {
#pragma warning disable IL2057
            var type = Type.GetType(tc);
#pragma warning restore IL2057
            if (type != null)
            {
                TypeConverter = Activator.CreateInstance(type) as TypeConverter;
            }
        }

        Type = info.PropertyType;
        OriginalValue = Info.GetValue(Source.Value);
        SetPropertyValue(ValueProperty, OriginalValue, new ValueSetOptions
        {
            DontRaiseOnErrorsChanged = true,
            DontRaiseOnPropertyChanged = true,
            DontRaiseOnPropertyChanging = true
        });
    }

    /// <summary>
    /// Gets the owning <see cref="PropertyGridSource{T}"/> that provides the selected object.
    /// </summary>
#if NETFRAMEWORK
    public PropertyGridSource Source { get; }
#else
    public PropertyGridSource<T> Source { get; }
#endif

    /// <summary>
    /// Gets the reflected property information for the wrapped property.
    /// </summary>
    public PropertyInfo Info { get; }

    ///  <inheritdoc/>
    public override string Name { get => base.Name!; }

    /// <summary>
    /// Gets or sets the <see cref="TypeConverter"/> used to convert the value of the associated object to and from
    /// other types.
    /// </summary>  
    public virtual TypeConverter? TypeConverter { get; set; }

    /// <summary>
    /// Gets or sets the string used to separate items in a list.
    /// </summary>
    public virtual string? ListSeparator { get; set; }

    /// <summary>
    /// Gets or sets the format string used to define the text output representation.
    /// </summary>
    public virtual string? Format { get; set; }

    /// <summary>
    /// Gets the last committed value read from the underlying object.
    /// Updated on construction and after a successful <see cref="CommitOrRollbackChanges"/>.
    /// </summary>
    public object? OriginalValue { get; protected set; }

    /// <summary>
    /// Gets a value indicating if the property has no validation errors.
    /// </summary>
    public bool IsValid => Error == null;

    /// <summary>
    /// Gets a value indicating if the property currently has validation errors.
    /// </summary>
    public bool IsInvalid => !IsValid;

    /// <summary>
    /// Gets the aggregated error string for this property or <see langword="null"/> when no errors.
    /// </summary>
    public string? ErrorText => GetError(null);

    /// <summary>
    /// Gets or sets options that influence editor creation/behavior and sorting.
    /// </summary>
    public virtual PropertyGridPropertyOptionsAttribute Options { get; set; }

    /// <summary>
    /// Gets or sets the CLR type of the wrapped property.
    /// </summary>
    public Type? Type { get => (Type)GetPropertyValue(TypeProperty)!; set => SetPropertyValue(TypeProperty, value); }

    /// <summary>
    /// Gets or sets the sort weight for display ordering (higher first).
    /// </summary>
    public int SortOrder { get => (int)GetPropertyValue(SortOrderProperty)!; set => SetPropertyValue(SortOrderProperty, value); }

    /// <summary>
    /// Gets or sets a user-facing name for display purposes.
    /// </summary>
    public string? DisplayName { get => (string?)GetPropertyValue(DisplayNameProperty); set => SetPropertyValue(DisplayNameProperty, value); }

    /// <summary>
    /// Gets or sets an optional description used by the UI as tooltip or help text.
    /// </summary>
    public string? Description { get => (string?)GetPropertyValue(DescriptionProperty); set => SetPropertyValue(DescriptionProperty, value); }

    /// <summary>
    /// Gets or sets the category used to group properties in the grid.
    /// </summary>
    public string? Category { get => (string?)GetPropertyValue(CategoryProperty); set => SetPropertyValue(CategoryProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether a default value exists for this property.
    /// </summary>
    public bool HasDefaultValue { get => (bool)GetPropertyValue(HasDefaultValueProperty)!; set => SetPropertyValue(HasDefaultValueProperty, value); }

    /// <summary>
    /// Gets or sets the default value used as a fallback when converting <see cref="Value"/> fails.
    /// </summary>
    public object? DefaultValue { get => GetPropertyValue(DefaultValueProperty); set => SetPropertyValue(DefaultValueProperty, value); }

    /// <summary>
    /// Gets or sets whether changes to <see cref="Value"/> are committed immediately to the source object.
    /// </summary>
    public bool LiveSync { get => (bool)GetPropertyValue(LiveSyncProperty)!; set => SetPropertyValue(LiveSyncProperty, value); }

    /// <summary>
    /// Gets or sets the current editable value (untyped). Conversion to the target type is attempted on commit.
    /// </summary>
    public object? Value { get => GetPropertyValue(ValueProperty); set => SetPropertyValue(ValueProperty, value); }

    /// <summary>
    /// Gets a value indicating whether this property is editable (<see langword="true"/>) or read-only (<see langword="false"/>).
    /// </summary>
    public bool IsReadWrite => !IsReadOnly;

    /// <summary>
    /// Gets or sets a value indicating whether the property is read-only in the UI.
    /// </summary>
    public bool IsReadOnly { get => (bool)GetPropertyValue(IsReadOnlyProperty)!; set => SetPropertyValue(IsReadOnlyProperty, value); }

    /// <summary>
    /// Attempts to convert the current <see cref="Value"/> to the reflected <see cref="Type"/>.
    /// Falls back to <see cref="DefaultValue"/> (when available) if conversion fails.
    /// </summary>
    /// <param name="value">The converted value when the method returns <see langword="true"/>; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> when conversion succeeds; otherwise <see langword="false"/>.</returns>
    public virtual bool TryGetTargetValue(out object? value)
    {
        if (Type == null)
        {
            value = null;
            return false;
        }

        var v = Value;

        if (Type.IsNullable() && v == null || v is string s && string.IsNullOrWhiteSpace(s))
        {
            value = null;
            return true;
        }

        var tc = TypeConverter;
        if (tc != null)
        {
            if (v == null)
            {
                if (!Type.IsValueType)
                {
                    value = null;
                    return true;
                }

                try
                {
#pragma warning disable IL2072
                    value = Activator.CreateInstance(Type);
#pragma warning restore IL2072
                    if (value is not null)
                        return true;
                }
                catch
                {
                    // continue
                }

                value = null;
                return false;
            }

            if (tc.CanConvertTo(Type))
            {
                try
                {
                    value = tc.ConvertTo(v, Type);
                    return true;
                }
                catch
                {
                    // continue
                }
            }

            if (tc.CanConvertFrom(v.GetType()))
            {
                try
                {
                    value = tc.ConvertFrom(v);
                    return true;
                }
                catch
                {
                    // continue
                }
            }
        }

        if (SimpleConvert(Type, HasDefaultValue, DefaultValue, v, out value))
            return true;

#if NETFRAMEWORK
        if (Conversions.TryChangeType(v, Type, out value))
            return true;

        if (HasDefaultValue && Conversions.TryChangeType(DefaultValue, Type, out value))
            return true;
#else
        if (Conversions.TryChangeObjectType(v, Type, out value))
            return true;

        if (HasDefaultValue && Conversions.TryChangeObjectType(DefaultValue, Type, out value))
            return true;
#endif

        if (v is string str)
        {
            var elementType = Type.GetEnumeratedType();
            if (elementType != null)
            {
                if (ListSeparator != null)
                {
                    var parts = str.Split([ListSeparator], StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        if (Type.IsArray)
                        {
#pragma warning disable IL3050
                            var array = Array.CreateInstance(elementType, parts.Length);
#pragma warning restore IL3050
                            if (array is null)
                            {
                                value = null;
                                return false;
                            }

                            for (var i = 0; i < parts.Length; i++)
                            {
                                if (SimpleConvert(elementType, false, null, parts[i].Trim(), out var ev))
                                {
                                    array.SetValue(ev, i);
                                }
                            }
                            value = array;
                            return true;
                        }

                        if (typeof(IList).IsAssignableFrom(Type))
                        {
#pragma warning disable IL2072
                            if (Activator.CreateInstance(Type) is IList list)
#pragma warning restore IL2072
                            {
                                foreach (var part in parts)
                                {
                                    if (SimpleConvert(elementType, false, null, part.Trim(), out var ev))
                                    {
                                        list.Add(ev);
                                    }
                                }
                                value = list;
                                return true;
                            }
                        }
                    }
                    catch
                    {
                        // continue
                    }
                }
            }
        }

        return false;
    }

    private static bool SimpleConvert(Type type, bool hasDefaultValue, object? defaultValue, object? v, out object? value)
    {
#if NETFRAMEWORK
        if (Conversions.TryChangeType(v, type, out value))
            return true;

        if (hasDefaultValue && Conversions.TryChangeType(defaultValue, type, out value))
            return true;
#else
        if (Conversions.TryChangeObjectType(v, type, out value))
            return true;

        if (hasDefaultValue && Conversions.TryChangeObjectType(defaultValue, type, out value))
            return true;
#endif
        return false;
    }

    /// <summary>
    /// Attempts to retrieve the target value as the specified type.
    /// </summary>
    /// <typeparam name="TValue">The type to which the target value should be converted.</typeparam>
    /// <param name="value">When this method returns, contains the target value converted to the specified type, if the conversion was
    /// successful;  otherwise, the default value for the type <typeparamref name="TValue"/>.</param>
    /// <returns><see langword="true"/> if the target value was successfully retrieved and converted to the specified type; 
    /// otherwise, <see langword="false"/>.</returns>
    public bool TryGetTargetValue<TValue>(out TValue? value)
    {
        if (!TryGetTargetValue(out object? obj))
        {
            if (HasDefaultValue && Conversions.TryChangeType(DefaultValue, out value))
                return true;

            value = default;
            return false;
        }

        if (Conversions.TryChangeType(obj, out value))
            return true;

        if (HasDefaultValue && Conversions.TryChangeType(DefaultValue, out value))
            return true;

        value = default;
        return false;
    }

    /// <summary>
    /// Gets or sets a string representation of <see cref="Value"/> suitable for text editors.
    /// </summary>
    public virtual string? TextValue
    {
        get
        {
            var value = Value;
            var tc = TypeConverter;
            if (tc != null && tc.CanConvertTo(typeof(string)))
            {
                try
                {
                    return tc.ConvertToString(value);
                }
                catch
                {
                    // ignore and fall back to other conversions
                }
            }

            if (value is string s)
                return s;

            if (Format != null && value != null)
            {
                try
                {
                    return string.Format(Format, value);
                }
                catch
                {
                    // ignore and fall back to other conversions
                }
            }

            if (value is byte[] bytes)
                return bytes.ToHexa(7, true);

            if (value is IEnumerable enumerable)
                return string.Join(ListSeparator ?? string.Empty, enumerable.Cast<object>());

            return Conversions.ChangeType<string>(value);
        }
        set => Value = value;
    }

    /// <inheritdoc/>
    public override string ToString() => Name + "=" + Value;

    private sealed class ValueSetOptions : BaseObjectSetOptions
    {
    }

    /// <inheritdoc/>
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        if (property == IsReadOnlyProperty)
        {
            OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(IsReadWrite)));
            return true;
        }

        if (property == ValueProperty && options is not ValueSetOptions)
        {
            OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(TextValue)));
            OnValueChanged(this, new ValueEventArgs(Value));

            if (LiveSync)
            {
                CommitOrRollbackChanges();
                Source.Grid.GetPropertyVisuals(Name)?.ValueVisual?.UpdateEditor();
            }

            Source.OnPropertyValueChanged(Source, new(Name));
            Source.Grid.OnPropertyValueChanged(Source, new(Name));
            return true;
        }
        return true;
    }

    /// <summary>
    /// Attempts to commit <see cref="Value"/> to the underlying object if this property is writable. If conversion fails,
    /// reverts <see cref="Value"/> to <see cref="OriginalValue"/> and writes it back.
    /// </summary>
    /// <returns><see langword="true"/> when a commit succeeded; <see langword="false"/> when a rollback occurred.</returns>
    public virtual bool CommitOrRollbackChanges()
    {
        if (!IsReadWrite)
            return false;

        if (TryGetTargetValue(out var value))
        {
            SetDescriptorValue(value);
            OriginalValue = Info.GetValue(Source.Value);
            return true;
        }

        Value = OriginalValue;
        SetDescriptorValue(OriginalValue);
        return false;
    }

    /// <summary>
    /// Refreshes <see cref="OriginalValue"/> and <see cref="Value"/> from the selected object.
    /// </summary>
    public virtual void UpdateValueFromSource()
    {
        OriginalValue = Info.GetValue(Source.Value);
        Value = OriginalValue;
    }

    /// <summary>
    /// Converts and assigns a value to the selected object's property using reflection.
    /// </summary>
    /// <param name="value">The value to assign (may be <see langword="null"/>).</param>
    protected virtual void SetDescriptorValue(object? value)
    {
#if NETFRAMEWORK
        var cv = Conversions.ChangeType(value, Info.PropertyType);
#else
        var cv = Conversions.ChangeObjectType(value, Info.PropertyType);
#endif
        Info.SetValue(Source.Value, cv);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(sender, e);
        base.OnPropertyChanged(sender, new PropertyChangedEventArgs(nameof(IsValid)));
        base.OnPropertyChanged(sender, new PropertyChangedEventArgs(nameof(IsInvalid)));
        base.OnPropertyChanged(sender, new PropertyChangedEventArgs(nameof(ErrorText)));
    }

    /// <summary>
    /// Raises the ValueChanged event to notify subscribers when the value has changed.
    /// </summary>
    /// <param name="sender">The source of the event, typically the object whose value has changed.</param>
    /// <param name="e">An instance of ValueEventArgs that contains the new value and any associated event data.</param>
    protected virtual void OnValueChanged(object? sender, ValueEventArgs e) => ValueChanged?.Invoke(sender, e);

    /// <inheritdoc/>
    protected override bool AreErrorsEqual(IEnumerable? errors1, IEnumerable? errors2) => false;

    /// <inheritdoc/>
    protected override IEnumerable GetErrors(string? propertyName)
    {
        if (propertyName == null || propertyName == nameof(Value))
        {
#if NETFRAMEWORK
            if (Source.Value is IPropertyGridPropertyValidator validator)
#else
            if (Source.Value is IPropertyGridPropertyValidator<T> validator)
#endif
            {
                foreach (var error in validator.ValidateValue(this))
                {
                    yield return error;
                }
            }
        }

        foreach (var error in base.GetErrors(propertyName))
        {
            yield return error;
        }
    }

#if NETFRAMEWORK
    int IComparable.CompareTo(object? obj) => CompareTo(obj as PropertyGridProperty);
#else
    int IComparable.CompareTo(object? obj) => CompareTo(obj as PropertyGridProperty<T>);
#endif

    /// <summary>
    /// Compares the current <see cref="PropertyGridProperty{T}"/> instance with another instance of the same type and
    /// returns an integer that indicates their relative order.
    /// </summary>
    /// <param name="other">The <see cref="PropertyGridProperty{T}"/> instance to compare with the current instance. Cannot be <see
    /// langword="null"/>.</param>
    /// <returns>A signed integer that indicates the relative order of the objects being compared: <list type="bullet">
    /// <item><description>Less than zero if this instance precedes <paramref name="other"/> in the sort
    /// order.</description></item> <item><description>Zero if this instance occurs in the same position as <paramref
    /// name="other"/> in the sort order.</description></item> <item><description>Greater than zero if this instance
    /// follows <paramref name="other"/> in the sort order.</description></item> </list></returns>
#if NETFRAMEWORK
    public virtual int CompareTo(PropertyGridProperty? other)
#else
    public virtual int CompareTo(PropertyGridProperty<T>? other)
#endif
    {
        ExceptionExtensions.ThrowIfNull(other, nameof(other));
        var cmp = SortOrder.CompareTo(other!.SortOrder);
        if (cmp != 0)
            return cmp;

        return string.Compare(DisplayName, other.DisplayName, StringComparison.OrdinalIgnoreCase);
    }
}
