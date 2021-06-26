using System.Collections.Generic;
using System.Linq;
using DirectN;

namespace Wice
{
    public class Typography
    {
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

        public Typography(params DWRITE_FONT_FEATURE[] features)
        {
            Features = new List<DWRITE_FONT_FEATURE>();
            if (features != null)
            {
                foreach (var feature in features)
                {
                    Features.Add(feature);
                }
            }
        }

        // see https://docs.microsoft.com/en-us/typography/opentype/spec/featuretags
        public static Typography WithLigatures { get; } = new Typography(
            new DWRITE_FONT_FEATURE(DWRITE_FONT_FEATURE_TAG.DWRITE_FONT_FEATURE_TAG_CONTEXTUAL_LIGATURES),
            new DWRITE_FONT_FEATURE(DWRITE_FONT_FEATURE_TAG.DWRITE_FONT_FEATURE_TAG_REQUIRED_LIGATURES),
            new DWRITE_FONT_FEATURE(DWRITE_FONT_FEATURE_TAG.DWRITE_FONT_FEATURE_TAG_STANDARD_LIGATURES),
            new DWRITE_FONT_FEATURE(DWRITE_FONT_FEATURE_TAG.DWRITE_FONT_FEATURE_TAG_DISCRETIONARY_LIGATURES));

        internal string CacheKey => string.Join("\0", Features.Select(f => ((int)f.nameTag).ToString() + "\0" + f.parameter));
        public virtual IList<DWRITE_FONT_FEATURE> Features { get; }
        public IComObject<IDWriteTypography> DWriteTypography => Application.Current.ResourceManager.GetTypography(this);
    }
}
