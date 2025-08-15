namespace Wice.PropertyGrid;

/// <summary>
/// Default editor factory for <see cref="PropertyValueVisual{T}"/> that renders a simple <see cref="TextBox"/>.
/// </summary>
/// <typeparam name="T">
/// The selected object type for the owning <see cref="PropertyGrid{T}"/>. Annotated to preserve public properties
/// for trimming/AOT so editors can reflect on them.
/// </typeparam>
/// <remarks>
/// Behavior:
/// - Produces a <see cref="TextBox"/> bound to the property's textual representation (<see cref="PropertyGridProperty{T}.TextValue"/>).<br/>
/// - Commits changes on focus loss (<see cref="EventTrigger.LostFocus"/>).<br/>
/// - Respects read-only state via <c>IsEditable</c>.<br/>
/// - Applies character trimming and a tooltip with the full text.<br/>
/// - Copies inherited visual settings from the parent when attached to the tree.
/// </remarks>
/// <seealso cref="IEditorCreator{T}"/>
/// <seealso cref="PropertyValueVisual{T}"/>
/// <seealso cref="TextBox"/>
public class DefaultEditorCreator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : IEditorCreator<T>
{
    /// <summary>
    /// Creates a new <see cref="TextBox"/> editor for the specified property value visual.
    /// </summary>
    /// <param name="value">The property value visual requesting an editor.</param>
    /// <returns>
    /// A configured <see cref="TextBox"/> instance, or <see langword="null"/> when creation is not applicable.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// The editor:
    /// - Commits text on focus loss.<br/>
    /// - Mirrors the property's read/write capability.<br/>
    /// - Shows a tooltip with the full text content.<br/>
    /// - Copies layout/visual settings from the parent upon attachment.
    /// </remarks>
    public object? CreateEditor(PropertyValueVisual<T> value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var text = new TextBox
        {
#if DEBUG
            Name = "editorText",
#endif
            TextChangedTrigger = EventTrigger.LostFocus,
            IsEditable = value.Property.IsReadWrite,
            TrimmingGranularity = DWRITE_TRIMMING_GRANULARITY.DWRITE_TRIMMING_GRANULARITY_CHARACTER,
            Text = value.Property.TextValue ?? string.Empty
        };
        text.ToolTipContentCreator = tt => Window.CreateDefaultToolTipContent(tt, text.Text);
        value.DoWhenAttachedToParent(() =>
        {
            text.CopyFrom(value.Parent);
        });
        return text;
    }

    /// <summary>
    /// Updates the provided editor instance with the latest property value or returns it unchanged.
    /// </summary>
    /// <param name="value">The property value visual that owns the editor.</param>
    /// <param name="editor">The current editor instance to update, or <see langword="null"/>.</param>
    /// <returns>
    /// The updated editor instance (same reference when applicable), or <see langword="null"/> when no editor is available.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// If the editor is a <see cref="TextBox"/>, its <see cref="TextBox.Text"/> is refreshed from
    /// <see cref="PropertyGridProperty{T}.TextValue"/>.
    /// </remarks>
    public object? UpdateEditor(PropertyValueVisual<T> value, object? editor)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (editor is TextBox tb)
        {
            tb.Text = value.Property.TextValue ?? string.Empty;
        }

        return editor;
    }
}
