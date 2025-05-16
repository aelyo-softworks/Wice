namespace Wice;

public partial struct WPARAM : IEquatable<WPARAM>
{
    public static readonly WPARAM Null = new();

    public nuint Value;

    public WPARAM(nuint value) => this.Value = value;
    public override string ToString() => $"0x{Value:x}";

    public override readonly bool Equals(object? obj) => obj is WPARAM value && Equals(value);
    public readonly bool Equals(WPARAM other) => other.Value == Value;
    public override readonly int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(WPARAM left, WPARAM right) => left.Equals(right);
    public static bool operator !=(WPARAM left, WPARAM right) => !left.Equals(right);
    public static implicit operator nuint(WPARAM value) => value.Value;
    public static implicit operator WPARAM(nuint value) => new(value);
}
