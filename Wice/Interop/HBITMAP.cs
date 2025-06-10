namespace Wice.Interop;

public struct HBITMAP(nint value) : IEquatable<HBITMAP>
{
    public static readonly HBITMAP Null = new();

    public nint Value = value;

    public override string ToString() => $"0x{Value:x}";

    public override readonly bool Equals(object? obj) => obj is HBITMAP value && Equals(value);
    public readonly bool Equals(HBITMAP other) => other.Value == Value;
    public override readonly int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(HBITMAP left, HBITMAP right) => left.Equals(right);
    public static bool operator !=(HBITMAP left, HBITMAP right) => !left.Equals(right);
    public static implicit operator nint(HBITMAP value) => value.Value;
    public static implicit operator HBITMAP(nint value) => new(value);
}
