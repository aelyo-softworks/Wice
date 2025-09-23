namespace Wice;

/// <summary>
/// A canvas that owns a composition <see cref="LayerVisual"/> and supports applying a
/// <see cref="CompositionEffectBrush"/> to the entire visual subtree.
/// </summary>
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
    [Category(CategoryRender)]
    public CompositionEffectBrush RenderEffect { get => (CompositionEffectBrush)GetPropertyValue(RenderEffectProperty)!; set => SetPropertyValue(RenderEffectProperty, value); }

    /// <summary>
    /// Gets the underlying composition visual typed as <see cref="LayerVisual"/>.
    /// </summary>
    [Category(CategoryRender)]
    public new LayerVisual? CompositionVisual => (LayerVisual?)base.CompositionVisual;

    /// <inheritdoc/>
    protected override ContainerVisual? CreateCompositionVisual() => Window?.Compositor?.CreateLayerVisual();

    /// <inheritdoc/>
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
