namespace Wice.Utilities;

public static class CompositionUtilities
{
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

    public static string? Trace(this CompositionBrush? brush)
    {
        if (brush == null)
            return null;

        if (brush is CompositionColorBrush color)
            return color.Color.ToString();

        return brush.GetType().Name;
    }
}
