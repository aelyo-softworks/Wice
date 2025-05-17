namespace Wice.Interop;

public struct HBRUSH(nint value) : IEquatable<HBRUSH>
{
    public static readonly HBRUSH Null = new();

    public nint Value = value;

    public override readonly string ToString() => $"0x{Value:x}";

    public override readonly bool Equals(object? obj) => obj is HBRUSH value && Equals(value);
    public readonly bool Equals(HBRUSH other) => other.Value == Value;
    public override readonly int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(HBRUSH left, HBRUSH right) => left.Equals(right);
    public static bool operator !=(HBRUSH left, HBRUSH right) => !left.Equals(right);
    public static implicit operator nint(HBRUSH value) => value.Value;
    public static implicit operator HBRUSH(nint value) => new(value);
}
