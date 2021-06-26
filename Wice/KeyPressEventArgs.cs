using System.ComponentModel;
using DirectN;

namespace Wice
{
    public class KeyPressEventArgs : HandledEventArgs
    {
        internal KeyPressEventArgs(char[] characters)
        {
            WithShift = NativeWindow.IsKeyPressed(VirtualKeys.ShiftKey);
            WithControl = NativeWindow.IsKeyPressed(VirtualKeys.ControlKey);
            WithMenu = NativeWindow.IsKeyPressed(VirtualKeys.Menu);
            UTF32Character = characters[0];
            Characters = characters;
        }

        internal KeyPressEventArgs(int character)
        {
            WithShift = NativeWindow.IsKeyPressed(VirtualKeys.ShiftKey);
            WithControl = NativeWindow.IsKeyPressed(VirtualKeys.ControlKey);
            WithMenu = NativeWindow.IsKeyPressed(VirtualKeys.Menu);
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
                Characters = new[] { (char)character };
            }
        }

        public int UTF32Character { get; }
        public char UTF16Character => Characters[0];
        public char[] Characters { get; }
        public bool WithShift { get; }
        public bool WithControl { get; }
        public bool WithMenu { get; }

        public override string ToString() => "C:" + UTF32Character + " CH:'" + UTF16Character + "'";
    }
}
