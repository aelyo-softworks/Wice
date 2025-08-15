namespace Wice.Effects;

#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1TurbulenceString)]
#else
[Guid(Constants.CLSID_D2D1TurbulenceString)]
#endif
/// <summary>
/// Wraps the built-in Direct2D Turbulence effect (CLSID_D2D1Turbulence).
/// Produces procedural fractal noise (Perlin-based) that can be used to synthesize
/// clouds, marble, fire, etc. Values are exposed through <see cref="EffectProperty"/>
/// descriptors and queried by D2D at render time.
/// </summary>
/// <remarks>
/// - The applied GUID differs between .NET Framework and .NET 5+ builds via conditional compilation.
/// - Property indices match the D2D effect parameter order:
///   0: Offset, 1: Size, 2: BaseFrequency, 3: NumOctaves, 4: Seed, 5: Noise, 6: Stitchable.
/// - Default values follow the D2D defaults unless otherwise noted.
/// </remarks>
public partial class TurbulenceEffect : Effect
{
    /// <summary>
    /// Effect metadata descriptor for <see cref="Offset"/>.
    /// Parameter index: 0.
    /// </summary>
    public static EffectProperty OffsetProperty { get; }

    /// <summary>
    /// Effect metadata descriptor for <see cref="Size"/>.
    /// Parameter index: 1.
    /// </summary>
    public static EffectProperty SizeProperty { get; }

    /// <summary>
    /// Effect metadata descriptor for <see cref="BaseFrequency"/>.
    /// Parameter index: 2.
    /// </summary>
    public static EffectProperty BaseFrequencyProperty { get; }

    /// <summary>
    /// Effect metadata descriptor for <see cref="NumOctaves"/>.
    /// Parameter index: 3.
    /// </summary>
    public static EffectProperty NumOctavesProperty { get; }

    /// <summary>
    /// Effect metadata descriptor for <see cref="Seed"/>.
    /// Parameter index: 4.
    /// </summary>
    public static EffectProperty SeedProperty { get; }

    /// <summary>
    /// Effect metadata descriptor for <see cref="Noise"/>.
    /// Parameter index: 5.
    /// </summary>
    public static EffectProperty NoiseProperty { get; }

    /// <summary>
    /// Effect metadata descriptor for <see cref="Stitchable"/>.
    /// Parameter index: 6.
    /// </summary>
    public static EffectProperty StitchableProperty { get; }

    // Registers effect properties and their indices/defaults for D2D interop.
    static TurbulenceEffect()
    {
        OffsetProperty = EffectProperty.Add(typeof(TurbulenceEffect), nameof(Offset), 0, new D2D_VECTOR_2F());
        SizeProperty = EffectProperty.Add(typeof(TurbulenceEffect), nameof(Size), 1, new D2D_VECTOR_2F());
        BaseFrequencyProperty = EffectProperty.Add(typeof(TurbulenceEffect), nameof(BaseFrequency), 2, new D2D_VECTOR_2F(0.01f, 0.01f));
        NumOctavesProperty = EffectProperty.Add(typeof(TurbulenceEffect), nameof(NumOctaves), 3, 1u);
        SeedProperty = EffectProperty.Add(typeof(TurbulenceEffect), nameof(Seed), 4, 1u);
        NoiseProperty = EffectProperty.Add(typeof(TurbulenceEffect), nameof(Noise), 5, D2D1_TURBULENCE_NOISE.D2D1_TURBULENCE_NOISE_FRACTAL_SUM);
        StitchableProperty = EffectProperty.Add(typeof(TurbulenceEffect), nameof(Stitchable), 6, false);
    }

    /// <summary>
    /// Gets or sets the top-left offset (in DIPs) of the turbulence tile in the output space.
    /// </summary>
    /// <value>Defaults to (0, 0).</value>
    public D2D_VECTOR_2F Offset { get => (D2D_VECTOR_2F)GetPropertyValue(OffsetProperty)!; set => SetPropertyValue(OffsetProperty, value); }

    /// <summary>
    /// Gets or sets the tile size (in DIPs) used to generate the noise.
    /// When <c>(0, 0)</c>, the effect may infer size from the render target.
    /// </summary>
    /// <value>Defaults to (0, 0).</value>
    public D2D_VECTOR_2F Size { get => (D2D_VECTOR_2F)GetPropertyValue(SizeProperty)!; set => SetPropertyValue(SizeProperty, value); }

    /// <summary>
    /// Gets or sets the base frequency for the noise along X and Y axes.
    /// Higher values produce more rapid variation; typical range is [0, 1].
    /// </summary>
    /// <value>Defaults to (0.01, 0.01).</value>
    public D2D_VECTOR_2F BaseFrequency { get => (D2D_VECTOR_2F)GetPropertyValue(BaseFrequencyProperty)!; set => SetPropertyValue(BaseFrequencyProperty, value); }

    /// <summary>
    /// Gets or sets the number of octaves used to accumulate fractal noise.
    /// More octaves increase detail (and cost).
    /// </summary>
    /// <value>Defaults to 1.</value>
    public uint NumOctaves { get => (uint)GetPropertyValue(NumOctavesProperty)!; set => SetPropertyValue(NumOctavesProperty, value); }

    /// <summary>
    /// Gets or sets the randomization seed for noise generation.
    /// Changing this value yields a different noise pattern for the same parameters.
    /// </summary>
    /// <value>Defaults to 1.</value>
    public uint Seed { get => (uint)GetPropertyValue(SeedProperty)!; set => SetPropertyValue(SeedProperty, value); }

    /// <summary>
    /// Gets or sets the noise type.
    /// <see cref="D2D1_TURBULENCE_NOISE.D2D1_TURBULENCE_NOISE_FRACTAL_SUM"/> produces soft fractal sum;
    /// <c>D2D1_TURBULENCE_NOISE_TURBULENCE</c> produces absolute-value turbulence.
    /// </summary>
    /// <value>Defaults to <see cref="D2D1_TURBULENCE_NOISE.D2D1_TURBULENCE_NOISE_FRACTAL_SUM"/>.</value>
    public D2D1_TURBULENCE_NOISE Noise { get => (D2D1_TURBULENCE_NOISE)GetPropertyValue(NoiseProperty)!; set => SetPropertyValue(NoiseProperty, value); }

    /// <summary>
    /// Gets or sets whether the generated noise is stitchable across tile boundaries.
    /// When <see langword="true"/>, the pattern repeats seamlessly over <see cref="Size"/>.
    /// </summary>
    /// <value>Defaults to <see langword="false"/>.</value>
    public bool Stitchable { get => (bool)GetPropertyValue(StitchableProperty)!; set => SetPropertyValue(StitchableProperty, value); }
}
