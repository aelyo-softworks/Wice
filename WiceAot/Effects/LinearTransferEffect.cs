namespace Wice.Effects;

/// <summary>
/// Represents an effect that applies a linear transfer function to the red, green, blue, and alpha channels of an
/// image.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1LinearTransferString)]
#else
[Guid(Constants.CLSID_D2D1LinearTransferString)]
#endif
public partial class LinearTransferEffect : EffectWithSource
{
    /// <summary>
    /// Gets the dependency property that represents the Y-intercept value for the red channel in an effect.
    /// </summary>
    public static EffectProperty RedYInterceptProperty { get; }

    /// <summary>
    /// Gets the effect property that controls the red slope adjustment in the effect.
    /// </summary>
    public static EffectProperty RedSlopeProperty { get; }

    /// <summary>
    /// Gets the property that determines whether the red channel is disabled in the effect.
    /// </summary>
    public static EffectProperty RedDisableProperty { get; }

    /// <summary>
    /// Gets the effect property that represents the Y-intercept value for the green channel.
    /// </summary>
    public static EffectProperty GreenYInterceptProperty { get; }

    /// <summary>
    /// Gets the effect property that controls the green slope adjustment in the color grading process.
    /// </summary>
    public static EffectProperty GreenSlopeProperty { get; }

    /// <summary>
    /// Gets the static property representing the effect configuration for disabling the green channel.
    /// </summary>
    public static EffectProperty GreenDisableProperty { get; }

    /// <summary>
    /// Gets the static property that represents the blue Y-intercept value for an effect.
    /// </summary>
    public static EffectProperty BlueYInterceptProperty { get; }

    /// <summary>
    /// Gets the effect property that controls the slope of the blue channel in the effect.
    /// </summary>
    public static EffectProperty BlueSlopeProperty { get; }

    /// <summary>
    /// Gets the static property that represents the ability to disable the blue effect in the associated effect system.
    /// </summary>
    public static EffectProperty BlueDisableProperty { get; }

    /// <summary>
    /// Gets the effect property that represents the Y-intercept of the alpha channel.
    /// </summary>
    public static EffectProperty AlphaYInterceptProperty { get; }

    /// <summary>
    /// Gets the effect property that represents the alpha slope value.
    /// </summary>
    public static EffectProperty AlphaSlopeProperty { get; }

    /// <summary>
    /// Gets the effect property that determines whether alpha blending is disabled.
    /// </summary>
    public static EffectProperty AlphaDisableProperty { get; }

    /// <summary>
    /// Gets the property that determines whether the effect output is clamped to a specific range.
    /// </summary>
    public static EffectProperty ClampOutputProperty { get; }

    /// <summary>
    /// Initializes static properties for the <see cref="LinearTransferEffect"/> class.
    /// </summary>
    static LinearTransferEffect()
    {
        RedYInterceptProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(RedYIntercept), 0, 0f);
        RedSlopeProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(RedSlope), 1, 1f);
        RedDisableProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(RedDisable), 2, false);
        GreenYInterceptProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(GreenYIntercept), 3, 0f);
        GreenSlopeProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(GreenSlope), 4, 1f);
        GreenDisableProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(GreenDisable), 5, false);
        BlueYInterceptProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(BlueYIntercept), 6, 0f);
        BlueSlopeProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(BlueSlope), 7, 1f);
        BlueDisableProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(BlueDisable), 8, false);
        AlphaYInterceptProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(AlphaYIntercept), 9, 0f);
        AlphaSlopeProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(AlphaSlope), 10, 1f);
        AlphaDisableProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(AlphaDisable), 11, false);
        ClampOutputProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(ClampOutput), 12, false);
    }

    /// <summary>
    /// Gets or sets the Y-intercept value for the red channel in a linear transformation.
    /// </summary>
    public float RedYIntercept { get => (float)GetPropertyValue(RedYInterceptProperty)!; set => SetPropertyValue(RedYInterceptProperty, value); }

    /// <summary>
    /// Gets or sets the slope value for the red channel.
    /// </summary>
    public float RedSlope { get => (float)GetPropertyValue(RedSlopeProperty)!; set => SetPropertyValue(RedSlopeProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the red component is disabled.
    /// </summary>
    public bool RedDisable { get => (bool)GetPropertyValue(RedDisableProperty)!; set => SetPropertyValue(RedDisableProperty, value); }

    /// <summary>
    /// Gets or sets the Y-intercept value for the green channel in a color adjustment operation.
    /// </summary>
    public float GreenYIntercept { get => (float)GetPropertyValue(GreenYInterceptProperty)!; set => SetPropertyValue(GreenYInterceptProperty, value); }

    /// <summary>
    /// Gets or sets the slope of the green channel in the color adjustment process.
    /// </summary>
    public float GreenSlope { get => (float)GetPropertyValue(GreenSlopeProperty)!; set => SetPropertyValue(GreenSlopeProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the green functionality is disabled.
    /// </summary>
    public bool GreenDisable { get => (bool)GetPropertyValue(GreenDisableProperty)!; set => SetPropertyValue(GreenDisableProperty, value); }

    /// <summary>
    /// Gets or sets the Y-intercept value for the blue component in a linear transformation.
    /// </summary>
    public float BlueYIntercept { get => (float)GetPropertyValue(BlueYInterceptProperty)!; set => SetPropertyValue(BlueYInterceptProperty, value); }

    /// <summary>
    /// Gets or sets the slope value for the blue channel.
    /// </summary>
    public float BlueSlope { get => (float)GetPropertyValue(BlueSlopeProperty)!; set => SetPropertyValue(BlueSlopeProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the blue feature is disabled.
    /// </summary>
    public bool BlueDisable { get => (bool)GetPropertyValue(BlueDisableProperty)!; set => SetPropertyValue(BlueDisableProperty, value); }

    /// <summary>
    /// Gets or sets the Y-intercept value for the alpha calculation.
    /// </summary>
    public float AlphaYIntercept { get => (float)GetPropertyValue(AlphaYInterceptProperty)!; set => SetPropertyValue(AlphaYInterceptProperty, value); }

    /// <summary>
    /// Gets or sets the slope of the alpha channel adjustment.
    /// </summary>
    public float AlphaSlope { get => (float)GetPropertyValue(AlphaSlopeProperty)!; set => SetPropertyValue(AlphaSlopeProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the alpha channel is disabled.
    /// </summary>
    public bool AlphaDisable { get => (bool)GetPropertyValue(AlphaDisableProperty)!; set => SetPropertyValue(AlphaDisableProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the output should be clamped to a predefined range.
    /// </summary>
    public bool ClampOutput { get => (bool)GetPropertyValue(ClampOutputProperty)!; set => SetPropertyValue(ClampOutputProperty, value); }
}
