namespace Wice.Effects;

/// <summary>
/// Wraps the Direct2D Color Management effect (CLSID_D2D1ColorManagement).
/// Converts pixels between color spaces and optionally changes alpha interpretation.
/// </summary>
/// <remarks>
/// Properties are exposed via strongly-typed accessors and registered through <see cref="EffectProperty"/> descriptors
/// to support D2D/Win2D interop. Property indices map as follows:
/// 0 = <see cref="SourceColorContext"/>,
/// 1 = <see cref="SourceRenderingIntent"/>,
/// 2 = <see cref="DestinationColorContext"/>,
/// 3 = <see cref="DestinationRenderingIntent"/>,
/// 4 = <see cref="AlphaMode"/>,
/// 5 = <see cref="Quality"/>.
/// </remarks>
/// <seealso cref="EffectWithSource"/>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1ColorManagementString)]
#else
[Guid(Constants.CLSID_D2D1ColorManagementString)]
#endif
public partial class ColorManagementEffect : EffectWithSource
{
    /// <summary>
    /// Descriptor for <see cref="SourceColorContext"/> (index 0).
    /// </summary>
    public static EffectProperty SourceColorContextProperty { get; }

    /// <summary>
    /// Descriptor for <see cref="SourceRenderingIntent"/> (index 1). Default is
    /// <see cref="D2D1_COLORMANAGEMENT_RENDERING_INTENT.D2D1_COLORMANAGEMENT_RENDERING_INTENT_PERCEPTUAL"/>.
    /// </summary>
    public static EffectProperty SourceRenderingIntentProperty { get; }

    /// <summary>
    /// Descriptor for <see cref="DestinationColorContext"/> (index 2).
    /// </summary>
    public static EffectProperty DestinationColorContextProperty { get; }

    /// <summary>
    /// Descriptor for <see cref="DestinationRenderingIntent"/> (index 3). Default is
    /// <see cref="D2D1_COLORMANAGEMENT_RENDERING_INTENT.D2D1_COLORMANAGEMENT_RENDERING_INTENT_PERCEPTUAL"/>.
    /// </summary>
    public static EffectProperty DestinationRenderingIntentProperty { get; }

    /// <summary>
    /// Descriptor for <see cref="AlphaMode"/> (index 4). Mapped as
    /// <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLORMATRIX_ALPHA_MODE"/>;
    /// default is <see cref="D2D1_COLORMANAGEMENT_ALPHA_MODE.D2D1_COLORMANAGEMENT_ALPHA_MODE_PREMULTIPLIED"/>.
    /// </summary>
    public static EffectProperty AlphaModeProperty { get; }

    /// <summary>
    /// Descriptor for <see cref="Quality"/> (index 5). Default is
    /// <see cref="D2D1_COLORMANAGEMENT_QUALITY.D2D1_COLORMANAGEMENT_QUALITY_NORMAL"/>.
    /// </summary>
    public static EffectProperty QualityProperty { get; }

    /// <summary>
    /// Registers effect property descriptors and their defaults/mappings.
    /// </summary>
    static ColorManagementEffect()
    {
        SourceColorContextProperty = EffectProperty.Add<ID2D1ColorContext>(typeof(ColorManagementEffect), nameof(SourceColorContext), 0);
        SourceRenderingIntentProperty = EffectProperty.Add(typeof(ColorManagementEffect), nameof(SourceRenderingIntent), 1, D2D1_COLORMANAGEMENT_RENDERING_INTENT.D2D1_COLORMANAGEMENT_RENDERING_INTENT_PERCEPTUAL);
        DestinationColorContextProperty = EffectProperty.Add<ID2D1ColorContext>(typeof(ColorManagementEffect), nameof(DestinationColorContext), 2);
        DestinationRenderingIntentProperty = EffectProperty.Add(typeof(ColorManagementEffect), nameof(DestinationRenderingIntent), 3, D2D1_COLORMANAGEMENT_RENDERING_INTENT.D2D1_COLORMANAGEMENT_RENDERING_INTENT_PERCEPTUAL);
        AlphaModeProperty = EffectProperty.Add(typeof(ColorManagementEffect), nameof(AlphaMode), 4, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLORMATRIX_ALPHA_MODE, D2D1_COLORMANAGEMENT_ALPHA_MODE.D2D1_COLORMANAGEMENT_ALPHA_MODE_PREMULTIPLIED);
        QualityProperty = EffectProperty.Add(typeof(ColorManagementEffect), nameof(Quality), 5, D2D1_COLORMANAGEMENT_QUALITY.D2D1_COLORMANAGEMENT_QUALITY_NORMAL);
    }

    /// <summary>
    /// Gets or sets the source color context (ICC profile) that describes the input pixels.
    /// </summary>
    public ID2D1ColorContext SourceColorContext { get => (ID2D1ColorContext)GetPropertyValue(SourceColorContextProperty)!; set => SetPropertyValue(SourceColorContextProperty, value); }

    /// <summary>
    /// Gets or sets the rendering intent to use when converting from the source color space.
    /// Defaults to <see cref="D2D1_COLORMANAGEMENT_RENDERING_INTENT.D2D1_COLORMANAGEMENT_RENDERING_INTENT_PERCEPTUAL"/>.
    /// </summary>
    public D2D1_COLORMANAGEMENT_RENDERING_INTENT SourceRenderingIntent { get => (D2D1_COLORMANAGEMENT_RENDERING_INTENT)GetPropertyValue(SourceRenderingIntentProperty)!; set => SetPropertyValue(SourceRenderingIntentProperty, value); }

    /// <summary>
    /// Gets or sets the destination color context (ICC profile) for the output pixels.
    /// </summary>
    public ID2D1ColorContext DestinationColorContext { get => (ID2D1ColorContext)GetPropertyValue(DestinationColorContextProperty)!; set => SetPropertyValue(DestinationColorContextProperty, value); }

    /// <summary>
    /// Gets or sets the rendering intent to use when converting to the destination color space.
    /// Defaults to <see cref="D2D1_COLORMANAGEMENT_RENDERING_INTENT.D2D1_COLORMANAGEMENT_RENDERING_INTENT_PERCEPTUAL"/>.
    /// </summary>
    public D2D1_COLORMANAGEMENT_RENDERING_INTENT DestinationRenderingIntent { get => (D2D1_COLORMANAGEMENT_RENDERING_INTENT)GetPropertyValue(DestinationRenderingIntentProperty)!; set => SetPropertyValue(DestinationRenderingIntentProperty, value); }

    /// <summary>
    /// Gets or sets how the effect interprets the alpha channel of the input/output.
    /// Default is <see cref="D2D1_COLORMANAGEMENT_ALPHA_MODE.D2D1_COLORMANAGEMENT_ALPHA_MODE_PREMULTIPLIED"/>.
    /// </summary>
    public D2D1_COLORMANAGEMENT_ALPHA_MODE AlphaMode { get => (D2D1_COLORMANAGEMENT_ALPHA_MODE)GetPropertyValue(AlphaModeProperty)!; set => SetPropertyValue(AlphaModeProperty, value); }

    /// <summary>
    /// Gets or sets the quality of the color conversion. Higher quality may be slower.
    /// Default is <see cref="D2D1_COLORMANAGEMENT_QUALITY.D2D1_COLORMANAGEMENT_QUALITY_NORMAL"/>.
    /// </summary>
    public D2D1_COLORMANAGEMENT_QUALITY Quality { get => (D2D1_COLORMANAGEMENT_QUALITY)GetPropertyValue(QualityProperty)!; set => SetPropertyValue(QualityProperty, value); }
}
