namespace Wice;

public partial struct HICON : IEquatable<HICON>
{
    public static readonly HICON Null = new();

    public nint Value;

    public HICON(nint value) => this.Value = value;
    public override string ToString() => $"0x{Value:x}";

    public override readonly bool Equals(object? obj) => obj is HICON value && Equals(value);
    public readonly bool Equals(HICON other) => other.Value == Value;
    public override readonly int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(HICON left, HICON right) => left.Equals(right);
    public static bool operator !=(HICON left, HICON right) => !left.Equals(right);
    public static implicit operator nint(HICON value) => value.Value;
    public static implicit operator HICON(nint value) => new(value);
}
