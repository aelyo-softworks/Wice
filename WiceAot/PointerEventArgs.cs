namespace Wice;

/// <summary>
/// Base class for pointer-related event arguments.
/// Provides accessors to query Win32 pointer information for the given <see cref="PointerId"/>.
/// </summary>
public abstract class PointerEventArgs(uint pointerId)
    : HandledEventArgs
{
    /// <summary>
    /// Gets the system-assigned pointer identifier for this event.
    /// </summary>
    public uint PointerId { get; } = pointerId;

    /// <summary>
    /// Gets the <see cref="POINTER_INPUT_TYPE"/> for the pointer.
    /// </summary>
    public POINTER_INPUT_TYPE PointerType
    {
        get { WiceCommons.GetPointerType(PointerId, out var type); return type; }
    }

    /// <summary>
    /// Gets the basic pointer information.
    /// </summary>
    public POINTER_INFO PointerInfo
    {
        get { WiceCommons.GetPointerInfo(PointerId, out var info); return info; }
    }

    /// <summary>
    /// Gets pen-specific pointer information.
    /// </summary>
    public POINTER_PEN_INFO PointerPenInfo
    {
        get { WiceCommons.GetPointerPenInfo(PointerId, out var info); return info; }
    }

    /// <summary>
    /// Gets touch-specific pointer information.
    /// </summary>
    public POINTER_TOUCH_INFO PointerTouchInfo
    {
        get { WiceCommons.GetPointerTouchInfo(PointerId, out var info); return info; }
    }

    /// <summary>
    /// Gets the normalized pressure for the pointer, when available.
    /// </summary>
    public int Pressure => PointerType switch
    {
        POINTER_INPUT_TYPE.PT_PEN => (int)PointerPenInfo.pressure,
        POINTER_INPUT_TYPE.PT_TOUCH => (int)PointerTouchInfo.pressure,
        _ => 0,
    };

    /// <summary>
    /// Gets the logical mouse button involved in this pointer event, if any.
    /// </summary>
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

    /// <summary>
    /// Returns a string containing the pointer identifier.
    /// </summary>
    public override string ToString() => "Id=" + PointerId;
}
