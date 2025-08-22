namespace Wice;

// only (re)created if window loses focus
/// <summary>
/// Focus adornment visual rendered as an overlay around the currently focused element.
/// Lives under the <see cref="Window"/> (a <see cref="Canvas"/>), avoids bubbling invalidations
/// to the parent and performs its own local measure/arrange when its properties change.
/// </summary>
/// <remarks>
/// - Non-interactive: disables key and pointer events.
/// - Shape: uses a parent-provided <c>FocusVisualShapeType</c> when available, otherwise a <see cref="RoundedRectangle"/>.
/// - Placement: computes position/size from the focused visual's absolute render rect with an optional offset from theme/parent.
/// - Clipping: clips to the focused visual's parent clip during rendering.
/// </remarks>
public partial class FocusVisual : Border
{
    /// <summary>
    /// Initializes a new instance of <see cref="FocusVisual"/> and disables input to keep it non-interactive.
    /// </summary>
    public FocusVisual()
    {
        DisableKeyEvents = true;
        DisablePointerEvents = true;
    }

    /// <summary>
    /// Gets the parent window hosting this focus visual.
    /// </summary>
    public new Window? Parent => (Window?)base.Parent;

    /// <summary>
    /// Ensures the parent is a <see cref="Window"/> and completes attachment.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    /// <exception cref="InvalidOperationException">Thrown when the parent is not a <see cref="Window"/>.</exception>
    protected override void OnAttachedToParent(object? sender, EventArgs e)
    {
        if (Parent is null)
            throw new InvalidOperationException();

        base.OnAttachedToParent(sender, e);
    }

    /// <summary>
    /// Prevents bubbling any invalidate modes to the parent <see cref="Window"/>; layout is managed locally.
    /// </summary>
    /// <param name="mode">The child invalidate mode.</param>
    /// <param name="defaultParentModes">Default parent modes.</param>
    /// <param name="reason">Invalidate reason.</param>
    /// <returns>Always <see cref="VisualPropertyInvalidateModes.None"/>.</returns>
    protected internal override VisualPropertyInvalidateModes GetParentInvalidateModes(InvalidateMode mode, VisualPropertyInvalidateModes defaultParentModes, InvalidateReason reason) => VisualPropertyInvalidateModes.None;

    /// <summary>
    /// Intercepts property sets to short-circuit parent invalidation by measuring/arranging locally when needed.
    /// </summary>
    /// <param name="property">The property descriptor being set.</param>
    /// <param name="value">The new value.</param>
    /// <param name="options">Optional set options.</param>
    /// <returns>True if the stored value changed; otherwise false.</returns>
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        var im = ((property as VisualProperty)?.InvalidateModes).GetValueOrDefault();

        // kinda hack to avoid invalidating parent (window)
        var mode = VisualProperty.GetInvalidateMode(im);
        if (mode == InvalidateMode.Arrange)
        {
            ArrangeWithParent();
        }
        else if (mode == InvalidateMode.Measure)
        {
            if (Parent != null)
            {
                Measure(Parent.DesiredSize);
                ArrangeWithParent();
            }
        }

        return true;
    }

    private void ArrangeWithParent()
    {
        if (DesiredSize.IsInvalid)
            return;

        var childRect = Canvas.GetRect(DesiredSize, this);
        Arrange(childRect);
    }

    /// <summary>
    /// Updates the focus adornment to track a newly focused visual.
    /// Selects or creates the focus shape, disables its input, and positions/sizes this visual accordingly.
    /// </summary>
    /// <param name="newFocusedVisual">The newly focused visual (or its parent providing focus settings).</param>
    /// <exception cref="ArgumentNullException">When <paramref name="newFocusedVisual"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="newFocusedVisual"/> is not effectively visible
    /// or its absolute render rectangle is invalid.
    /// </exception>
    protected virtual internal void OnUpdateFocus(Visual newFocusedVisual)
    {
        ExceptionExtensions.ThrowIfNull(newFocusedVisual, nameof(newFocusedVisual));
        if (!newFocusedVisual.IsActuallyVisible)
            throw new ArgumentException(null, nameof(newFocusedVisual));

        if (newFocusedVisual.AbsoluteRenderRect.IsInvalid)
            throw new ArgumentException(null, nameof(newFocusedVisual));

        var focused = newFocusedVisual;
        var parent = newFocusedVisual as IFocusableParent;
        if (parent != null && parent.FocusableVisual != null)
        {
            focused = parent.FocusableVisual;
        }

        if (parent != null && parent.FocusVisualShapeType != null)
        {
            if (Child == null || !parent.FocusVisualShapeType.IsAssignableFrom(Child.GetType()))
            {
                Child = (SingleShape)Activator.CreateInstance(parent.FocusVisualShapeType)!;
            }
        }
        else
        {
            if (Child is not RoundedRectangle)
            {
                Child = new RoundedRectangle();
            }
        }

        Child.DisableKeyEvents = true;
        Child.DisablePointerEvents = true;

#if DEBUG
        Child.Name ??= nameof(FocusVisual) + ".child";
#endif

        var ar = focused.AbsoluteRenderRect;
        var offset = parent?.FocusOffset ?? GetWindowTheme().FocusOffset;

        var l = Canvas.GetLeft(this);
        var t = Canvas.GetTop(this);
        var w = Width;
        var h = Height;

        Canvas.SetLeft(this, ar.left - 1 + offset);
        Canvas.SetTop(this, ar.top - 1 + offset);
        Width = Math.Max(0, ar.Width - offset * 2);
        Height = Math.Max(0, ar.Height - offset * 2);
    }

    /// <summary>
    /// Applies composition updates:
    /// - Clips the focus adornment to the focused visual parent clip.
    /// - Updates the child shape stroke from the current theme.
    /// </summary>
    protected override void Render()
    {
        base.Render();

        var fv = Window?.FocusedVisual;
        if (fv == null || Compositor == null || CompositionVisual == null)
            return;

        // clip to focused' parent
        var ar = fv.AbsoluteRenderRect;
        var pclip = fv.ParentsAbsoluteClipRect;

        var clip = Compositor.CreateInsetClip();
        if (pclip.HasValue && ar.IsValid)
        {
            clip.LeftInset = Math.Max(0, pclip.Value.left - ar.left);
            clip.TopInset = Math.Max(0, pclip.Value.top - ar.top);
            clip.RightInset = Math.Max(0, ar.right - pclip.Value.right);
            clip.BottomInset = Math.Max(0, ar.bottom - pclip.Value.bottom);
        }

        CompositionVisual.Clip = clip;

        if (Child is SingleShape singleShape)
        {
            var theme = GetWindowTheme();
            singleShape.StrokeBrush = Compositor.CreateColorBrush(theme.FocusColor.ToColor());
            singleShape.StrokeThickness = theme.FocusThickness;
            singleShape.StrokeDashArray = theme.FocusDashArray;
        }
    }
}
