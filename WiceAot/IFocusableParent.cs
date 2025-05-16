namespace Wice;

public interface IFocusableParent
{
    Visual? FocusableVisual { get; }
#if !NETFRAMEWORK
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    Type? FocusVisualShapeType { get; }
    float? FocusOffset { get; }
}
