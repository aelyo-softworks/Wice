﻿namespace Wice.Effects;

/// <summary>
/// Represents a 3D lookup table (LUT) effect that applies color transformations to an image source.
/// </summary>
/// <remarks>This effect uses a 3D lookup table to perform color grading or other color transformations on an
/// image. The lookup table is defined by the <see cref="Lut"/> property, and the alpha channel behavior is controlled
/// by the <see cref="AlphaMode"/> property. This effect is commonly used in scenarios such as image processing and
/// video editing to achieve specific color effects.</remarks>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1LookupTable3DString)]
#else
[Guid(Constants.CLSID_D2D1LookupTable3DString)]
#endif
public partial class LookupTable3DEffect : EffectWithSource
{
    /// <summary>
    /// Gets the effect property representing the lookup table (LUT) used for color grading.
    /// </summary>
    public static EffectProperty LutProperty { get; }

    /// <summary>
    /// Gets the static property that represents the alpha mode configuration for an effect.
    /// </summary>
    /// <remarks>The alpha mode determines how transparency and blending are handled in the effect.  This
    /// property is typically used to configure or query the alpha blending behavior of an effect.</remarks>
    public static EffectProperty AlphaModeProperty { get; }

    /// <summary>
    /// Initializes static properties for the <see cref="LookupTable3DEffect"/> class.
    /// </summary>
    /// <remarks>This static constructor sets up the effect properties, including the lookup table (LUT) and
    /// alpha mode,  which are used to configure the behavior of the 3D lookup table effect. The properties are
    /// registered  with specific mappings and default values.</remarks>
    static LookupTable3DEffect()
    {
        LutProperty = EffectProperty.Add<object>(typeof(LookupTable3DEffect), nameof(Lut), 0);
        AlphaModeProperty = EffectProperty.Add(typeof(LookupTable3DEffect), nameof(AlphaMode), 1, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLORMATRIX_ALPHA_MODE, D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_PREMULTIPLIED);
    }

    /// <summary>
    /// Gets or sets the lookup table (LUT) associated with this object.
    /// </summary>
    public object Lut { get => GetPropertyValue(LutProperty)!; set => SetPropertyValue(LutProperty, value); }

    /// <summary>
    /// Gets or sets the alpha mode used for rendering operations.
    /// </summary>
    /// <remarks>The alpha mode determines whether the alpha channel is ignored, premultiplied, or straight. 
    /// Ensure the value is compatible with the rendering context to avoid unexpected behavior.</remarks>
    public D2D1_ALPHA_MODE AlphaMode { get => (D2D1_ALPHA_MODE)GetPropertyValue(AlphaModeProperty)!; set => SetPropertyValue(AlphaModeProperty, value); }
}
