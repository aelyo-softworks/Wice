namespace Wice.Effects;

/// <summary>
/// Implements the Direct2D "Posterize" effect,
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
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1PosterizeString)]
#else
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
    /// Effect property descriptor for <see cref="GreenValueCount"/>.
    /// </summary>
    /// <remarks>
    /// - Index: 1
    /// - Mapping: DIRECT
    /// - Default: 4
    /// </remarks>
    public static EffectProperty GreenValueCountProperty { get; }

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
        GreenValueCountProperty = EffectProperty.Add(typeof(PosterizeEffect), nameof(GreenValueCount), 1, 4);
        BlueValueCountProperty = EffectProperty.Add(typeof(PosterizeEffect), nameof(BlueValueCount), 2, 4);
    }

    /// <summary>
    /// Gets or sets the count of red values, constrained to a range between 2 and 16.
    /// </summary>
    public int RedValueCount { get => (int)GetPropertyValue(RedValueCountProperty)!; set => SetPropertyValue(RedValueCountProperty, value.Clamp(2, 16)); }

    /// <summary>
    /// Gets or sets the count of Green values.
    /// </summary>
    public int GreenValueCount { get => (int)GetPropertyValue(GreenValueCountProperty)!; set => SetPropertyValue(GreenValueCountProperty, value.Clamp(2, 16)); }

    /// <summary>
    /// Gets or sets the count of blue values, constrained to a range between 2 and 16.
    /// </summary>
    public int BlueValueCount { get => (int)GetPropertyValue(BlueValueCountProperty)!; set => SetPropertyValue(BlueValueCountProperty, value.Clamp(2, 16)); }
}
