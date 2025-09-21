namespace Wice.Effects;

/// <summary>
/// Cross-fades between <see cref="EffectWithTwoSources.Source1"/> and <see cref="EffectWithTwoSources.Source2"/>.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1CrossFadeString)]
#else
[Guid(Constants.CLSID_D2D1CrossFadeString)]
#endif
public partial class CrossFadeEffect : EffectWithTwoSources
{
    /// <summary>
    /// Gets the descriptor for the <see cref="Weight"/> effect property.
    /// </summary>
    public static EffectProperty WeightProperty { get; }

    /// <summary>
    /// Initializes effect metadata for <see cref="CrossFadeEffect"/> by registering <see cref="WeightProperty"/>.
    /// </summary>
    static CrossFadeEffect()
    {
        WeightProperty = EffectProperty.Add(typeof(CrossFadeEffect), nameof(Weight), 0, 0.5f);
    }

    /// <summary>
    /// Gets or sets the normalized contribution of <see cref="EffectWithTwoSources.Source2"/> in the cross-fade.
    /// </summary>
    /// <value>
    /// A value in the [0, 1] range:
    /// - 0 produces only <see cref="EffectWithTwoSources.Source1"/>.
    /// - 1 produces only <see cref="EffectWithTwoSources.Source2"/>.
    /// - 0.5 produces an even mix of both inputs.
    /// </value>
    public float Weight { get => (float)GetPropertyValue(WeightProperty)!; set => SetPropertyValue(WeightProperty, value.Clamp(-0f, 1f)); }
}
