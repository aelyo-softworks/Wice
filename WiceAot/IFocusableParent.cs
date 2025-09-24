namespace Wice;

/// <summary>
/// Represents a parent element that can delegate focus to a specific visual and provides customization options for
/// focus cues.
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
    /// Gets an optional offset applied when drawing the focus cue.
    /// </summary>
    /// <value>
    /// A positive or negative offset in DIPs to expand or contract the focus outline relative to the
    /// visual bounds; <see langword="null"/> to use the theme default.
    /// </value>
    float? FocusOffset { get; }
}
