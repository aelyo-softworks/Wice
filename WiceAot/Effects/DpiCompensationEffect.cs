namespace Wice.Effects;

/// <summary>
/// Represents an effect that compensates for differences in dots per inch (DPI) between input and output images.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1DpiCompensationString)]
#else
[Guid(Constants.CLSID_D2D1DpiCompensationString)]
#endif
public partial class DpiCompensationEffect : EffectWithSource
{
    /// <summary>
    /// Gets the property that specifies the interpolation mode used by the effect.
    /// </summary>
    public static EffectProperty InterpolationModeProperty { get; }

    /// <summary>
    /// Gets the dependency property that specifies the border mode for an effect.
    /// </summary>
    public static EffectProperty BorderModeProperty { get; }

    /// <summary>
    /// Gets the effect property that specifies the input DPI (dots per inch) for the effect.
    /// </summary>
    public static EffectProperty InputDpiProperty { get; }

    static DpiCompensationEffect()
    {
        InterpolationModeProperty = EffectProperty.Add(typeof(DpiCompensationEffect), nameof(InterpolationMode), 0, D2D1_DPICOMPENSATION_INTERPOLATION_MODE.D2D1_DPICOMPENSATION_INTERPOLATION_MODE_LINEAR);
        BorderModeProperty = EffectProperty.Add(typeof(DpiCompensationEffect), nameof(BorderMode), 1, D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT);
        InputDpiProperty = EffectProperty.Add(typeof(DpiCompensationEffect), nameof(InputDpi), 2, 96f);
    }

    /// <summary>
    /// Gets or sets the interpolation mode used for DPI compensation.
    /// </summary>
    public D2D1_DPICOMPENSATION_INTERPOLATION_MODE InterpolationMode { get => (D2D1_DPICOMPENSATION_INTERPOLATION_MODE)GetPropertyValue(InterpolationModeProperty)!; set => SetPropertyValue(InterpolationModeProperty, value); }

    /// <summary>
    /// Gets or sets the border mode, which determines how the edges of the content are treated when sampled outside the
    /// content bounds.
    /// </summary>
    public D2D1_BORDER_MODE BorderMode { get => (D2D1_BORDER_MODE)GetPropertyValue(BorderModeProperty)!; set => SetPropertyValue(BorderModeProperty, value); }

    /// <summary>
    /// Gets or sets the input dots per inch (DPI) value.
    /// </summary>
    public float InputDpi { get => (float)GetPropertyValue(InputDpiProperty)!; set => SetPropertyValue(InputDpiProperty, value.Clamp(0f, 360f)); }
}
