namespace Wice.Effects;

#if NETFRAMEWORK
/// <summary>
/// Implements the Direct2D "Posterize" effect (CLSID <see cref="D2D1Constants.CLSID_D2D1PosterizeString"/>),
/// exposing typed properties for the red/green/blue quantization levels.
/// </summary>
/// <remarks>
/// - Inherits from <see cref="EffectWithSource"/>, therefore requires at least one <see cref="IGraphicsEffectSource"/> input.
/// - Properties are registered as <see cref="EffectProperty"/> descriptors and serialized to the D2D property bag
///   using DIRECT mapping semantics.
/// - Each channel value is clamped to the inclusive range [2, 16] when set.
/// </remarks>
/// <seealso cref="EffectWithSource"/>
/// <seealso cref="EffectProperty"/>
[Guid(D2D1Constants.CLSID_D2D1PosterizeString)]
#else
/// <summary>
/// Implements the Direct2D "Posterize" effect (CLSID <see cref="Constants.CLSID_D2D1PosterizeString"/>),
/// exposing typed properties for the red/green/blue quantization levels.
/// </summary>
/// <remarks>
/// - Inherits from <see cref="EffectWithSource"/>, therefore requires at least one <see cref="IGraphicsEffectSource"/> input.
/// - Properties are registered as <see cref="EffectProperty"/> descriptors and serialized to the D2D property bag
///   using DIRECT mapping semantics.
/// - Each channel value is clamped to the inclusive range [2, 16] when set.
/// </remarks>
/// <seealso cref="EffectWithSource"/>
/// <seealso cref="EffectProperty"/>
[Guid(Constants.CLSID_D2D1PosterizeString)]
#endif
public partial class PosterizeEffect : EffectWithSource
{
    /// <summary>
    /// Effect property descriptor for <see cref="RedValueCount"/>.
    /// </summary>
    /// <remarks>
    /// - Index: 0
    /// - Mapping: DIRECT
    /// - Default: 4
    /// </remarks>
    public static EffectProperty RedValueCountProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="GreeValueCount"/>.
    /// </summary>
    /// <remarks>
    /// - Index: 1
    /// - Mapping: DIRECT
    /// - Default: 4
    /// </remarks>
    public static EffectProperty GreeValueCountProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="BlueValueCount"/>.
    /// </summary>
    /// <remarks>
    /// - Index: 2
    /// - Mapping: DIRECT
    /// - Default: 4
    /// </remarks>
    public static EffectProperty BlueValueCountProperty { get; }

    /// <summary>
    /// Registers effect properties and their default values.
    /// </summary>
    /// <remarks>
    /// Properties are exposed in index order to the D2D interop layer.
    /// </remarks>
    static PosterizeEffect()
    {
        RedValueCountProperty = EffectProperty.Add(typeof(PosterizeEffect), nameof(RedValueCount), 0, 4);
        GreeValueCountProperty = EffectProperty.Add(typeof(PosterizeEffect), nameof(GreeValueCount), 1, 4);
        BlueValueCountProperty = EffectProperty.Add(typeof(PosterizeEffect), nameof(BlueValueCount), 2, 4);
    }

    public int RedValueCount { get => (int)GetPropertyValue(RedValueCountProperty)!; set => SetPropertyValue(RedValueCountProperty, value.Clamp(2, 16)); }
    public int GreeValueCount { get => (int)GetPropertyValue(GreeValueCountProperty)!; set => SetPropertyValue(GreeValueCountProperty, value.Clamp(2, 16)); }
    public int BlueValueCount { get => (int)GetPropertyValue(BlueValueCountProperty)!; set => SetPropertyValue(BlueValueCountProperty, value.Clamp(2, 16)); }
}
