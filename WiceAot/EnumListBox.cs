namespace Wice;

/// <summary>
/// Represents a specialized <see cref="ListBox"/> control for selecting values from an enumeration.
/// </summary>
public partial class EnumListBox : ListBox, IValueable, EnumListBox.IBindList
{
    /// <summary>
    /// Gets the visual property representing the value of the control.
    /// </summary>
    public static VisualProperty ValueProperty { get; } = VisualProperty.Add<object>(typeof(EnumListBox), nameof(Value), VisualPropertyInvalidateModes.Measure, convert: EnumTypeCheck);

    /// <summary>
    /// Raised whenever <see cref="Value"/> changes, either via property set or as a result of selection changes.
    /// </summary>
    public event EventHandler<ValueEventArgs>? ValueChanged;

    internal interface IBindList
    {
        Type? Type { get; set; }
        bool NeedBind { get; set; }
    }

    internal static object? EnumTypeCheck(BaseObject obj, object? value)
    {
        if (value != null)
        {
            var type = value.GetType();
            if (!type.IsEnum)
                throw new ArgumentException(null, nameof(value));

            var elb = (IBindList)obj;
            if (elb.Type != type)
            {
                elb.Type = type;
                elb.NeedBind = true;
            }
        }

        return value;
    }

    /// <summary>
    /// Gets or sets the current enum value represented by this control.
    /// Assigning a new enum type schedules a rebind of the <see cref="ListBox.DataSource"/>.
    /// </summary>
    [Category(CategoryBehavior)]
    public object Value { get => GetPropertyValue(ValueProperty)!; set => SetPropertyValue(ValueProperty, value); }

    /// <inheritdoc />
    bool IValueable.CanChangeValue { get => IsEnabled; set => IsEnabled = value; }

    /// <inheritdoc />
    object IValueable.Value => Value;

    /// <summary>
    /// Attempts to set <see cref="Value"/> to a non-null enum instance.
    /// </summary>
    /// <param name="value">The candidate value.</param>
    /// <returns>true when the value is a non-null enum and was applied; otherwise, false.</returns>
    bool IValueable.TrySetValue(object? value)
    {
        if (value == null)
            return false;

        var type = value.GetType();
        if (!type.IsEnum)
            return false;

        Value = value;
        return true;
    }

    Type? IBindList.Type { get; set; }
    bool IBindList.NeedBind { get; set; }

    /// <inheritdoc/>
    protected override void OnSelectionChanged()
    {
        base.OnSelectionChanged();
        if (SelectedItem?.Data is EnumBitValue value)
        {
            Value = value.Value;
            return;
        }
    }

    /// <summary>
    /// Raises the <see cref="ValueChanged"/> event.
    /// </summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event args carrying the new value.</param>
    protected virtual void OnValueChanged(object sender, ValueEventArgs e) => ValueChanged?.Invoke(sender, e);

    /// <inheritdoc/>
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        if (property == ValueProperty)
        {
            OnValueChanged(this, new ValueEventArgs(value));
            var ibl = (IBindList)this;
            if (ibl.NeedBind)
            {
                // Bind items to reflect the enum's values. Expected to create EnumBitValue items.
                DataSource = EnumDataSource.FromValue(Value);
                ibl.NeedBind = false;
            }
        }
        return true;
    }
}
