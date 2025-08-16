namespace Wice.Effects;

/// <summary>
/// Direct2D Crop effect wrapper.
/// </summary>
/// <remarks>
/// - Requires a source (<see cref="EffectWithSource.Source"/>) and crops it to a rectangle in device-independent pixels (DIPs).
/// - The effect CLSID is provided via the <see cref="GuidAttribute"/> to support D2D interop.
/// - Exposes two properties:
///   - <see cref="Rect"/>: crop rectangle (Left, Top, Right, Bottom), mapped as a Vector4.
///   - <see cref="BorderMode"/>: sampling behavior at the edge of the crop.
/// </remarks>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1CropString)]
#else
[Guid(Constants.CLSID_D2D1CropString)]
#endif
public partial class CropEffect : EffectWithSource
{
    /// <summary>
    /// Descriptor for <see cref="Rect"/> (effect parameter index 0).
    /// </summary>
    /// <remarks>
    /// Mapping: <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RECT_TO_VECTOR4"/>.<br/>
    /// Default: <c>(-∞, -∞, +∞, +∞)</c> (no cropping).
    /// </remarks>
    public static EffectProperty RectProperty { get; }

    /// <summary>
    /// Descriptor for <see cref="BorderMode"/> (effect parameter index 1).
    /// </summary>
    /// <remarks>
    /// Default: <see cref="D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT"/>.
    /// </remarks>
    public static EffectProperty BorderModeProperty { get; }

    static CropEffect()
    {
        RectProperty = EffectProperty.Add(typeof(CropEffect), nameof(Rect), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RECT_TO_VECTOR4, new D2D_VECTOR_4F(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue));
        BorderModeProperty = EffectProperty.Add(typeof(CropEffect), nameof(BorderMode), 1, D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT);
    }

    /// <summary>
    /// Gets or sets the crop rectangle in DIPs, expressed as (Left, Top, Right, Bottom).
    /// </summary>
    /// <remarks>
    /// - Serialized as a Vector4 using <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RECT_TO_VECTOR4"/>.<br/>
    /// - Default value <c>(-∞, -∞, +∞, +∞)</c> effectively disables cropping.<br/>
    /// - Coordinates are in the input's space before the effect graph applies additional transforms.
    /// </remarks>
    public D2D_VECTOR_4F Rect { get => (D2D_VECTOR_4F)GetPropertyValue(RectProperty)!; set => SetPropertyValue(RectProperty, value); }

    /// <summary>
    /// Gets or sets how the effect samples pixels at and beyond the crop boundary.
    /// </summary>
    /// <remarks>
    /// - <see cref="D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT"/> blends using the nearest valid pixels.<br/>
    /// - <see cref="D2D1_BORDER_MODE.D2D1_BORDER_MODE_HARD"/> clamps with hard edges.<br/>
    /// - Default is <see cref="D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT"/>.
    /// </remarks>
    public D2D1_BORDER_MODE BorderMode { get => (D2D1_BORDER_MODE)GetPropertyValue(BorderModeProperty)!; set => SetPropertyValue(BorderModeProperty, value); }
}
