namespace Wice.Interop;

public struct HWND(nint value) : IEquatable<HWND>
{
    public static readonly HWND Null = new();
    public static readonly HWND BOTTOM = new() { Value = 1 };
    public static readonly HWND NOTOPMOST = new() { Value = -2 };
    public static readonly HWND TOP = new();
    public static readonly HWND TOPMOST = new() { Value = -1 };

    public nint Value = value;

    public override readonly string ToString() => $"0x{Value:x}";

    public override readonly bool Equals(object? obj) => obj is HWND value && Equals(value);
    public readonly bool Equals(HWND other) => other.Value == Value;
    public override readonly int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(HWND left, HWND right) => left.Equals(right);
    public static bool operator !=(HWND left, HWND right) => !left.Equals(right);
    public static implicit operator nint(HWND value) => value.Value;
    public static implicit operator HWND(nint value) => new(value);
}
