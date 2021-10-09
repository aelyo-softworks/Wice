using System.ComponentModel;
using DirectN;

namespace Wice
{
    public class PointerEventArgs : HandledEventArgs
    {
        public PointerEventArgs(int pointerId)
        {
            PointerId = pointerId;
        }

        public int PointerId { get; }
        public POINTER_INPUT_TYPE PointerType => WindowsFunctions.GetPointerType(PointerId);
        public POINTER_INFO PointerInfo => WindowsFunctions.GetPointerInfo(PointerId);
        public POINTER_PEN_INFO PointerPenInfo => WindowsFunctions.GetPointerPenInfo(PointerId);
        public POINTER_TOUCH_INFO PointerTouchInfo => WindowsFunctions.GetPointerTouchInfo(PointerId);

        public MouseButton? MouseButton
        {
            get
            {
                var info = PointerInfo;
                if (info.pointerFlags.HasFlag(POINTER_FLAGS.POINTER_FLAG_FIRSTBUTTON) ||
                    info.ButtonChangeType == POINTER_BUTTON_CHANGE_TYPE.POINTER_CHANGE_FIRSTBUTTON_UP ||
                    info.ButtonChangeType == POINTER_BUTTON_CHANGE_TYPE.POINTER_CHANGE_FIRSTBUTTON_DOWN)
                    return Wice.MouseButton.Left;

                if (info.pointerFlags.HasFlag(POINTER_FLAGS.POINTER_FLAG_SECONDBUTTON) ||
                    info.ButtonChangeType == POINTER_BUTTON_CHANGE_TYPE.POINTER_CHANGE_SECONDBUTTON_UP ||
                    info.ButtonChangeType == POINTER_BUTTON_CHANGE_TYPE.POINTER_CHANGE_SECONDBUTTON_DOWN)
                    return Wice.MouseButton.Right;

                if (info.pointerFlags.HasFlag(POINTER_FLAGS.POINTER_FLAG_THIRDBUTTON) ||
                    info.ButtonChangeType == POINTER_BUTTON_CHANGE_TYPE.POINTER_CHANGE_THIRDBUTTON_UP ||
                    info.ButtonChangeType == POINTER_BUTTON_CHANGE_TYPE.POINTER_CHANGE_THIRDBUTTON_DOWN)
                    return Wice.MouseButton.Middle;

                if (info.pointerFlags.HasFlag(POINTER_FLAGS.POINTER_FLAG_FOURTHBUTTON) ||
                    info.ButtonChangeType == POINTER_BUTTON_CHANGE_TYPE.POINTER_CHANGE_FOURTHBUTTON_UP ||
                    info.ButtonChangeType == POINTER_BUTTON_CHANGE_TYPE.POINTER_CHANGE_FOURTHBUTTON_DOWN)
                    return Wice.MouseButton.X1;

                if (info.pointerFlags.HasFlag(POINTER_FLAGS.POINTER_FLAG_FIFTHBUTTON) ||
                    info.ButtonChangeType == POINTER_BUTTON_CHANGE_TYPE.POINTER_CHANGE_FIFTHBUTTON_UP ||
                    info.ButtonChangeType == POINTER_BUTTON_CHANGE_TYPE.POINTER_CHANGE_FIFTHBUTTON_DOWN)
                    return Wice.MouseButton.X2;

                return null;
            }
        }

        public override string ToString() => "Id=" + PointerId;
    }
}
