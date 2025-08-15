namespace Wice;

/// <summary>
/// Defines the contract for visuals that manage selection of <see cref="ItemVisual"/> instances,
/// including selection mode, current selection, and selection change notifications.
/// </summary>
/// <remarks>
/// Implementations are expected to synchronize the <see cref="ISelectable.IsSelected"/> state of
/// <see cref="ItemVisual"/> instances and raise <see cref="SelectionChanged"/> when appropriate,
/// typically honoring <see cref="RaiseOnSelectionChanged"/> and <see cref="SelectionMode"/>.
/// </remarks>
public interface ISelectorVisual
{
    /// <summary>
    /// Occurs after the selection set changes (items selected or unselected).
    /// </summary>
    /// <remarks>
    /// Implementations may suppress raising this event when <see cref="RaiseOnSelectionChanged"/> is <c>false</c>.
    /// The event is generally raised once per atomic change to the selection set.
    /// </remarks>
    event EventHandler<EventArgs>? SelectionChanged;

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="SelectionChanged"/> should be raised
    /// when the selection set changes.
    /// </summary>
    /// <remarks>
    /// Setting this to <c>false</c> allows batching multiple selection operations without
    /// intermediate notifications. Implementations may raise a single notification after
    /// batching is complete when this is toggled back to <c>true</c>.
    /// </remarks>
    bool RaiseOnSelectionChanged { get; set; }

    /// <summary>
    /// Gets the selection mode that governs how many items can be selected at once.
    /// </summary>
    /// <remarks>
    /// Typical values are single- or multi-selection. Implementations should enforce this mode
    /// when <see cref="Select(System.Collections.Generic.IEnumerable{ItemVisual})"/> is called.
    /// </remarks>
    SelectionMode SelectionMode { get; }

    /// <summary>
    /// Gets the sequence of currently selected <see cref="ItemVisual"/> instances.
    /// </summary>
    /// <remarks>
    /// The returned sequence may be live or a snapshot depending on the implementation.
    /// </remarks>
    IEnumerable<ItemVisual> SelectedItems { get; }

    /// <summary>
    /// Gets the primary selected item, if any.
    /// </summary>
    /// <remarks>
    /// In single-selection mode this is the single selected item (or <c>null</c> when none).
    /// In multi-selection mode this is typically the first or active selection, or <c>null</c> when none.
    /// </remarks>
    ItemVisual? SelectedItem { get; }

    /// <summary>
    /// Marks the specified visuals as selected, updating the current selection set.
    /// </summary>
    /// <param name="visuals">The visuals to select.</param>
    /// <remarks>
    /// Implementations should honor <see cref="SelectionMode"/>. For single-selection mode,
    /// selecting a new item typically clears any previous selection first.
    /// </remarks>
    void Select(IEnumerable<ItemVisual> visuals);

    /// <summary>
    /// Marks the specified visuals as unselected, updating the current selection set.
    /// </summary>
    /// <param name="visuals">The visuals to unselect.</param>
    void Unselect(IEnumerable<ItemVisual> visuals);
}
