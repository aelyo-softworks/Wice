namespace Wice;

/// <summary>
/// Represents a radial gradient brush that can be materialized for a given <see cref="RenderContext"/>.
/// </summary>
/// <remarks>
/// This brush encapsulates the properties and gradient stops required to create a Direct2D
/// radial gradient brush (for example, an <c>ID2D1RadialGradientBrush</c>) via
/// <see cref="RenderContext.CreateRadialGradientBrush(D2D1_RADIAL_GRADIENT_BRUSH_PROPERTIES, D2D1_GAMMA, D2D1_EXTEND_MODE, D2D1_GRADIENT_STOP[])"/>.
/// </remarks>
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
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="stops"/> is <see langword="null"/> or empty.
    /// </exception>
    /// <remarks>
    /// The default interpolation <see cref="Gamma"/> is <see cref="D2D1_GAMMA.D2D1_GAMMA_2_2"/> and
    /// the default <see cref="ExtendMode"/> is <see cref="D2D1_EXTEND_MODE.D2D1_EXTEND_MODE_CLAMP"/>.
    /// </remarks>
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
    /// <remarks>
    /// The rendering backend will use these stops to build a gradient stop collection.
    /// </remarks>
    public D2D1_GRADIENT_STOP[] Stops { get; }

    /// <summary>
    /// Gets or sets the gamma space used for color interpolation between gradient stops.
    /// </summary>
    /// <remarks>Defaults to <see cref="D2D1_GAMMA.D2D1_GAMMA_2_2"/>.</remarks>
    public D2D1_GAMMA Gamma { get; set; }

    /// <summary>
    /// Gets or sets the extend mode that defines how the gradient is drawn outside the [0,1] range.
    /// </summary>
    /// <remarks>Defaults to <see cref="D2D1_EXTEND_MODE.D2D1_EXTEND_MODE_CLAMP"/>.</remarks>
    public D2D1_EXTEND_MODE ExtendMode { get; set; }

    /// <inheritdoc />
    /// <remarks>
    /// Materializes a Direct2D radial gradient brush from the stored properties and gradient stops.
    /// </remarks>
    protected internal override IComObject<ID2D1Brush> GetBrush(RenderContext context) => context.CreateRadialGradientBrush(Properties, Gamma, ExtendMode, Stops);

    /// <summary>
    /// Determines whether the specified object is equal to the current brush instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current brush.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is a <see cref="Brush"/> and
    /// is equal to this instance; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj) => Equals(obj as LinearGradientBrush);

    /// <summary>
    /// Determines whether the specified brush is equal to the current brush instance.
    /// </summary>
    /// <param name="other">The other brush to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if the type, <see cref="Properties"/>, <see cref="Gamma"/>,
    /// <see cref="ExtendMode"/>, and all <see cref="Stops"/> are equal; otherwise, <see langword="false"/>.
    /// </returns>
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

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code that reflects the values of <see cref="Properties"/>, <see cref="Gamma"/>,
    /// <see cref="ExtendMode"/>, and all <see cref="Stops"/>.
    /// </returns>
    /// <remarks>
    /// Equal instances (as defined by <see cref="Equals(Brush?)"/>) will return the same hash code.
    /// </remarks>
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
