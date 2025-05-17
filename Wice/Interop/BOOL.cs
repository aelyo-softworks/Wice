namespace Wice.Interop;

public struct BOOL : IEquatable<BOOL>
{
    public static readonly BOOL TRUE = new(true);
    public static readonly BOOL FALSE = new();

    public int Value;

    public BOOL(int value) => Value = value;
    public BOOL(bool value) => Value = value ? 1 : 0;

    public override readonly string ToString() => Value != 0 ? "TRUE" : "FALSE";
    public override readonly bool Equals(object? obj) => obj is BOOL value && Equals(value);
    public readonly bool Equals(BOOL other) => other.Value == Value;
    public override readonly int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(BOOL left, BOOL right) => left.Equals(right);
    public static bool operator !=(BOOL left, BOOL right) => !left.Equals(right);
    public static implicit operator int(BOOL value) => value.Value;
    public static implicit operator BOOL(int value) => new(value);
    public static implicit operator BOOL(bool result) => new(result);
    public static implicit operator bool(BOOL b) => b.Value != 0;
}
