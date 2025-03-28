namespace Wice.PropertyGrid;

public interface IEditorCreator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>
{
    object? CreateEditor(PropertyValueVisual<T> value);
    object? UpdateEditor(PropertyValueVisual<T> value, object? editor);
}
