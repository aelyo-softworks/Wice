namespace Wice;

public class KeyPressEventArgs : HandledEventArgs
{
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

    public uint UTF32Character { get; }
    public char UTF16Character => Characters[0];
    public char[] Characters { get; }
    public virtual bool WithShift { get; set; }
    public virtual bool WithControl { get; set; }
    public virtual bool WithMenu { get; set; }

    public override string ToString() => "C:" + UTF32Character + " CH:'" + UTF16Character + "'";
}
