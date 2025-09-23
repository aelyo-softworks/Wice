namespace Wice;

/// <summary>
/// Base visual that renders into a Direct2D-backed <see cref="SpriteVisual"/> surface.
/// Adds background clearing, optional transparent fallback, and handles extremely large surfaces
/// by clamping the composition bitmap size and applying a device-context transform when needed.
/// </summary>
public abstract class RenderVisual : Visual
{
    /// <summary>
    /// Attached property that stores the background color filled in <see cref="RenderBackgroundCore(RenderContext)"/>.
    /// Changing this property invalidates the render pass.
    /// </summary>
    public static VisualProperty BackgroundColorProperty { get; } = VisualProperty.Add<D3DCOLORVALUE?>(typeof(RenderVisual), nameof(BackgroundColor), VisualPropertyInvalidateModes.Render);

    private float? _widthMaxed;
    private float? _heightMaxed;

    /// <summary>
    /// Initializes a new instance of <see cref="RenderVisual"/>.
    /// </summary>
    protected RenderVisual()
    {
    }

    /// <summary>
    /// Optional Direct2D render rectangle to use when creating the drawing surface.
    /// Derived classes can override to restrict/expand the painted area.
    /// </summary>
    protected virtual RECT? Direct2DRenderRect => null;

    /// <summary>
    /// Gets a value indicating whether this visual should render during the pass.
    /// Derived classes can override to skip drawing under specific conditions.
    /// </summary>
    protected virtual bool ShouldRender => true;

    /// <summary>
    /// Gets a value indicating whether, in absence of a <see cref="BackgroundColor"/>, the background should be cleared to transparent.
    /// </summary>
    protected virtual bool FallbackToTransparentBackground => false;

    /// <summary>
    /// Gets a value indicating whether device-context rendering should snap to pixels.
    /// Provided for derived classes; not directly used here.
    /// </summary>
    protected virtual bool RenderSnapToPixels => true;

    /// <summary>
    /// Gets a value indicating whether to apply a translation transform on the D2D device context when the composition visual was clamped.
    /// </summary>
    protected virtual bool TransformMaxed => true;

    /// <summary>
    /// Gets the X translation used when the composition visual has been clamped due to exceeding maximum surface size.
    /// Non-null means clamping occurred and translation must be applied during rendering.
    /// </summary>
    protected float? CompositionWidthMaxed => _widthMaxed;

    /// <summary>
    /// Gets the Y translation used when the composition visual has been clamped due to exceeding maximum surface size.
    /// Non-null means clamping occurred and translation must be applied during rendering.
    /// </summary>
    protected float? CompositionHeightMaxed => _heightMaxed;

    /// <summary>
    /// Gets or sets the background color cleared at the start of the D2D draw pass.
    /// When set, the device context is cleared using this color in <see cref="RenderBackgroundCore(RenderContext)"/>.
    /// </summary>
    [Category(CategoryRender)]
    public D3DCOLORVALUE? BackgroundColor { get => (D3DCOLORVALUE?)GetPropertyValue(BackgroundColorProperty); set => SetPropertyValue(BackgroundColorProperty, value); }

    /// <summary>
    /// Gets the background color applied to ascendant elements in the rendering hierarchy.
    /// </summary>
    [Category(CategoryRender)]
    public D3DCOLORVALUE? AscendantsBackgroundColor => GetAscendantsBackgroundColor(this);

    private static D3DCOLORVALUE? GetAscendantsBackgroundColor(Visual? visual)
    {
        if (visual == null)
            return null;

        if (visual is RenderVisual rv && rv.BackgroundColor.HasValue)
            return rv.BackgroundColor.Value;

        if (visual.RenderBrush is CompositionColorBrush colorBrush)
            return colorBrush.Color.ToColor();

        if (visual.RenderBrush is CompositionEffectBrush effectBrush)
        {
            var color = AcrylicBrush.GetTintColor(effectBrush);
            if (color.HasValue)
                return color.Value;
        }

        return GetAscendantsBackgroundColor(visual.Parent);
    }

    /// <inheritdoc/>
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (property == RenderBrushProperty)
        {
            var brush = (CompositionBrush)value!;
            if (CompositionObjectEqualityComparer.Default.Equals(brush, RenderBrush))
                return false;

            if (value != null)
            {
                if (value is not CompositionColorBrush colorBrush)
                    throw new NotSupportedException();

                BackgroundColor = colorBrush.Color.ToColor();
                return base.SetPropertyValue(property, value, options);
            } // else continue
        }

        return base.SetPropertyValue(property, value, options);
    }

    /// <inheritdoc/>
    protected override void Render()
    {
        _widthMaxed = null;
        _heightMaxed = null;

        base.Render();
        RenderD2DSurface(null, null);
    }

    /// <summary>
    /// Draws the content on a Direct2D surface associated with the underlying <see cref="SpriteVisual"/>.
    /// Applies an extra translation transform when the composition surface size is clamped.
    /// </summary>
    /// <param name="creationOptions">Optional surface creation options (e.g., pixel format).</param>
    /// <param name="rect">Optional drawing rectangle. If null, the full visual rect is used.</param>
    protected virtual void RenderD2DSurface(SurfaceCreationOptions? creationOptions = null, RECT? rect = null) => RenderD2DSurface(RenderCore, creationOptions, rect);

    /// <summary>
    /// Draws the content on a Direct2D surface associated with the underlying <see cref="SpriteVisual"/>.
    /// Applies an extra translation transform when the composition surface size is clamped.
    /// </summary>
    /// <param name="action">The action to perform on the render context. This should contain the actual drawing logic.</param>
    /// <param name="creationOptions">Optional surface creation options (e.g., pixel format).</param>
    /// <param name="rect">Optional drawing rectangle. If null, the full visual rect is used.</param>
    protected virtual void RenderD2DSurface(Action<RenderContext> action, SurfaceCreationOptions? creationOptions = null, RECT? rect = null)
    {
        ExceptionExtensions.ThrowIfNull(action, nameof(action));
        if (CompositionVisual is not SpriteVisual visual)
            return;

        if (!CompositionVisual.IsVisible)
            return;

        if (SuspendedCompositionParts.HasFlag(CompositionUpdateParts.D2DSurface))
            return;

        var win = Window;
        if (win == null || !ShouldRender)
            return;

        visual.DrawOnSurface(win.CompositionDevice, dc => RenderContext.WithRenderContext(dc, rc =>
        {
            var transform = dc.GetTransform();
            if (TransformMaxed && _widthMaxed.HasValue && _heightMaxed.HasValue)
            {
                dc.SetTransform(transform * D2D_MATRIX_3X2_F.Translation(_widthMaxed.Value, _heightMaxed.Value));
            }

            Parent?.BeforeRenderChildCore(rc, this);
            action(rc);
            Parent?.AfterRenderChildCore(rc, this);

            if (TransformMaxed && _widthMaxed.HasValue && _heightMaxed.HasValue)
            {
                dc.SetTransform(transform);
            }
        }, creationOptions, rect), creationOptions, rect);
    }

    /// <summary>
    /// Core drawing routine for derived classes. Default behavior clears the background via <see cref="RenderBackgroundCore(RenderContext)"/>.
    /// </summary>
    /// <param name="context">The active render context for this draw call.</param>
    protected internal virtual void RenderCore(RenderContext context) => RenderBackgroundCore(context);

    /// <summary>
    /// Clears the device context to <see cref="BackgroundColor"/> if set; otherwise clears to transparent when
    /// <see cref="FallbackToTransparentBackground"/> is true. Does nothing if neither condition is met.
    /// </summary>
    /// <param name="context">The active render context.</param>
    protected virtual void RenderBackgroundCore(RenderContext context)
    {
        ExceptionExtensions.ThrowIfNull(context, nameof(context));
        if (context.DeviceContext == null)
            throw new InvalidOperationException();

        var bg = BackgroundColor;
        if (bg.HasValue)
        {
            context.DeviceContext.Clear(bg.Value);
        }
        else if (FallbackToTransparentBackground)
        {
            context.DeviceContext.Clear(D3DCOLORVALUE.Transparent);
        }
    }

    /// <inheritdoc/>
    protected override void SetCompositionVisualSizeAndOffset(ContainerVisual visual)
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));

        // I don't think it's documented but experience shows sprite visuals (backed by DirectX texture) width or height
        // must be below D2D bitmap limit (which is 16384), with a slight 2px offset
        // (if we declare a visual of 16384, max DirectX, it becomes 16386 at DirectX11 level for some reason...)
        // So if with or height are over this limit, we must use a transform to scale the visual instead of DComp's offset

        var rr = RelativeRenderRect;
        var maxed = false;
        if (!SuspendedCompositionParts.HasFlag(CompositionUpdateParts.Size))
        {
            // MaximumBitmapSize is 16384, remove 2 to be sure we are below internal DComp limit
            var max = Window.MaximumBitmapSize - 2;
            var size = rr.Size;
            if (size.width > max)
            {
                size.width = max;
                maxed = true;
            }

            if (size.height > max)
            {
                size.height = max;
                maxed = true;
            }

            visual.Size = size.ToVector2();
        }

        if (!SuspendedCompositionParts.HasFlag(CompositionUpdateParts.Offset))
        {
            var offset = RenderOffset + new Vector3(rr.left, rr.top, 0);
            if (maxed)
            {
                // this must be handled by a D2D transform
                _widthMaxed = offset.X;
                _heightMaxed = offset.Y;

                visual.Offset = new Vector3();
            }
            else
            {
                _widthMaxed = null;
                _heightMaxed = null;

                visual.Offset = offset;
            }
        }
    }
}
