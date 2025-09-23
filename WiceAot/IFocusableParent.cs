namespace Wice;

/// <summary>
/// Contract for a visual that can delegate keyboard focus to a designated child
/// and provide customization hints for how a focus cue (focus adorners/visual)
/// should be rendered around it.
/// </summary>
public interface IFocusableParent
{
    /// <summary>
    /// Gets the visual that should receive focus when this parent is focused.
    /// </summary>
    /// <value>
    /// A child <see cref="Visual"/> that can accept focus. Return <see langword="null"/> if the
    /// implementer itself is focusable or if focus delegation is not applicable.
    /// </value>
    Visual? FocusableVisual { get; }

    /// <summary>
    /// Gets the CLR <see cref="Type"/> of a shape/visual used to render the focus cue.
    /// </summary>
    /// <value>
    /// A <see cref="Type"/> describing the focus visual/shape to use; <see langword="null"/> to
    /// fall back to the theme or framework default.
    /// </value>
#if !NETFRAMEWORK
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    Type? FocusVisualShapeType { get; }

    /// <summary>
    /// Gets an optional offset, in device-independent pixels (DIPs), applied when drawing the focus cue.
    /// </summary>
    /// <value>
    /// A positive or negative offset in DIPs to expand or contract the focus outline relative to the
    /// visual bounds; <see langword="null"/> to use the theme default.
    /// </value>
    float? FocusOffset { get; }
}
