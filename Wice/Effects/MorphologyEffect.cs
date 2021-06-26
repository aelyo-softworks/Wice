using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1MorphologyString)]
    public class MorphologyEffect : EffectWithSource
    {
        public static EffectProperty ModeProperty = EffectProperty.Add(typeof(MorphologyEffect), nameof(Mode), 0, D2D1_MORPHOLOGY_MODE.D2D1_MORPHOLOGY_MODE_ERODE);
        public static EffectProperty WidthProperty = EffectProperty.Add(typeof(MorphologyEffect), nameof(Width), 1, 1u);
        public static EffectProperty HeightProperty = EffectProperty.Add(typeof(MorphologyEffect), nameof(Height), 2, 1u);

        public D2D1_MORPHOLOGY_MODE Mode { get => (D2D1_MORPHOLOGY_MODE)GetPropertyValue(ModeProperty); set => SetPropertyValue(ModeProperty, value); }
        public uint Width { get => (uint)GetPropertyValue(WidthProperty); set => SetPropertyValue(WidthProperty, value.Clamp(1, 100)); }
        public uint Height { get => (uint)GetPropertyValue(HeightProperty); set => SetPropertyValue(HeightProperty, value.Clamp(1, 100)); }
    }
}
