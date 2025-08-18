namespace Wice;

/// <summary>
/// Base visual that renders into a Direct2D-backed <see cref="SpriteVisual"/> surface.
/// Adds background clearing, optional transparent fallback, and handles extremely large surfaces
/// by clamping the composition bitmap size and applying a device-context transform when needed.
/// </summary>
/// <remarks>
/// Key responsibilities:
/// - Exposes <see cref="BackgroundColor"/> plus ancestor background discovery via <see cref="AscendantsBackgroundColor"/>.
/// - Bridges composition brush assignment to <see cref="BackgroundColor"/> when a <see cref="CompositionColorBrush"/> is set.
/// - Performs a Direct2D draw pass via <see cref="RenderD2DSurface(SurfaceCreationOptions?, RECT?)"/> during <see cref="Render"/>.
/// - Clamps visual size to the maximum supported D2D bitmap size and compensates using a transform translation,
///   tracked by <see cref="CompositionWidthMaxed"/>/<see cref="CompositionHeightMaxed"/>.
/// </remarks>
public abstract class RenderVisual : Visual
{
    /// <summary>
    /// Attached property that stores the background color filled in <see cref="RenderBackgroundCore(RenderContext)"/>.
    /// Changing this property invalidates the render pass.
    /// </summary>
    public static VisualProperty BackgroundColorProperty { get; } = VisualProperty.Add<D3DCOLORVALUE?>(typeof(RenderVisual), nameof(BackgroundColor), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// When the composition visual is clamped due to maximum bitmap limits, holds the X translation (in composition space)
    /// to apply on the D2D device context so that the final output aligns with the intended render offset.
    /// </summary>
    private float? _widthMaxed;

    /// <summary>
    /// When the composition visual is clamped due to maximum bitmap limits, holds the Y translation (in composition space)
    /// to apply on the D2D device context so that the final output aligns with the intended render offset.
    /// </summary>
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
    /// Gets the first non-null background color discovered by walking up the visual tree.
    /// It first checks <see cref="BackgroundColor"/> on <see cref="RenderVisual"/> ancestors, then attempts to
    /// extract a color from <see cref="RenderBrush"/> when it is a <see cref="CompositionColorBrush"/> or an
    /// acrylic effect brush.
    /// </summary>
    [Category(CategoryRender)]
    public D3DCOLORVALUE? AscendantsBackgroundColor => GetAscendantsBackgroundColor(this);

    /// <summary>
    /// Walks up the visual tree to resolve a background color.
    /// </summary>
    /// <param name="visual">The starting visual.</param>
    /// <returns>The first background color found, or null.</returns>
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

    /// <summary>
    /// Overrides property setting to keep <see cref="BackgroundColor"/> in sync when <see cref="RenderBrush"/> is a solid color brush.
    /// Only <see cref="CompositionColorBrush"/> is accepted. Other brush types must be applied by derived classes explicitly.
    /// </summary>
    /// <param name="property">The property being set.</param>
    /// <param name="value">The new value.</param>
    /// <param name="options">Optional set options.</param>
    /// <returns>true if the stored value changed; otherwise, false.</returns>
    /// <exception cref="NotSupportedException">Thrown when attempting to set <see cref="RenderBrush"/> with a non-color brush.</exception>
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

    /// <summary>
    /// Performs the render pass for this visual:
    /// - Resets clamping state,
    /// - Calls <see cref="Visual.Render"/> for composition updates,
    /// - Invokes <see cref="RenderD2DSurface(SurfaceCreationOptions?, RECT?)"/> to draw D2D content.
    /// </summary>
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
    /// <exception cref="InvalidOperationException">Thrown when the render context has no device context.</exception>
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

    /// <summary>
    /// Sets the composition visual size and offset based on <see cref="RelativeRenderRect"/> and <see cref="RenderOffset"/>.
    /// If the desired size exceeds the maximum supported bitmap size, clamps the size and records a translation to be
    /// applied on the D2D device context during rendering.
    /// </summary>
    /// <param name="visual">The target composition visual.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="visual"/> is null.</exception>
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
