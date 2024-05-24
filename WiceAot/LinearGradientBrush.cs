namespace Wice;

public class LinearGradientBrush : Brush
{
    public LinearGradientBrush(D2D1_LINEAR_GRADIENT_BRUSH_PROPERTIES properties, params D2D1_GRADIENT_STOP[] stops)
    {
        if (stops.IsEmpty())
            throw new ArgumentException(null, nameof(stops));

        Properties = properties;
        Stops = stops;
    }

    public D2D1_LINEAR_GRADIENT_BRUSH_PROPERTIES Properties { get; }
    public D2D1_GRADIENT_STOP[] Stops { get; }

    protected internal override IComObject<ID2D1Brush> GetBrush(RenderContext context) => context.CreateLinearGradientBrush(Properties, Stops);

    public override bool Equals(object? obj) => Equals(obj as LinearGradientBrush);
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
