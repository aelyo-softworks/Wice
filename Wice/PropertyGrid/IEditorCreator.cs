namespace Wice.PropertyGrid;

public interface IEditorCreator
{
    object CreateEditor(PropertyValueVisual value);
    object UpdateEditor(PropertyValueVisual value, object editor);
}
