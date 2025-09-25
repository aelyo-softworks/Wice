namespace Wice.PropertyGrid;

/// <summary>
/// Provides functionality to create and configure editors for properties, with support for password masking.
/// </summary>
/// <typeparam name="T"></typeparam>
#if NETFRAMEWORK
public class PasswordEditorCreator : IEditorCreator
#else
public class PasswordEditorCreator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : IEditorCreator<T>
#endif
{
    /// <summary>
    /// Creates and returns an editor for the specified property value visual.
    /// </summary>
    /// <param name="value">The property value visual for which the editor is to be created.  This parameter cannot be <see
    /// langword="null"/>.</param>
    /// <returns>An object representing the editor for the specified property value visual.  If the editor supports password
    /// capabilities, password mode is enabled, and a custom password character may be applied.</returns>
#if NETFRAMEWORK
    public virtual object? CreateEditor(PropertyValueVisual value)
#else
    public virtual object? CreateEditor(PropertyValueVisual<T> value)
#endif
    {
        ExceptionExtensions.ThrowIfNull(value, nameof(value));

        var editor = value.CreateDefaultEditor();
        if (editor is RenderVisual rv)
        {
            rv.BackgroundColor = D3DCOLORVALUE.Red.ChangeAlpha(20);
        }

        if (editor is IPasswordCapable pc)
        {
            pc.IsPasswordModeEnabled = true;
#if NETFRAMEWORK
            var pw = PropertyGridDynamicPropertyAttribute.GetValueFromProperty<char>(value.Property, nameof(TextBox.PasswordCharacter));
#else
            var pw = PropertyGridDynamicPropertyAttribute.GetValueFromProperty<T, char>(value.Property, nameof(TextBox.PasswordCharacter));
#endif
            if (pw != 0)
            {
                pc.SetPasswordCharacter(pw);
            }
        }
        return editor;
    }

    /// <summary>
    /// Updates the specified editor based on the provided property value visual representation.
    /// </summary>
    /// <param name="value">The visual representation of the property value. Cannot be <see langword="null"/>.</param>
    /// <param name="editor">The editor object to be updated. Can be <see langword="null"/>.</param>
    /// <returns>The updated editor object, or the original editor if no updates are applied.</returns>
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
