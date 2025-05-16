namespace Wice.PropertyGrid;

public class PropertyGridProperty : BaseObject, IComparable, IComparable<PropertyGridProperty>
{
    public static VisualProperty LiveSyncProperty { get; } = VisualProperty.Add(typeof(PropertyGridProperty), nameof(LiveSync), VisualPropertyInvalidateModes.Render, false);
    public static VisualProperty IsReadOnlyProperty { get; } = VisualProperty.Add(typeof(PropertyGridProperty), nameof(IsReadOnly), VisualPropertyInvalidateModes.Render, false);
    public static VisualProperty DefaultValueProperty { get; } = VisualProperty.Add<object>(typeof(PropertyGridProperty), nameof(DefaultValue), VisualPropertyInvalidateModes.Measure, null);
    public static VisualProperty ValueProperty { get; } = VisualProperty.Add<object>(typeof(PropertyGridProperty), nameof(Value), VisualPropertyInvalidateModes.Measure, null);
    public static VisualProperty NameProperty { get; } = VisualProperty.Add<string>(typeof(PropertyGridProperty), nameof(Name), VisualPropertyInvalidateModes.Render, null);
    public static VisualProperty SortOrderProperty { get; } = VisualProperty.Add(typeof(PropertyGridProperty), nameof(SortOrder), VisualPropertyInvalidateModes.Render, 0);
    public static VisualProperty HasDefaultValueProperty { get; } = VisualProperty.Add(typeof(PropertyGridProperty), nameof(HasDefaultValue), VisualPropertyInvalidateModes.Render, false);
    public static VisualProperty DisplayNameProperty { get; } = VisualProperty.Add<string>(typeof(PropertyGridProperty), nameof(DisplayName), VisualPropertyInvalidateModes.Render);
    public static VisualProperty DescriptionProperty { get; } = VisualProperty.Add<string>(typeof(PropertyGridProperty), nameof(Description), VisualPropertyInvalidateModes.Render);
    public static VisualProperty TypeProperty { get; } = VisualProperty.Add<Type>(typeof(PropertyGridProperty), nameof(Type), VisualPropertyInvalidateModes.Render);
    public static VisualProperty CategoryProperty { get; } = VisualProperty.Add<string>(typeof(PropertyGridProperty), nameof(Category), VisualPropertyInvalidateModes.Render);

    public PropertyGridProperty(PropertyGridSource source, PropertyDescriptor descriptor)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (descriptor == null)
            throw new ArgumentNullException(nameof(descriptor));

        Source = source;
        Descriptor = descriptor;
        LiveSync = source.Grid.LiveSync;
        Options = Descriptor.GetAttribute<PropertyGridPropertyOptionsAttribute>();
        if (Options != null)
        {
            SortOrder = Options.SortOrder;
        }
        else
        {
            Options = new PropertyGridPropertyOptionsAttribute();
        }

        Name = descriptor.Name;
        Category = descriptor.GetAttribute<CategoryAttribute>()?.Category.Nullify();
        DisplayName = descriptor.GetAttribute<DisplayNameAttribute>()?.DisplayName.Nullify();
        DisplayName = DisplayName ?? Conversions.Decamelize(Name);
        Description = descriptor.GetAttribute<DescriptionAttribute>()?.Description.Nullify();
        IsReadOnly = descriptor.IsReadOnly;

        var dva = descriptor.GetAttribute<DefaultValueAttribute>();
        if (dva != null)
        {
            HasDefaultValue = true;
            DefaultValue = dva.Value;
        }

        if (Options.IsEnum)
        {
            Type = Options.CreateEnumType(this);
            OriginalValue = Conversions.ChangeType(Descriptor.GetValue(Source.Value), Type);
            Value = OriginalValue;
        }
        else
        {
            Type = descriptor.PropertyType;
            OriginalValue = Descriptor.GetValue(Source.Value);
            Value = OriginalValue;
        }
    }

    public PropertyGridSource Source { get; }
    public PropertyDescriptor Descriptor { get; }
    public object OriginalValue { get; protected set; }
    public bool IsValid => Error == null;
    public bool IsInvalid => !IsValid;
    public string ErrorText => GetError(null);
    public virtual PropertyGridPropertyOptionsAttribute Options { get; set; }
    public Type Type { get => (Type)GetPropertyValue(TypeProperty); set => SetPropertyValue(TypeProperty, value); }
    public int SortOrder { get => (int)GetPropertyValue(SortOrderProperty); set => SetPropertyValue(SortOrderProperty, value); }
    public override string Name { get => (string)GetPropertyValue(NameProperty); set => SetPropertyValue(NameProperty, value); }
    public string DisplayName { get => (string)GetPropertyValue(DisplayNameProperty); set => SetPropertyValue(DisplayNameProperty, value); }
    public string Description { get => (string)GetPropertyValue(DescriptionProperty); set => SetPropertyValue(DescriptionProperty, value); }
    public string Category { get => (string)GetPropertyValue(CategoryProperty); set => SetPropertyValue(CategoryProperty, value); }
    public bool HasDefaultValue { get => (bool)GetPropertyValue(HasDefaultValueProperty); set => SetPropertyValue(HasDefaultValueProperty, value); }
    public object DefaultValue { get => GetPropertyValue(DefaultValueProperty); set => SetPropertyValue(DefaultValueProperty, value); }
    public bool LiveSync { get => (bool)GetPropertyValue(LiveSyncProperty); set => SetPropertyValue(LiveSyncProperty, value); }
    public object Value { get => GetPropertyValue(ValueProperty); set => SetPropertyValue(ValueProperty, value); }
    public bool IsReadWrite => !IsReadOnly;
    public bool IsReadOnly { get => (bool)GetPropertyValue(IsReadOnlyProperty); set => SetPropertyValue(IsReadOnlyProperty, value); }

    public virtual bool TryGetTargetValue(out object value)
    {
        if (Conversions.TryChangeType(Value, Type, out value))
            return true;

        if (HasDefaultValue && Conversions.TryChangeType(DefaultValue, Type, out value))
            return true;

        return false;
    }

    public bool TryGetTargetValue<T>(out T value)
    {
        if (Conversions.TryChangeType(Value, out value))
            return true;

        if (HasDefaultValue && Conversions.TryChangeType(DefaultValue, out value))
            return true;

        return false;
    }

    public virtual string TextValue
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

    protected override bool SetPropertyValue(BaseObjectProperty property, object value, BaseObjectSetOptions options = null)
    {
        if (property == ValueProperty)
        {
        }

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
                Source.Grid.GetVisuals(Name)?.ValueVisual?.UpdateEditor();
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
            OriginalValue = Descriptor.GetValue(Source.Value);
            return true;
        }

        Value = OriginalValue;
        SetDescriptorValue(OriginalValue);
        return false;
    }

    public virtual void UpdateValueFromSource()
    {
        OriginalValue = Descriptor.GetValue(Source.Value);
        Value = OriginalValue;
    }

    protected virtual void SetDescriptorValue(object value)
    {
        var cv = Conversions.ChangeType(value, Descriptor.PropertyType);
        Descriptor.SetValue(Source.Value, cv);
    }

    protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(sender, e);
        base.OnPropertyChanged(sender, new PropertyChangedEventArgs(nameof(IsValid)));
        base.OnPropertyChanged(sender, new PropertyChangedEventArgs(nameof(IsInvalid)));
        base.OnPropertyChanged(sender, new PropertyChangedEventArgs(nameof(ErrorText)));
    }

    protected override bool AreErrorsEqual(IEnumerable errors1, IEnumerable errors2) => false;

    protected override IEnumerable GetErrors(string propertyName)
    {
        if (propertyName == null || propertyName == nameof(Value))
        {
            if (Source.Value is IPropertyGridPropertyValidator validator)
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

    int IComparable.CompareTo(object obj) => CompareTo(obj as PropertyGridProperty);
    public virtual int CompareTo(PropertyGridProperty other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        var cmp = -SortOrder.CompareTo(other.SortOrder);
        if (cmp != 0)
            return cmp;

        return string.Compare(DisplayName, other.DisplayName, StringComparison.OrdinalIgnoreCase);
    }
}
