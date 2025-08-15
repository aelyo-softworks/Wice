namespace Wice;

/// <summary>
/// A ListBox specialized for enumerations, including [Flags] enums.
/// - When <see cref="Value"/> is set with an enum instance, the control auto-binds its <see cref="ListBox.DataSource"/>
///   to items representing the enum values (via <c>EnumDataSource.FromValue</c>) the first time a new enum type is encountered.
/// - Selecting an item of type <see cref="EnumBitValue"/> updates <see cref="Value"/> accordingly.
/// - Changes to <see cref="Value"/> raise <see cref="ValueChanged"/>.
/// </summary>
/// <remarks>
/// The control defers (re)binding until the first assignment of a different enum <see cref="Type"/> to optimize work.
/// Non-enum values are rejected by the property converter.
/// </remarks>
public partial class EnumListBox : ListBox, IValueable, EnumListBox.IBindList
{
    /// <summary>
    /// Backing <see cref="VisualProperty"/> for <see cref="Value"/>. Changing triggers a Measure invalidation
    /// and routes through <see cref="EnumTypeCheck(BaseObject, object?)"/> for validation/bind-triggering.
    /// </summary>
    public static VisualProperty ValueProperty { get; } = VisualProperty.Add<object>(typeof(EnumListBox), nameof(Value), VisualPropertyInvalidateModes.Measure, convert: EnumTypeCheck);

    /// <summary>
    /// Raised whenever <see cref="Value"/> changes, either via property set or as a result of selection changes.
    /// </summary>
    public event EventHandler<ValueEventArgs>? ValueChanged;

    /// <summary>
    /// Internal contract used to track the current enum <see cref="Type"/> and whether items need to be re-bound.
    /// </summary>
    internal interface IBindList
    {
        /// <summary>
        /// The enum <see cref="Type"/> currently bound. Becomes non-null after the first valid <see cref="Value"/> assignment.
        /// </summary>
        Type? Type { get; set; }
        /// <summary>
        /// Indicates that the <see cref="ListBox.DataSource"/> must be rebuilt from the current <see cref="Value"/>.
        /// Set to true when the enum type changes.
        /// </summary>
        bool NeedBind { get; set; }
    }

    /// <summary>
    /// Validates that the incoming <paramref name="value"/> is an enum and, when the enum <see cref="Type"/> changes,
    /// marks the instance to rebind its items.
    /// </summary>
    /// <param name="obj">The target object (expected to be an <see cref="EnumListBox"/>).</param>
    /// <param name="value">The value being set.</param>
    /// <returns>The validated value.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is non-null and not an enum.</exception>
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
    /// <exception cref="ArgumentException">When assigned value is not an enum (enforced by <see cref="EnumTypeCheck(BaseObject, object?)"/>).</exception>
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

    /// <inheritdoc cref="IBindList.Type"/>
    Type? IBindList.Type { get; set; }
    /// <inheritdoc cref="IBindList.NeedBind"/>
    bool IBindList.NeedBind { get; set; }

    /// <summary>
    /// Updates <see cref="Value"/> from the selected item when it is an <see cref="EnumBitValue"/>.
    /// </summary>
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

    /// <summary>
    /// Intercepts property sets to:
    /// - Raise <see cref="ValueChanged"/> when <see cref="ValueProperty"/> changes.
    /// - Rebuild the <see cref="ListBox.DataSource"/> from <see cref="Value"/> when a new enum type was detected.
    /// Delegates all other properties to the base implementation.
    /// </summary>
    /// <param name="property">The property being set.</param>
    /// <param name="value">The new value.</param>
    /// <param name="options">Optional set options.</param>
    /// <returns>true if the stored value changed; otherwise false.</returns>
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
