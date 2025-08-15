namespace Wice;

/// <summary>
/// Provides additional context for list-style data binding scenarios,
/// extending <see cref="DataBindContext"/> with the item index, a last-item flag,
/// and an optional per-item separator <see cref="Visual"/>.
/// </summary>
/// <remarks>
/// Typical usage:
/// - Created by list controls to pass item-specific metadata to visual creators/binders.
/// - <see cref="Index"/> indicates the zero-based position of the item.
/// - <see cref="IsLast"/> allows templates to adjust spacing or separators for the trailing item.
/// - <see cref="SeparatorVisual"/> can be set by the item template to provide a visual separator
///   between items (e.g., a thin line). It is optional and can be null when no separator is desired.
/// </remarks>
/// <param name="data">The data item being bound for this list entry.</param>
/// <param name="index">The zero-based index of the item within the source collection.</param>
/// <param name="isLast">True when this item is the final element in the source collection; otherwise false.</param>
/// <seealso cref="DataBindContext"/>
/// <seealso cref="Visual"/>
public class ListBoxDataBindContext(object? data, int index, bool isLast)
    : DataBindContext(data)
{
    /// <summary>
    /// Gets the zero-based index of the item in the source collection.
    /// </summary>
    public int Index { get; } = index;

    /// <summary>
    /// Gets a value indicating whether this item is the last one in the sequence.
    /// </summary>
    public bool IsLast { get; } = isLast;

    /// <summary>
    /// Gets or sets an optional separator <see cref="Visual"/> associated with this item.
    /// </summary>
    /// <remarks>
    /// This visual can be supplied by the item template to render a divider between items.
    /// When null, no separator is displayed by default.
    /// </remarks>
    public virtual Visual? SeparatorVisual { get; set; }
}
