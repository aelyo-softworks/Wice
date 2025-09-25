namespace Wice.PropertyGrid;

/// <summary>
/// Provides per-property configuration for <c>PropertyGrid</c> rendering and editing.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class PropertyGridPropertyOptionsAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the explicit sort order of the property within its category/group.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the string used to separate items in a list if the property is an enumerable instance.
    /// </summary>
    public string? ListSeparator { get; set; }

    /// <summary>
    /// Gets or sets the format string used to define the text output representation of a property value.
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets an optional editor factory type used to create/update the editor for this property.
    /// </summary>
#if NETFRAMEWORK
#else
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    public Type? EditorCreatorType { get; set; }

#if NETFRAMEWORK
    internal (object?, IEditorCreator?) CreateEditor(PropertyValueVisual value)
#else
    internal (object? Editor, IEditorCreator<T>? EditorCreator) CreateEditor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(PropertyValueVisual<T> value)
#endif
    {
#if NETFRAMEWORK
        var creator = value.Property.Value as IEditorCreator;
#else
        var creator = value.Property.Value as IEditorCreator<T>;
#endif
        if (creator == null)
        {
            if (EditorCreatorType != null)
            {
                var editorCreator = Activator.CreateInstance(EditorCreatorType);
#if NETFRAMEWORK
                creator = editorCreator as IEditorCreator;
                if (creator == null)
                    throw new WiceException("0024: type '" + EditorCreatorType.FullName + "' doesn't implement the " + nameof(IEditorCreator) + " interface.");
#else
                creator = editorCreator as IEditorCreator<T>;
                if (creator == null)
                    throw new WiceException("0024: type '" + EditorCreatorType.FullName + "' doesn't implement the " + nameof(IEditorCreator<T>) + " interface.");
#endif
            }
        }

        if (creator != null)
        {
            var editor = creator.CreateEditor(value);
            if (editor != null)
                return (editor, creator);
        }

        // fall back to default editor
        return (value.CreateDefaultEditor(), null);
    }
}