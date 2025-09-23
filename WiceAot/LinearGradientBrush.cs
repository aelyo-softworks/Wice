namespace Wice;

/// <summary>
/// A brush that paints an area with a linear gradient using Direct2D.
/// </summary>
public class LinearGradientBrush : Brush
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LinearGradientBrush"/> class.
    /// </summary>
    /// <param name="properties">The Direct2D linear gradient brush properties (start/end points, opacity, etc.).</param>
    /// <param name="stops">The ordered collection of gradient stops that define the gradient.</param>
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
    protected internal override IComObject<ID2D1Brush> GetBrush(RenderContext context) => context.CreateLinearGradientBrush(Properties, Stops);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as LinearGradientBrush);

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
