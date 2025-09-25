namespace Wice.PropertyGrid;

/// <summary>
/// Default editor factory for <see cref="PropertyValueVisual{T}"/> that renders a simple <see cref="TextBox"/>.
/// </summary>
/// <typeparam name="T">
/// The selected object type for the owning <see cref="PropertyGrid{T}"/>. Annotated to preserve public properties
/// for trimming/AOT so editors can reflect on them.
/// </typeparam>
#if NETFRAMEWORK
public class DefaultEditorCreator : IEditorCreator
#else
public class DefaultEditorCreator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : IEditorCreator<T>
#endif
{
    private readonly TextBox _textBox = new();

    /// <summary>
    /// Creates a new <see cref="TextBox"/> editor for the specified property value visual.
    /// </summary>
    /// <param name="value">The property value visual requesting an editor.</param>
    /// <returns>
    /// A configured <see cref="TextBox"/> instance, or <see langword="null"/> when creation is not applicable.
    /// </returns>
#if NETFRAMEWORK
    public virtual object? CreateEditor(PropertyValueVisual value)
#else
    public virtual object? CreateEditor(PropertyValueVisual<T> value)
#endif
    {
        ExceptionExtensions.ThrowIfNull(value, nameof(value));


#if DEBUG
        _textBox.Name = "editorText";
#endif
        _textBox.TextChangedTrigger = EventTrigger.LostFocus;
        _textBox.IsEditable = value.Property.IsReadWrite;
        _textBox.IsFocusable = true;
        _textBox.IsEnabled = value.Property.IsReadWrite;
        _textBox.AcceptsReturn = true; // allow multiline
        _textBox.TrimmingGranularity = DWRITE_TRIMMING_GRANULARITY.DWRITE_TRIMMING_GRANULARITY_CHARACTER;
        _textBox.Text = value.Property.TextValue ?? string.Empty;

        _textBox.KeyDown += (s, e) =>
        {
            // commit on enter (unless shift is down in which case it will create a new line inside textbox)
            var shift = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_SHIFT);
            if (e.Key == VIRTUAL_KEY.VK_RETURN && !shift)
            {
                _textBox.CommitChanges();
                _textBox.Window?.MoveFocus(FocusDirection.Next);
                e.Handled = true;
            }
        };

        _textBox.ToolTipContentCreator = tt => Window.CreateDefaultToolTipContent(tt, _textBox.Text);
        value.DoWhenAttachedToParent(() =>
        {
            _textBox.CopyFrom(value.Parent);
        });

        return _textBox;
    }

    /// <summary>
    /// Updates the provided editor instance with the latest property value or returns it unchanged.
    /// </summary>
    /// <param name="value">The property value visual that owns the editor.</param>
    /// <param name="editor">The current editor instance to update, or <see langword="null"/>.</param>
    /// <returns>
    /// The updated editor instance (same reference when applicable), or <see langword="null"/> when no editor is available.
    /// </returns>
#if NETFRAMEWORK
    public virtual object? UpdateEditor(PropertyValueVisual value, object? editor)
#else
    public virtual object? UpdateEditor(PropertyValueVisual<T> value, object? editor)
#endif
    {
        ExceptionExtensions.ThrowIfNull(value, nameof(value));
        _textBox.Text = value.Property.TextValue ?? string.Empty;
        return editor;
    }
}
