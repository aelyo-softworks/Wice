using DirectN;

namespace Wice
{
    public class PointerEnterEventArgs : PointerUpdateEventArgs
    {
        public PointerEnterEventArgs(int pointerId, int x, int y, POINTER_MESSAGE_FLAGS flags)
            : base(pointerId, x, y, flags)
        {
        }
    }
}
