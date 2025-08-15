namespace Wice.Effects;
/// <summary>
/// Wraps the Direct2D D2D1 ColorMatrix effect, exposing a single input (<see cref="EffectWithSource.Source"/>)
/// and three configurable properties: <see cref="ColorMatrix"/>, <see cref="AlphaMode"/>, and
/// <see cref="ClampOutput"/>.
/// </summary>
/// <remarks>
/// - The effect CLSID is provided via <see cref="GuidAttribute"/> for interop.
/// - Effect properties are registered through <see cref="EffectProperty"/> with indices matching the D2D contract:
///   0 = ColorMatrix, 1 = AlphaMode, 2 = ClampOutput.
/// - The default <see cref="ColorMatrix"/> is the identity matrix.
/// </remarks>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1ColorMatrixString)]
#else
[Guid(Constants.CLSID_D2D1ColorMatrixString)]
#endif
public partial class ColorMatrixEffect : EffectWithSource
{
    /// <summary>
    /// Descriptor for <see cref="ColorMatrix"/> (effect property index 0).
    /// </summary>
    public static EffectProperty ColorMatrixProperty { get; }
    /// <summary>
    /// Descriptor for <see cref="AlphaMode"/> (effect property index 1).
    /// </summary>
    public static EffectProperty AlphaModeProperty { get; }
    /// <summary>
    /// Descriptor for <see cref="ClampOutput"/> (effect property index 2).
    /// </summary>
    public static EffectProperty ClampOutputProperty { get; }

    static ColorMatrixEffect()
    {
        ColorMatrixProperty = EffectProperty.Add(typeof(ColorMatrixEffect), nameof(ColorMatrix), 0, new D2D_MATRIX_5X4_F(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f));
        AlphaModeProperty = EffectProperty.Add(typeof(ColorMatrixEffect), nameof(AlphaMode), 1, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLORMATRIX_ALPHA_MODE, D2D1_COLORMATRIX_ALPHA_MODE.D2D1_COLORMATRIX_ALPHA_MODE_PREMULTIPLIED);
        ClampOutputProperty = EffectProperty.Add(typeof(ColorMatrixEffect), nameof(ClampOutput), 2, false);
    }

    /// <summary>
    /// Gets or sets the 5x4 color matrix applied to the input source.
    /// </summary>
    /// <remarks>
    /// - Stored as a row-major <see cref="D2D_MATRIX_5X4_F"/> (four columns per row, five rows: RGB+A bias).
    /// - The default is the identity matrix, which leaves the input unchanged.
    /// </remarks>
    public D2D_MATRIX_5X4_F ColorMatrix { get => (D2D_MATRIX_5X4_F)GetPropertyValue(ColorMatrixProperty)!; set => SetPropertyValue(ColorMatrixProperty, value); }

    /// <summary>
    /// Gets or sets how the effect interprets the alpha channel of the input.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="D2D1_COLORMATRIX_ALPHA_MODE.D2D1_COLORMATRIX_ALPHA_MODE_PREMULTIPLIED"/>.
    /// </remarks>
    public D2D1_COLORMATRIX_ALPHA_MODE AlphaMode { get => (D2D1_COLORMATRIX_ALPHA_MODE)GetPropertyValue(AlphaModeProperty)!; set => SetPropertyValue(AlphaModeProperty, value); }

    /// <summary>
    /// Gets or sets whether the output color values are clamped to the [0, 1] range.
    /// </summary>
    public bool ClampOutput { get => (bool)GetPropertyValue(ClampOutputProperty)!; set => SetPropertyValue(ClampOutputProperty, value); }
}
