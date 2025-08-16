namespace Wice.Effects;

/// <summary>
/// Direct2D Directional Blur effect.
/// </summary>
/// <remarks>
/// - Requires at least one input source (see <see cref="EffectWithSource.Source"/>).
/// - Exposes four effect parameters mapped in this order:
///   0: <see cref="StandardDeviation"/> (float),
///   1: <see cref="Angle"/> (float, specified in radians; mapped to degrees for D2D),
///   2: <see cref="Optimization"/> (<see cref="D2D1_DIRECTIONALBLUR_OPTIMIZATION"/>),
///   3: <see cref="BorderMode"/> (<see cref="D2D1_BORDER_MODE"/>).
/// </remarks>
/// <seealso cref="EffectWithSource"/>
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
    /// <remarks>Default value: 3.0f. Mapping: <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_DIRECT"/>.</remarks>
    public static EffectProperty StandardDeviationProperty { get; }

    /// <summary>
    /// Descriptor for <see cref="Angle"/> (parameter index 1).
    /// </summary>
    /// <remarks>
    /// Default value: 0.0f. Mapping: <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RADIANS_TO_DEGREES"/> (stored in radians, converted for D2D).
    /// </remarks>
    public static EffectProperty AngleProperty { get; }

    /// <summary>
    /// Descriptor for <see cref="Optimization"/> (parameter index 2).
    /// </summary>
    /// <remarks>Default value: <see cref="D2D1_DIRECTIONALBLUR_OPTIMIZATION.D2D1_DIRECTIONALBLUR_OPTIMIZATION_BALANCED"/>.</remarks>
    public static EffectProperty OptimizationProperty { get; }

    /// <summary>
    /// Descriptor for <see cref="BorderMode"/> (parameter index 3).
    /// </summary>
    /// <remarks>Default value: <see cref="D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT"/>.</remarks>
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
    /// <remarks>
    /// Higher values produce stronger blur along the specified <see cref="Angle"/>.
    /// Default: 3.0f.
    /// </remarks>
    public float StandardDeviation { get => (float)GetPropertyValue(StandardDeviationProperty)!; set => SetPropertyValue(StandardDeviationProperty, value); }

    /// <summary>
    /// Gets or sets the blur direction angle, in radians.
    /// </summary>
    /// <remarks>
    /// The value is stored as radians in the managed graph and is converted to degrees for D2D via
    /// <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RADIANS_TO_DEGREES"/>.
    /// Default: 0.0f.
    /// </remarks>
    public float Angle { get => (float)GetPropertyValue(AngleProperty)!; set => SetPropertyValue(AngleProperty, value); }

    /// <summary>
    /// Gets or sets the optimization mode used by the effect.
    /// </summary>
    /// <remarks>Default: <see cref="D2D1_DIRECTIONALBLUR_OPTIMIZATION.D2D1_DIRECTIONALBLUR_OPTIMIZATION_BALANCED"/>.</remarks>
    public D2D1_DIRECTIONALBLUR_OPTIMIZATION Optimization { get => (D2D1_DIRECTIONALBLUR_OPTIMIZATION)GetPropertyValue(OptimizationProperty)!; set => SetPropertyValue(OptimizationProperty, value); }

    /// <summary>
    /// Gets or sets how the effect samples pixels outside the input boundaries.
    /// </summary>
    /// <remarks>Default: <see cref="D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT"/>.</remarks>
    public D2D1_BORDER_MODE BorderMode { get => (D2D1_BORDER_MODE)GetPropertyValue(BorderModeProperty)!; set => SetPropertyValue(BorderModeProperty, value); }
}
