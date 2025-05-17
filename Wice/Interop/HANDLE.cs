namespace Wice.Interop;

public struct HANDLE(nint value) : IEquatable<HANDLE>
{
    public static readonly HANDLE Null = new();

    public nint Value = value;

    public override readonly string ToString() => $"0x{Value:x}";

    public override readonly bool Equals(object? obj) => obj is HANDLE value && Equals(value);
    public readonly bool Equals(HANDLE other) => other.Value == Value;
    public override readonly int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(HANDLE left, HANDLE right) => left.Equals(right);
    public static bool operator !=(HANDLE left, HANDLE right) => !left.Equals(right);
    public static implicit operator nint(HANDLE value) => value.Value;
    public static implicit operator HANDLE(nint value) => new(value);
}
