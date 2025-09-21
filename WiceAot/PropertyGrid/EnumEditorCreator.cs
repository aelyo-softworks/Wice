namespace Wice.PropertyGrid;

/// <summary>
/// Creates and configures an editor host for enum properties within a <see cref="PropertyGrid"/>.
/// </summary>
/// <typeparam name="T">
/// The selected object type for the owning <see cref="PropertyGrid{T}"/>. The attribute requires public properties for trimming.
/// </typeparam>
public class EnumEditorCreator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : IEditorCreator<T>
{
    /// <summary>
    /// Creates and wires an editor host that lets the user pick a value for an enum (or [Flags] enum) property.
    /// </summary>
    /// <param name="value">The value visual hosting the editor for a single grid property.</param>
    /// <returns>
    /// The created editor host instance, or null when the grid declined to create one.
    /// </returns>
    public object? CreateEditor(PropertyValueVisual<T> value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // Ask the grid to provide a host for the editor UI of this property.
        var editor = value.Property.Source.Grid.CreateEditorHost(value);
        if (editor != null)
        {
            // Copy header typography from the parent once the visual is part of the tree.
            value.DoWhenAttachedToParent(() =>
            {
                var parentFontSize = (float?)TextBox.FontSizeProperty.GetValue(value.Parent!);
                // Keep the editor header text style consistent with the parent value visual.
                editor.Header.Text.CopyFrom(value.Parent);

                var fontSize = parentFontSize ?? value.GetWindowTheme().DefaultFontSize;
                editor.Header.Height = fontSize + 4;
            });

            // Populate the editor dialog lazily on open.
            editor.DialogOpened += (s, e) =>
            {
                object child;

                // Choose a specialized editor depending on whether the enum is flagged.
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

                // Harmonize item visuals typography with the surrounding UI.
                if (child is ListBox lb)
                {
                    lb.ItemDataBound += (s2, e2) =>
                    {
                        if (e2.Value?.DataVisual is TextBox tb)
                        {
                            tb.CopyFrom(value.Parent);
                        }

                        // TODO: adjust item brushes if needed
                        // if (e2.Value.ItemVisual != null)
                        // {
                        //     e2.Value.ItemVisual.RenderBrush = null;
                        // }
                    };
                }

                // Inject the picker visual into the dialog content.
                if (child is Visual visual)
                {
                    editor.Dialog?.Content.Children.Add(visual);
                }

                // Bridge editor value changes back to the underlying property value.
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

    /// <summary>
    /// Updates an existing editor instance for the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The host value visual.</param>
    /// <param name="editor">The current editor instance.</param>
    /// <returns>The same <paramref name="editor"/> instance (no-op).</returns>
    public object? UpdateEditor(PropertyValueVisual<T> value, object? editor)
    {
        ArgumentNullException.ThrowIfNull(value);
        return editor;
    }
}
