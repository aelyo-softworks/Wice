namespace Wice;

/// <summary>
/// Provides event data that carries a value and supports optional cancellation and read-only semantics.
/// Inherits from <see cref="System.ComponentModel.CancelEventArgs"/> to allow event handlers to request cancellation.
/// </summary>
/// <param name="value">The initial value carried by the event.</param>
/// <param name="isValueReadOnly">
/// True to make <see cref="Value"/> read-only (attempting to set it will throw); otherwise false.
/// </param>
/// <param name="isCancellable">
/// True if the event publisher allows cancellation (handlers may set <see cref="System.ComponentModel.CancelEventArgs.Cancel"/> to true);
/// otherwise false.
/// </param>
public class ValueEventArgs(object? value, bool isValueReadOnly = true, bool isCancellable = false) : CancelEventArgs
{
    private object? _value = value;

    /// <summary>
    /// Gets a value indicating whether the event is intended to be cancellable.
    /// When true, handlers may set <see cref="CancelEventArgs.Cancel"/> to true; when false, publishers may ignore cancellation requests.
    /// </summary>
    public virtual bool Cancellable { get; } = isCancellable;

    /// <summary>
    /// Gets a value indicating whether <see cref="Value"/> is read-only.
    /// When true, attempts to set <see cref="Value"/> will throw an <see cref="ArgumentException"/>.
    /// </summary>
    public virtual bool IsValueReadOnly { get; } = isValueReadOnly;

    /// <summary>
    /// Gets or sets the value carried by the event.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when attempting to set the value while <see cref="IsValueReadOnly"/> is true.
    /// </exception>
    public virtual object? Value
    {
        get => _value;
        set
        {
            if (IsValueReadOnly)
                throw new ArgumentException(null, nameof(Value));
            _value = value;
        }
    }
}

/// <summary>
/// Provides strongly-typed event data that carries a value of type <typeparamref name="T"/> and supports
/// optional cancellation and read-only semantics.
/// </summary>
/// <typeparam name="T">The type of the value carried by the event.</typeparam>
/// <param name="value">The initial value carried by the event.</param>
/// <param name="isValueReadOnly">
/// True to make <see cref="Value"/> read-only (attempting to set it will throw); otherwise false.
/// </param>
/// <param name="isCancellable">
/// True if the event publisher allows cancellation (handlers may set <see cref="System.ComponentModel.CancelEventArgs.Cancel"/> to true);
/// otherwise false.
/// </param>
public class ValueEventArgs<T>(T value, bool isValueReadOnly = true, bool isCancellable = false) : ValueEventArgs(value, isValueReadOnly, isCancellable)
{
    /// <summary>
    /// Gets or sets the strongly-typed value carried by the event.
    /// Setting the value will throw an <see cref="ArgumentException"/> when <see cref="ValueEventArgs.IsValueReadOnly"/> is true.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when attempting to set the value while <see cref="ValueEventArgs.IsValueReadOnly"/> is true.
    /// </exception>
    public new T? Value
    {
        get => (T?)base.Value;
        set => base.Value = value;
    }
}
