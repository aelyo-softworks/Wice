namespace Wice.PropertyGrid;

/// <summary>
/// Represents a single item (row) in a property grid, exposing a name, value, and helper flags
/// used by editors and styling (unset, zero, tri-state checked).
/// </summary>
public partial class PropertyGridItem : BaseObject
{
    /// <summary>
    /// Backing store for <see cref="Name"/>.
    /// Default: <see langword="null"/>. Invalidation: <see cref="VisualPropertyInvalidateModes.Render"/>.
    /// </summary>
    public static VisualProperty NameProperty { get; } = VisualProperty.Add<string?>(typeof(PropertyGridItem), nameof(Name), VisualPropertyInvalidateModes.Render, null);

    /// <summary>
    /// Backing store for <see cref="IsZero"/>.
    /// Default: <see langword="false"/>. Invalidation: <see cref="VisualPropertyInvalidateModes.Render"/>.
    /// </summary>
    public static VisualProperty IsZeroProperty { get; } = VisualProperty.Add(typeof(PropertyGridItem), nameof(IsZero), VisualPropertyInvalidateModes.Render, false);

    /// <summary>
    /// Backing store for <see cref="IsUnset"/>.
    /// Default: <see langword="false"/>. Invalidation: <see cref="VisualPropertyInvalidateModes.Render"/>.
    /// </summary>
    public static VisualProperty IsUnsetProperty { get; } = VisualProperty.Add(typeof(PropertyGridItem), nameof(IsUnset), VisualPropertyInvalidateModes.Render, false);

    /// <summary>
    /// Backing store for <see cref="IsChecked"/> (tri-state).
    /// Default: <see langword="null"/> (CLR default is set to <see langword="false"/> in the constructor).
    /// Invalidation: <see cref="VisualPropertyInvalidateModes.Render"/>.
    /// </summary>
    public static VisualProperty IsCheckedProperty { get; } = VisualProperty.Add<bool?>(typeof(PropertyGridItem), nameof(IsChecked), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Backing store for <see cref="Value"/>.
    /// Default: <see langword="null"/>. Invalidation: <see cref="VisualPropertyInvalidateModes.Render"/>.
    /// </summary>
    public static VisualProperty ValueProperty { get; } = VisualProperty.Add<object?>(typeof(PropertyGridItem), nameof(Value), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Initializes a new instance of <see cref="PropertyGridItem"/> with <see cref="IsChecked"/> set to <see langword="false"/>.
    /// </summary>
    public PropertyGridItem()
    {
        IsChecked = false;
    }

    /// <summary>
    /// Gets or sets whether this item is logically "unset" (no explicit value provided).
    /// Property grids can use this to show placeholders or enable a "reset to default" affordance.
    /// </summary>
    public bool IsUnset { get => (bool)GetPropertyValue(IsUnsetProperty)!; set => SetPropertyValue(IsUnsetProperty, value); }

    /// <summary>
    /// Gets or sets whether the current <see cref="Value"/> is considered zero or default
    /// for styling/visual cues (does not modify <see cref="Value"/> itself).
    /// </summary>
    public bool IsZero { get => (bool)GetPropertyValue(IsZeroProperty)!; set => SetPropertyValue(IsZeroProperty, value); }

    /// <inheritdoc cref="BaseObject.Name"/>
    /// <summary>
    /// Gets or sets the display name for this property grid item.
    /// </summary>
    public override string? Name { get => (string?)GetPropertyValue(NameProperty); set => SetPropertyValue(NameProperty, value); }

    /// <summary>
    /// Gets or sets the raw value represented by this item. Editors bind to this property.
    /// </summary>
    public object? Value { get => GetPropertyValue(ValueProperty); set => SetPropertyValue(ValueProperty, value); }

    /// <summary>
    /// Gets or sets the tri-state checked state used by checkbox-like editors.
    /// true = checked, false = unchecked, null = indeterminate/mixed.
    /// </summary>
    public bool? IsChecked { get => (bool?)GetPropertyValue(IsCheckedProperty)!; set => SetPropertyValue(IsCheckedProperty, value); }

    /// <summary>
    /// Returns <see cref="Name"/> or an empty string when not set.
    /// </summary>
    public override string ToString() => Name ?? string.Empty;
}
