namespace Wice;

/// <summary>
/// Provides data for a key-press event, including the produced Unicode character(s)
/// and the state of modifier keys at the time of the event.
/// </summary>
/// <remarks>
/// The event captures a single Unicode scalar value (UTF-32). When constructed from a UTF-32 value,
/// <see cref="Characters"/> contains one or two UTF-16 code units depending on the code point.
/// </remarks>
public class KeyPressEventArgs : HandledEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyPressEventArgs"/> class from UTF-16 code unit(s).
    /// </summary>
    /// <param name="characters">One or two UTF-16 code units representing the pressed character.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="characters"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="characters"/> is empty.</exception>
    /// <remarks>
    /// The modifier key state (<see cref="WithShift"/>, <see cref="WithControl"/>, <see cref="WithMenu"/>) is captured at construction time.
    /// The <see cref="UTF32Character"/> is initialized from the first UTF-16 code unit provided.
    /// </remarks>
    public KeyPressEventArgs(char[] characters)
    {
        ExceptionExtensions.ThrowIfNull(characters, nameof(characters));
        if (characters.Length == 0)
            throw new ArgumentException(null, nameof(characters));

        WithShift = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_SHIFT);
        WithControl = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_CONTROL);
        WithMenu = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_MENU);
        UTF32Character = characters[0];
        Characters = characters;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyPressEventArgs"/> class from a UTF-32 code point.
    /// </summary>
    /// <param name="character">The Unicode scalar value (UTF-32 code point) for the key press.</param>
    /// <remarks>
    /// If <paramref name="character"/> is outside the Basic Multilingual Plane (greater than 0xFFFF),
    /// <see cref="Characters"/> is a surrogate pair (two UTF-16 code units); otherwise it contains a single code unit.
    /// The modifier key state (<see cref="WithShift"/>, <see cref="WithControl"/>, <see cref="WithMenu"/>) is captured at construction time.
    /// </remarks>
    public KeyPressEventArgs(uint character)
    {
        WithShift = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_SHIFT);
        WithControl = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_CONTROL);
        WithMenu = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_MENU);
        UTF32Character = character;

        // convert the UTF32 character code from the Window message to UTF16, yielding 1-2 code-units.
        // then advance the caret position by how many code-units were inserted.
        // if above the basic multi-lingual plane, split into leading and trailing surrogates.
        if (character > 0xFFFF)
        {
            // http://unicode.org/faq/utf_bom.html#35
            Characters = new char[2];
            Characters[0] = (char)(0xD800 + (character >> 10) - (0x10000 >> 10));
            Characters[1] = (char)(0xDC00 + (character & 0x3FF));
        }
        else
        {
            Characters = [(char)character];
        }
    }

    /// <summary>
    /// Gets the Unicode scalar value (UTF-32) associated with the key press.
    /// </summary>
    public uint UTF32Character { get; }

    /// <summary>
    /// Gets the first UTF-16 code unit of the produced character(s).
    /// </summary>
    /// <remarks>
    /// For characters represented by a surrogate pair, this is the high surrogate.
    /// </remarks>
    public char UTF16Character => Characters[0];

    /// <summary>
    /// Gets the UTF-16 code unit(s) produced by the key press.
    /// </summary>
    /// <remarks>
    /// The array length is 1 for BMP characters or 2 for supplementary characters (surrogate pair).
    /// </remarks>
    public char[] Characters { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the Shift key was pressed when the event was created.
    /// </summary>
    public virtual bool WithShift { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the Control key was pressed when the event was created.
    /// </summary>
    public virtual bool WithControl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the Alt (Menu) key was pressed when the event was created.
    /// </summary>
    public virtual bool WithMenu { get; set; }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>
    /// A string containing the UTF-32 value and the first UTF-16 code unit of the character.
    /// </returns>
    public override string ToString() => "C:" + UTF32Character + " CH:'" + UTF16Character + "'";
}
