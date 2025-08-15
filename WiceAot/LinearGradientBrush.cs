namespace Wice;

/// <summary>
/// A brush that paints an area with a linear gradient using Direct2D.
/// </summary>
/// <remarks>
/// Instances are immutable and compare equality by their Direct2D properties and the ordered list of gradient stops.
/// </remarks>
public class LinearGradientBrush : Brush
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LinearGradientBrush"/> class.
    /// </summary>
    /// <param name="properties">The Direct2D linear gradient brush properties (start/end points, opacity, etc.).</param>
    /// <param name="stops">The ordered collection of gradient stops that define the gradient.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="stops"/> is null or empty.</exception>
    public LinearGradientBrush(D2D1_LINEAR_GRADIENT_BRUSH_PROPERTIES properties, params D2D1_GRADIENT_STOP[] stops)
    {
        if (stops == null || stops.Length == 0)
            throw new ArgumentException(null, nameof(stops));

        Properties = properties;
        Stops = stops;
    }

    /// <summary>
    /// Gets the Direct2D-specific properties that define the linear gradient (e.g., start and end points).
    /// </summary>
    public D2D1_LINEAR_GRADIENT_BRUSH_PROPERTIES Properties { get; }

    /// <summary>
    /// Gets the ordered collection of gradient stops used by this brush.
    /// </summary>
    public D2D1_GRADIENT_STOP[] Stops { get; }

    /// <inheritdoc />
    /// <remarks>
    /// Creates an underlying Direct2D linear gradient brush bound to the supplied <paramref name="context"/>.
    /// </remarks>
    protected internal override IComObject<ID2D1Brush> GetBrush(RenderContext context) => context.CreateLinearGradientBrush(Properties, Stops);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as LinearGradientBrush);

    /// <summary>
    /// Determines whether this brush is equal to another brush.
    /// </summary>
    /// <param name="other">The other brush to compare against.</param>
    /// <returns>
    /// <see langword="true"/> when <paramref name="other"/> is a <see cref="LinearGradientBrush"/> with the same
    /// <see cref="Properties"/> and an identical ordered sequence of <see cref="Stops"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(Brush? other)
    {
        if (other is not LinearGradientBrush brush)
            return false;

        if (!Properties.Equals(brush.Properties))
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
    /// <remarks>
    /// Combines the hash codes of <see cref="Properties"/> and each stop in <see cref="Stops"/> in order.
    /// </remarks>
    public override int GetHashCode()
    {
        var code = Properties.GetHashCode();
        foreach (var stop in Stops)
        {
            code ^= stop.GetHashCode();
        }
        return code;
    }
}
