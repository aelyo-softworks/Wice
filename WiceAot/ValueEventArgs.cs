namespace Wice;

public class ValueEventArgs(object? value, bool isValueReadOnly = true, bool isCancellable = false) : CancelEventArgs
{
    private object? _value = value;

    public virtual bool IsCancellable { get; } = isCancellable;
    public virtual bool IsValueReadOnly { get; } = isValueReadOnly;
    public virtual object? Value { get => _value; set { if (IsValueReadOnly) throw new ArgumentException(null, nameof(Value)); _value = value; } }
}

public class ValueEventArgs<T>(T value, bool isValueReadOnly = true, bool isCancellable = false) : ValueEventArgs(value, isValueReadOnly, isCancellable)
{
    public new T? Value { get => (T?)base.Value; set => base.Value = value; }
}
