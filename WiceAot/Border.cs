namespace Wice;

/// <summary>
/// Renders an optional background, a border (with optional corner radii), and hosts a single child visual.
/// </summary>
/// <remarks>
/// Notes:
/// - For border thickness rendering quality, it can be visually better to compose two borders rather than using a single
///   border with a non-zero <see cref="BorderThickness"/> (sub-pixel stroking differences can show when magnified).
/// - Corner clipping is handled via Windows.UI.Composition geometric clips, while drawing is done via Direct2D; their
///   rounded corner interpretations might not perfectly match.
/// - This type differs from WPF's <c>Border</c>; it targets Windows composition directly.
/// </remarks>
public partial class Border : RenderVisual, IOneChildParent
{
    /// <summary>
    /// Identifies the <see cref="BorderBrush"/> property.
    /// </summary>
    public static VisualProperty BorderBrushProperty { get; } = VisualProperty.Add<Brush>(typeof(Border), nameof(BorderBrush), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Identifies the <see cref="BorderThickness"/> property.
    /// </summary>
    public static VisualProperty BorderThicknessProperty { get; } = VisualProperty.Add(typeof(Border), nameof(BorderThickness), VisualPropertyInvalidateModes.Measure, 0f);

    /// <summary>
    /// Identifies the <see cref="CornerRadius"/> property.
    /// </summary>
    /// <remarks>
    /// The value is a <see cref="Vector2"/> where X/Y describe the radii in DIPs applied to all corners.
    /// </remarks>
    public static VisualProperty CornerRadiusProperty { get; } = VisualProperty.Add(typeof(RoundedRectangle), nameof(CornerRadius), VisualPropertyInvalidateModes.Render, new Vector2(), ValidateEmptyVector2);

    /// <inheritdoc/>
    protected override bool FallbackToTransparentBackground => true;

    /// <summary>
    /// Gets or sets the corner radius used when rendering the background and border.
    /// </summary>
    /// <remarks>
    /// This is the composition (Windows.UI.Composition) corner radius, not the WPF one.
    /// A non-zero radius enables rounded clipping when <see cref="Visual.ClipChildren"/> is true and
    /// rounded rectangle drawing for background/border in Direct2D.
    /// </remarks>
    [Category(CategoryRender)]
    public Vector2 CornerRadius { get => (Vector2)GetPropertyValue(CornerRadiusProperty)!; set => SetPropertyValue(CornerRadiusProperty, value); }

    /// <summary>
    /// Gets or sets the brush used to stroke the border.
    /// </summary>
    [Category(CategoryRender)]
    public Brush BorderBrush { get => (Brush)GetPropertyValue(BorderBrushProperty)!; set => SetPropertyValue(BorderBrushProperty, value); }

    /// <summary>
    /// Gets or sets the border thickness in DIPs.
    /// </summary>
    /// <remarks>
    /// A value greater than 0 will trigger Direct2D drawing for the border which disables Windows.UI.Composition animations
    /// on this visual while active (see <see cref="RenderD2DSurface"/>).
    /// </remarks>
    [Category(CategoryRender)]
    public float BorderThickness { get => (float)GetPropertyValue(BorderThicknessProperty)!; set => SetPropertyValue(BorderThicknessProperty, value); }

    /// <summary>
    /// Gets or sets the single child of this border.
    /// </summary>
    /// <remarks>
    /// Setting a new child replaces the previous one. The child is measured/arranged within the inner
    /// content rectangle defined by <see cref="Visual.Padding"/> minus <see cref="BorderThickness"/>.
    /// </remarks>
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
    /// Creates the children collection with a capacity of 1 (single child parent).
    /// </summary>
    protected override BaseObjectCollection<Visual> CreateChildren() => new(1);

    /// <summary>
    /// Measures the border and its child, accounting for padding and border thickness.
    /// </summary>
    /// <param name="constraint">The available size including this visual's margin.</param>
    /// <returns>The desired size excluding margin.</returns>
    /// <remarks>
    /// - The inner constraint passed to the child is reduced by <see cref="Visual.Padding"/> minus <see cref="BorderThickness"/>.
    /// - The desired size re-adds those paddings after measuring the child.
    /// </remarks>
    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
    {
        var padding = Padding - BorderThickness.ToZero();

        var leftPadding = padding.left.IsSet() && padding.left > 0;
        if (leftPadding && constraint.width.IsSet())
        {
            constraint.width = Math.Max(0, constraint.width - padding.left);
        }

        var topPadding = padding.top.IsSet() && padding.top > 0;
        if (topPadding && constraint.height.IsSet())
        {
            constraint.height = Math.Max(0, constraint.height - padding.top);
        }

        var rightPadding = padding.right.IsSet() && padding.right > 0;
        if (rightPadding && constraint.width.IsSet())
        {
            constraint.width = Math.Max(0, constraint.width - padding.right);
        }

        var bottomPadding = padding.bottom.IsSet() && padding.bottom > 0;
        if (bottomPadding && constraint.height.IsSet())
        {
            constraint.height = Math.Max(0, constraint.height - padding.bottom);
        }

        var width = 0f;
        var height = 0f;

        var child = Child;
        if (child != null)
        {
            child.Measure(constraint);
            var size = child.DesiredSize;
            width = size.width;
            height = size.height;
        }

        if (leftPadding)
        {
            width += padding.left;
        }

        if (topPadding)
        {
            height += padding.top;
        }

        if (rightPadding)
        {
            width += padding.right;
        }

        if (bottomPadding)
        {
            height += padding.bottom;
        }

        return new D2D_SIZE_F(width, height);
    }

    /// <summary>
    /// Arranges the child within the final rectangle, honoring padding and border thickness.
    /// </summary>
    /// <param name="finalRect">The final rectangle excluding margin.</param>
    protected override void ArrangeCore(D2D_RECT_F finalRect)
    {
        var child = Child;
        if (child != null)
        {
            var padding = Padding - BorderThickness.ToZero();
            var rc = new D2D_RECT_F();
            if (padding.left.IsSet() && padding.left > 0)
            {
                rc.left = padding.left;
            }

            if (padding.top.IsSet() && padding.top > 0)
            {
                rc.top = padding.top;
            }

            if (padding.right.IsSet() && padding.right > 0)
            {
                rc.Width = Math.Max(0, finalRect.Width - padding.right - rc.left);
            }
            else
            {
                rc.Width = finalRect.Width;
            }

            if (padding.bottom.IsSet() && padding.bottom > 0)
            {
                rc.Height = Math.Max(0, finalRect.Height - padding.bottom - rc.top);
            }
            else
            {
                rc.Height = finalRect.Height;
            }

            var rc2 = Canvas.GetRect(rc.Size, child);
            child.Arrange(new D2D_RECT_F(rc.left + rc2.left, rc.top + rc2.top, rc2.Size));
        }
    }

    /// <summary>
    /// Updates composition state and applies rounded clipping when <see cref="Visual.ClipChildren"/> is enabled.
    /// </summary>
    /// <remarks>
    /// Rounded clipping uses composition geometry. Due to differences with Direct2D, edges may not match pixel-perfectly
    /// with the drawn rounded background/border.
    /// </remarks>
    protected override void Render()
    {
        base.Render();
        if (ClipChildren)
        {
            // use a rounded clip
            var radius = CornerRadius;
            if (radius.IsNotZero() && Compositor != null && CompositionVisual != null)
            {
                // note this is not visually perfect as clipping is done by Windows.UI.Composition
                // while rendering is done by D2D and both do not 100% agree on what's a radius
                var geometry = Compositor.CreateRoundedRectangleGeometry();
                geometry.CornerRadius = radius;
                geometry.Size = RenderSize.ToVector2();
                CompositionVisual.Clip = Compositor.CreateGeometricClip(geometry);
            }
        }
    }

    /// <summary>
    /// Renders the background. When <see cref="CornerRadius"/> is non-zero, draws a rounded rectangle background.
    /// </summary>
    /// <param name="context">The current render context.</param>
    protected override void RenderBackgroundCore(RenderContext context)
    {
        var radius = CornerRadius;
        if (radius.IsNotZero())
        {
            // draw rounded background
            context.DeviceContext.Clear(D3DCOLORVALUE.Transparent);
            var bg = BackgroundColor;
            if (bg.HasValue)
            {
                var borderThickness = BorderThickness.ToZero();
                var rc = new D2D_RECT_F(RenderSize);
                rc = rc.Deflate(borderThickness / 2);
                var rr = new D2D1_ROUNDED_RECT
                {
                    rect = rc,
                    radiusX = radius.X.ToZero(),
                    radiusY = radius.Y.ToZero()
                };
                context.DeviceContext.Object.FillRoundedRectangle(rr, context.CreateSolidColorBrush(bg.Value).Object);
            }
            return;
        }
        base.RenderBackgroundCore(context);
    }

    /// <summary>
    /// Ensures a D2D surface exists only when needed (rounded corners or non-zero border thickness).
    /// </summary>
    /// <param name="creationOptions">Optional surface creation options.</param>
    /// <param name="rect">Optional sub-rect to render.</param>
    /// <remarks>
    /// Using D2D cancels Windows.UI.Composition animations; we avoid creating a D2D surface when possible.
    /// </remarks>
    protected override void RenderD2DSurface(SurfaceCreationOptions? creationOptions = null, RECT? rect = null)
    {
        // using D2D cancels Windows.UI.Composition animations, so avoid when possible...
        // note however that this means we currently don't support animations on borders with cornerradius or borderthickness set
        if (BorderThickness.ToZero() == 0 && CornerRadius.IsZero())
            return;

        base.RenderD2DSurface(creationOptions, rect);
    }

    /// <summary>
    /// Draws the border (rounded or rectangular) using Direct2D when <see cref="BorderThickness"/> is greater than zero.
    /// </summary>
    /// <param name="context">The render context.</param>
    protected internal override void RenderCore(RenderContext context)
    {
        base.RenderCore(context);
        if (CompositionVisual == null || !CompositionVisual.IsVisible)
            return;

        // draw border
        var borderThickness = BorderThickness.ToZero();
        if (borderThickness > 0)
        {
            var brush = BorderBrush?.GetBrush(context);
            if (brush != null)
            {
                var rc = new D2D_RECT_F(RenderSize);
                rc = rc.Deflate(borderThickness / 2);

                // rounded border?
                var radius = CornerRadius;
                if (radius.IsNotZero())
                {
                    var rr = new D2D1_ROUNDED_RECT
                    {
                        rect = rc,
                        radiusX = radius.X.ToZero(),
                        radiusY = radius.Y.ToZero()
                    };
                    context.DeviceContext.Object.DrawRoundedRectangle(rr, brush.Object, borderThickness, null);
                }
                else
                {
                    context.DeviceContext.Object.DrawRectangle(rc, brush.Object, borderThickness, null);
                }
            }
        }
    }
}
