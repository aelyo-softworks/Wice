namespace Wice;

/// <summary>
/// Provides additional context for list-style data binding scenarios,
/// extending <see cref="DataBindContext"/> with the item index, a last-item flag,
/// and an optional per-item separator <see cref="Visual"/>.
/// </summary>
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
    public virtual Visual? SeparatorVisual { get; set; }
}
