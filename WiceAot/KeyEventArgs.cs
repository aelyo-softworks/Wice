namespace Wice;

/// <summary>
/// Describes a keyboard event and decodes its state information.
/// </summary>
/// <param name="vk">The virtual key of the event.</param>
/// <param name="states">
/// Encoded state bits for the key event (see remarks for bit layout as interpreted by this type).
/// </param>
public class KeyEventArgs(VIRTUAL_KEY vk, uint states) : HandledEventArgs
{
    /// <summary>
    /// Gets the virtual key associated with the event.
    /// </summary>
    public VIRTUAL_KEY Key { get; } = vk;

    /// <summary>
    /// Gets the character produced from <see cref="Key"/> using the current keyboard layout.
    /// </summary>
    public char Character { get; } = NativeWindow.VirtualKeyToCharacter(vk);

    /// <summary>
    /// Gets the scan code nibble extracted from the high word of <c>states</c>.
    /// </summary>
    public int ScanCode { get; } = (int)((states >> 16) & 0xF);

    /// <summary>
    /// Gets the repeat count extracted from the low-order byte of <c>states</c>.
    /// </summary>
    public int RepeatCount { get; } = (int)(states & 0xFF);

    /// <summary>
    /// Gets a value indicating whether the key transition is an up event.
    /// </summary>
    public bool IsUp { get; } = (states & 0x80000000) != 0; // bit 31

    /// <summary>
    /// Gets a value indicating whether the key transition is a down event.
    /// </summary>
    public bool IsDown => !IsUp;

    /// <summary>
    /// Gets a value indicating whether the key is an extended key.
    /// </summary>
    public bool IsExtendedKey { get; } = (states & 0x1000) != 0;

    /// <summary>
    /// Gets a value indicating whether the key was previously down before this event.
    /// </summary>
    public bool WasDown { get; } = (states & 0x40000000) != 0;

    /// <summary>
    /// Gets or sets a value indicating whether the Shift modifier was pressed at the time of the event.
    /// </summary>
    public virtual bool WithShift { get; set; } = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_SHIFT);

    /// <summary>
    /// Gets or sets a value indicating whether the Control modifier was pressed at the time of the event.
    /// </summary>
    public virtual bool WithControl { get; set; } = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_CONTROL);

    /// <summary>
    /// Gets or sets a value indicating whether the Menu (Alt) modifier was pressed at the time of the event.
    /// </summary>
    public virtual bool WithMenu { get; set; } = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_MENU);

    /// <inheritdoc/>
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
