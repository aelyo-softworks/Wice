﻿namespace Wice.PropertyGrid;

/// <summary>
/// Provides per-property configuration for <c>PropertyGrid</c> rendering and editing.
/// </summary>
/// <remarks>
/// Use this attribute on a property to control:
/// - The sort order within its category (<see cref="SortOrder"/>).
/// - The editor creation strategy via a custom editor factory type (<see cref="EditorType"/>).
/// If no custom editor creator is supplied or it produces <see langword="null"/>, the grid falls back to the default editor.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public sealed class PropertyGridPropertyOptionsAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the explicit sort order of the property within its category/group.
    /// </summary>
    /// <remarks>
    /// Lower values are displayed earlier. Defaults to <c>0</c>.
    /// </remarks>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets an optional editor factory type used to create/update the editor for this property.
    /// </summary>
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