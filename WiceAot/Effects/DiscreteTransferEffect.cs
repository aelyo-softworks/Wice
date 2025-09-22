namespace Wice.Effects;

/// <summary>
/// Represents an effect that applies discrete transfer functions to the red, green, blue, and alpha channels of an
/// image.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1DiscreteTransferString)]
#else
[Guid(Constants.CLSID_D2D1DiscreteTransferString)]
#endif
public partial class DiscreteTransferEffect : EffectWithSource
{
    /// <summary>
    /// Gets the static property representing the red color adjustment table for an effect.
    /// </summary>
    public static EffectProperty RedTableProperty { get; }

    /// <summary>
    /// Gets the effect property that determines whether the red channel is disabled in the effect.
    /// </summary>
    public static EffectProperty RedDisableProperty { get; }

    /// <summary>
    /// Gets the effect property representing the green color adjustment table.
    /// </summary>
    public static EffectProperty GreenTableProperty { get; }

    /// <summary>
    /// Gets the static property that represents the ability to disable the green channel effect.
    /// </summary>
    public static EffectProperty GreenDisableProperty { get; }

    /// <summary>
    /// Gets the effect property that represents the blue color adjustment table.
    /// </summary>
    public static EffectProperty BlueTableProperty { get; }

    /// <summary>
    /// Gets the static property that represents the effect configuration for disabling the blue channel.
    /// </summary>
    public static EffectProperty BlueDisableProperty { get; }

    /// <summary>
    /// Gets the effect property that represents the alpha table used for modifying the transparency of an effect.
    /// </summary>
    public static EffectProperty AlphaTableProperty { get; }

    /// <summary>
    /// Gets the effect property that determines whether alpha blending is disabled.
    /// </summary>
    public static EffectProperty AlphaDisableProperty { get; }

    /// <summary>
    /// Gets the property that determines whether the effect output is clamped to a specific range.
    /// </summary>
    public static EffectProperty ClampOutputProperty { get; }

    static DiscreteTransferEffect()
    {
        RedTableProperty = EffectProperty.Add(typeof(DiscreteTransferEffect), nameof(RedTable), 0, new float[] { 0f, 1f });
        RedDisableProperty = EffectProperty.Add(typeof(DiscreteTransferEffect), nameof(RedDisable), 1, false);
        GreenTableProperty = EffectProperty.Add(typeof(DiscreteTransferEffect), nameof(GreenTable), 2, new float[] { 0f, 1f });
        GreenDisableProperty = EffectProperty.Add(typeof(DiscreteTransferEffect), nameof(GreenDisable), 3, false);
        BlueTableProperty = EffectProperty.Add(typeof(DiscreteTransferEffect), nameof(BlueTable), 4, new float[] { 0f, 1f });
        BlueDisableProperty = EffectProperty.Add(typeof(DiscreteTransferEffect), nameof(BlueDisable), 5, false);
        AlphaTableProperty = EffectProperty.Add(typeof(DiscreteTransferEffect), nameof(AlphaTable), 6, new float[] { 0f, 1f });
        AlphaDisableProperty = EffectProperty.Add(typeof(DiscreteTransferEffect), nameof(AlphaDisable), 7, false);
        ClampOutputProperty = EffectProperty.Add(typeof(DiscreteTransferEffect), nameof(ClampOutput), 8, false);
    }

    /// <summary>
    /// Gets or sets the red color adjustment table.
    /// </summary>
    public float[]? RedTable { get => (float[]?)GetPropertyValue(RedTableProperty); set => SetPropertyValue(RedTableProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the red functionality is disabled.
    /// </summary>
    public bool RedDisable { get => (bool)GetPropertyValue(RedDisableProperty)!; set => SetPropertyValue(RedDisableProperty, value); }

    /// <summary>
    /// Gets or sets the green color adjustment table.
    /// </summary>
    public float[]? GreenTable { get => (float[]?)GetPropertyValue(GreenTableProperty); set => SetPropertyValue(GreenTableProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the green functionality is disabled.
    /// </summary>
    public bool GreenDisable { get => (bool)GetPropertyValue(GreenDisableProperty)!; set => SetPropertyValue(GreenDisableProperty, value); }

    /// <summary>
    /// Gets or sets the blue color adjustment table.
    /// </summary>
    public float[]? BlueTable { get => (float[]?)GetPropertyValue(BlueTableProperty); set => SetPropertyValue(BlueTableProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the blue feature is disabled.
    /// </summary>
    public bool BlueDisable { get => (bool)GetPropertyValue(BlueDisableProperty)!; set => SetPropertyValue(BlueDisableProperty, value); }

    /// <summary>
    /// Gets or sets the alpha table used for processing or calculations. 
    /// </summary>
    public float[]? AlphaTable { get => (float[]?)GetPropertyValue(AlphaTableProperty); set => SetPropertyValue(AlphaTableProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the alpha channel is disabled.
    /// </summary>
    public bool AlphaDisable { get => (bool)GetPropertyValue(AlphaDisableProperty)!; set => SetPropertyValue(AlphaDisableProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the output should be clamped to a predefined range.
    /// </summary>
    public bool ClampOutput { get => (bool)GetPropertyValue(ClampOutputProperty)!; set => SetPropertyValue(ClampOutputProperty, value); }
}
