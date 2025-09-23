namespace Wice;

/// <summary>
/// Represents a radial gradient brush that can be materialized for a given <see cref="RenderContext"/>.
/// </summary>
public class RadialGradientBrush : Brush
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RadialGradientBrush"/> class.
    /// </summary>
    /// <param name="properties">
    /// The radial gradient brush properties (center, gradient origin offset, radius X, radius Y).
    /// </param>
    /// <param name="stops">
    /// The gradient stops defining color and position along the gradient. Must contain at least one element.
    /// </param>
    public RadialGradientBrush(D2D1_RADIAL_GRADIENT_BRUSH_PROPERTIES properties, params D2D1_GRADIENT_STOP[] stops)
    {
        if (stops == null || stops.Length == 0)
            throw new ArgumentException(null, nameof(stops));

        Properties = properties;
        Stops = stops;
        Gamma = D2D1_GAMMA.D2D1_GAMMA_2_2;
        ExtendMode = D2D1_EXTEND_MODE.D2D1_EXTEND_MODE_CLAMP;
    }

    /// <summary>
    /// Gets the Direct2D radial gradient brush properties (center, gradient origin offset, radii).
    /// </summary>
    public D2D1_RADIAL_GRADIENT_BRUSH_PROPERTIES Properties { get; }

    /// <summary>
    /// Gets the gradient stops that define the colors and positions of the gradient.
    /// </summary>
    public D2D1_GRADIENT_STOP[] Stops { get; }

    /// <summary>
    /// Gets or sets the gamma space used for color interpolation between gradient stops.
    /// </summary>
    public D2D1_GAMMA Gamma { get; set; }

    /// <summary>
    /// Gets or sets the extend mode that defines how the gradient is drawn outside the [0,1] range.
    /// </summary>
    public D2D1_EXTEND_MODE ExtendMode { get; set; }

    /// <inheritdoc />
    protected internal override IComObject<ID2D1Brush> GetBrush(RenderContext context) => context.CreateRadialGradientBrush(Properties, Gamma, ExtendMode, Stops);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as LinearGradientBrush);

    /// <inheritdoc/>
    public override bool Equals(Brush? other)
    {
        if (other is not RadialGradientBrush brush)
            return false;

        if (!Properties.Equals(brush.Properties))
            return false;

        if (Gamma != brush.Gamma)
            return false;

        if (ExtendMode != brush.ExtendMode)
            return false;

        if (Stops.Length != brush.Stops.Length)
            return false;

        for (var i = 0; i < Stops.Length; i++)
        {
            if (!Stops[i].Equals(brush.Stops[i]))
                return false;
        }
        return true;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var code = Properties.GetHashCode() ^ Gamma.GetHashCode() ^ ExtendMode.GetHashCode();
        foreach (var stop in Stops)
        {
            code ^= stop.GetHashCode();
        }
        return code;
    }
}
