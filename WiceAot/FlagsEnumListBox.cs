namespace Wice;

/// <summary>
/// A CheckBoxList specialization that binds to a [Flags] enum and edits its value via multi-selection.
/// </summary>
/// <remarks>
/// Behavior:
/// - Value is an enum instance (required). Each item represents a single bit value (<see cref="EnumBitValue"/>).
/// - Toggling an item updates the underlying enum value by setting/clearing its corresponding bit.
/// - Selection changes are synchronized back to the UI so that item states reflect the new value.
/// - Initial binding is delayed until <see cref="Value"/> is first set (see <see cref="EnumListBox.IBindList.NeedBind"/>).
/// Thread-safety:
/// - UI-thread affinity follows base <see cref="Visual"/> pipeline rules enforced by <see cref="VisualProperty"/>.
/// </remarks>
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
    /// <remarks>
    /// - Must be a non-null enum instance (typically a [Flags] enum).
    /// - Setting the value may trigger a deferred initial bind of items and updates selection states.
    /// </remarks>
    [Category(CategoryBehavior)]
    public object? Value { get => GetPropertyValue(ValueProperty); set => SetPropertyValue(ValueProperty, value); }

    /// <inheritdoc />
    bool IValueable.CanChangeValue { get => IsEnabled; set => IsEnabled = value; }

    /// <inheritdoc />
    object? IValueable.Value => Value;

    /// <inheritdoc />
    /// <remarks>
    /// Returns false when <paramref name="value"/> is null or not an enum; otherwise sets <see cref="Value"/>.
    /// </remarks>
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
    /// <remarks>
    /// Set by the enum list infrastructure during binding. Not intended for external use.
    /// </remarks>
    Type? EnumListBox.IBindList.Type { get; set; }

    /// <summary>
    /// Indicates whether the data source needs to be (re)bound on the next <see cref="Value"/> set.
    /// </summary>
    bool EnumListBox.IBindList.NeedBind { get; set; }

    /// <summary>
    /// Applies a selection state to an item and updates the underlying enum bit mask accordingly.
    /// </summary>
    /// <param name="visual">The item visual.</param>
    /// <param name="select">
    /// The new selection state:
    /// - true to select (set bit),
    /// - false to unselect (clear bit),
    /// - null to only refresh visual state without changing selection.
    /// </param>
    /// <returns>True when the selection state changed; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="visual"/> is null.</exception>
    /// <remarks>
    /// Implementation details:
    /// - Uses a reentrancy guard to avoid recursion when updating <see cref="Items"/> selection after <see cref="Value"/> changes.
    /// - Translates the per-item <see cref="EnumBitValue"/> into a 64-bit mask and modifies the enum value via bitwise operations.
    /// - After updating <see cref="Value"/>, re-syncs each item's <see cref="ItemVisual.IsSelected"/> to reflect the new flags.
    /// - Always calls and returns the result of the base implementation.
    /// </remarks>
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

    /// <summary>
    /// Reacts to property changes, raising <see cref="ValueChanged"/> and performing deferred binding when needed.
    /// </summary>
    /// <param name="property">The property being set.</param>
    /// <param name="value">The new value.</param>
    /// <param name="options">Optional set options.</param>
    /// <returns>true if the stored value changed; otherwise false.</returns>
    /// <remarks>
    /// When <paramref name="property"/> equals <see cref="ValueProperty"/>:
    /// - Raises <see cref="ValueChanged"/> after the value is stored.
    /// - If <see cref="EnumListBox.IBindList.NeedBind"/> is true, initializes <see cref="ListBox.DataSource"/> from the new value
    ///   and clears the flag to avoid rebinding until needed again.
    /// </remarks>
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
