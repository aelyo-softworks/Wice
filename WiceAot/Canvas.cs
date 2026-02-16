namespace Wice;

/// <summary>
/// Absolute layout container similar to WPF's Canvas.
/// Positions children using the attached properties <see cref="LeftProperty"/>, <see cref="TopProperty"/>,
/// <see cref="RightProperty"/>, and <see cref="BottomProperty"/>. When both opposite edges are specified
/// (e.g., Left and Right) and the parent size is known, the child's size is inferred unless an explicit
/// <see cref="Visual.Width"/>/<see cref="Visual.Height"/> is set. Supports optional proportional resizing
/// and stretching via <see cref="GetRectOptions"/>.
/// </summary>
public partial class Canvas : Visual
{
    /// <summary>
    /// Controls whether the canvas returns its size from the union of its children during Measure.
    /// </summary>
    public static VisualProperty MeasureToContentProperty { get; } = VisualProperty.Add(typeof(Window), nameof(Canvas), VisualPropertyInvalidateModes.Measure, DimensionOptions.Manual);

    // review: invalidate measure or arrange?
    /// <summary>
    /// Attached property storing the left offset (DIPs) of a child within a <see cref="Canvas"/>.
    /// Use <see cref="float.NaN"/> to clear the value. Changing it triggers a parent measure pass.
    /// </summary>
    public static VisualProperty LeftProperty { get; } = VisualProperty.Add(typeof(Canvas), "Left", VisualPropertyInvalidateModes.ParentMeasure, float.NaN);

    /// <summary>
    /// Attached property storing the top offset (DIPs) of a child within a <see cref="Canvas"/>.
    /// Use <see cref="float.NaN"/> to clear the value. Changing it triggers a parent measure pass.
    /// </summary>
    public static VisualProperty TopProperty { get; } = VisualProperty.Add(typeof(Canvas), "Top", VisualPropertyInvalidateModes.ParentMeasure, float.NaN);

    /// <summary>
    /// Attached property storing the right offset (DIPs) of a child within a <see cref="Canvas"/>.
    /// Use <see cref="float.NaN"/> to clear the value. Changing it triggers a parent measure pass.
    /// </summary>
    public static VisualProperty RightProperty { get; } = VisualProperty.Add(typeof(Canvas), "Right", VisualPropertyInvalidateModes.ParentMeasure, float.NaN);

    /// <summary>
    /// Attached property storing the bottom offset (DIPs) of a child within a <see cref="Canvas"/>.
    /// Use <see cref="float.NaN"/> to clear the value. Changing it triggers a parent measure pass.
    /// </summary>
    public static VisualProperty BottomProperty { get; } = VisualProperty.Add(typeof(Canvas), "Bottom", VisualPropertyInvalidateModes.ParentMeasure, float.NaN);

    /// <summary>
    /// Computes the child rectangle relative to the parent based on attached edge values and alignment.
    /// </summary>
    /// <param name="parentSize">The parent's available size. Individual dimensions may be unset.</param>
    /// <param name="child">The child visual to position.</param>
    /// <param name="options">
    /// Layout options:
    /// - <see cref="GetRectOptions.KeepProportions"/>: preserves the measured aspect ratio when resizing from edges/stretch.
    /// - <see cref="GetRectOptions.StretchWidthToParent"/>/<see cref="GetRectOptions.StretchHeightToParent"/>:
    ///   when alignment is Stretch and the child has no explicit size, stretches to the parent if the parent dimension is set.
    /// </param>
    /// <returns>The rectangle (position and size) for the child.</returns>
    public static D2D_RECT_F GetRect(D2D_SIZE_F parentSize, Visual child, GetRectOptions options = GetRectOptions.Default)
    {
        ExceptionExtensions.ThrowIfNull(child, nameof(child));
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

            // if top & bottom are set and parent height is set, size is forced
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
        foreach (var child in children.Where(c => c.Parent != null))
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
        foreach (var child in children.Where(c => c.Parent != null))
        {
            var childRect = GetRect(finalSize, child);
            if (childRect.IsSet)
            {
                child.Arrange(childRect);
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the canvas sizes to the union of its children during Measure.
    /// </summary>
    [Category(CategoryLayout)]
    public DimensionOptions MeasureToContent { get => (DimensionOptions)GetPropertyValue(MeasureToContentProperty)!; set => SetPropertyValue(MeasureToContentProperty, value); }

    /// <inheritdoc/>
    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint) => MeasureCore(this, constraint, MeasureToContent);

    /// <inheritdoc/>
    protected override void ArrangeCore(D2D_RECT_F finalRect) => ArrangeCore(this, finalRect);

    /// <summary>
    /// Sets all four attached offsets on a target object from a rectangle.
    /// </summary>
    /// <param name="properties">The target that stores attached properties.</param>
    /// <param name="rect">The rectangle whose left/top/right/bottom to apply.</param>
    public static void SetRect(IPropertyOwner properties, D2D_RECT_F rect)
    {
        ExceptionExtensions.ThrowIfNull(properties, nameof(properties));
        SetLeft(properties, rect.left);
        SetTop(properties, rect.top);
        SetRight(properties, rect.right);
        SetBottom(properties, rect.bottom);
    }

    /// <summary>
    /// Sets the attached Left offset (DIPs). Use <see cref="float.NaN"/> to clear/unset.
    /// </summary>
    /// <param name="properties">The target object.</param>
    /// <param name="value">Left offset in DIPs, or <see cref="float.NaN"/> to unset.</param>
    public static void SetLeft(IPropertyOwner properties, float value)
    {
        ExceptionExtensions.ThrowIfNull(properties, nameof(properties));
        if (value.IsNotSet())
        {
            properties.ResetPropertyValue(LeftProperty, out _);
            return;
        }
        properties.SetPropertyValue(LeftProperty, value);
    }

    /// <summary>
    /// Gets the attached Left offset (DIPs). Returns <see cref="float.NaN"/> when not set.
    /// </summary>
    /// <param name="properties">The target object.</param>
    /// <returns>The left offset or <see cref="float.NaN"/>.</returns>
    public static float GetLeft(IPropertyOwner properties)
    {
        ExceptionExtensions.ThrowIfNull(properties, nameof(properties));
        return (float)properties.GetPropertyValue(LeftProperty)!;
    }

    /// <summary>
    /// Sets the attached Top offset (DIPs). Use <see cref="float.NaN"/> to clear/unset.
    /// </summary>
    /// <param name="properties">The target object.</param>
    /// <param name="value">Top offset in DIPs, or <see cref="float.NaN"/> to unset.</param>
    public static void SetTop(IPropertyOwner properties, float value)
    {
        ExceptionExtensions.ThrowIfNull(properties, nameof(properties));
        if (value.IsNotSet())
        {
            properties.ResetPropertyValue(TopProperty, out _);
            return;
        }
        properties.SetPropertyValue(TopProperty, value);
    }

    /// <summary>
    /// Gets the attached Top offset (DIPs). Returns <see cref="float.NaN"/> when not set.
    /// </summary>
    /// <param name="properties">The target object.</param>
    /// <returns>The top offset or <see cref="float.NaN"/>.</returns>
    public static float GetTop(IPropertyOwner properties)
    {
        ExceptionExtensions.ThrowIfNull(properties, nameof(properties));
        return (float)properties.GetPropertyValue(TopProperty)!;
    }

    /// <summary>
    /// Sets the attached Right offset (DIPs). Use <see cref="float.NaN"/> to clear/unset.
    /// </summary>
    /// <param name="properties">The target object.</param>
    /// <param name="value">Right offset in DIPs, or <see cref="float.NaN"/> to unset.</param>
    public static void SetRight(IPropertyOwner properties, float value)
    {
        ExceptionExtensions.ThrowIfNull(properties, nameof(properties));
        if (value.IsNotSet())
        {
            properties.ResetPropertyValue(RightProperty, out _);
            return;
        }
        properties.SetPropertyValue(RightProperty, value);
    }

    /// <summary>
    /// Gets the attached Right offset (DIPs). Returns <see cref="float.NaN"/> when not set.
    /// </summary>
    /// <param name="properties">The target object.</param>
    /// <returns>The right offset or <see cref="float.NaN"/>.</returns>
    public static float GetRight(IPropertyOwner properties)
    {
        ExceptionExtensions.ThrowIfNull(properties, nameof(properties));
        return (float)properties.GetPropertyValue(RightProperty)!;
    }

    /// <summary>
    /// Sets the attached Bottom offset (DIPs). Use <see cref="float.NaN"/> to clear/unset.
    /// </summary>
    /// <param name="properties">The target object.</param>
    /// <param name="value">Bottom offset in DIPs, or <see cref="float.NaN"/> to unset.</param>
    public static void SetBottom(IPropertyOwner properties, float value)
    {
        ExceptionExtensions.ThrowIfNull(properties, nameof(properties));
        if (value.IsNotSet())
        {
            properties.ResetPropertyValue(BottomProperty, out _);
            return;
        }
        properties.SetPropertyValue(BottomProperty, value);
    }

    /// <summary>
    /// Gets the attached Bottom offset (DIPs). Returns <see cref="float.NaN"/> when not set.
    /// </summary>
    /// <param name="properties">The target object.</param>
    /// <returns>The bottom offset or <see cref="float.NaN"/>.</returns>
    public static float GetBottom(IPropertyOwner properties)
    {
        ExceptionExtensions.ThrowIfNull(properties, nameof(properties));
        return (float)properties.GetPropertyValue(BottomProperty)!;
    }
}
