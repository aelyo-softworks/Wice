using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1EmbossString)]
    public class EmbossEffect : EffectWithSource
    {
        public static EffectProperty HeightProperty = EffectProperty.Add(typeof(EmbossEffect), nameof(Height), 0, 0.5f);
        public static EffectProperty DirectionProperty = EffectProperty.Add(typeof(EmbossEffect), nameof(Direction), 1, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RADIANS_TO_DEGREES, 0f);

        public float Height { get => (float)GetPropertyValue(HeightProperty); set => SetPropertyValue(HeightProperty, value.Clamp(0f, 10f)); }
        public float Direction { get => (float)GetPropertyValue(DirectionProperty); set => SetPropertyValue(DirectionProperty, value.Clamp(0f, 360f)); }
    }
}
