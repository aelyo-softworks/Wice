using System.ComponentModel;
using DirectN;

namespace Wice
{
    public class PointerEventArgs : HandledEventArgs
    {
        internal PointerEventArgs(int pointerId)
        {
            PointerId = pointerId;
        }

        public int PointerId { get; }
        public POINTER_INPUT_TYPE PointerType => WindowsFunctions.GetPointerType(PointerId);
        public POINTER_INFO PointerInfo => WindowsFunctions.GetPointerInfo(PointerId);
        public POINTER_PEN_INFO PointerPenInfo => WindowsFunctions.GetPointerPenInfo(PointerId);
        public POINTER_TOUCH_INFO PointerTouchInfo => WindowsFunctions.GetPointerTouchInfo(PointerId);

        public override string ToString() => "Id=" + PointerId;
    }
}
