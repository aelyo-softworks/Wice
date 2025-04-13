using System;
using DirectN;
using Wice.Interop;

namespace Wice
{
    public class DragDropQueryContinueEventArgs : EventArgs
    {
        public DragDropQueryContinueEventArgs(bool escapedPressed, MODIFIERKEYS_FLAGS keyFlags)
        {
            EscapedPressed = escapedPressed;
            KeyFlags = keyFlags;
        }

        public bool EscapedPressed { get; }
        public MODIFIERKEYS_FLAGS KeyFlags { get; }
        public HRESULT Result { get; set; }
    }
}