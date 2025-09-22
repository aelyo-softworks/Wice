namespace Wice.Effects;

/// <summary>
/// Represents an effect that displaces the pixels of an image based on the intensity of another image.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1DisplacementMapString)]
#else
[Guid(Constants.CLSID_D2D1DisplacementMapString)]
#endif
public partial class DisplacementMapEffect : EffectWithSource
{
    /// <summary>
    /// Gets the property that represents the scale effect applied to an object.
    /// </summary>
    public static EffectProperty ScaleProperty { get; }

    /// <summary>
    /// Gets the static property representing the selection of the X channel for an effect.
    /// </summary>
    public static EffectProperty XChannelSelectProperty { get; }

    /// <summary>
    /// Gets the effect property that specifies the Y channel selection for an operation.
    /// </summary>
    public static EffectProperty YChannelSelectProperty { get; }

    static DisplacementMapEffect()
    {
        ScaleProperty = EffectProperty.Add(typeof(DisplacementMapEffect), nameof(Scale), 0, 0f);
        XChannelSelectProperty = EffectProperty.Add(typeof(DisplacementMapEffect), nameof(XChannelSelect), 1, D2D1_CHANNEL_SELECTOR.D2D1_CHANNEL_SELECTOR_A);
        YChannelSelectProperty = EffectProperty.Add(typeof(DisplacementMapEffect), nameof(YChannelSelect), 2, D2D1_CHANNEL_SELECTOR.D2D1_CHANNEL_SELECTOR_A);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DisplacementMapEffect"/> class.
    /// </summary>
    public DisplacementMapEffect()
            : base(2)
    {
    }

    /// <summary>
    /// Gets or sets the scale factor applied to the object.
    /// </summary>
    public float Scale { get => (float)GetPropertyValue(ScaleProperty)!; set => SetPropertyValue(ScaleProperty, value); }

    /// <summary>
    /// Gets or sets the channel selector for the X-axis. 
    /// </summary>
    public D2D1_CHANNEL_SELECTOR XChannelSelect { get => (D2D1_CHANNEL_SELECTOR)GetPropertyValue(XChannelSelectProperty)!; set => SetPropertyValue(XChannelSelectProperty, value); }

    /// <summary>
    /// Gets or sets the channel selector for the Y channel of the effect.
    /// </summary>
    public D2D1_CHANNEL_SELECTOR YChannelSelect { get => (D2D1_CHANNEL_SELECTOR)GetPropertyValue(YChannelSelectProperty)!; set => SetPropertyValue(YChannelSelectProperty, value); }

    /// <summary>
    /// Gets or sets the displacement map used to modify the appearance of the effect.
    /// </summary>
    public IGraphicsEffectSource? Displacement { get => GetSource(1); set => SetSource(1, value); }
}
