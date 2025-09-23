namespace Wice.Effects;

/// <summary>
/// Represents an effect that applies a Gaussian blur to an image or visual.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1GaussianBlurString)]
#else
[Guid(Constants.CLSID_D2D1GaussianBlurString)]
#endif
public partial class GaussianBlurEffect : EffectWithSource
{
    /// <summary>
    /// Gets the property representing the standard deviation used in the effect.
    /// </summary>
    public static EffectProperty StandardDeviationProperty { get; }

    /// <summary>
    /// Gets the property that represents optimization settings for the effect.
    /// </summary>
    public static EffectProperty OptimizationProperty { get; }

    /// <summary>
    /// Gets the dependency property that specifies the border mode for an effect.
    /// </summary>
    public static EffectProperty BorderModeProperty { get; }

    static GaussianBlurEffect()
    {
        StandardDeviationProperty = EffectProperty.Add(typeof(GaussianBlurEffect), nameof(StandardDeviation), 0, 3f);
        OptimizationProperty = EffectProperty.Add(typeof(GaussianBlurEffect), nameof(Optimization), 1, D2D1_GAUSSIANBLUR_OPTIMIZATION.D2D1_GAUSSIANBLUR_OPTIMIZATION_BALANCED);
        BorderModeProperty = EffectProperty.Add(typeof(GaussianBlurEffect), nameof(BorderMode), 2, D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT);
    }

    /// <summary>
    /// Gets or sets the standard deviation value.
    /// </summary>
    public float StandardDeviation { get => (float)GetPropertyValue(StandardDeviationProperty)!; set => SetPropertyValue(StandardDeviationProperty, value); }

    /// <summary>
    /// Gets or sets the optimization mode for the Gaussian blur effect.
    /// </summary>
    public D2D1_GAUSSIANBLUR_OPTIMIZATION Optimization { get => (D2D1_GAUSSIANBLUR_OPTIMIZATION)GetPropertyValue(OptimizationProperty)!; set => SetPropertyValue(OptimizationProperty, value); }

    /// <summary>
    /// Gets or sets the border mode used to determine how image edges are handled during rendering.
    /// </summary>
    public D2D1_BORDER_MODE BorderMode { get => (D2D1_BORDER_MODE)GetPropertyValue(BorderModeProperty)!; set => SetPropertyValue(BorderModeProperty, value); }
}
