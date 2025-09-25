namespace Wice.Utilities;

/// <summary>
/// Utilities for working with Windows Composition brushes.
/// </summary>
public static class CompositionUtilities
{
    /// <summary>
    /// Creates a deep copy of the specified <see cref="CompositionBrush"/> instance.
    /// </summary>
    /// <param name="brush">The <see cref="CompositionBrush"/> to clone. Can be <see langword="null"/>.</param>
    /// <returns>A new <see cref="CompositionBrush"/> instance that is a copy of the input brush, or <see langword="null"/> if
    /// <paramref name="brush"/> is <see langword="null"/>.</returns>
#if NET
    [return: NotNullIfNotNull(nameof(brush))]
#endif
    public static CompositionBrush? Clone(this CompositionBrush? brush)
    {
        if (brush == null)
            return null;

        if (brush is CompositionColorBrush color)
            return color.Compositor.CreateColorBrush(color.Color);

        if (brush is CompositionBackdropBrush back)
            return back.Compositor.CreateBackdropBrush();

        if (brush is CompositionMaskBrush mask)
        {
            var clone = mask.Compositor.CreateMaskBrush();
            clone.Source = Clone(mask.Source);
            clone.Mask = Clone(mask.Mask);
            return clone;
        }

        if (brush is CompositionNineGridBrush nine)
        {
            var clone = nine.Compositor.CreateNineGridBrush();
            clone.BottomInset = nine.BottomInset;
            clone.BottomInsetScale = nine.BottomInsetScale;
            clone.IsCenterHollow = nine.IsCenterHollow;
            clone.LeftInset = nine.LeftInset;
            clone.LeftInsetScale = nine.LeftInsetScale;
            clone.RightInset = nine.RightInset;
            clone.RightInsetScale = nine.RightInsetScale;
            clone.Source = Clone(nine.Source);
            clone.TopInset = nine.TopInset;
            clone.TopInsetScale = nine.TopInsetScale;
            return clone;
        }

        throw new NotSupportedException();
    }

    /// <summary>
    /// Calculates the cumulative offset of the specified visual relative to the root visual.
    /// </summary>
    /// <param name="visual">The visual for which to calculate the root-relative offset. Cannot be <see langword="null"/>.</param>
    /// <returns>A <see cref="Vector3"/> representing the total offset of the visual relative to the root visual.  If the visual
    /// has no parent, the offset is equal to its own offset.</returns>
    public static Vector3 GetRootOffset(this Windows.UI.Composition.Visual visual)
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));

        var offset = Vector3.Zero;
        var currentVisual = visual;
        while (currentVisual != null)
        {
            offset += currentVisual.Offset;
            currentVisual = currentVisual.Parent;
        }
        return offset;
    }

    /// <summary>
    /// Produces a short, human-readable description of a brush for tracing or logging.
    /// </summary>
    /// <param name="brush">The brush to describe. May be <c>null</c>.</param>
    /// <returns>
    /// If <paramref name="brush"/> is <c>null</c>, returns <c>null</c>.
    /// If it is a <c>CompositionColorBrush</c>, returns the color string.
    /// Otherwise, returns the runtime type name of the brush.
    /// </returns>
    public static string? Trace(this CompositionBrush? brush)
    {
        if (brush == null)
            return null;

        if (brush is CompositionColorBrush color)
            return color.Color.ToString();

        return brush.GetType().Name;
    }
}
