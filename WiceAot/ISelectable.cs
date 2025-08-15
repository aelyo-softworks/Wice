namespace Wice;

/// <summary>
/// Defines a selectable component that exposes selection state and notifies when it changes.
/// </summary>
/// <remarks>
/// Implementations should raise <see cref="IsSelectedChanged"/> when <see cref="IsSelected"/> changes value.
/// The event argument's <see cref="ValueEventArgs{Boolean}.Value"/> contains the new selection state.
/// If the event is raised with a cancellable <see cref="ValueEventArgs{Boolean}"/>, handlers may set
/// <see cref="System.ComponentModel.CancelEventArgs.Cancel"/> to true to veto the change.
/// </remarks>
public interface ISelectable
{
    /// <summary>
    /// Occurs when the <see cref="IsSelected"/> property value changes.
    /// </summary>
    /// <remarks>
    /// The <see cref="ValueEventArgs{Boolean}.Value"/> represents the new selection state.
    /// Implementations should only raise this event when the value actually changes.
    /// </remarks>
    event EventHandler<ValueEventArgs<bool>> IsSelectedChanged;

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="IsSelectedChanged"/> should be raised when
    /// <see cref="IsSelected"/> is modified.
    /// </summary>
    /// <remarks>
    /// Implementations should honor this flag to suppress event notifications when set to false.
    /// </remarks>
    bool RaiseIsSelectedChanged { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the item is selected.
    /// </summary>
    /// <remarks>
    /// Setting this property should trigger <see cref="IsSelectedChanged"/> (subject to
    /// <see cref="RaiseIsSelectedChanged"/>) when the value differs from the current state.
    /// </remarks>
    bool IsSelected { get; set; }
}
