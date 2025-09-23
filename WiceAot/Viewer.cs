namespace Wice;

/// <summary>
/// Hosts a single child and positions it within the available space with optional constraints.
/// Supports:
/// - Unconstrained width/height for the child (content can grow beyond parent on the selected axis).
/// - Optional aspect ratio preservation when fitting/stretching the child.
/// - Manual panning offsets (<see cref="ChildOffsetLeft"/>/<see cref="ChildOffsetTop"/>).
/// </summary>
public partial class Viewer : Visual, IOneChildParent
{
    /// <summary>
    /// When <see langword="true"/>, the child is measured and arranged without width constraint from the parent.
    /// Changes trigger a new measure pass.
    /// Default: true.
    /// </summary>
    public static VisualProperty IsWidthUnconstrainedProperty { get; } = VisualProperty.Add(typeof(Viewer), nameof(IsWidthUnconstrained), VisualPropertyInvalidateModes.Measure, true);

    /// <summary>
    /// When <see langword="true"/>, the child is measured and arranged without height constraint from the parent.
    /// Changes trigger a new measure pass.
    /// Default: true.
    /// </summary>
    public static VisualProperty IsHeightUnconstrainedProperty { get; } = VisualProperty.Add(typeof(Viewer), nameof(IsHeightUnconstrained), VisualPropertyInvalidateModes.Measure, true);

    /// <summary>
    /// When <see langword="true"/>, preserves the child's aspect ratio while fitting/stretching into the parent rect.
    /// Only applies when at least one axis is constrained by the parent.
    /// Changes trigger a new measure pass.
    /// Default: false.
    /// </summary>
    public static VisualProperty KeepProportionsProperty { get; } = VisualProperty.Add(typeof(Viewer), nameof(KeepProportions), VisualPropertyInvalidateModes.Measure, false);

    /// <inheritdoc/>
    protected override BaseObjectCollection<Visual> CreateChildren() => new(1);

    private float _childOffsetLeft;
    private float _childOffsetTop;

    /// <summary>
    /// Gets or sets the single child of this viewer.
    /// Backed by <see cref="Visual.Children"/>; setting a new child replaces the existing one.
    /// </summary>
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

            if (value != null)
            {
                Children.Add(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the child is measured/arranged with an unconstrained width.
    /// When <see langword="true"/>, the child's width is not limited by the available width.
    /// </summary>
    [Category(CategoryLayout)]
    public bool IsWidthUnconstrained { get => (bool)GetPropertyValue(IsWidthUnconstrainedProperty)!; set => SetPropertyValue(IsWidthUnconstrainedProperty, value); }

    /// <summary>
    /// Gets or sets whether the child is measured/arranged with an unconstrained height.
    /// When <see langword="true"/>, the child's height is not limited by the available height.
    /// </summary>
    [Category(CategoryLayout)]
    public bool IsHeightUnconstrained { get => (bool)GetPropertyValue(IsHeightUnconstrainedProperty)!; set => SetPropertyValue(IsHeightUnconstrainedProperty, value); }

    /// <summary>
    /// Gets or sets whether the child's aspect ratio is preserved when stretched to the parent.
    /// Effective only when at least one axis (width/height) is constrained by the parent.
    /// </summary>
    [Category(CategoryLayout)]
    public bool KeepProportions { get => (bool)GetPropertyValue(KeepProportionsProperty)!; set => SetPropertyValue(KeepProportionsProperty, value); }

    /// <summary>
    /// Gets or sets an additional horizontal offset (in DIPs) applied to the arranged child rect.
    /// Useful to pan content when one or both axes are unconstrained.
    /// Triggers a child measure invalidation when changed.
    /// </summary>
    [Category(CategoryLayout)]
    public virtual float ChildOffsetLeft
    {
        get => _childOffsetLeft;
        set
        {
            if (_childOffsetLeft != value)
            {
                _childOffsetLeft = value;
                OnPropertyChanged();
                Child?.Invalidate(VisualPropertyInvalidateModes.Measure);
            }
        }
    }

    /// <summary>
    /// Gets or sets an additional vertical offset (in DIPs) applied to the arranged child rect.
    /// Useful to pan content when one or both axes are unconstrained.
    /// Triggers a child measure invalidation when changed.
    /// </summary>
    [Category(CategoryLayout)]
    public virtual float ChildOffsetTop
    {
        get => _childOffsetTop;
        set
        {
            if (_childOffsetTop != value)
            {
                _childOffsetTop = value;
                OnPropertyChanged();
                Child?.Invalidate(VisualPropertyInvalidateModes.Measure);
            }
        }
    }

    /// <summary>
    /// Gets the base left offset computed during arrange before applying <see cref="ChildOffsetLeft"/>.
    /// This is the offset returned by rect computation (e.g., <c>Canvas.GetRect</c>).
    /// </summary>
    [Category(CategoryLayout)]
    public float BaseChildOffsetLeft { get; private set; }

    /// <summary>
    /// Gets the base top offset computed during arrange before applying <see cref="ChildOffsetTop"/>.
    /// This is the offset returned by rect computation (e.g., <c>Canvas.GetRect</c>).
    /// </summary>
    [Category(CategoryLayout)]
    public float BaseChildOffsetTop { get; private set; }

    /// <inheritdoc/>
    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
    {
        var child = Child;
        if (child != null)
        {
            // Start unconstrained, then apply constraints only on the axes that are not flagged as unconstrained.
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

    /// <inheritdoc/>
    protected override void ArrangeCore(D2D_RECT_F finalRect)
    {
        var child = Child;
        if (child != null)
        {
            var w = IsWidthUnconstrained;
            var h = IsHeightUnconstrained;
            D2D_RECT_F rc;

            // Both axes constrained: child fills the final rect.
            if (!w && !h)
            {
                rc = finalRect;
            }
            // Width constrained only.
            else if (!w)
            {
                var options = GetRectOptions.StretchWidthToParent;
                if (KeepProportions)
                {
                    options |= GetRectOptions.KeepProportions;
                }
                rc = Canvas.GetRect(finalRect.Size, child, options);
            }
            // Height constrained only.
            else if (!h)
            {
                var options = GetRectOptions.StretchHeightToParent;
                if (KeepProportions)
                {
                    options |= GetRectOptions.KeepProportions;
                }
                rc = Canvas.GetRect(finalRect.Size, child, options);
            }
            // Both axes unconstrained.
            else
            {
                rc = Canvas.GetRect(finalRect.Size, child, KeepProportions ? GetRectOptions.KeepProportions : GetRectOptions.None);
            }

            // Keep track of the computed base offsets prior to manual panning.
            BaseChildOffsetLeft = rc.left;
            BaseChildOffsetTop = rc.top;

            // Apply manual offsets for panning.
            rc.Move(new D2D_VECTOR_2F(ChildOffsetLeft, ChildOffsetTop));
            child.Arrange(rc);
        }
    }
}
