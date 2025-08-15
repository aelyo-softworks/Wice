namespace Wice;

/// <summary>
/// Represents a collection of OpenType font features and a bridge to a cached DirectWrite
/// <see cref="IDWriteTypography"/> COM object used by the rendering pipeline.
/// </summary>
/// <remarks>
/// - This type provides a managed representation of font features (<see cref="DWRITE_FONT_FEATURE"/>) and
///   is used to request/construct an <see cref="IDWriteTypography"/> instance through
///   <see cref="Application.CurrentResourceManager"/>.
/// - Instances are lightweight; the actual COM object is retrieved on demand via
///   <see cref="DWriteTypography"/> and may be cached by the resource manager using <see cref="CacheKey"/>.
/// </remarks>
public class Typography
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Typography"/> class by copying the font features
    /// from an existing native <see cref="IDWriteTypography"/> instance.
    /// </summary>
    /// <param name="typography">
    /// The native DirectWrite <see cref="IDWriteTypography"/> to copy features from. If <c>null</c>,
    /// the instance is initialized with an empty feature set.
    /// </param>
    /// <remarks>
    /// This constructor does not retain a reference to the provided COM object. It enumerates the features
    /// via <c>GetFontFeatureCount</c>/<c>GetFontFeature</c> and stores copies in <see cref="Features"/>.
    /// </remarks>
    public Typography(IDWriteTypography typography)
        : this()
    {
        if (typography != null)
        {
            var count = typography.GetFontFeatureCount();
            for (var i = 0; i < count; i++)
            {
                typography.GetFontFeature((uint)i, out var feature).ThrowOnError();
                Features.Add(feature);
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Typography"/> class with the specified
    /// OpenType font features.
    /// </summary>
    /// <param name="features">
    /// One or more <see cref="DWRITE_FONT_FEATURE"/> values to include. If <c>null</c>, the instance
    /// is initialized with an empty feature set.
    /// </param>
    public Typography(params DWRITE_FONT_FEATURE[] features)
    {
        Features = [];
        if (features != null)
        {
            foreach (var feature in features)
            {
                Features.Add(feature);
            }
        }
    }

    /// <summary>
    /// Gets a <see cref="Typography"/> instance preconfigured to enable common ligature-related
    /// OpenType features: contextual, required, standard, and discretionary ligatures.
    /// </summary>
    /// <remarks>
    /// See the OpenType feature tags specification for details:
    /// <see href="https://learn.microsoft.com/typography/opentype/spec/featuretags"/>.
    /// </remarks>
    public static Typography WithLigatures { get; } = new
    (
        new DWRITE_FONT_FEATURE { nameTag = DWRITE_FONT_FEATURE_TAG.DWRITE_FONT_FEATURE_TAG_CONTEXTUAL_LIGATURES },
        new DWRITE_FONT_FEATURE { nameTag = DWRITE_FONT_FEATURE_TAG.DWRITE_FONT_FEATURE_TAG_REQUIRED_LIGATURES },
        new DWRITE_FONT_FEATURE { nameTag = DWRITE_FONT_FEATURE_TAG.DWRITE_FONT_FEATURE_TAG_STANDARD_LIGATURES },
        new DWRITE_FONT_FEATURE { nameTag = DWRITE_FONT_FEATURE_TAG.DWRITE_FONT_FEATURE_TAG_DISCRETIONARY_LIGATURES }
    );

    /// <summary>
    /// Gets an internal, deterministic cache key that uniquely identifies the feature set represented
    /// by this instance. Used by the resource manager to cache <see cref="IDWriteTypography"/> objects.
    /// </summary>
    internal string CacheKey => string.Join("\0", Features.Select(f => ((int)f.nameTag).ToString() + "\0" + f.parameter));

    /// <summary>
    /// Gets the mutable list of OpenType font features that define this <see cref="Typography"/>.
    /// </summary>
    /// <remarks>
    /// Modifying this list changes the identity of the typography and may affect caching via <see cref="CacheKey"/>.
    /// </remarks>
    public virtual IList<DWRITE_FONT_FEATURE> Features { get; }

    /// <summary>
    /// Gets the cached COM wrapper for the native <see cref="IDWriteTypography"/> corresponding to
    /// the feature set in <see cref="Features"/>. The instance is obtained from
    /// <see cref="Application.CurrentResourceManager"/>.
    /// </summary>
    public IComObject<IDWriteTypography> DWriteTypography => Application.CurrentResourceManager.GetTypography(this)!;
}
