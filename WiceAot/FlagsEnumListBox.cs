namespace Wice;

/// <summary>
/// A CheckBoxList specialization that binds to a [Flags] enum and edits its value via multi-selection.
/// </summary>
public partial class FlagsEnumListBox : CheckBoxList, IValueable, EnumListBox.IBindList
{
    /// <summary>
    /// Backing property for <see cref="Value"/>.
    /// Uses <see cref="EnumListBox.EnumTypeCheck"/> to validate/normalize the incoming value
    /// and invalidates measure on change to refresh layout as needed.
    /// </summary>
    public static VisualProperty ValueProperty { get; } = VisualProperty.Add<object>(typeof(FlagsEnumListBox), nameof(Value), VisualPropertyInvalidateModes.Measure, convert: EnumListBox.EnumTypeCheck);

    /// <summary>
    /// Raised when <see cref="Value"/> changes (after storage).
    /// </summary>
    public event EventHandler<ValueEventArgs>? ValueChanged;

    private bool _in;

    /// <summary>
    /// Gets or sets the current enum value represented by this list.
    /// </summary>
    [Category(CategoryBehavior)]
    public object? Value { get => GetPropertyValue(ValueProperty); set => SetPropertyValue(ValueProperty, value); }

    /// <inheritdoc />
    bool IValueable.CanChangeValue { get => IsEnabled; set => IsEnabled = value; }

    /// <inheritdoc />
    object? IValueable.Value => Value;

    /// <inheritdoc />
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

    /// <summary>
    /// Gets or sets the enum <see cref="System.Type"/> bound to this list.
    /// </summary>
    Type? EnumListBox.IBindList.Type { get; set; }

    /// <summary>
    /// Indicates whether the data source needs to be (re)bound on the next <see cref="Value"/> set.
    /// </summary>
    bool EnumListBox.IBindList.NeedBind { get; set; }

    /// <inheritdoc/>
    public override bool UpdateItemSelection(ItemVisual visual, bool? select)
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));

        if (select.HasValue && !_in && visual.Data is EnumBitValue bitValue)
        {
            // avoid reentrency in UpdateItemSelection
            _in = true;
            try
            {
                var value = GetUint64Value();
                if (select.Value)
                {
                    value |= bitValue.UInt64BitValue;
                }
                else
                {
                    value &= ~bitValue.UInt64BitValue;
                }

                var oldValue = Conversions.ChangeType<ulong>(Value);
                if (oldValue != value)
                {
#if NETFRAMEWORK
                    Value = Conversions.ChangeType(value, ((EnumListBox.IBindList)this).Type!);
#else
                    Value = Conversions.ChangeObjectType(value, ((EnumListBox.IBindList)this).Type!);
#endif
                    var ds = EnumDataSource.FromValue(Value);
                    if (ds != null)
                    {
                        foreach (var bv in ds)
                        {
                            var item = Items.FirstOrDefault(i => bv.Equals(i.Data));
                            if (item != null)
                            {
                                item.IsSelected = bv.IsSelected;
                            }
                        }
                    }
                }
            }
            finally
            {
                _in = false;
            }
        }
        return base.UpdateItemSelection(visual, select);
    }

    private ulong GetUint64Value()
    {
        ulong value = 0;
        var selected = SelectedItems.Select(i => i.Data as EnumBitValue).Where(i => i != null).ToArray();
        foreach (var bitValue in selected)
        {
            value |= bitValue!.UInt64BitValue;
        }
        return value;
    }

    /// <summary>
    /// Raises <see cref="ValueChanged"/>.
    /// </summary>
    /// <param name="sender">The event sender (this control).</param>
    /// <param name="e">The event args containing the new value.</param>
    protected virtual void OnValueChanged(object sender, ValueEventArgs e) => ValueChanged?.Invoke(sender, e);

    /// <inheritdoc/>
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        if (property == ValueProperty)
        {
            OnValueChanged(this, new ValueEventArgs(value));
            var ibl = (EnumListBox.IBindList)this;
            if (ibl.NeedBind)
            {
                DataSource = EnumDataSource.FromValue(Value!);
                ibl.NeedBind = false;
            }
        }
        return true;
    }
}
