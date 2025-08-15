namespace Wice;

/// <summary>
/// Base class for pointer-related event arguments.
/// Provides accessors to query Win32 pointer information for the given <see cref="PointerId"/>.
/// </summary>
/// <remarks>
/// - Each property call retrieves fresh data from the underlying pointer APIs via <c>WiceCommons</c>.
/// - The returned info reflects the state at the time of the query, which may differ from earlier reads.
/// - Some properties are meaningful only for specific pointer types (e.g., pen or touch).

/// Example:
/// <code language="csharp">
/// void OnPointer(object sender, PointerEventArgs e)
/// {
///     if (e.MouseButton == MouseButton.Left)
///     {
///         var pressure = e.Pressure; // 0 for mouse, > 0 for supported pen/touch
///         var type = e.PointerType;
///     }
/// }
/// </code>
/// </remarks>
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
    /// <remarks>
    /// Internally calls <c>WiceCommons.GetPointerType</c> on each access.
    /// </remarks>
    public POINTER_INPUT_TYPE PointerType
    {
        get { WiceCommons.GetPointerType(PointerId, out var type); return type; }
    }

    /// <summary>
    /// Gets the basic pointer information.
    /// </summary>
    /// <remarks>
    /// Internally calls <c>WiceCommons.GetPointerInfo</c> on each access.
    /// </remarks>
    public POINTER_INFO PointerInfo
    {
        get { WiceCommons.GetPointerInfo(PointerId, out var info); return info; }
    }

    /// <summary>
    /// Gets pen-specific pointer information.
    /// </summary>
    /// <remarks>
    /// Meaningful when <see cref="PointerType"/> is <see cref="POINTER_INPUT_TYPE.PT_PEN"/>.
    /// For other types, values may be defaulted.
    /// Internally calls <c>WiceCommons.GetPointerPenInfo</c> on each access.
    /// </remarks>
    public POINTER_PEN_INFO PointerPenInfo
    {
        get { WiceCommons.GetPointerPenInfo(PointerId, out var info); return info; }
    }

    /// <summary>
    /// Gets touch-specific pointer information.
    /// </summary>
    /// <remarks>
    /// Meaningful when <see cref="PointerType"/> is <see cref="POINTER_INPUT_TYPE.PT_TOUCH"/>.
    /// For other types, values may be defaulted.
    /// Internally calls <c>WiceCommons.GetPointerTouchInfo</c> on each access.
    /// </remarks>
    public POINTER_TOUCH_INFO PointerTouchInfo
    {
        get { WiceCommons.GetPointerTouchInfo(PointerId, out var info); return info; }
    }

    /// <summary>
    /// Gets the normalized pressure for the pointer, when available.
    /// </summary>
    /// <remarks>
    /// - For pen: returns <c>(int)PointerPenInfo.pressure</c>.
    /// - For touch: returns <c>(int)PointerTouchInfo.pressure</c>.
    /// - For mouse or unsupported types: returns 0.
    /// </remarks>
    public int Pressure => PointerType switch
    {
        POINTER_INPUT_TYPE.PT_PEN => (int)PointerPenInfo.pressure,
        POINTER_INPUT_TYPE.PT_TOUCH => (int)PointerTouchInfo.pressure,
        _ => 0,
    };

    /// <summary>
    /// Gets the logical mouse button involved in this pointer event, if any.
    /// </summary>
    /// <remarks>
    /// Derived from <see cref="POINTER_INFO.pointerFlags"/> and <see cref="POINTER_INFO.ButtonChangeType"/>:
    /// - Left: first button
    /// - Right: second button
    /// - Middle: third button
    /// - X1: fourth button
    /// - X2: fifth button
    /// Returns <c>null</c> when no mouse button is implicated (e.g., pen hover or generic pointer move).
    /// </remarks>
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
