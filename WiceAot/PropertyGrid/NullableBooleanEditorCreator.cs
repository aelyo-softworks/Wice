namespace Wice.PropertyGrid;

/// <summary>
/// Provides functionality to create and update editor instances for nullable boolean properties.
/// </summary>
/// <typeparam name="T"></typeparam>
#if NETFRAMEWORK
public class NullableBooleanEditorCreator : IEditorCreator
#else
public class NullableBooleanEditorCreator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : IEditorCreator<T>
#endif
{
    /// <summary>
    /// Creates and returns an editor visual for the specified property value.
    /// </summary>
    /// <param name="value">The property value for which the editor is being created. This parameter cannot be <see langword="null"/>.</param>
    /// <returns>An editor control, typically a <see cref="NullableCheckBox"/>, configured to represent the specified property
    /// value.</returns>
#if NETFRAMEWORK
    public virtual object? CreateEditor(PropertyValueVisual value)
#else
    public virtual object? CreateEditor(PropertyValueVisual<T> value)
#endif
    {
        ExceptionExtensions.ThrowIfNull(value, nameof(value));
        var checkBox = new NullableCheckBox { HorizontalAlignment = Alignment.Near };
        if (value.Property.TryGetTargetValue(out bool targetValue))
        {
            checkBox.Value = targetValue;
        }

        return checkBox;
    }

    /// <summary>
    /// Updates the existing editor instance. This implementation is a no-op and returns the provided editor unchanged.
    /// </summary>
    /// <param name="value">The property value visual that owns the editor.</param>
    /// <param name="editor">The current editor instance.</param>
    /// <returns>The same editor instance that was provided.</returns>
#if NETFRAMEWORK
    public virtual object? UpdateEditor(PropertyValueVisual value, object? editor)
#else
    public virtual object? UpdateEditor(PropertyValueVisual<T> value, object? editor)
#endif
    {
        ExceptionExtensions.ThrowIfNull(value, nameof(value));
        return editor;
    }
}
