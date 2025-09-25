namespace Wice.PropertyGrid;

/// <summary>
/// Creates and configures an editor host for enum properties within a <see cref="PropertyGrid"/>.
/// </summary>
/// <typeparam name="T">
/// The selected object type for the owning <see cref="PropertyGrid{T}"/>. The attribute requires public properties for trimming.
/// </typeparam>
#if NETFRAMEWORK
public class EnumEditorCreator : IEditorCreator
#else
public class EnumEditorCreator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : IEditorCreator<T>
#endif
{
    /// <summary>
    /// Creates and wires an editor host that lets the user pick a value for an enum (or [Flags] enum) property.
    /// </summary>
    /// <param name="value">The value visual hosting the editor for a single grid property.</param>
    /// <returns>
    /// The created editor host instance, or null when the grid declined to create one.
    /// </returns>
#if NETFRAMEWORK
    public virtual object? CreateEditor(PropertyValueVisual value)
#else
    public virtual object? CreateEditor(PropertyValueVisual<T> value)
#endif
    {
        ExceptionExtensions.ThrowIfNull(value, nameof(value));

        // Ask the grid to provide a host for the editor UI of this property.
        var host = value.Property.Source.Grid.CreateEditorHost(value);
        if (host != null)
        {
            // Copy header typography from the parent once the visual is part of the tree.
            value.DoWhenAttachedToParent(() =>
            {
                var parentFontSize = (float?)TextBox.FontSizeProperty.GetValue(value.Parent!);
                // Keep the editor header text style consistent with the parent value visual.
                host.Header.Text.CopyFrom(value.Parent);

                var fontSize = parentFontSize ?? value.GetWindowTheme().DefaultFontSize;
                host.Header.Height = fontSize + 4;

                var margin = host.Header.Panel.Margin;
                margin.left = 0;
                host.Header.Panel.Margin = margin;
            });

            // Populate the editor dialog lazily on open.
            host.DialogOpened += (s, e) =>
            {
                object child;

                // Choose a specialized editor depending on whether the enum is flagged.
                var flags = Conversions.IsFlagsEnum(value.Property.Type!);
                if (flags)
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
                    };
                }

                // Inject the picker visual into the dialog content.
                if (child is Visual visual)
                {
                    host.Dialog?.Content.Children.Add(visual);
                }

                // Bridge editor value changes back to the underlying property value.
                if (child is IValueable valueable)
                {
                    valueable.ValueChanged += (s2, e2) =>
                    {
                        value.Property.Value = e2.Value;
                        if (!flags)
                        {
                            host.CloseDialog();
                        }
                    };
                }
            };
        }
        return host;
    }

    /// <summary>
    /// Updates an existing editor instance for the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The host value visual.</param>
    /// <param name="editor">The current editor instance.</param>
    /// <returns>The same <paramref name="editor"/> instance (no-op).</returns>
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
