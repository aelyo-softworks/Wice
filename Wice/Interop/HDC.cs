namespace Wice.Interop;

public struct HDC(nint value) : IEquatable<HDC>
{
    public static readonly HDC Null = new();

    public nint Value = value;

    public override readonly string ToString() => $"0x{Value:x}";

    public override readonly bool Equals(object? obj) => obj is HDC value && Equals(value);
    public readonly bool Equals(HDC other) => other.Value == Value;
    public override readonly int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(HDC left, HDC right) => left.Equals(right);
    public static bool operator !=(HDC left, HDC right) => !left.Equals(right);
    public static implicit operator nint(HDC value) => value.Value;
    public static implicit operator HDC(nint value) => new(value);
}
