namespace Wice.Effects;

#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1ConvolveMatrixString)]
#else
[Guid(Constants.CLSID_D2D1ConvolveMatrixString)]
#endif
public partial class ConvolveMatrixEffect : EffectWithSource
{
    public static EffectProperty KernelUnitLengthProperty { get; }
    public static EffectProperty ScaleModeProperty { get; }
    public static EffectProperty KernelSizeXProperty { get; }
    public static EffectProperty KernelSizeYProperty { get; }
    public static EffectProperty KernelMatrixProperty { get; }
    public static EffectProperty DivisorProperty { get; }
    public static EffectProperty BiasProperty { get; }
    public static EffectProperty KernelOffsetProperty { get; }
    public static EffectProperty PreserveAlphaProperty { get; }
    public static EffectProperty BorderModeProperty { get; }
    public static EffectProperty ClampOutputProperty { get; }

    static ConvolveMatrixEffect()
    {
        KernelUnitLengthProperty = EffectProperty.Add(typeof(ConvolveMatrixEffect), nameof(KernelUnitLength), 0, new D2D_VECTOR_2F(1f, 1f));
        ScaleModeProperty = EffectProperty.Add(typeof(ConvolveMatrixEffect), nameof(ScaleMode), 1, D2D1_CONVOLVEMATRIX_SCALE_MODE.D2D1_CONVOLVEMATRIX_SCALE_MODE_LINEAR);
        KernelSizeXProperty = EffectProperty.Add(typeof(ConvolveMatrixEffect), nameof(KernelSizeX), 2, 3u);
        KernelSizeYProperty = EffectProperty.Add(typeof(ConvolveMatrixEffect), nameof(KernelSizeY), 3, 3u);
        KernelMatrixProperty = EffectProperty.Add(typeof(ConvolveMatrixEffect), nameof(KernelMatrix), 4, new float[] { 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f });
        DivisorProperty = EffectProperty.Add(typeof(ConvolveMatrixEffect), nameof(Divisor), 5, 1f);
        BiasProperty = EffectProperty.Add(typeof(ConvolveMatrixEffect), nameof(Bias), 6, 0f);
        KernelOffsetProperty = EffectProperty.Add(typeof(ConvolveMatrixEffect), nameof(KernelOffset), 7, new D2D_VECTOR_2F());
        PreserveAlphaProperty = EffectProperty.Add(typeof(ConvolveMatrixEffect), nameof(PreserveAlpha), 8, false);
        BorderModeProperty = EffectProperty.Add(typeof(ConvolveMatrixEffect), nameof(BorderMode), 9, D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT);
        ClampOutputProperty = EffectProperty.Add(typeof(ConvolveMatrixEffect), nameof(ClampOutput), 10, false);
    }

    public D2D_VECTOR_2F KernelUnitLength { get => (D2D_VECTOR_2F)GetPropertyValue(KernelUnitLengthProperty)!; set => SetPropertyValue(KernelUnitLengthProperty, value); }
    public D2D1_CONVOLVEMATRIX_SCALE_MODE ScaleMode { get => (D2D1_CONVOLVEMATRIX_SCALE_MODE)GetPropertyValue(ScaleModeProperty)!; set => SetPropertyValue(ScaleModeProperty, value); }
    public uint KernelSizeX { get => (uint)GetPropertyValue(KernelSizeXProperty)!; set => SetPropertyValue(KernelSizeXProperty, value); }
    public uint KernelSizeY { get => (uint)GetPropertyValue(KernelSizeYProperty)!; set => SetPropertyValue(KernelSizeYProperty, value); }
    public float[] KernelMatrix { get => (float[])GetPropertyValue(KernelMatrixProperty)!; set => SetPropertyValue(KernelMatrixProperty, value); }
    public float Divisor { get => (float)GetPropertyValue(DivisorProperty)!; set => SetPropertyValue(DivisorProperty, value); }
    public float Bias { get => (float)GetPropertyValue(BiasProperty)!; set => SetPropertyValue(BiasProperty, value); }
    public D2D_VECTOR_2F KernelOffset { get => (D2D_VECTOR_2F)GetPropertyValue(KernelOffsetProperty)!; set => SetPropertyValue(KernelOffsetProperty, value); }
    public bool PreserveAlpha { get => (bool)GetPropertyValue(PreserveAlphaProperty)!; set => SetPropertyValue(PreserveAlphaProperty, value); }
    public D2D1_BORDER_MODE BorderMode { get => (D2D1_BORDER_MODE)GetPropertyValue(BorderModeProperty)!; set => SetPropertyValue(BorderModeProperty, value); }
    public bool ClampOutput { get => (bool)GetPropertyValue(ClampOutputProperty)!; set => SetPropertyValue(ClampOutputProperty, value); }
}
