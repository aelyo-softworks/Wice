namespace Wice;

public class RadialGradientBrush : Brush
{
    public RadialGradientBrush(D2D1_RADIAL_GRADIENT_BRUSH_PROPERTIES properties, params D2D1_GRADIENT_STOP[] stops)
    {
        if (stops == null || stops.Length == 0)
            throw new ArgumentException(null, nameof(stops));

        Properties = properties;
        Stops = stops;
        Gamma = D2D1_GAMMA.D2D1_GAMMA_2_2;
        ExtendMode = D2D1_EXTEND_MODE.D2D1_EXTEND_MODE_CLAMP;
    }

    public D2D1_RADIAL_GRADIENT_BRUSH_PROPERTIES Properties { get; }
    public D2D1_GRADIENT_STOP[] Stops { get; }
    public D2D1_GAMMA Gamma { get; set; }
    public D2D1_EXTEND_MODE ExtendMode { get; set; }

    protected internal override IComObject<ID2D1Brush> GetBrush(RenderContext context) => context.CreateRadialGradientBrush(Properties, Gamma, ExtendMode, Stops);

    public override bool Equals(object? obj) => Equals(obj as LinearGradientBrush);
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
