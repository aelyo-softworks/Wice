namespace Wice.Interop;

public struct DPI_AWARENESS_CONTEXT(nint value) : IEquatable<DPI_AWARENESS_CONTEXT>
{
    public static readonly DPI_AWARENESS_CONTEXT Null = new();
    public static readonly DPI_AWARENESS_CONTEXT DPI_AWARENESS_CONTEXT_INVALID = new() { Value = 0 }; // not in Windows
    public static readonly DPI_AWARENESS_CONTEXT DPI_AWARENESS_CONTEXT_UNAWARE = new() { Value = -1 };
    public static readonly DPI_AWARENESS_CONTEXT DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = new() { Value = -2 };
    public static readonly DPI_AWARENESS_CONTEXT DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = new() { Value = -3 };
    public static readonly DPI_AWARENESS_CONTEXT DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new() { Value = -4 };
    public static readonly DPI_AWARENESS_CONTEXT DPI_AWARENESS_CONTEXT_UNAWARE_GDISCALED = new() { Value = -5 };

    public nint Value = value;

    public override readonly string ToString() => $"0x{Value:x}";

    public override readonly bool Equals(object? obj) => obj is DPI_AWARENESS_CONTEXT value && Equals(value);
    public readonly bool Equals(DPI_AWARENESS_CONTEXT other) => other.Value == Value;
    public override readonly int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(DPI_AWARENESS_CONTEXT left, DPI_AWARENESS_CONTEXT right) => left.Equals(right);
    public static bool operator !=(DPI_AWARENESS_CONTEXT left, DPI_AWARENESS_CONTEXT right) => !left.Equals(right);
    public static implicit operator nint(DPI_AWARENESS_CONTEXT value) => value.Value;
    public static implicit operator DPI_AWARENESS_CONTEXT(nint value) => new(value);
}
