namespace Wice.PropertyGrid;

public class DefaultEditorCreator : IEditorCreator
{
    public object CreateEditor(PropertyValueVisual value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var text = new TextBox();
#if DEBUG
        text.Name = "editorText";
#endif
        text.TextChangedTrigger = EventTrigger.LostFocus;
        text.IsEditable = value.Property.IsReadWrite;
        text.TrimmingGranularity = DWRITE_TRIMMING_GRANULARITY.DWRITE_TRIMMING_GRANULARITY_CHARACTER;
        text.Text = value.Property.TextValue;
        text.ToolTipContentCreator = tt => Window.CreateDefaultToolTipContent(tt, text.Text);
        value.DoWhenAttachedToParent(() =>
        {
            text.CopyFrom(value.Parent);
        });
        return text;
    }

    public object UpdateEditor(PropertyValueVisual value, object editor)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        if (editor is TextBox tb)
        {
            tb.Text = value.Property.TextValue;
        }

        return editor;
    }
}
