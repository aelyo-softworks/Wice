namespace Wice;

/// <summary>
/// Defines the contract for visuals that manage selection of <see cref="ItemVisual"/> instances,
/// including selection mode, current selection, and selection change notifications.
/// </summary>
public interface ISelectorVisual
{
    /// <summary>
    /// Occurs after the selection set changes (items selected or unselected).
    /// </summary>
    event EventHandler<EventArgs>? SelectionChanged;

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="SelectionChanged"/> should be raised
    /// when the selection set changes.
    /// </summary>
    bool RaiseOnSelectionChanged { get; set; }

    /// <summary>
    /// Gets the selection mode that governs how many items can be selected at once.
    /// </summary>
    SelectionMode SelectionMode { get; }

    /// <summary>
    /// Gets the sequence of currently selected <see cref="ItemVisual"/> instances.
    /// </summary>
    IEnumerable<ItemVisual> SelectedItems { get; }

    /// <summary>
    /// Gets the primary selected item, if any.
    /// </summary>
    ItemVisual? SelectedItem { get; }

    /// <summary>
    /// Marks the specified visuals as selected, updating the current selection set.
    /// </summary>
    /// <param name="visuals">The visuals to select.</param>
    void Select(IEnumerable<ItemVisual> visuals);

    /// <summary>
    /// Marks the specified visuals as unselected, updating the current selection set.
    /// </summary>
    /// <param name="visuals">The visuals to unselect.</param>
    void Unselect(IEnumerable<ItemVisual> visuals);
}
