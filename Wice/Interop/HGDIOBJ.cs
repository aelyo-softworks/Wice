namespace Wice.Interop;

public struct HGDIOBJ(nint value) : IEquatable<HGDIOBJ>
{
    public static readonly HGDIOBJ Null = new();

    public nint Value = value;

    public override readonly string ToString() => $"0x{Value:x}";

    public override readonly bool Equals(object? obj) => obj is HGDIOBJ value && Equals(value);
    public readonly bool Equals(HGDIOBJ other) => other.Value == Value;
    public override readonly int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(HGDIOBJ left, HGDIOBJ right) => left.Equals(right);
    public static bool operator !=(HGDIOBJ left, HGDIOBJ right) => !left.Equals(right);
    public static implicit operator nint(HGDIOBJ value) => value.Value;
    public static implicit operator HGDIOBJ(nint value) => new(value);
}
