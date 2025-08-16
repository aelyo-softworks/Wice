﻿namespace Wice.PropertyGrid;

/// <summary>
///  Provides an editor factory for boolean properties within a<see cref="PropertyGrid{T}"/>.
/// Creates a <see cref = "ToggleSwitch" /> aligned to the left and initializes its state from the bound property value.
/// </summary>
/// <typeparam name="T">
/// The selected object type displayed by the <see cref="PropertyGrid{T}"/>. The attribute ensures public properties
/// are preserved for trimming/AOT scenarios.
/// </typeparam>
public class BooleanEditorCreator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : IEditorCreator<T>
{
    /// <summary>
    /// Creates a new <see cref="ToggleSwitch"/> editor for the given property value visual.
    /// </summary>
    /// <param name="value">The property value visual requesting an editor.</param>
    /// <returns>
    /// A <see cref="ToggleSwitch"/> initialized from the underlying boolean property when available.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public object? CreateEditor(PropertyValueVisual<T> value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var toggle = new ToggleSwitch { HorizontalAlignment = Alignment.Near };
        toggle.CopyStyleFrom(value.Property.Source.Grid);

        if (value.Property.Name != null)
        {
            var visuals = value.Property.Source.Grid.GetVisuals(value.Property.Name);
            if (visuals?.Text is TextBox tb)
            {
                if (tb.FontSize.HasValue)
                {
                    toggle.AutoSize = false;
                    toggle.Height = tb.FontSize.Value;
                    toggle.Width = tb.FontSize.Value * 2; // Approximate width for toggle switch
                }

                if (((IPropertyOwner)tb).TryGetPropertyValue(TextBox.ForegroundBrushProperty, out var foreground) &&
                    foreground is SolidColorBrush color)
                {
                    toggle.DoWhenAttachedToComposition(() =>
                    {
                        toggle.OffPathBrush = toggle.Compositor!.CreateColorBrush(color.Color.ToColor());
                    });
                }
            }
        }

        if (value.Property.TryGetTargetValue(out bool targetValue))
        {
            toggle.Value = targetValue;
        }
        return toggle;
    }

    /// <summary>
    /// Updates the existing editor instance. This implementation is a no-op and returns the provided editor unchanged.
    /// </summary>
    /// <param name="value">The property value visual that owns the editor.</param>
    /// <param name="editor">The current editor instance.</param>
    /// <returns>The same editor instance that was provided.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public object? UpdateEditor(PropertyValueVisual<T> value, object? editor)
    {
        ArgumentNullException.ThrowIfNull(value);
        return editor;
    }
}
