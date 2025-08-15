namespace Wice;

/// <summary>
/// A canvas that owns a composition <see cref="LayerVisual"/> and supports applying a
/// <see cref="CompositionEffectBrush"/> to the entire visual subtree.
/// </summary>
/// <remarks>
/// - This element creates a <see cref="LayerVisual"/> via <see cref="CreateCompositionVisual"/> so the effect
///   can be applied to the whole layer (including all children).
/// - The effect is driven by <see cref="RenderEffect"/> and applied during <see cref="Render"/> unless
///   the <see cref="SuspendedCompositionParts"/> includes <see cref="CompositionUpdateParts.Effect"/>.
/// - Changing <see cref="RenderEffect"/> triggers a render invalidation (see <see cref="RenderEffectProperty"/>).
/// </remarks>
public partial class RenderLayerVisual : Canvas
{
    /// <summary>
    /// Visual property descriptor for <see cref="RenderEffect"/>.
    /// Changing this property invalidates rendering.
    /// </summary>
    public static VisualProperty RenderEffectProperty { get; } = VisualProperty.Add<CompositionEffectBrush>(typeof(Visual), nameof(RenderEffect), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Gets or sets the composition effect brush applied to this layer visual.
    /// When set, the effect is assigned to <see cref="LayerVisual.Effect"/> during <see cref="Render"/>.
    /// </summary>
    /// <remarks>
    /// - The effect impacts this element and all its children as they are composed into the layer.
    /// - Set to null to clear the effect.
    /// </remarks>
    [Category(CategoryRender)]
    public CompositionEffectBrush RenderEffect { get => (CompositionEffectBrush)GetPropertyValue(RenderEffectProperty)!; set => SetPropertyValue(RenderEffectProperty, value); }

    /// <summary>
    /// Gets the underlying composition visual typed as <see cref="LayerVisual"/>.
    /// </summary>
    /// <remarks>
    /// Returns null when the visual is not attached to a <see cref="Window"/> or the composition
    /// visual has not yet been created.
    /// </remarks>
    [Category(CategoryRender)]
    public new LayerVisual? CompositionVisual => (LayerVisual?)base.CompositionVisual;

    /// <summary>
    /// Creates the backing composition visual for this element.
    /// </summary>
    /// <returns>A new <see cref="LayerVisual"/> created from the current window's compositor; otherwise null.</returns>
    protected override ContainerVisual? CreateCompositionVisual() => Window?.Compositor?.CreateLayerVisual();

    /// <summary>
    /// Applies composition-time updates for this visual and assigns the effect to the layer when appropriate.
    /// </summary>
    /// <remarks>
    /// - Calls <see cref="Canvas.Render"/> first to ensure base composition state (size/offset, transforms, etc.) is applied.
    /// - Skips touching the effect when <see cref="SuspendedCompositionParts"/> has <see cref="CompositionUpdateParts.Effect"/>.
    /// - If the effect is non-null and a <see cref="LayerVisual"/> is present, assigns <see cref="LayerVisual.Effect"/>.
    /// </remarks>
    /// <exception cref="NotSupportedException">
    /// Thrown if the underlying composition visual is not a <see cref="LayerVisual"/>.
    /// This should not occur because <see cref="CreateCompositionVisual"/> creates a <see cref="LayerVisual"/>.
    /// </exception>
    protected override void Render()
    {
        base.Render();

        if (!SuspendedCompositionParts.HasFlag(CompositionUpdateParts.Effect))
        {
            var cv = CompositionVisual;
            if (cv == null)
                return;

            var effect = RenderEffect;
            if (effect != null)
            {
                if (cv is LayerVisual layer)
                {
                    layer.Effect = effect;
                }
                else
                    throw new NotSupportedException();
            }
        }
    }
}
