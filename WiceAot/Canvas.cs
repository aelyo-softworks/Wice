namespace Wice;

public class Canvas : Visual
{
    public static VisualProperty MeasureToContentProperty { get; } = VisualProperty.Add(typeof(Window), nameof(Canvas), VisualPropertyInvalidateModes.Measure, DimensionOptions.Manual);

    // review: invalidate measure or arrange?
    public static VisualProperty LeftProperty { get; } = VisualProperty.Add(typeof(Canvas), "Left", VisualPropertyInvalidateModes.ParentMeasure, float.NaN);
    public static VisualProperty TopProperty { get; } = VisualProperty.Add(typeof(Canvas), "Top", VisualPropertyInvalidateModes.ParentMeasure, float.NaN);
    public static VisualProperty RightProperty { get; } = VisualProperty.Add(typeof(Canvas), "Right", VisualPropertyInvalidateModes.ParentMeasure, float.NaN);
    public static VisualProperty BottomProperty { get; } = VisualProperty.Add(typeof(Canvas), "Bottom", VisualPropertyInvalidateModes.ParentMeasure, float.NaN);

    public static D2D_RECT_F GetRect(D2D_SIZE_F parentSize, Visual child, GetRectOptions options = GetRectOptions.Default)
    {
        ArgumentNullException.ThrowIfNull(child);
        var csize = child.DesiredSize;
        var size = csize;
        var keepProportions = options.HasFlag(GetRectOptions.KeepProportions);
        var proportion = csize.width != 0 ? csize.height / csize.width : 1;
        float left;
        float top;
        var value = GetLeft(child);
        if (value.IsSet())
        {
            left = value;

            // if left & right are set and parent width is set, size is forced
            var other = GetRight(child);
            if (parentSize.width.IsSet() && child.Width.IsNotSet())
            {
                if (other.IsSet())
                {
                    size.width = Math.Max(0, parentSize.width - left - other);
                    if (keepProportions)
                    {
                        size.height = size.width * proportion;
                    }
                }
                else if (child.HorizontalAlignment == Alignment.Stretch)
                {
                    // right not set, but stretch if required
                    size.width = Math.Max(0, parentSize.width - left);
                    if (keepProportions)
                    {
                        size.height = size.width * proportion;
                    }
                }
            }
        }
        else
        {
            value = GetRight(child);
            left = 0;
            if (parentSize.width.IsSet())
            {
                if (value.IsSet())
                {
                    left = parentSize.width - value - size.width;
                    if (child.HorizontalAlignment == Alignment.Stretch && child.Width.IsNotSet())
                    {
                        size.width = Math.Max(0, parentSize.width - left);
                        if (keepProportions)
                        {
                            size.height = size.width * proportion;
                        }
                    }
                }
                else
                {
                    switch (child.HorizontalAlignment)
                    {
                        case Alignment.Far:
                            left = parentSize.width - size.width;
                            break;

                        case Alignment.Center:
                            left = (parentSize.width - size.width) / 2;
                            break;

                        // note this is the usual default case
                        case Alignment.Stretch:
                            if (options.HasFlag(GetRectOptions.StretchWidthToParent) && child.Width.IsNotSet())
                            {
                                size.width = parentSize.width;
                                if (keepProportions)
                                {
                                    size.height = size.width * proportion;
                                }
                            }
                            else
                            {
                                left = (parentSize.width - size.width) / 2;
                            }
                            break;
                    }
                }
            }
        }

        value = GetTop(child);
        if (value.IsSet())
        {
            top = value;

            // if top & bottom are set and parent heigt is set, size is forced
            var other = GetBottom(child);
            if (parentSize.height.IsSet() && child.Height.IsNotSet())
            {
                if (other.IsSet())
                {
                    size.height = Math.Max(0, parentSize.height - top - other);
                    if (keepProportions)
                    {
                        size.width = size.height / proportion;
                    }
                }
                else if (child.VerticalAlignment == Alignment.Stretch)
                {
                    // bottom not set, but stretch if required
                    size.height = Math.Max(0, parentSize.height - top);
                    if (keepProportions)
                    {
                        size.width = size.height / proportion;
                    }
                }
            }
        }
        else
        {
            value = GetBottom(child);
            if (value.IsSet() && parentSize.height.IsSet())
            {
                top = parentSize.height - value - size.height;
            }
            else
            {
                top = 0;
                if (parentSize.height.IsSet())
                {
                    switch (child.VerticalAlignment)
                    {
                        case Alignment.Far:
                            top = parentSize.height - size.height;
                            break;

                        case Alignment.Center:
                            top = (parentSize.height - size.height) / 2;
                            break;

                        // note this is the usual default case
                        case Alignment.Stretch:
                            if (options.HasFlag(GetRectOptions.StretchHeightToParent) && child.Height.IsNotSet())
                            {
                                size.height = parentSize.height;
                                if (keepProportions)
                                {
                                    size.width = size.height / proportion;
                                }
                            }
                            else
                            {
                                top = (parentSize.height - size.height) / 2;
                            }
                            break;
                    }
                }
            }
        }

        return new D2D_RECT_F(left, top, size);
    }

    internal static D2D_SIZE_F MeasureCore(Visual visual, D2D_SIZE_F constraint, DimensionOptions sizeToContent)
    {
        D2D_RECT_F? rect = null;
        var children = visual.VisibleChildren.ToArray();
        foreach (var child in children)
        {
            var childConstraint = D2D_SIZE_F.PositiveInfinity;
            if (child.HorizontalAlignment == Alignment.Stretch && constraint.width.IsSet())
            {
                childConstraint.width = constraint.width;
            }

            if (child.VerticalAlignment == Alignment.Stretch && constraint.height.IsSet())
            {
                childConstraint.height = constraint.height;
            }

            child.Measure(childConstraint);

            if (sizeToContent != DimensionOptions.Manual)
            {
                var childRect = GetRect(childConstraint, child);
                if (rect.HasValue)
                {
                    rect = rect.Value.Union(childRect);
                }
                else
                {
                    rect = childRect;
                }
            }
        }

        if (sizeToContent == DimensionOptions.Manual || !rect.HasValue)
            return new D2D_SIZE_F();

        if (sizeToContent.HasFlag(DimensionOptions.Width) && sizeToContent.HasFlag(DimensionOptions.Height))
            return rect.Value.Size;

        if (sizeToContent.HasFlag(DimensionOptions.Width))
            return new D2D_SIZE_F(rect.Value.Size.width, 0);

        return new D2D_SIZE_F(0, rect.Value.Size.height);
    }

    internal static void ArrangeCore(Visual visual, D2D_RECT_F finalRect)
    {
        var finalSize = finalRect.Size;
        var children = visual.VisibleChildren.ToArray();
        foreach (var child in children)
        {
            var childRect = GetRect(finalSize, child);
            child.Arrange(childRect);
        }
    }

    [Category(CategoryLayout)]
    public DimensionOptions MeasureToContent { get => (DimensionOptions)GetPropertyValue(MeasureToContentProperty)!; set => SetPropertyValue(MeasureToContentProperty, value); }

    // we only support stretch alignment (means we adapt to parent's constraint)
    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint) => MeasureCore(this, constraint, MeasureToContent);
    protected override void ArrangeCore(D2D_RECT_F finalRect) => ArrangeCore(this, finalRect);

    public static void SetRect(IPropertyOwner properties, D2D_RECT_F rect)
    {
        ArgumentNullException.ThrowIfNull(properties);
        SetLeft(properties, rect.left);
        SetTop(properties, rect.top);
        SetRight(properties, rect.right);
        SetBottom(properties, rect.bottom);
    }

    public static void SetLeft(IPropertyOwner properties, float value)
    {
        ArgumentNullException.ThrowIfNull(properties);
        if (value.IsNotSet())
        {
            properties.ResetPropertyValue(LeftProperty, out _);
            return;
        }
        properties.SetPropertyValue(LeftProperty, value);
    }

    public static float GetLeft(IPropertyOwner properties)
    {
        ArgumentNullException.ThrowIfNull(properties);
        return (float)properties.GetPropertyValue(LeftProperty)!;
    }

    public static void SetTop(IPropertyOwner properties, float value)
    {
        ArgumentNullException.ThrowIfNull(properties);
        if (value.IsNotSet())
        {
            properties.ResetPropertyValue(TopProperty, out _);
            return;
        }
        properties.SetPropertyValue(TopProperty, value);
    }

    public static float GetTop(IPropertyOwner properties)
    {
        ArgumentNullException.ThrowIfNull(properties);
        return (float)properties.GetPropertyValue(TopProperty)!;
    }

    public static void SetRight(IPropertyOwner properties, float value)
    {
        ArgumentNullException.ThrowIfNull(properties);
        if (value.IsNotSet())
        {
            properties.ResetPropertyValue(RightProperty, out _);
            return;
        }
        properties.SetPropertyValue(RightProperty, value);
    }

    public static float GetRight(IPropertyOwner properties)
    {
        ArgumentNullException.ThrowIfNull(properties);
        return (float)properties.GetPropertyValue(RightProperty)!;
    }

    public static void SetBottom(IPropertyOwner properties, float value)
    {
        ArgumentNullException.ThrowIfNull(properties);
        if (value.IsNotSet())
        {
            properties.ResetPropertyValue(BottomProperty, out _);
            return;
        }
        properties.SetPropertyValue(BottomProperty, value);
    }

    public static float GetBottom(IPropertyOwner properties)
    {
        ArgumentNullException.ThrowIfNull(properties);
        return (float)properties.GetPropertyValue(BottomProperty)!;
    }
}
