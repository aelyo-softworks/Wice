namespace Wice.Interop;

public partial struct HRGN(nint value) : IEquatable<HRGN>
{
    public static readonly HRGN Null = new();

    public nint Value = value;

    public override string ToString() => $"0x{Value:x}";

    public override readonly bool Equals(object? obj) => obj is HRGN value && Equals(value);
    public readonly bool Equals(HRGN other) => other.Value == Value;
    public override readonly int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(HRGN left, HRGN right) => left.Equals(right);
    public static bool operator !=(HRGN left, HRGN right) => !left.Equals(right);
    public static implicit operator nint(HRGN value) => value.Value;
    public static implicit operator HRGN(nint value) => new(value);
}
