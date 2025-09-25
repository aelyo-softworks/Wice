namespace Wice.PropertyGrid;

/// <summary>
/// Defines a factory contract for creating and updating editor visuals associated with a property value.
/// </summary>
/// <typeparam name="T">
/// The selected object type. Annotated to preserve public properties for trimming/AOT so editors can reflect on them.
/// </typeparam>
#if NETFRAMEWORK
public interface IEditorCreator
#else
public interface IEditorCreator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>
#endif
{
    /// <summary>
    /// Creates a new editor instance for the specified property value visual.
    /// </summary>
    /// <param name="value">The property value visual requesting an editor.</param>
    /// <returns>
    /// The created editor instance, or <see langword="null" /> if no suitable editor can be created.
    /// </returns>
#if NETFRAMEWORK
    object? CreateEditor(PropertyValueVisual value);
#else
    object? CreateEditor(PropertyValueVisual<T> value);
#endif

    /// <summary>
    /// Updates the given editor instance for the specified property value visual or creates a new one if needed.
    /// </summary>
    /// <param name="value">The property value visual that owns the editor.</param>
    /// <param name="editor">The current editor instance to update, or <see langword="null" /> to create a new one.</param>
    /// <returns>
    /// The updated editor instance (which may be the same instance, a replacement, or <see langword="null" /> if not applicable).
    /// </returns>
#if NETFRAMEWORK
    object? UpdateEditor(PropertyValueVisual value, object? editor);
#else
    object? UpdateEditor(PropertyValueVisual<T> value, object? editor);
#endif
}
