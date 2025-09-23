﻿namespace Wice.Utilities;

/// <summary>
/// Represents a selectable enumeration "bit" value with metadata suitable for UI binding.
/// </summary>
public class EnumBitValue : ISelectable, IBindingDisplayName, IValueable, IEquatable<EnumBitValue>
{
    /// <summary>
    /// Raised when <see cref="IsSelected"/> changes.
    /// </summary>
    public event EventHandler<ValueEventArgs<bool>>? IsSelectedChanged;

    private bool _isSelected;
    private readonly Lazy<bool> _isMultiValued;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumBitValue"/> class.
    /// </summary>
    /// <param name="value">The underlying enum value or token represented by this instance.</param>
    /// <param name="name">The programmatic name of the value (e.g., the enum member name).</param>
    public EnumBitValue(object value, string name)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(name);
        Value = value;
        Name = name;
        RaiseIsSelectedChanged = true;
        _isMultiValued = new Lazy<bool>(GetIsMultiValued);
    }

    /// <summary>
    /// Gets the underlying value represented by this instance (typically an enum member).
    /// </summary>
    public object Value { get; }

    /// <summary>
    /// Gets the programmatic name for the value (typically the enum member name).
    /// </summary>
    public virtual string Name { get; }

    /// <summary>
    /// Gets or sets a display-friendly name. If not set, <see cref="Name"/> is used for display.
    /// </summary>
    public virtual string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the bit value (mask) corresponding to <see cref="Value"/>.
    /// </summary>
    public virtual object? BitValue { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this item is selected.
    /// </summary>
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

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="IsSelectedChanged"/> should be raised
    /// when <see cref="IsSelected"/> is modified.
    /// </summary>
    protected virtual bool RaiseIsSelectedChanged { get; set; }

    /// <summary>
    /// Gets the bit value converted to an unsigned 64-bit integer.
    /// </summary>
    public virtual ulong UInt64BitValue => Conversions.EnumToUInt64(BitValue!);

    /// <summary>
    /// Gets a value indicating whether the bit mask is zero.
    /// </summary>
    public bool IsZero => UInt64BitValue == 0;

    /// <summary>
    /// Gets a value indicating whether the bit mask contains more than one bit set.
    /// </summary>
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

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode();

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as EnumBitValue);

    /// <summary>
    /// Determines whether the specified <see cref="EnumBitValue"/> is equal to the current object.
    /// </summary>
    /// <param name="other">The other instance to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both refer to equal <see cref="Value"/> instances; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(EnumBitValue? other) => other != null && Equals(Value, other.Value);

    /// <summary>
    /// Raises <see cref="IsSelectedChanged"/>.
    /// </summary>
    /// <param name="sender">The sender object.</param>
    /// <param name="e">The event arguments containing the new selection state.</param>
    protected virtual void OnIsSelectedChanged(object sender, ValueEventArgs<bool> e) => IsSelectedChanged?.Invoke(sender, e);

    /// <summary>
    /// Gets a human-readable display name using <see cref="DisplayName"/> if available; otherwise <see cref="Name"/>.
    /// </summary>
    /// <param name="context">An optional context object that may influence the result (not used).</param>
    /// <returns>A display-friendly name.</returns>
    public string GetName(object context) => Conversions.Decamelize(DisplayName ?? Name);

    /// <inheritdoc/>
    public override string ToString() => Value + " '" + DisplayName ?? Name + "' " + (IsSelected ? "On" : "Off");
}
