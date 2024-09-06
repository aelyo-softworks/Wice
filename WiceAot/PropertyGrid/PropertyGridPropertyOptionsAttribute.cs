namespace Wice.PropertyGrid;

[AttributeUsage(AttributeTargets.Property)]
public sealed class PropertyGridPropertyOptionsAttribute : Attribute
{
    public int SortOrder { get; set; }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public Type? EditorType { get; set; }

    internal object? CreateEditor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(PropertyValueVisual<T> value)
    {
        var creator = value.Property.Value as IEditorCreator<T>;
        if (creator == null)
        {
            if (EditorType != null)
            {
                var editorCreator = Activator.CreateInstance(EditorType);
                creator = editorCreator as IEditorCreator<T>;
                if (creator == null)
                    throw new WiceException("0024: type '" + EditorType.FullName + "' doesn't implement the " + nameof(IEditorCreator<T>) + " interface.");
            }
        }

        if (creator != null)
        {
            var editor = creator.CreateEditor(value);
            if (editor != null)
                return editor;
        }

        // fall back to default editor
        return value.CreateDefaultEditor();
    }
}