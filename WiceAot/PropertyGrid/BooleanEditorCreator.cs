namespace Wice.PropertyGrid;

/// <summary>
///  Provides an editor factory for boolean properties within a<see cref="PropertyGrid{T}"/>.
/// Creates a <see cref = "ToggleSwitch" /> aligned to the left and initializes its state from the bound property value.
/// </summary>
/// <typeparam name="T">
/// The selected object type displayed by the <see cref="PropertyGrid{T}"/>. The attribute ensures public properties
/// are preserved for trimming/AOT scenarios.
/// </typeparam>
#if NETFRAMEWORK
public class BooleanEditorCreator : IEditorCreator
#else
public class BooleanEditorCreator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : IEditorCreator<T>
#endif
{
    private readonly ToggleSwitch _toggle = new();

    /// <summary>
    /// Creates a new <see cref="ToggleSwitch"/> editor for the given property value visual.
    /// </summary>
    /// <param name="value">The property value visual requesting an editor.</param>
    /// <returns>
    /// A <see cref="ToggleSwitch"/> initialized from the underlying boolean property when available.
    /// </returns>
#if NETFRAMEWORK
    public virtual object? CreateEditor(PropertyValueVisual value)
#else
    public virtual object? CreateEditor(PropertyValueVisual<T> value)
#endif
    {
        ExceptionExtensions.ThrowIfNull(value, nameof(value));

        _toggle.HorizontalAlignment = Alignment.Near;
        _toggle.CopyStyleFrom(value.Property.Source.Grid);

        if (value.Property.Name != null)
        {
            var visuals = value.Property.Source.Grid.GetVisuals(value.Property.Name);
            if (visuals?.Text is TextBox tb)
            {
                if (tb.FontSize.HasValue)
                {
                    _toggle.AutoSize = false;
                    _toggle.Height = tb.FontSize.Value;
                    _toggle.Width = tb.FontSize.Value * 2; // Approximate width for toggle switch
                }

                if (((IPropertyOwner)tb).TryGetPropertyValue(TextBox.ForegroundBrushProperty, out var foreground) &&
                    foreground is SolidColorBrush color)
                {
                    _toggle.DoWhenAttachedToComposition(() =>
                    {
                        _toggle.OffPathBrush = _toggle.Compositor!.CreateColorBrush(color.Color.ToColor());
                    });
                }
            }
        }

        if (value.Property.TryGetTargetValue(out bool targetValue))
        {
            _toggle.Value = targetValue;
        }

        return _toggle;
    }

    /// <summary>
    /// Updates the existing editor instance. This implementation is a no-op and returns the provided editor unchanged.
    /// </summary>
    /// <param name="value">The property value visual that owns the editor.</param>
    /// <param name="editor">The current editor instance.</param>
    /// <returns>The same editor instance that was provided.</returns>
#if NETFRAMEWORK
    public virtual object? UpdateEditor(PropertyValueVisual value, object? editor)
#else
    public virtual object? UpdateEditor(PropertyValueVisual<T> value, object? editor)
#endif
    {
        ExceptionExtensions.ThrowIfNull(value, nameof(value));

        if (value.Property.TryGetTargetValue(out bool targetValue))
        {
            _toggle.Value = targetValue;
        }
        return editor;
    }
}
