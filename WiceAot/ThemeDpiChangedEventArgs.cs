namespace Wice;

public class ThemeDpiChangedEventArgs(uint oldDpi, uint newDpi)
{
    public uint OldDpi { get; } = oldDpi;
    public uint NewDpi { get; } = newDpi;

    public override string ToString() => $"{OldDpi} => {NewDpi}";
}
