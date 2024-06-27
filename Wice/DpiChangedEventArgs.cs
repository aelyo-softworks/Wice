using System.ComponentModel;
using DirectN;

namespace Wice
{
    public class DpiChangedEventArgs : HandledEventArgs
    {
        public DpiChangedEventArgs(D2D_SIZE_U newDpi, tagRECT suggestedRect)
        {
            NewDpi = newDpi;
            SuggestedRect = suggestedRect;
        }

        public D2D_SIZE_U NewDpi { get; }
        public tagRECT SuggestedRect { get; }
    }
}
