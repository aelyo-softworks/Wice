namespace Wice;

public partial class Stack : Visual
{
    public static VisualProperty OrientationProperty { get; } = VisualProperty.Add(typeof(Stack), nameof(Orientation), VisualPropertyInvalidateModes.Measure, Orientation.Vertical);
    public static VisualProperty LinesSizeProperty { get; } = VisualProperty.Add(typeof(Stack), nameof(LinesSize), VisualPropertyInvalidateModes.Measure, 0f);
    public static VisualProperty LastChildFillProperty { get; } = VisualProperty.Add(typeof(Stack), nameof(LastChildFill), VisualPropertyInvalidateModes.Measure, true);

    [Category(CategoryLayout)]
    public Orientation Orientation { get => (Orientation)GetPropertyValue(OrientationProperty)!; set => SetPropertyValue(OrientationProperty, value); }

    [Category(CategoryLayout)]
    public float LinesSize { get => (float)GetPropertyValue(LinesSizeProperty)!; set => SetPropertyValue(LinesSizeProperty, value); }

    // stupidly doesn't exist in WPF...
    [Category(CategoryLayout)]
    public bool LastChildFill { get => (bool)GetPropertyValue(LastChildFillProperty)!; set => SetPropertyValue(LastChildFillProperty, value); }

    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
    {
        var width = 0f;
        var height = 0f;

        var sizeConstraint = constraint;
        var children = VisibleChildren.ToArray();
        foreach (var child in children)
        {
            child.Measure(sizeConstraint);
            var childSize = child.DesiredSize;

            // use child size as parent size
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
                width += childRect.Width; // we don't use horizontal position (since we stack)
                height = Math.Max(height, childRect.bottom);
                sizeConstraint.width = Math.Max(0, sizeConstraint.width - width);
            }
            else
            {
                height += childRect.Height; // we don't use vertical position (since we stack)
                width = Math.Max(width, childRect.right);
                sizeConstraint.height = Math.Max(0, sizeConstraint.height - height);
            }
        }

        return new D2D_SIZE_F(width.ClampMinMax(), height.ClampMinMax());
    }

    protected override void ArrangeCore(D2D_RECT_F finalRect)
    {
        var position = 0f;
        var finalSize = finalRect.Size;
        var children = VisibleChildren.ToArray();
        var lastChildFill = LastChildFill;
        var noFillCount = children.Length - (lastChildFill ? 1 : 0);
        for (var i = 0; i < children.Length; i++)
        {
            if (position.IsMinOrMax())
                break;

            var child = children[i];
            var rc = Canvas.GetRect(finalSize, child);
            D2D_RECT_F childRect;
            if (Orientation == Orientation.Horizontal)
            {
                if (i < noFillCount)
                {
                    if (child.DesiredSize.width == 0)
                        continue;
                }

                childRect = D2D_RECT_F.Sized(position, rc.top, rc.Width, rc.Height);
                position += rc.Width;
                finalSize.width = Math.Max(0, finalSize.width - rc.Width);
            }
            else
            {
                if (i < noFillCount)
                {
                    if (child.DesiredSize.height == 0)
                        continue;
                }

                childRect = D2D_RECT_F.Sized(rc.left, position, rc.Width, rc.Height);
                position += rc.Height;
                finalSize.height = Math.Max(0, finalSize.height - rc.Height);
            }

            child.Arrange(childRect);
        }
    }
}
