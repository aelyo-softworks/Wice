namespace Wice.Interop;

public struct HINSTANCE(nint value) : IEquatable<HINSTANCE>
{
    public static readonly HINSTANCE Null = new();

    public nint Value = value;

    public override string ToString() => $"0x{Value:x}";

    public override readonly bool Equals(object? obj) => obj is HINSTANCE value && Equals(value);
    public readonly bool Equals(HINSTANCE other) => other.Value == Value;
    public override readonly int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(HINSTANCE left, HINSTANCE right) => left.Equals(right);
    public static bool operator !=(HINSTANCE left, HINSTANCE right) => !left.Equals(right);
    public static implicit operator nint(HINSTANCE value) => value.Value;
    public static implicit operator HINSTANCE(nint value) => new(value);
}
