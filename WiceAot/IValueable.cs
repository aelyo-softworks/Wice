namespace Wice;

/// <summary>
/// Defines a component that exposes a value which can be observed for changes and,
/// when allowed, updated via <see cref="TrySetValue(object?)"/>.
/// </summary>
public interface IValueable
{
    /// <summary>
    /// Occurs when the <see cref="Value"/> is changing or has changed.
    /// </summary>
    /// <remarks>
    /// Implementations should raise this event whenever <see cref="Value"/> is modified, whether
    /// the modification originates from <see cref="TrySetValue(object?)"/> or from internal logic.
    /// When the associated <see cref="ValueEventArgs"/> indicates that the change is cancellable,
    /// subscribers may set <see cref="System.ComponentModel.CancelEventArgs.Cancel"/> to true to veto the change.
    /// The exact timing (before or after the value changes) is implementation-defined; consult the implementer’s documentation.
    /// </remarks>
    event EventHandler<ValueEventArgs> ValueChanged;

    /// <summary>
    /// Gets the current value.
    /// </summary>
    /// <remarks>
    /// The value may be <see langword="null"/>.
    /// </remarks>
    object? Value { get; }

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="TrySetValue(object?)"/> is allowed
    /// to change the <see cref="Value"/>.
    /// </summary>
    /// <remarks>
    /// Implementations may still refuse changes for other reasons (e.g., validation failures),
    /// even when this property is <see langword="true"/>.
    /// </remarks>
    bool CanChangeValue { get; set; }

    /// <summary>
    /// Attempts to change the <see cref="Value"/> to the specified value.
    /// </summary>
    /// <param name="value">The proposed new value. May be <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the value was changed; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Implementations should return <see langword="false"/> without changing state when
    /// <see cref="CanChangeValue"/> is <see langword="false"/> or when the value is rejected.
    /// On a successful change, implementations should raise <see cref="ValueChanged"/> with an
    /// appropriate <see cref="ValueEventArgs"/>.
    /// </remarks>
    bool TrySetValue(object? value);
}
