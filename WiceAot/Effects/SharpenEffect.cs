namespace Wice.Effects;

/// <summary>
/// Direct2D Sharpen effect wrapper.
/// </summary>
/// <remarks>
/// - Requires at least one input <see cref="IGraphicsEffectSource"/> (see <see cref="EffectWithSource.Source"/>).
/// - Exposes two parameters mapped to the underlying D2D1 effect:
///   - Index 0: <see cref="Sharpness"/> (0..10, default 0)
///   - Index 1: <see cref="Threshold"/> (0..1, default 0)
/// - Use <see cref="Effect.Sources"/> for additional inputs if needed.
/// </remarks>
/// <seealso cref="EffectWithSource"/>
/// <seealso cref="IGraphicsEffectSource"/>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1SharpenString)]
#else
[Guid(Constants.CLSID_D2D1SharpenString)]
#endif
public partial class SharpenEffect : EffectWithSource
{
    /// <summary>
    /// Effect property descriptor for <see cref="Sharpness"/>.
    /// </summary>
    /// <remarks>
    /// - Effect parameter index: 0
    /// - Mapping: DIRECT
    /// - Default: 0f
    /// </remarks>
    public static EffectProperty SharpnessProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="Threshold"/>.
    /// </summary>
    /// <remarks>
    /// - Effect parameter index: 1
    /// - Mapping: DIRECT
    /// - Default: 0f
    /// </remarks>
    public static EffectProperty ThresholdProperty { get; }

    // Registers effect properties and their indices for D2D interop.
    static SharpenEffect()
    {
        SharpnessProperty = EffectProperty.Add(typeof(SharpenEffect), nameof(Sharpness), 0, 0f);
        ThresholdProperty = EffectProperty.Add(typeof(SharpenEffect), nameof(Threshold), 1, 0f);
    }

    /// <summary>
    /// Controls the amount of sharpening applied to high-frequency details.
    /// </summary>
    /// <value>Clamped to the [0, 10] range. Default is 0.</value>
    public float Sharpness
    {
        get => (float)GetPropertyValue(SharpnessProperty)!;
        set => SetPropertyValue(SharpnessProperty, value.Clamp(0f, 10f));
    }

    /// <summary>
    /// Sets the minimum local contrast required for sharpening to be applied.
    /// </summary>
    /// <value>Clamped to the [0, 1] range. Default is 0.</value>
    public float Threshold
    {
        get => (float)GetPropertyValue(ThresholdProperty)!;
        set => SetPropertyValue(ThresholdProperty, value.Clamp(0f, 1f));
    }
}
