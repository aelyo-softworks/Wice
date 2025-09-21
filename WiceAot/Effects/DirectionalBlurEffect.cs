namespace Wice.Effects;

/// <summary>
/// Direct2D Directional Blur effect.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1DirectionalBlurString)]
#else
[Guid(Constants.CLSID_D2D1DirectionalBlurString)]
#endif
public partial class DirectionalBlurEffect : EffectWithSource
{
    /// <summary>
    /// Descriptor for <see cref="StandardDeviation"/> (parameter index 0).
    /// </summary>
    public static EffectProperty StandardDeviationProperty { get; }

    /// <summary>
    /// Descriptor for <see cref="Angle"/> (parameter index 1).
    /// </summary>
    public static EffectProperty AngleProperty { get; }

    /// <summary>
    /// Descriptor for <see cref="Optimization"/> (parameter index 2).
    /// </summary>
    public static EffectProperty OptimizationProperty { get; }

    /// <summary>
    /// Descriptor for <see cref="BorderMode"/> (parameter index 3).
    /// </summary>
    public static EffectProperty BorderModeProperty { get; }

    static DirectionalBlurEffect()
    {
        StandardDeviationProperty = EffectProperty.Add(typeof(DirectionalBlurEffect), nameof(StandardDeviation), 0, 3f);
        AngleProperty = EffectProperty.Add(typeof(DirectionalBlurEffect), nameof(Angle), 1, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RADIANS_TO_DEGREES, 0f);
        OptimizationProperty = EffectProperty.Add(typeof(DirectionalBlurEffect), nameof(Optimization), 2, D2D1_DIRECTIONALBLUR_OPTIMIZATION.D2D1_DIRECTIONALBLUR_OPTIMIZATION_BALANCED);
        BorderModeProperty = EffectProperty.Add(typeof(DirectionalBlurEffect), nameof(BorderMode), 3, D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT);
    }

    /// <summary>
    /// Gets or sets the standard deviation of the blur kernel.
    /// </summary>
    public float StandardDeviation { get => (float)GetPropertyValue(StandardDeviationProperty)!; set => SetPropertyValue(StandardDeviationProperty, value); }

    /// <summary>
    /// Gets or sets the blur direction angle, in radians.
    /// </summary>
    public float Angle { get => (float)GetPropertyValue(AngleProperty)!; set => SetPropertyValue(AngleProperty, value); }

    /// <summary>
    /// Gets or sets the optimization mode used by the effect.
    /// </summary>
    public D2D1_DIRECTIONALBLUR_OPTIMIZATION Optimization { get => (D2D1_DIRECTIONALBLUR_OPTIMIZATION)GetPropertyValue(OptimizationProperty)!; set => SetPropertyValue(OptimizationProperty, value); }

    /// <summary>
    /// Gets or sets how the effect samples pixels outside the input boundaries.
    /// </summary>
    public D2D1_BORDER_MODE BorderMode { get => (D2D1_BORDER_MODE)GetPropertyValue(BorderModeProperty)!; set => SetPropertyValue(BorderModeProperty, value); }
}
