namespace Wice;

/// <summary>
/// Renders an optional background, a border (with optional corner radii), and hosts a single child visual.
/// </summary>
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
    public static VisualProperty CornerRadiusProperty { get; } = VisualProperty.Add(typeof(RoundedRectangle), nameof(CornerRadius), VisualPropertyInvalidateModes.Render, new Vector2(), ValidateEmptyVector2);

    /// <inheritdoc/>
    protected override bool FallbackToTransparentBackground => true;

    /// <summary>
    /// Gets or sets the corner radius used when rendering the background and border.
    /// </summary>
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
    [Category(CategoryRender)]
    public float BorderThickness { get => (float)GetPropertyValue(BorderThicknessProperty)!; set => SetPropertyValue(BorderThicknessProperty, value); }

    /// <summary>
    /// Gets or sets the single child of this border.
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

    /// <inheritdoc/>
    protected override BaseObjectCollection<Visual> CreateChildren() => new(1);

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    protected override void RenderD2DSurface(SurfaceCreationOptions? creationOptions = null, RECT? rect = null)
    {
        // using D2D cancels Windows.UI.Composition animations, so avoid when possible...
        // note however that this means we currently don't support animations on borders with cornerradius or borderthickness set
        if (BorderThickness.ToZero() == 0 && CornerRadius.IsZero())
            return;

        base.RenderD2DSurface(creationOptions, rect);
    }

    /// <inheritdoc/>
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
