namespace Wice.PropertyGrid;

/// <summary>
/// Wraps a reflected property (<see cref="PropertyInfo"/>) of the selected object and exposes
/// editable state, metadata, validation, and commit/rollback logic for <see cref="PropertyGrid{T}"/>.
/// </summary>
/// <typeparam name="T">
/// The selected object type hosted by the grid. Marked with
/// <see cref="DynamicallyAccessedMemberTypes.PublicProperties"/> to cooperate with trimming/AOT.
/// </typeparam>
/// <remarks>
/// Responsibilities:
/// - Holds reflection metadata (<see cref="Info"/>) and presentation metadata (<see cref="DisplayName"/>, <see cref="Description"/>, <see cref="Category"/>).
/// - Manages the editable <see cref="Value"/> and conversion to the target <see cref="Type"/> via <c>Conversions</c>.
/// - Coordinates live synchronization (<see cref="LiveSync"/>) with the underlying object and editor updates.
/// - Surfaces validation errors through <see cref="INotifyDataErrorInfo"/> when the source implements
///   <see cref="IPropertyGridPropertyValidator{T}"/>.
/// - Supports default values via <see cref="DefaultValueAttribute"/> and commit/rollback semantics.
/// </remarks>
public partial class PropertyGridProperty<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : BaseObject, IComparable, IComparable<PropertyGridProperty<T>>
{
    /// <summary>
    /// Controls whether this property applies changes to the underlying object as the user edits
    /// (<see cref="true"/>), or defers to an explicit commit action (<see cref="false"/>).
    /// </summary>
    public static VisualProperty LiveSyncProperty { get; } = VisualProperty.Add(typeof(PropertyGridProperty<T>), nameof(LiveSync), VisualPropertyInvalidateModes.Render, false);

    /// <summary>
    /// Indicates whether the property is read-only in the editor (derived from <see cref="PropertyInfo.CanWrite"/>).
    /// Affects editor enabled state and opacity.
    /// </summary>
    public static VisualProperty IsReadOnlyProperty { get; } = VisualProperty.Add(typeof(PropertyGridProperty<T>), nameof(IsReadOnly), VisualPropertyInvalidateModes.Render, false);

    /// <summary>
    /// Holds a default value (when available from <see cref="DefaultValueAttribute"/>) used when
    /// <see cref="Value"/> cannot be converted to the target <see cref="Type"/>.
    /// </summary>
    public static VisualProperty DefaultValueProperty { get; } = VisualProperty.Add<object?>(typeof(PropertyGridProperty<T>), nameof(DefaultValue), VisualPropertyInvalidateModes.Measure, null);

    /// <summary>
    /// The current editable value for this property. Changes can trigger live sync and validation.
    /// </summary>
    public static VisualProperty ValueProperty { get; } = VisualProperty.Add<object?>(typeof(PropertyGridProperty<T>), nameof(Value), VisualPropertyInvalidateModes.Measure, null);

    /// <summary>
    /// The CLR property name (same as <see cref="PropertyInfo.Name"/>).
    /// </summary>
    public static VisualProperty NameProperty { get; } = VisualProperty.Add<string?>(typeof(PropertyGridProperty<T>), nameof(Name), VisualPropertyInvalidateModes.Render, null);

    /// <summary>
    /// Sort weight used when ordering properties in the grid. Higher values appear first.
    /// </summary>
    public static VisualProperty SortOrderProperty { get; } = VisualProperty.Add(typeof(PropertyGridProperty<T>), nameof(SortOrder), VisualPropertyInvalidateModes.Render, 0);

    /// <summary>
    /// Indicates whether a default value is available for this property (typically from <see cref="DefaultValueAttribute"/>).
    /// </summary>
    public static VisualProperty HasDefaultValueProperty { get; } = VisualProperty.Add(typeof(PropertyGridProperty<T>), nameof(HasDefaultValue), VisualPropertyInvalidateModes.Render, false);

    /// <summary>
    /// Human-friendly name shown in the grid. Defaults to a de-camelized <see cref="Name"/> when not specified
    /// via <see cref="DisplayNameAttribute"/>.
    /// </summary>
    public static VisualProperty DisplayNameProperty { get; } = VisualProperty.Add<string?>(typeof(PropertyGridProperty<T>), nameof(DisplayName), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Description shown as secondary text or tooltip when available (from <see cref="DescriptionAttribute"/>).
    /// </summary>
    public static VisualProperty DescriptionProperty { get; } = VisualProperty.Add<string?>(typeof(PropertyGridProperty<T>), nameof(Description), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// The CLR type of the reflected property (<see cref="PropertyInfo.PropertyType"/>).
    /// </summary>
    public static VisualProperty TypeProperty { get; } = VisualProperty.Add<Type>(typeof(PropertyGridProperty<T>), nameof(Type), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Category name used to group properties in the grid (from <see cref="CategoryAttribute"/> when present).
    /// </summary>
    public static VisualProperty CategoryProperty { get; } = VisualProperty.Add<string?>(typeof(PropertyGridProperty<T>), nameof(Category), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Initializes a new <see cref="PropertyGridProperty{T}"/> bound to a reflected property of the selected object.
    /// </summary>
    /// <param name="source">The property source owning this wrapper.</param>
    /// <param name="info">The reflected property to wrap.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="info"/> is null.</exception>
    public PropertyGridProperty(PropertyGridSource<T> source, PropertyInfo info)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(info);

        Source = source;
        Info = info;
        LiveSync = source.Grid.LiveSync;
        var options = Info.GetCustomAttribute<PropertyGridPropertyOptionsAttribute>();
        if (options != null)
        {
            SortOrder = options.SortOrder;
            Options = options;
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
        IsReadOnly = !info.CanWrite;

        var dva = info.GetCustomAttribute<DefaultValueAttribute>();
        if (dva != null)
        {
            HasDefaultValue = true;
            DefaultValue = dva.Value;
        }

        Type = info.PropertyType;
        OriginalValue = Info.GetValue(Source.Value);
        Value = OriginalValue;
    }

    /// <summary>
    /// Gets the owning <see cref="PropertyGridSource{T}"/> that provides the selected object.
    /// </summary>
    public PropertyGridSource<T> Source { get; }

    /// <summary>
    /// Gets the reflected property information for the wrapped property.
    /// </summary>
    public PropertyInfo Info { get; }

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

    /// <inheritdoc/>
    public override string? Name { get => (string?)GetPropertyValue(NameProperty); set => SetPropertyValue(NameProperty, value); }

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

        if (Conversions.TryChangeObjectType(Value, Type, out value))
            return true;

        if (HasDefaultValue && Conversions.TryChangeObjectType(DefaultValue, Type, out value))
            return true;

        return false;
    }

    /// <summary>
    /// Attempts to convert the current <see cref="Value"/> to the specified <typeparamref name="TValue"/>.
    /// Falls back to <see cref="DefaultValue"/> (when available) if conversion fails.
    /// </summary>
    /// <typeparam name="TValue">The desired target type.</typeparam>
    /// <param name="value">The converted value when the method returns <see langword="true"/>; otherwise default.</param>
    /// <returns><see langword="true"/> when conversion succeeds; otherwise <see langword="false"/>.</returns>
    public bool TryGetTargetValue<TValue>(out TValue? value)
    {
        if (Conversions.TryChangeType(Value, out value))
            return true;

        if (HasDefaultValue && Conversions.TryChangeType(DefaultValue, out value))
            return true;

        return false;
    }

    /// <summary>
    /// Gets or sets a string representation of <see cref="Value"/> suitable for text editors.
    /// </summary>
    /// <remarks>
    /// Conversion rules:
    /// - <see cref="string"/> values are returned as-is.<br/>
    /// - <see cref="byte"/>[] are formatted as hexadecimal (compact) via <c>ToHexa(7, true)</c>.<br/>
    /// - <see cref="IEnumerable"/> values are concatenated via <see cref="string.Join(string, object[])"/>.<br/>
    /// - Otherwise uses <c>Conversions.ChangeType&lt;string&gt;()</c>.<br/>
    /// Setting assigns directly to <see cref="Value"/>.
    /// </remarks>
    public virtual string? TextValue
    {
        get
        {
            if (Value is string s)
                return s;

            if (Value is byte[] bytes)
                return bytes.ToHexa(7, true);

            if (Value is IEnumerable enumerable)
                return string.Join(string.Empty, enumerable.Cast<object>());

            return Conversions.ChangeType<string>(Value);
        }
        set => Value = value;
    }

    /// <summary>
    /// Returns a simple "Name=Value" string useful for diagnostics.
    /// </summary>
    public override string ToString() => Name + "=" + Value;

    /// <summary>
    /// Intercepts property changes to raise dependent notifications and to apply live synchronization behavior.
    /// </summary>
    /// <param name="property">The property descriptor being changed.</param>
    /// <param name="value">The new value.</param>
    /// <param name="options">Optional set behavior flags.</param>
    /// <returns><see langword="true"/> if the stored value changed; otherwise <see langword="false"/>.</returns>
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        if (property == IsReadOnlyProperty)
        {
            OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(IsReadWrite)));
            return true;
        }

        if (property == ValueProperty)
        {
            Source.OnPropertyChanged(this);
            OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(TextValue)));

            if (LiveSync)
            {
                CommitOrRollbackChanges();
                if (Name != null)
                {
                    Source.Grid.GetVisuals(Name)?.ValueVisual?.UpdateEditor();
                }
            }
            return true;
        }
        return true;
    }

    /// <summary>
    /// Attempts to commit <see cref="Value"/> to the underlying object. If conversion fails,
    /// reverts <see cref="Value"/> to <see cref="OriginalValue"/> and writes it back.
    /// </summary>
    /// <returns><see langword="true"/> when a commit succeeded; <see langword="false"/> when a rollback occurred.</returns>
    public virtual bool CommitOrRollbackChanges()
    {
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
        var cv = Conversions.ChangeType(value, Info.PropertyType);
        Info.SetValue(Source.Value, cv);
    }

    /// <summary>
    /// Raises standard change notifications and keeps validity flags in sync.
    /// </summary>
    /// <param name="sender">The change sender.</param>
    /// <param name="e">Change arguments.</param>
    protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(sender, e);
        base.OnPropertyChanged(sender, new PropertyChangedEventArgs(nameof(IsValid)));
        base.OnPropertyChanged(sender, new PropertyChangedEventArgs(nameof(IsInvalid)));
        base.OnPropertyChanged(sender, new PropertyChangedEventArgs(nameof(ErrorText)));
    }

    /// <summary>
    /// Always returns <see langword="false"/> to force error-diff notifications on value changes.
    /// </summary>
    /// <param name="errors1">First error sequence.</param>
    /// <param name="errors2">Second error sequence.</param>
    /// <returns>Always <see langword="false"/>.</returns>
    protected override bool AreErrorsEqual(IEnumerable? errors1, IEnumerable? errors2) => false;

    /// <summary>
    /// Yields validation errors for <see cref="Value"/>. If the selected object implements
    /// <see cref="IPropertyGridPropertyValidator{T}"/>, delegates to it, then appends base errors.
    /// </summary>
    /// <param name="propertyName">The property name being queried; <see langword="null"/> for entity-level errors.</param>
    /// <returns>An enumeration of errors (possibly empty).</returns>
    protected override IEnumerable GetErrors(string? propertyName)
    {
        if (propertyName == null || propertyName == nameof(Value))
        {
            if (Source.Value is IPropertyGridPropertyValidator<T> validator)
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

    /// <inheritdoc/>
    int IComparable.CompareTo(object? obj) => CompareTo(obj as PropertyGridProperty<T>);

    /// <summary>
    /// Compares properties for display ordering: first by <see cref="SortOrder"/> descending,
    /// then by <see cref="DisplayName"/> ascending (case-insensitive).
    /// </summary>
    /// <param name="other">The other property to compare.</param>
    /// <returns>A negative, zero, or positive value according to comparison semantics.</returns>
    public virtual int CompareTo(PropertyGridProperty<T>? other)
    {
        ArgumentNullException.ThrowIfNull(other);
        var cmp = -SortOrder.CompareTo(other.SortOrder);
        if (cmp != 0)
            return cmp;

        return string.Compare(DisplayName, other.DisplayName, StringComparison.OrdinalIgnoreCase);
    }
}
