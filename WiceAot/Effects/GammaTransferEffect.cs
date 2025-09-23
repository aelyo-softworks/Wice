namespace Wice.Effects;

/// <summary>
/// Represents an effect that applies per-channel gamma correction to an image.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1GammaTransferString)]
#else
[Guid(Constants.CLSID_D2D1GammaTransferString)]
#endif
public partial class GammaTransferEffect : EffectWithSource
{
    /// <summary>
    /// Gets the effect property that controls the amplitude of the red channel in the effect.
    /// </summary>
    public static EffectProperty RedAmplitudeProperty { get; }

    /// <summary>
    /// Gets the effect property that controls the red channel exponent in the effect.
    /// </summary>
    public static EffectProperty RedExponentProperty { get; }

    /// <summary>
    /// Gets the dependency property that represents the red color offset effect.
    /// </summary>
    public static EffectProperty RedOffsetProperty { get; }

    /// <summary>
    /// Gets the effect property that determines whether the red channel is disabled in the effect.
    /// </summary>
    public static EffectProperty RedDisableProperty { get; }

    /// <summary>
    /// Gets the effect property that controls the amplitude of the green channel in the effect.
    /// </summary>
    public static EffectProperty GreenAmplitudeProperty { get; }

    /// <summary>
    /// Gets the effect property that controls the green channel exponent in the effect.
    /// </summary>
    public static EffectProperty GreenExponentProperty { get; }

    /// <summary>
    /// Gets the effect property that represents the green color offset in an effect.
    /// </summary>
    public static EffectProperty GreenOffsetProperty { get; }

    /// <summary>
    /// Gets the static property that represents the effect configuration for disabling the green channel.
    /// </summary>
    public static EffectProperty GreenDisableProperty { get; }

    /// <summary>
    /// Gets the effect property that controls the amplitude of the blue channel in the effect.
    /// </summary>
    public static EffectProperty BlueAmplitudeProperty { get; }

    /// <summary>
    /// Gets the effect property that controls the exponent applied to the blue channel in the effect.
    /// </summary>
    public static EffectProperty BlueExponentProperty { get; }

    /// <summary>
    /// Gets the effect property that represents the blue color offset adjustment.
    /// </summary>
    public static EffectProperty BlueOffsetProperty { get; }

    /// <summary>
    /// Gets the static property that represents the ability to disable the blue effect in the associated effect system.
    /// </summary>
    public static EffectProperty BlueDisableProperty { get; }

    /// <summary>
    /// Gets the effect property that controls the amplitude of the alpha channel in the effect.
    /// </summary>
    public static EffectProperty AlphaAmplitudeProperty { get; }

    /// <summary>
    /// Gets the effect property that controls the alpha exponent used in rendering calculations.
    /// </summary>
    public static EffectProperty AlphaExponentProperty { get; }

    /// <summary>
    /// Gets the effect property that represents the alpha offset value.
    /// </summary>
    public static EffectProperty AlphaOffsetProperty { get; }

    /// <summary>
    /// Gets the effect property that determines whether alpha blending is disabled.
    /// </summary>
    public static EffectProperty AlphaDisableProperty { get; }

    /// <summary>
    /// Gets the property that determines whether the effect output is clamped to a specific range.
    /// </summary>
    public static EffectProperty ClampOutputProperty { get; }

    static GammaTransferEffect()
    {
        RedAmplitudeProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(RedAmplitude), 0, 1f);
        RedExponentProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(RedExponent), 1, 1f);
        RedOffsetProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(RedOffset), 2, 0f);
        RedDisableProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(RedDisable), 3, false);
        GreenAmplitudeProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(GreenAmplitude), 4, 1f);
        GreenExponentProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(GreenExponent), 5, 1f);
        GreenOffsetProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(GreenOffset), 6, 0f);
        GreenDisableProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(GreenDisable), 7, false);
        BlueAmplitudeProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(BlueAmplitude), 8, 1f);
        BlueExponentProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(BlueExponent), 9, 1f);
        BlueOffsetProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(BlueOffset), 10, 0f);
        BlueDisableProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(BlueDisable), 11, false);
        AlphaAmplitudeProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(AlphaAmplitude), 12, 1f);
        AlphaExponentProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(AlphaExponent), 13, 1f);
        AlphaOffsetProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(AlphaOffset), 14, 0f);
        AlphaDisableProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(AlphaDisable), 15, false);
        ClampOutputProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(ClampOutput), 16, false);
    }

    /// <summary>
    /// Gets or sets the amplitude of the red channel.
    /// </summary>
    public float RedAmplitude { get => (float)GetPropertyValue(RedAmplitudeProperty)!; set => SetPropertyValue(RedAmplitudeProperty, value); }

    /// <summary>
    /// Gets or sets the exponent applied to the red channel for color adjustment.
    /// </summary>
    public float RedExponent { get => (float)GetPropertyValue(RedExponentProperty)!; set => SetPropertyValue(RedExponentProperty, value); }

    /// <summary>
    /// Gets or sets the offset value applied to the red channel of the color adjustment.
    /// </summary>
    public float RedOffset { get => (float)GetPropertyValue(RedOffsetProperty)!; set => SetPropertyValue(RedOffsetProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the red functionality is disabled.
    /// </summary>
    public bool RedDisable { get => (bool)GetPropertyValue(RedDisableProperty)!; set => SetPropertyValue(RedDisableProperty, value); }

    /// <summary>
    /// Gets or sets the amplitude of the green channel.
    /// </summary>
    public float GreenAmplitude { get => (float)GetPropertyValue(GreenAmplitudeProperty)!; set => SetPropertyValue(GreenAmplitudeProperty, value); }

    /// <summary>
    /// Gets or sets the exponent applied to the green channel for color adjustment.
    /// </summary>
    public float GreenExponent { get => (float)GetPropertyValue(GreenExponentProperty)!; set => SetPropertyValue(GreenExponentProperty, value); }

    /// <summary>
    /// Gets or sets the offset value applied to the green channel of the color adjustment.
    /// </summary>
    public float GreenOffset { get => (float)GetPropertyValue(GreenOffsetProperty)!; set => SetPropertyValue(GreenOffsetProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the green functionality is disabled.
    /// </summary>
    public bool GreenDisable { get => (bool)GetPropertyValue(GreenDisableProperty)!; set => SetPropertyValue(GreenDisableProperty, value); }

    /// <summary>
    /// Gets or sets the amplitude of the blue channel.
    /// </summary>
    public float BlueAmplitude { get => (float)GetPropertyValue(BlueAmplitudeProperty)!; set => SetPropertyValue(BlueAmplitudeProperty, value); }

    /// <summary>
    /// Gets or sets the exponent value applied to the blue channel in color calculations.
    /// </summary>
    public float BlueExponent { get => (float)GetPropertyValue(BlueExponentProperty)!; set => SetPropertyValue(BlueExponentProperty, value); }

    /// <summary>
    /// Gets or sets the offset value applied to the blue channel of the color adjustment.
    /// </summary>
    public float BlueOffset { get => (float)GetPropertyValue(BlueOffsetProperty)!; set => SetPropertyValue(BlueOffsetProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the blue feature is disabled.
    /// </summary>
    public bool BlueDisable { get => (bool)GetPropertyValue(BlueDisableProperty)!; set => SetPropertyValue(BlueDisableProperty, value); }

    /// <summary>
    /// Gets or sets the amplitude of the alpha channel adjustment.
    /// </summary>
    public float AlphaAmplitude { get => (float)GetPropertyValue(AlphaAmplitudeProperty)!; set => SetPropertyValue(AlphaAmplitudeProperty, value); }

    /// <summary>
    /// Gets or sets the alpha exponent used to adjust the transparency curve.
    /// </summary>
    public float AlphaExponent { get => (float)GetPropertyValue(AlphaExponentProperty)!; set => SetPropertyValue(AlphaExponentProperty, value); }

    /// <summary>
    /// Gets or sets the alpha offset value used to adjust the transparency level.
    /// </summary>
    public float AlphaOffset { get => (float)GetPropertyValue(AlphaOffsetProperty)!; set => SetPropertyValue(AlphaOffsetProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the alpha channel is disabled.
    /// </summary>
    public bool AlphaDisable { get => (bool)GetPropertyValue(AlphaDisableProperty)!; set => SetPropertyValue(AlphaDisableProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the output should be clamped to a predefined range.
    /// </summary>
    public bool ClampOutput { get => (bool)GetPropertyValue(ClampOutputProperty)!; set => SetPropertyValue(ClampOutputProperty, value); }
}
