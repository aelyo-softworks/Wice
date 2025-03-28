namespace Wice.PropertyGrid;

public class PropertyVisuals<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>
{
    public Visual? Text { get; set; }
    public PropertyValueVisual<T>? ValueVisual { get; set; }
}
