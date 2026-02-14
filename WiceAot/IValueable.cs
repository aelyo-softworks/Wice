namespace Wice;

/// <summary>
/// Defines a visual that exposes a value which can be observed for changes and,
/// when allowed, updated via <see cref="TrySetValue(object?)"/>.
/// </summary>
public interface IValueable
{
    /// <summary>
    /// Occurs when the <see cref="Value"/> is changing or has changed.
    /// </summary>
    event EventHandler<ValueEventArgs> ValueChanged;

    /// <summary>
    /// Gets the current value.
    /// </summary>
    object? Value { get; }

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="TrySetValue(object?)"/> is allowed
    /// to change the <see cref="Value"/>.
    /// </summary>
    bool CanChangeValue { get; set; }

    /// <summary>
    /// Attempts to change the <see cref="Value"/> to the specified value.
    /// </summary>
    /// <param name="value">The proposed new value. May be <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the value was changed; otherwise, <see langword="false"/>.
    /// </returns>
    bool TrySetValue(object? value);
}
