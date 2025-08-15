namespace Wice.PropertyGrid;

/// <summary>
/// Provides per-property configuration for <c>PropertyGrid</c> rendering and editing.
/// </summary>
/// <remarks>
/// Use this attribute on a property to control:
/// - The sort order within its category (<see cref="SortOrder"/>).
/// - The editor creation strategy via a custom editor factory type (<see cref="EditorType"/>).
/// If no custom editor creator is supplied or it produces <see langword="null"/>, the grid falls back to the default editor.
/// </remarks>
/// <example>
/// [PropertyGridPropertyOptions(SortOrder = 10, EditorType = typeof(MyStringEditorCreator))]
/// public string Name { get; set; }
/// </example>
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
    /// <remarks>
    /// Requirements:
    /// - Must have a public parameterless constructor (instantiated via <see cref="Activator.CreateInstance(Type)"/>).<br/>
    /// - Must implement <see cref="IEditorCreator{T}"/> for the grid's selected object type <typeparamref name="T"/> at runtime.<br/>
    /// Trimming/AOT:
    /// - Annotated with <see cref="DynamicallyAccessedMembersAttribute"/> to preserve the public parameterless constructor.
    /// </remarks>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public Type? EditorType { get; set; }

    /// <summary>
    /// Creates an editor instance for the supplied value visual using a custom creator when available,
    /// or falls back to the default editor.
    /// </summary>
    /// <typeparam name="T">
    /// The selected object type for the owning <see cref="PropertyGrid{T}"/>.
    /// Annotated to preserve public properties for trimming/AOT.
    /// </typeparam>
    /// <param name="value">The property value visual requesting an editor.</param>
    /// <returns>
    /// The created editor instance (custom or default). Never returns <see langword="null"/>; default editors are used as fallback.
    /// </returns>
    /// <exception cref="WiceException">
    /// Thrown when <see cref="EditorType"/> is set but the created instance does not implement <see cref="IEditorCreator{T}"/> (error code "0024").
    /// </exception>
    /// <remarks>
    /// Behavior:
    /// - Try to reuse a per-value creator by casting <c>value.Property.Value</c> to <see cref="IEditorCreator{T}"/>.<br/>
    /// - If not available and <see cref="EditorType"/> is provided, instantiate it and cast to <see cref="IEditorCreator{T}"/>.<br/>
    /// - If a creator is present, call <see cref="IEditorCreator{T}.CreateEditor(PropertyValueVisual{T})"/>; if it returns <see langword="null"/>, fallback applies.<br/>
    /// - Fallback: return <see cref="PropertyValueVisual{T}.CreateDefaultEditor()"/>.
    /// </remarks>
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