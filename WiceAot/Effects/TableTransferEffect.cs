namespace Wice.Effects;

#if NETFRAMEWORK
/// <summary>
/// Direct2D effect CLSID for the TableTransfer effect (Framework build).
/// </summary>
[Guid(D2D1Constants.CLSID_D2D1TableTransferString)]
#else
/// <summary>
/// Direct2D effect CLSID for the TableTransfer effect (.NET build).
/// </summary>
[Guid(Constants.CLSID_D2D1TableTransferString)]
#endif
/// <summary>
/// Applies per-channel lookup table transfer curves to the RGBA components of the source.
/// </summary>
/// <remarks>
/// - Each table defines a piecewise linear transfer function with values in [0, 1].
/// - When a channel Disable flag is true, that channel bypasses the table and passes through unchanged.
/// - When <see cref="ClampOutput"/> is true, the final output is clamped to the [0, 1] range.
/// - Default tables map the identity function [0, 1].
/// </remarks>
/// <seealso cref="EffectWithSource"/>
public partial class TableTransferEffect : EffectWithSource
{
    /// <summary>
    /// Effect metadata for <see cref="RedTable"/> (parameter index 0).
    /// </summary>
    public static EffectProperty RedTableProperty { get; }
    /// <summary>
    /// Effect metadata for <see cref="RedDisable"/> (parameter index 1).
    /// </summary>
    public static EffectProperty RedDisableProperty { get; }
    /// <summary>
    /// Effect metadata for <see cref="GreenTable"/> (parameter index 2).
    /// </summary>
    public static EffectProperty GreenTableProperty { get; }
    /// <summary>
    /// Effect metadata for <see cref="GreenDisable"/> (parameter index 3).
    /// </summary>
    public static EffectProperty GreenDisableProperty { get; }
    /// <summary>
    /// Effect metadata for <see cref="BlueTable"/> (parameter index 4).
    /// </summary>
    public static EffectProperty BlueTableProperty { get; }
    /// <summary>
    /// Effect metadata for <see cref="BlueDisable"/> (parameter index 5).
    /// </summary>
    public static EffectProperty BlueDisableProperty { get; }
    /// <summary>
    /// Effect metadata for <see cref="AlphaTable"/> (parameter index 6).
    /// </summary>
    public static EffectProperty AlphaTableProperty { get; }
    /// <summary>
    /// Effect metadata for <see cref="AlphaDisable"/> (parameter index 7).
    /// </summary>
    public static EffectProperty AlphaDisableProperty { get; }
    /// <summary>
    /// Effect metadata for <see cref="ClampOutput"/> (parameter index 8).
    /// </summary>
    public static EffectProperty ClampOutputProperty { get; }

    static TableTransferEffect()
    {
        RedTableProperty = EffectProperty.Add(typeof(TableTransferEffect), nameof(RedTable), 0, new float[] { 0f, 1f });
        RedDisableProperty = EffectProperty.Add(typeof(TableTransferEffect), nameof(RedDisable), 1, false);
        GreenTableProperty = EffectProperty.Add(typeof(TableTransferEffect), nameof(GreenTable), 2, new float[] { 0f, 1f });
        GreenDisableProperty = EffectProperty.Add(typeof(TableTransferEffect), nameof(GreenDisable), 3, false);
        BlueTableProperty = EffectProperty.Add(typeof(TableTransferEffect), nameof(BlueTable), 4, new float[] { 0f, 1f });
        BlueDisableProperty = EffectProperty.Add(typeof(TableTransferEffect), nameof(BlueDisable), 5, false);
        AlphaTableProperty = EffectProperty.Add(typeof(TableTransferEffect), nameof(AlphaTable), 6, new float[] { 0f, 1f });
        AlphaDisableProperty = EffectProperty.Add(typeof(TableTransferEffect), nameof(AlphaDisable), 7, false);
        ClampOutputProperty = EffectProperty.Add(typeof(TableTransferEffect), nameof(ClampOutput), 8, false);
    }

    /// <summary>
    /// Gets or sets the lookup table for the red channel.
    /// </summary>
    /// <remarks>
    /// Values should be in the [0, 1] range. When <see cref="RedDisable"/> is true, this table is ignored.
    /// The default is an identity mapping [0, 1].
    /// </remarks>
    public float[]? RedTable { get => (float[]?)GetPropertyValue(RedTableProperty); set => SetPropertyValue(RedTableProperty, value); }

    /// <summary>
    /// Gets or sets whether the red channel bypasses the lookup table.
    /// </summary>
    public bool RedDisable { get => (bool)GetPropertyValue(RedDisableProperty)!; set => SetPropertyValue(RedDisableProperty, value); }

    /// <summary>
    /// Gets or sets the lookup table for the green channel.
    /// </summary>
    /// <remarks>
    /// Values should be in the [0, 1] range. When <see cref="GreenDisable"/> is true, this table is ignored.
    /// The default is an identity mapping [0, 1].
    /// </remarks>
    public float[]? GreenTable { get => (float[]?)GetPropertyValue(GreenTableProperty); set => SetPropertyValue(GreenTableProperty, value); }

    /// <summary>
    /// Gets or sets whether the green channel bypasses the lookup table.
    /// </summary>
    public bool GreenDisable { get => (bool)GetPropertyValue(GreenDisableProperty)!; set => SetPropertyValue(GreenDisableProperty, value); }

    /// <summary>
    /// Gets or sets the lookup table for the blue channel.
    /// </summary>
    /// <remarks>
    /// Values should be in the [0, 1] range. When <see cref="BlueDisable"/> is true, this table is ignored.
    /// The default is an identity mapping [0, 1].
    /// </remarks>
    public float[]? BlueTable { get => (float[]?)GetPropertyValue(BlueTableProperty); set => SetPropertyValue(BlueTableProperty, value); }

    /// <summary>
    /// Gets or sets whether the blue channel bypasses the lookup table.
    /// </summary>
    public bool BlueDisable { get => (bool)GetPropertyValue(BlueDisableProperty)!; set => SetPropertyValue(BlueDisableProperty, value); }

    /// <summary>
    /// Gets or sets the lookup table for the alpha channel.
    /// </summary>
    /// <remarks>
    /// Values should be in the [0, 1] range. When <see cref="AlphaDisable"/> is true, this table is ignored.
    /// The default is an identity mapping [0, 1].
    /// </remarks>
    public float[]? AlphaTable { get => (float[]?)GetPropertyValue(AlphaTableProperty); set => SetPropertyValue(AlphaTableProperty, value); }

    /// <summary>
    /// Gets or sets whether the alpha channel bypasses the lookup table.
    /// </summary>
    public bool AlphaDisable { get => (bool)GetPropertyValue(AlphaDisableProperty)!; set => SetPropertyValue(AlphaDisableProperty, value); }

    /// <summary>
    /// Gets or sets whether the final output is clamped to the [0, 1] range after the transfer is applied.
    /// </summary>
    public bool ClampOutput { get => (bool)GetPropertyValue(ClampOutputProperty)!; set => SetPropertyValue(ClampOutputProperty, value); }
}
