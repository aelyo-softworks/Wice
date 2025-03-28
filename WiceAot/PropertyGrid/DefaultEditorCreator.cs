namespace Wice.PropertyGrid;

public class DefaultEditorCreator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : IEditorCreator<T>
{
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
