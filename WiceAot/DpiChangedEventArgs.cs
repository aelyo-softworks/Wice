namespace Wice;

public class DpiChangedEventArgs(D2D_SIZE_U newDpi, RECT suggestedRect) : HandledEventArgs
{
    public D2D_SIZE_U NewDpi { get; } = newDpi;
    public RECT SuggestedRect { get; } = suggestedRect;
}
