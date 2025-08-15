namespace Wice;

/// <summary>
/// Describes a keyboard event and decodes its state information.
/// </summary>
/// <remarks>
/// This type is constructed with a virtual key and a 32-bit <c>states</c> value,
/// typically derived from a Windows message's <c>lParam</c>. This implementation interprets:
/// - RepeatCount: low-order byte (<c>states &amp; 0xFF</c>)
/// - ScanCode: low nibble of the high word (<c>(states &gt;&gt; 16) &amp; 0xF</c>)
/// - Extended key flag: bit 12 (<c>(states &amp; 0x1000) != 0</c>)
/// - Previous key state: bit 30 (<c>(states &amp; 0x40000000) != 0</c>)
/// - Transition state (key up): bit 31 (<c>(states &amp; 0x80000000) != 0</c>)
/// Modifier properties are initialized using current keyboard state at construction time and are settable.
/// </remarks>
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
    /// <remarks>
    /// The conversion is performed via <c>NativeWindow.VirtualKeyToCharacter</c>, and may be locale/layout dependent.
    /// </remarks>
    public char Character { get; } = NativeWindow.VirtualKeyToCharacter(vk);

    /// <summary>
    /// Gets the scan code nibble extracted from the high word of <c>states</c>.
    /// </summary>
    /// <remarks>
    /// This value is computed as <c>(int)((states &gt;&gt; 16) &amp; 0xF)</c>.
    /// </remarks>
    public int ScanCode { get; } = (int)((states >> 16) & 0xF);

    /// <summary>
    /// Gets the repeat count extracted from the low-order byte of <c>states</c>.
    /// </summary>
    /// <remarks>
    /// This value is computed as <c>(int)(states &amp; 0xFF)</c>.
    /// </remarks>
    public int RepeatCount { get; } = (int)(states & 0xFF);

    /// <summary>
    /// Gets a value indicating whether the key transition is an up event.
    /// </summary>
    /// <remarks>
    /// True when bit 31 of <c>states</c> is set.
    /// </remarks>
    public bool IsUp { get; } = (states & 0x80000000) != 0; // bit 31

    /// <summary>
    /// Gets a value indicating whether the key transition is a down event.
    /// </summary>
    public bool IsDown => !IsUp;

    /// <summary>
    /// Gets a value indicating whether the key is an extended key.
    /// </summary>
    /// <remarks>
    /// True when bit 12 of <c>states</c> is set.
    /// </remarks>
    public bool IsExtendedKey { get; } = (states & 0x1000) != 0;

    /// <summary>
    /// Gets a value indicating whether the key was previously down before this event.
    /// </summary>
    /// <remarks>
    /// True when bit 30 of <c>states</c> is set.
    /// </remarks>
    public bool WasDown { get; } = (states & 0x40000000) != 0;

    /// <summary>
    /// Gets or sets a value indicating whether the Shift modifier was pressed at the time of the event.
    /// </summary>
    /// <remarks>
    /// Initialized from <c>NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_SHIFT)</c>.
    /// </remarks>
    public virtual bool WithShift { get; set; } = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_SHIFT);

    /// <summary>
    /// Gets or sets a value indicating whether the Control modifier was pressed at the time of the event.
    /// </summary>
    /// <remarks>
    /// Initialized from <c>NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_CONTROL)</c>.
    /// </remarks>
    public virtual bool WithControl { get; set; } = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_CONTROL);

    /// <summary>
    /// Gets or sets a value indicating whether the Menu (Alt) modifier was pressed at the time of the event.
    /// </summary>
    /// <remarks>
    /// Initialized from <c>NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_MENU)</c>.
    /// </remarks>
    public virtual bool WithMenu { get; set; } = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_MENU);

    /// <summary>
    /// Returns a compact, human-readable representation of the key event.
    /// </summary>
    /// <returns>A string that describes transition, flags, repeat count, scan code, virtual key, and character.</returns>
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
