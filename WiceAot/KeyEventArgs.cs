namespace Wice;

public class KeyEventArgs : HandledEventArgs
{
    internal KeyEventArgs(VIRTUAL_KEY vk, uint states)
    {
        Key = vk;
        WithShift = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_SHIFT);
        WithControl = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_CONTROL);
        WithMenu = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_MENU);

        // https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-keydown
        ScanCode = (int)((states >> 16) & 0xF);
        RepeatCount = (int)(states & 0xFF);
        IsUp = (states & 0x80000000) != 0; // bit 31
        IsExtendedKey = (states & 0x1000) != 0;
        WasDown = (states & 0x40000000) != 0;
        Character = NativeWindow.VirtualKeyToCharacter(vk);
    }

    public VIRTUAL_KEY Key { get; }
    public char Character { get; }
    public int ScanCode { get; }
    public int RepeatCount { get; }
    public bool IsUp { get; }
    public bool IsDown => !IsUp;
    public bool IsExtendedKey { get; }
    public bool WasDown { get; }
    public bool WithShift { get; }
    public bool WithControl { get; }
    public bool WithMenu { get; }

    public override string ToString()
    {
        var s = IsUp ? "UP" : "DOWN";
        if (IsDown)
        {
            s += ",WD=" + WasDown;
        }

        if (IsExtendedKey)
        {
            s += ",EX=" + IsExtendedKey;
        }

        if (RepeatCount != 1)
        {
            s += ",RC=" + RepeatCount;
        }
        return s + ",SC=" + ScanCode + ",VK='" + Key + "',CH='" + Character + "'";
    }
}
