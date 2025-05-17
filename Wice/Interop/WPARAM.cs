namespace Wice.Interop;

public struct WPARAM(nuint value) : IEquatable<WPARAM>
{
    public static readonly WPARAM Null = new();

    public nuint Value = value;

    public override readonly string ToString() => $"0x{Value:x}";

    public override readonly bool Equals(object? obj) => obj is WPARAM value && Equals(value);
    public readonly bool Equals(WPARAM other) => other.Value == Value;
    public override readonly int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(WPARAM left, WPARAM right) => left.Equals(right);
    public static bool operator !=(WPARAM left, WPARAM right) => !left.Equals(right);
    public static implicit operator nuint(WPARAM value) => value.Value;
    public static implicit operator nint(WPARAM value) => (nint)value.Value;
    public static implicit operator WPARAM(nuint value) => new(value);
    public static implicit operator WPARAM(nint value) => new((nuint)value);
    public static implicit operator WPARAM(uint value) => new(value);
    public static implicit operator WPARAM(int value) => new((uint)value);
}
