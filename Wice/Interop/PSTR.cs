namespace Wice.Interop;

public struct PSTR
{
    public nint Value;

    public static readonly PSTR Null = new();

    public PSTR(nint value)
    {
        Value = value;
    }

    unsafe public PSTR(byte* value)
    {
        if (value != null)
        {
            Value = (nint)value;
        }
        else
        {
            Value = 0;
        }
    }

    public unsafe static PSTR From(string? str, Encoding? encoding = null)
    {
        if (str == null)
            return Null;

        encoding ??= Encoding.Default;
        var bytes = new byte[encoding.GetByteCount(str) + 1];
        encoding.GetBytes(str, 0, str.Length, bytes, 0);
        fixed (byte* p = bytes)
        {
            return new PSTR(p);
        }
    }

    public override readonly string? ToString() => Marshal.PtrToStringAnsi(Value);
}
