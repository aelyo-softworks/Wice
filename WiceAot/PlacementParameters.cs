namespace Wice;

/// <summary>
/// Describes the parameters used to compute the placement of a popup-like visual relative to a source <see cref="Visual"/>.
/// </summary>
public class PlacementParameters
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlacementParameters"/> class.
    /// </summary>
    /// <param name="visual">The source visual from which placement should be computed.</param>
    public PlacementParameters(Visual visual)
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));
        Visual = visual;
        UseRounding = true;
    }

    /// <summary>
    /// Gets the source visual used as the reference for placement computations.
    /// </summary>
    public Visual Visual { get; }

    /// <summary>
    /// Gets or sets a value indicating whether computed coordinates should be rounded to the nearest whole pixel.
    /// </summary>
    public virtual bool UseRounding { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the resulting placement coordinates are expressed in screen space.
    /// </summary>
    public virtual bool UseScreenCoordinates { get; set; }

    /// <summary>
    /// Gets or sets the target visual to position relative to, when different from <see cref="Visual"/>.
    /// </summary>
    public virtual Visual? Target { get; set; }

    /// <summary>
    /// Gets or sets the placement mode describing how the target is positioned relative to the anchor.
    /// </summary>
    public virtual PlacementMode Mode { get; set; }

    /// <summary>
    /// Gets or sets an additional horizontal offset applied to the computed placement point.
    /// </summary>
    public virtual float HorizontalOffset { get; set; }

    /// <summary>
    /// Gets or sets an additional vertical offset applied to the computed placement point.
    /// </summary>
    public virtual float VerticalOffset { get; set; }

    /// <summary>
    /// Gets or sets a custom placement function that can compute the final placement point.
    /// </summary>
    public virtual Func<PlacementParameters, D2D_POINT_2F>? CustomFunc { get; set; }

    /// <summary>
    /// Copies the current parameter values into another <see cref="PlacementParameters"/> instance.
    /// </summary>
    /// <param name="parameters">The destination parameters object to copy into.</param>
    protected virtual void CopyTo(PlacementParameters parameters)
    {
        ExceptionExtensions.ThrowIfNull(parameters, nameof(parameters));
        parameters.UseRounding = UseRounding;
        parameters.UseScreenCoordinates = UseScreenCoordinates;
        parameters.Target = Target;
        parameters.Mode = Mode;
        parameters.HorizontalOffset = HorizontalOffset;
        parameters.VerticalOffset = VerticalOffset;
        parameters.CustomFunc = CustomFunc;
    }

    /// <summary>
    /// Creates a new <see cref="PlacementParameters"/> instance with the same values as this one.
    /// </summary>
    /// <returns>A cloned instance whose values match the current object.</returns>
    public virtual PlacementParameters Clone()
    {
        var clone = new PlacementParameters(Visual);
        CopyTo(clone);
        return clone;
    }
}
