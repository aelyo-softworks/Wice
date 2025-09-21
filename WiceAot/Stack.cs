namespace Wice;

/// <summary>
/// Stacks child visuals in a single row or column.
/// </summary>
public partial class Stack : Visual
{
    /// <summary>
    /// Attached property backing <see cref="Orientation"/>.
    /// Default: <see cref="Orientation.Vertical"/>. Triggers a new measure pass on change.
    /// </summary>
    public static VisualProperty OrientationProperty { get; } = VisualProperty.Add(typeof(Stack), nameof(Orientation), VisualPropertyInvalidateModes.Measure, Orientation.Vertical);

    /// <summary>
    /// Attached property backing <see cref="LinesSize"/>. Default: 0.
    /// </summary>
    public static VisualProperty LinesSizeProperty { get; } = VisualProperty.Add(typeof(Stack), nameof(LinesSize), VisualPropertyInvalidateModes.Measure, 0f);

    /// <summary>
    /// Attached property backing <see cref="LastChildFill"/>.
    /// Default: true. Triggers a new measure pass on change.
    /// </summary>
    public static VisualProperty LastChildFillProperty { get; } = VisualProperty.Add(typeof(Stack), nameof(LastChildFill), VisualPropertyInvalidateModes.Measure, true);

    /// <summary>
    /// Attached property backing <see cref="Spacing"/>.
    /// Default: (0,0). Triggers a new measure pass on change.
    /// </summary>
    public static VisualProperty SpacingProperty { get; } = VisualProperty.Add(typeof(Stack), nameof(Spacing), VisualPropertyInvalidateModes.Measure, new D2D_SIZE_F());

    /// <summary>
    /// Gets or sets the stacking direction.
    /// </summary>
    [Category(CategoryLayout)]
    public Orientation Orientation { get => (Orientation)GetPropertyValue(OrientationProperty)!; set => SetPropertyValue(OrientationProperty, value); }

    /// <summary>
    /// Gets or sets a fixed line size used by some stacking scenarios.
    /// </summary>
    [Category(CategoryLayout)]
    public float LinesSize { get => (float)GetPropertyValue(LinesSizeProperty)!; set => SetPropertyValue(LinesSizeProperty, value); }

    /// <summary>
    /// Gets or sets the spacing around and between children.
    /// </summary>
    [Category(CategoryLayout)]
    public D2D_SIZE_F Spacing { get => (D2D_SIZE_F)GetPropertyValue(SpacingProperty)!; set => SetPropertyValue(SpacingProperty, value); }

    /// <summary>
    /// Gets or sets whether the last child can expand to fill remaining space along the stacking axis.
    /// </summary>
    [Category(CategoryLayout)]
    public bool LastChildFill { get => (bool)GetPropertyValue(LastChildFillProperty)!; set => SetPropertyValue(LastChildFillProperty, value); }

    /// <summary>
    /// Measures the aggregate desired size of children arranged in a stack, including spacing.
    /// </summary>
    /// <param name="constraint">The available size including margins.</param>
    /// <returns>The desired size of the stack including spacing, clamped to valid numeric ranges.</returns>
    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
    {
        var width = 0f;
        var height = 0f;

        var sizeConstraint = constraint;
        var children = VisibleChildren.ToArray();
        foreach (var child in children.Where(c => c.Parent != null))
        {
            child.Measure(sizeConstraint);
            var childSize = child.DesiredSize;

            // Use child desired size as input for parent-based rect computation when non-zero.
            var parentSize = sizeConstraint;
            if (childSize.width != 0)
            {
                parentSize.width = childSize.width;
            }

            if (childSize.height != 0)
            {
                parentSize.height = childSize.height;
            }

            var childRect = Canvas.GetRect(parentSize, child);

            if (Orientation == Orientation.Horizontal)
            {
                // Stack accumulates width; height is the max of children's bottom edges.
                width += childRect.Width;
                height = Math.Max(height, childRect.bottom);
                sizeConstraint.width = Math.Max(0, sizeConstraint.width - width);
            }
            else
            {
                // Stack accumulates height; width is the max of children's right edges.
                height += childRect.Height;
                width = Math.Max(width, childRect.right);
                sizeConstraint.height = Math.Max(0, sizeConstraint.height - height);
            }
        }

        // Apply spacing (outer padding and inter-item gaps).
        if (children.Length > 0)
        {
            var spacing = Spacing;
            if (Orientation == Orientation.Horizontal)
            {
                width += spacing.width * (children.Length + 1);
                height += spacing.height * 2;
            }
            else
            {
                height += spacing.height * (children.Length + 1);
                // Note: width side-padding uses spacing.height in this implementation.
                width += spacing.height * 2;
            }
        }

        return new D2D_SIZE_F(width.ClampMinMax(), height.ClampMinMax());
    }

    /// <summary>
    /// Arranges children sequentially along the stacking axis, applying spacing and optional last-child fill.
    /// </summary>
    /// <param name="finalRect">Final rectangle available for content (excluding margins).</param>
    protected override void ArrangeCore(D2D_RECT_F finalRect)
    {
        var position = 0f;
        var finalSize = finalRect.Size;
        var children = VisibleChildren.ToArray();
        var lastChildFill = LastChildFill;
        var noFillCount = children.Length - (lastChildFill ? 1 : 0);
        var spacing = Spacing;
        for (var i = 0; i < children.Length; i++)
        {
            if (position.IsMinOrMax())
                break;

            var child = children[i];
            if (child.Parent == null)
                continue;

            var rc = Canvas.GetRect(finalSize, child);
            D2D_RECT_F childRect;
            if (Orientation == Orientation.Horizontal)
            {
                // Non-last children with zero desired width are skipped.
                if (i < noFillCount)
                {
                    if (child.DesiredSize.width == 0)
                        continue;
                }

                childRect = D2D_RECT_F.Sized(position, rc.top, rc.Width, rc.Height);
                // Horizontal gaps: before first and between children; vertical padding once per side.
                childRect.left += spacing.width * (i + 1);
                childRect.right += spacing.width * (i + 1);
                childRect.top += spacing.height;
                childRect.bottom -= spacing.height;

                position += rc.Width;
                finalSize.width = Math.Max(0, finalSize.width - rc.Width);
            }
            else
            {
                // Non-last children with zero desired height are skipped.
                if (i < noFillCount)
                {
                    if (child.DesiredSize.height == 0)
                        continue;
                }

                childRect = D2D_RECT_F.Sized(rc.left, position, rc.Width, rc.Height);
                // Vertical gaps: before first and between children; horizontal padding once per side.
                childRect.top += spacing.height * (i + 1);
                childRect.bottom += spacing.height * (i + 1);
                childRect.left += spacing.width;
                childRect.right -= spacing.width;

                position += rc.Height;
                finalSize.height = Math.Max(0, finalSize.height - rc.Height);
            }

            child.Arrange(childRect);
        }
    }
}
