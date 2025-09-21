namespace Wice;

/// <summary>
/// Defines a selectable component that exposes selection state and notifies when it changes.
/// </summary>
public interface ISelectable
{
    /// <summary>
    /// Occurs when the <see cref="IsSelected"/> property value changes.
    /// </summary>
    event EventHandler<ValueEventArgs<bool>> IsSelectedChanged;

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="IsSelectedChanged"/> should be raised when
    /// <see cref="IsSelected"/> is modified.
    /// </summary>
    bool RaiseIsSelectedChanged { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the item is selected.
    /// </summary>
    bool IsSelected { get; set; }
}
