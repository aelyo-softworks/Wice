﻿namespace Wice;

/// <summary>
/// Represents a collection of OpenType font features and a bridge to a cached DirectWrite
/// <see cref="IDWriteTypography"/> COM object used by the rendering pipeline.
/// </summary>
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
    public virtual IList<DWRITE_FONT_FEATURE> Features { get; }

    /// <summary>
    /// Gets the cached COM wrapper for the native <see cref="IDWriteTypography"/> corresponding to
    /// the feature set in <see cref="Features"/>. The instance is obtained from
    /// <see cref="Application.CurrentResourceManager"/>.
    /// </summary>
    public IComObject<IDWriteTypography> DWriteTypography => Application.CurrentResourceManager.GetTypography(this)!;
}
