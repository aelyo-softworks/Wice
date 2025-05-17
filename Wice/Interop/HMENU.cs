namespace Wice.Interop;

public struct HMENU(nint value) : IEquatable<HMENU>
{
    public static readonly HMENU Null = new();

    public nint Value = value;

    public override readonly string ToString() => $"0x{Value:x}";

    public override readonly bool Equals(object? obj) => obj is HMENU value && Equals(value);
    public readonly bool Equals(HMENU other) => other.Value == Value;
    public override readonly int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(HMENU left, HMENU right) => left.Equals(right);
    public static bool operator !=(HMENU left, HMENU right) => !left.Equals(right);
    public static implicit operator nint(HMENU value) => value.Value;
    public static implicit operator HMENU(nint value) => new(value);
}
