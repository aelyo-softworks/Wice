using System;
using DirectN;
using Wice.Interop;

namespace Wice
{
    public class DragDropGiveFeedback : EventArgs
    {
        // TODO: remove once in DirectN
        public const int DRAGDROP_S_DROP = 0x00040100;
        public const int DRAGDROP_S_CANCEL = 0x00040101;
        public const int DRAGDROP_S_USEDEFAULTCURSORS = 0x00040102;

        public DragDropGiveFeedback(DROPEFFECT effect)
        {
            Effect = effect;
            Result = DRAGDROP_S_USEDEFAULTCURSORS;
        }

        public DROPEFFECT Effect { get; }
        public HRESULT Result { get; set; }
    }
}
