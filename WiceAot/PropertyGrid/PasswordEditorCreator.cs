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
    /// <inheritdoc/>
#if NETFRAMEWORK
    public virtual object? CreateEditor(PropertyValueVisual value)
#else
    public virtual object? CreateEditor(PropertyValueVisual<T> value)
#endif
    {
        ExceptionExtensions.ThrowIfNull(value, nameof(value));

        // we could also derive from DefaultEditorCreator and call base.CreateEditor
        var cae = value.CreateCreatorAndEditor(null);
        if (cae == null)
            return null;

        if (cae.Editor is Visual visual)
        {
            // disable tooltip on password box
            visual.ToolTipContentCreator = null;
        }

        if (cae.Editor is RenderVisual rv)
        {
            rv.BackgroundColor = D3DCOLORVALUE.Red.ChangeAlpha(20);
        }

        if (cae.Editor is IPasswordCapable pc)
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
        return cae.Editor;
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
