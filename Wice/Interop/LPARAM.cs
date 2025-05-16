namespace Wice;

public partial struct LPARAM : IEquatable<LPARAM>
{
    public static readonly LPARAM Null = new();

    public nint Value;

    public LPARAM(nint value) => this.Value = value;
    public override string ToString() => $"0x{Value:x}";

    public override readonly bool Equals(object? obj) => obj is LPARAM value && Equals(value);
    public readonly bool Equals(LPARAM other) => other.Value == Value;
    public override readonly int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(LPARAM left, LPARAM right) => left.Equals(right);
    public static bool operator !=(LPARAM left, LPARAM right) => !left.Equals(right);
    public static implicit operator nint(LPARAM value) => value.Value;
    public static implicit operator LPARAM(nint value) => new(value);
}
