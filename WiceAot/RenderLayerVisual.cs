namespace Wice;

public partial class RenderLayerVisual : Canvas
{
    public static VisualProperty RenderEffectProperty { get; } = VisualProperty.Add<CompositionEffectBrush>(typeof(Visual), nameof(RenderEffect), VisualPropertyInvalidateModes.Render);

    [Category(CategoryRender)]
    public CompositionEffectBrush RenderEffect { get => (CompositionEffectBrush)GetPropertyValue(RenderEffectProperty)!; set => SetPropertyValue(RenderEffectProperty, value); }

    [Category(CategoryRender)]
    public new LayerVisual? CompositionVisual => (LayerVisual?)base.CompositionVisual;
    protected override ContainerVisual? CreateCompositionVisual() => Window?.Compositor?.CreateLayerVisual();

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
