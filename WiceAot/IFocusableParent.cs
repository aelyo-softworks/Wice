namespace Wice;

public interface IFocusableParent
{
    Visual? FocusableVisual { get; }
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    Type? FocusVisualShapeType { get; }
    float? FocusOffset { get; }
}
