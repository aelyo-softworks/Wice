namespace Wice;

public class Viewer : Visual, IOneChildParent
{
    public static VisualProperty IsWidthUnconstrainedProperty { get; } = VisualProperty.Add(typeof(Viewer), nameof(IsWidthUnconstrained), VisualPropertyInvalidateModes.Measure, true);
    public static VisualProperty IsHeightUnconstrainedProperty { get; } = VisualProperty.Add(typeof(Viewer), nameof(IsHeightUnconstrained), VisualPropertyInvalidateModes.Measure, true);
    public static VisualProperty KeepProportionsProperty { get; } = VisualProperty.Add(typeof(Viewer), nameof(KeepProportions), VisualPropertyInvalidateModes.Measure, false);

    protected override BaseObjectCollection<Visual> CreateChildren() => new(1);

    [Browsable(false)]
    public Visual? Child
    {
        get => Children?.FirstOrDefault();
        set
        {
            var child = Child;
            if (child == value)
                return;

            if (child != null)
            {
                Children.Remove(child);
            }
            Children.Add(value);
        }
    }

    [Category(CategoryLayout)]
    public bool IsWidthUnconstrained { get => (bool)GetPropertyValue(IsWidthUnconstrainedProperty)!; set => SetPropertyValue(IsWidthUnconstrainedProperty, value); }

    [Category(CategoryLayout)]
    public bool IsHeightUnconstrained { get => (bool)GetPropertyValue(IsHeightUnconstrainedProperty)!; set => SetPropertyValue(IsHeightUnconstrainedProperty, value); }

    [Category(CategoryLayout)]
    public bool KeepProportions { get => (bool)GetPropertyValue(KeepProportionsProperty)!; set => SetPropertyValue(KeepProportionsProperty, value); }

    [Category(CategoryLayout)]
    public virtual float ChildOffsetLeft { get; set; }

    [Category(CategoryLayout)]
    public virtual float ChildOffsetTop { get; set; }

    [Category(CategoryLayout)]
    public float BaseChildOffsetLeft { get; private set; }

    [Category(CategoryLayout)]
    public float BaseChildOffsetTop { get; private set; }

    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
    {
        var child = Child;
        if (child != null)
        {
            var childConstraint = D2D_SIZE_F.PositiveInfinity;
            if (!IsWidthUnconstrained)
            {
                childConstraint.width = constraint.width;
            }

            if (!IsHeightUnconstrained)
            {
                childConstraint.height = constraint.height;
            }

            child.Measure(childConstraint);
            return child.DesiredSize;
        }

        return base.MeasureCore(constraint);
    }

    protected override void ArrangeCore(D2D_RECT_F finalRect)
    {
        var child = Child;
        if (child != null)
        {
            var w = IsWidthUnconstrained;
            var h = IsHeightUnconstrained;
            D2D_RECT_F rc;
            if (!w && !h)
            {
                rc = finalRect;
            }
            else if (!w)
            {
                var options = GetRectOptions.StretchWidthToParent;
                if (KeepProportions)
                {
                    options |= GetRectOptions.KeepProportions;
                }
                rc = Canvas.GetRect(finalRect.Size, child, options);
            }
            else if (!h)
            {
                var options = GetRectOptions.StretchHeightToParent;
                if (KeepProportions)
                {
                    options |= GetRectOptions.KeepProportions;
                }
                rc = Canvas.GetRect(finalRect.Size, child, options);
            }
            else // w && h
            {
                rc = Canvas.GetRect(finalRect.Size, child, KeepProportions ? GetRectOptions.KeepProportions : GetRectOptions.None);
            }

            BaseChildOffsetLeft = rc.left;
            BaseChildOffsetTop = rc.top;
            rc.Move(new D2D_VECTOR_2F(ChildOffsetLeft, ChildOffsetTop));
            child.Arrange(rc);
        }
    }
}
