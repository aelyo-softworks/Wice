namespace Wice.PropertyGrid;

public class EnumEditorCreator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : IEditorCreator<T>
{
    public object? CreateEditor(PropertyValueVisual<T> value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var editor = value.Property.Source.Grid.CreateEditorHost(value);
        if (editor != null)
        {
            value.DoWhenAttachedToParent(() =>
            {
                var parentFontSize = (float?)TextBox.FontSizeProperty.GetValue(value.Parent!);
                //editor.Header.Text.FontSize = parentFontSize;
                editor.Header.Text.CopyFrom(value.Parent);

                var fontSize = parentFontSize ?? Application.CurrentTheme.DefaultFontSize;
                editor.Header.Height = fontSize + 4;
            });

            editor.DialogOpened += (s, e) =>
            {
                object child;
                if (Conversions.IsFlagsEnum(value.Property.Type!))
                {
                    var flb = new FlagsEnumListBox();
                    if (value.Property.TryGetTargetValue(out var targetValue))
                    {
                        flb.Value = targetValue;
                    }

                    child = flb;
                }
                else
                {
                    var elb = new EnumListBox();
                    if (value.Property.TryGetTargetValue(out var targetValue))
                    {
                        elb.Value = targetValue!;
                    }

                    child = elb;
                }

                if (child is ListBox lb)
                {
                    lb.ItemDataBound += (s2, e2) =>
                    {
                        if (e2.Value?.DataVisual is TextBox tb)
                        {
                            tb.CopyFrom(value.Parent);
                        }

                        // TODO
                        //if (e2.Value.ItemVisual != null)
                        //{
                        //    e2.Value.ItemVisual.RenderBrush = null;// e2.Value.ItemVisual.Compositor.CreateColorBrush(D3DCOLORVALUE.Black);
                        //}
                    };
                }

                if (child is Visual visual)
                {
                    editor.Dialog?.Content.Children.Add(visual);
                }

                if (child is IValueable valueable)
                {
                    valueable.ValueChanged += (s2, e2) =>
                    {
                        value.Property.Value = e2.Value;
                    };
                }
            };
        }
        return editor;
    }

    public object? UpdateEditor(PropertyValueVisual<T> value, object? editor)
    {
        ArgumentNullException.ThrowIfNull(value);
        return editor;
    }
}
