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
    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
