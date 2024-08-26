namespace Wice.Utilities;

public class EnumBitValue : ISelectable, IBindingDisplayName, IValueable, IEquatable<EnumBitValue>
{
    public event EventHandler<ValueEventArgs<bool>>? IsSelectedChanged;

    private bool _isSelected;
    private readonly Lazy<bool> _isMultiValued;

    public EnumBitValue(object value, string name)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(name);
        Value = value;
        Name = name;
        RaiseIsSelectedChanged = true;
        _isMultiValued = new Lazy<bool>(GetIsMultiValued);
    }

    public object Value { get; }
    public virtual string Name { get; }
    public virtual string? DisplayName { get; set; }
    public virtual object? BitValue { get; set; }

    public virtual bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
                return;

            _isSelected = value;
            if (RaiseIsSelectedChanged)
            {
                OnIsSelectedChanged(this, new ValueEventArgs<bool>(_isSelected));
            }
        }
    }

    bool ISelectable.RaiseIsSelectedChanged { get => RaiseIsSelectedChanged; set => RaiseIsSelectedChanged = value; }
    protected virtual bool RaiseIsSelectedChanged { get; set; }

    public virtual ulong UInt64BitValue => Conversions.EnumToUInt64(BitValue!);
    public bool IsZero => UInt64BitValue == 0;
    public bool IsMultiValued => _isMultiValued.Value;

    private bool GetIsMultiValued()
    {
        var ul = UInt64BitValue;
        if (ul == 0 || ul == 1)
            return false;

        var one = false;
        for (var i = 0; i < 64; i++)
        {
            if ((ul & (1UL << i)) != 0)
            {
                if (one)
                    return true;

                one = true;
            }
        }
        return false;
    }

    bool IValueable.CanChangeValue { get => false; set => throw new NotSupportedException(); }
    bool IValueable.TrySetValue(object? value) => false;
    object IValueable.Value => Value;
    event EventHandler<ValueEventArgs> IValueable.ValueChanged { add { throw new NotSupportedException(); } remove { throw new NotSupportedException(); } }

    public override int GetHashCode() => Value.GetHashCode();
    public override bool Equals(object? obj) => Equals(obj as EnumBitValue);
    public bool Equals(EnumBitValue? other) => other != null && Equals(Value, other.Value);

    protected virtual void OnIsSelectedChanged(object sender, ValueEventArgs<bool> e) => IsSelectedChanged?.Invoke(sender, e);

    public string GetName(object context) => Conversions.Decamelize(DisplayName ?? Name);
    public override string ToString() => Value + " '" + DisplayName ?? Name + "' " + (IsSelected ? "On" : "Off");
}
