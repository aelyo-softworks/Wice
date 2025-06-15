namespace Wice;

public class KeyEventArgs(VIRTUAL_KEY vk, uint states) : HandledEventArgs
{
    public VIRTUAL_KEY Key { get; } = vk;
    public char Character { get; } = NativeWindow.VirtualKeyToCharacter(vk);
    public int ScanCode { get; } = (int)((states >> 16) & 0xF);
    public int RepeatCount { get; } = (int)(states & 0xFF);
    public bool IsUp { get; } = (states & 0x80000000) != 0; // bit 31
    public bool IsDown => !IsUp;
    public bool IsExtendedKey { get; } = (states & 0x1000) != 0;
    public bool WasDown { get; } = (states & 0x40000000) != 0;
    public virtual bool WithShift { get; set; } = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_SHIFT);
    public virtual bool WithControl { get; set; } = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_CONTROL);
    public virtual bool WithMenu { get; set; } = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_MENU);

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
