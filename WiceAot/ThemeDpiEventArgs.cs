namespace Wice;

public class ThemeDpiEventArgs(uint oldDpi, uint newDpi)
{
    // depending on context, oldDpi and newDpi may be the same
    public uint OldDpi { get; } = oldDpi;
    public uint NewDpi { get; } = newDpi;

    public override string ToString() => $"{OldDpi} => {NewDpi}";

    public static ThemeDpiEventArgs FromWindow(Window? window)
    {
        var dpi = window?.Dpi ?? WiceCommons.USER_DEFAULT_SCREEN_DPI;
        return new ThemeDpiEventArgs(dpi, dpi);
    }
}
