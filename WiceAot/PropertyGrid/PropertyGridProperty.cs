namespace Wice.PropertyGrid;

public partial class PropertyGridProperty<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : BaseObject, IComparable, IComparable<PropertyGridProperty<T>>
{
    public static VisualProperty LiveSyncProperty { get; } = VisualProperty.Add(typeof(PropertyGridProperty<T>), nameof(LiveSync), VisualPropertyInvalidateModes.Render, false);
    public static VisualProperty IsReadOnlyProperty { get; } = VisualProperty.Add(typeof(PropertyGridProperty<T>), nameof(IsReadOnly), VisualPropertyInvalidateModes.Render, false);
    public static VisualProperty DefaultValueProperty { get; } = VisualProperty.Add<object?>(typeof(PropertyGridProperty<T>), nameof(DefaultValue), VisualPropertyInvalidateModes.Measure, null);
    public static VisualProperty ValueProperty { get; } = VisualProperty.Add<object?>(typeof(PropertyGridProperty<T>), nameof(Value), VisualPropertyInvalidateModes.Measure, null);
    public static VisualProperty NameProperty { get; } = VisualProperty.Add<string?>(typeof(PropertyGridProperty<T>), nameof(Name), VisualPropertyInvalidateModes.Render, null);
    public static VisualProperty SortOrderProperty { get; } = VisualProperty.Add(typeof(PropertyGridProperty<T>), nameof(SortOrder), VisualPropertyInvalidateModes.Render, 0);
    public static VisualProperty HasDefaultValueProperty { get; } = VisualProperty.Add(typeof(PropertyGridProperty<T>), nameof(HasDefaultValue), VisualPropertyInvalidateModes.Render, false);
    public static VisualProperty DisplayNameProperty { get; } = VisualProperty.Add<string?>(typeof(PropertyGridProperty<T>), nameof(DisplayName), VisualPropertyInvalidateModes.Render);
    public static VisualProperty DescriptionProperty { get; } = VisualProperty.Add<string?>(typeof(PropertyGridProperty<T>), nameof(Description), VisualPropertyInvalidateModes.Render);
    public static VisualProperty TypeProperty { get; } = VisualProperty.Add<Type>(typeof(PropertyGridProperty<T>), nameof(Type), VisualPropertyInvalidateModes.Render);
    public static VisualProperty CategoryProperty { get; } = VisualProperty.Add<string?>(typeof(PropertyGridProperty<T>), nameof(Category), VisualPropertyInvalidateModes.Render);

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

    public PropertyGridSource<T> Source { get; }
    public PropertyInfo Info { get; }
    public object? OriginalValue { get; protected set; }
    public bool IsValid => Error == null;
    public bool IsInvalid => !IsValid;
    public string? ErrorText => GetError(null);
    public virtual PropertyGridPropertyOptionsAttribute Options { get; set; }
    public Type? Type { get => (Type)GetPropertyValue(TypeProperty)!; set => SetPropertyValue(TypeProperty, value); }
    public int SortOrder { get => (int)GetPropertyValue(SortOrderProperty)!; set => SetPropertyValue(SortOrderProperty, value); }
    public override string? Name { get => (string?)GetPropertyValue(NameProperty); set => SetPropertyValue(NameProperty, value); }
    public string? DisplayName { get => (string?)GetPropertyValue(DisplayNameProperty); set => SetPropertyValue(DisplayNameProperty, value); }
    public string? Description { get => (string?)GetPropertyValue(DescriptionProperty); set => SetPropertyValue(DescriptionProperty, value); }
    public string? Category { get => (string?)GetPropertyValue(CategoryProperty); set => SetPropertyValue(CategoryProperty, value); }
    public bool HasDefaultValue { get => (bool)GetPropertyValue(HasDefaultValueProperty)!; set => SetPropertyValue(HasDefaultValueProperty, value); }
    public object? DefaultValue { get => GetPropertyValue(DefaultValueProperty); set => SetPropertyValue(DefaultValueProperty, value); }
    public bool LiveSync { get => (bool)GetPropertyValue(LiveSyncProperty)!; set => SetPropertyValue(LiveSyncProperty, value); }
    public object? Value { get => GetPropertyValue(ValueProperty); set => SetPropertyValue(ValueProperty, value); }
    public bool IsReadWrite => !IsReadOnly;
    public bool IsReadOnly { get => (bool)GetPropertyValue(IsReadOnlyProperty)!; set => SetPropertyValue(IsReadOnlyProperty, value); }

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

    public bool TryGetTargetValue<TValue>(out TValue? value)
    {
        if (Conversions.TryChangeType(Value, out value))
            return true;

        if (HasDefaultValue && Conversions.TryChangeType(DefaultValue, out value))
            return true;

        return false;
    }

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

    public override string ToString() => Name + "=" + Value;

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

    public virtual void UpdateValueFromSource()
    {
        OriginalValue = Info.GetValue(Source.Value);
        Value = OriginalValue;
    }

    protected virtual void SetDescriptorValue(object? value)
    {
        var cv = Conversions.ChangeType(value, Info.PropertyType);
        Info.SetValue(Source.Value, cv);
    }

    protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(sender, e);
        base.OnPropertyChanged(sender, new PropertyChangedEventArgs(nameof(IsValid)));
        base.OnPropertyChanged(sender, new PropertyChangedEventArgs(nameof(IsInvalid)));
        base.OnPropertyChanged(sender, new PropertyChangedEventArgs(nameof(ErrorText)));
    }

    protected override bool AreErrorsEqual(IEnumerable? errors1, IEnumerable? errors2) => false;

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

    int IComparable.CompareTo(object? obj) => CompareTo(obj as PropertyGridProperty<T>);
    public virtual int CompareTo(PropertyGridProperty<T>? other)
    {
        ArgumentNullException.ThrowIfNull(other);
        var cmp = -SortOrder.CompareTo(other.SortOrder);
        if (cmp != 0)
            return cmp;

        return string.Compare(DisplayName, other.DisplayName, StringComparison.OrdinalIgnoreCase);
    }
}
