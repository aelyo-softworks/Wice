namespace Wice.Interop;

public partial struct PWSTR
{
    public nint Value;

    public static readonly PWSTR Null = new();

    public PWSTR(nint value)
    {
        Value = value;
    }

    unsafe public PWSTR(char* value)
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

    public unsafe static PWSTR From(string? str)
    {
        if (str == null)
            return Null;

        fixed (char* chars = str)
        {
            return new PWSTR(chars);
        }
    }

    public override readonly string? ToString() => Marshal.PtrToStringUni(Value);
}
