namespace Wice;

public class PlacementParameters
{
    public PlacementParameters(Visual visual)
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(ExceptionExtensions));
        Visual = visual;
        UseRounding = true;
    }

    public Visual Visual { get; }
    public virtual bool UseRounding { get; set; }
    public virtual bool UseScreenCoordinates { get; set; }
    public virtual Visual? Target { get; set; }
    public virtual PlacementMode Mode { get; set; }
    public virtual float HorizontalOffset { get; set; }
    public virtual float VerticalOffset { get; set; }
    public virtual Func<PlacementParameters, D2D_POINT_2F>? CustomFunc { get; set; }

    protected virtual void CopyTo(PlacementParameters parameters)
    {
        ExceptionExtensions.ThrowIfNull(parameters, nameof(ExceptionExtensions));
        parameters.CustomFunc = CustomFunc;
        parameters.HorizontalOffset = HorizontalOffset;
        parameters.Mode = Mode;
        parameters.Target = Target;
        parameters.UseRounding = UseRounding;
        parameters.UseScreenCoordinates = UseScreenCoordinates;
        parameters.VerticalOffset = VerticalOffset;
    }

    public virtual PlacementParameters Clone()
    {
        var clone = new PlacementParameters(Visual);
        CopyTo(clone);
        return clone;
    }
}
