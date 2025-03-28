using static Wice.EnumListBox;

namespace Wice;

public partial class FlagsEnumListBox : CheckBoxList, IValueable, IBindList
{
    public static VisualProperty ValueProperty { get; } = VisualProperty.Add<object>(typeof(FlagsEnumListBox), nameof(Value), VisualPropertyInvalidateModes.Measure, convert: EnumTypeCheck);

    public event EventHandler<ValueEventArgs>? ValueChanged;

    private bool _in;

    [Category(CategoryBehavior)]
    public object? Value { get => GetPropertyValue(ValueProperty); set => SetPropertyValue(ValueProperty, value); }

    bool IValueable.CanChangeValue { get => IsEnabled; set => IsEnabled = value; }
    object? IValueable.Value => Value;
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

    public override bool UpdateItemSelection(ItemVisual visual, bool? select)
    {
        ArgumentNullException.ThrowIfNull(visual);

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
                    Value = Conversions.ChangeObjectType(value, ((IBindList)this).Type!);
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

    protected virtual void OnValueChanged(object sender, ValueEventArgs e) => ValueChanged?.Invoke(sender, e);

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
                DataSource = EnumDataSource.FromValue(Value!);
                ibl.NeedBind = false;
            }
        }
        return true;
    }
}
