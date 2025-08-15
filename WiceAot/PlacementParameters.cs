namespace Wice;

/// <summary>
/// Describes the parameters used to compute the placement of a popup-like visual relative to a source <see cref="Visual"/>.
/// </summary>
/// <remarks>
/// - <see cref="Visual"/> identifies the source visual the placement is computed from; it is required and immutable.
/// - When <see cref="CustomFunc"/> is set, it is expected to compute and return the placement point and typically
///   takes precedence over <see cref="Mode"/> and offsets, depending on the caller.
/// - <see cref="UseRounding"/> indicates whether computed coordinates should be rounded to whole pixels (enabled by default).
/// - <see cref="UseScreenCoordinates"/> communicates the desired coordinate space for the final point (screen vs. window/composition).
/// - <see cref="HorizontalOffset"/> and <see cref="VerticalOffset"/> are additional deltas applied after the base computation.
/// - Use <see cref="Clone"/> to duplicate an instance before mutating shared settings.
/// </remarks>
public class PlacementParameters
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlacementParameters"/> class.
    /// </summary>
    /// <param name="visual">The source visual from which placement should be computed.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="visual"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// Initializes <see cref="UseRounding"/> to <see langword="true"/> by default.
    /// </remarks>
    public PlacementParameters(Visual visual)
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));
        Visual = visual;
        UseRounding = true;
    }

    /// <summary>
    /// Gets the source visual used as the reference for placement computations.
    /// </summary>
    /// <remarks>
    /// This value is set at construction time and cannot be changed.
    /// </remarks>
    public Visual Visual { get; }

    /// <summary>
    /// Gets or sets a value indicating whether computed coordinates should be rounded to the nearest whole pixel.
    /// </summary>
    /// <remarks>
    /// Enabling rounding can help avoid sub-pixel rendering artifacts. Defaults to <see langword="true"/>.
    /// </remarks>
    public virtual bool UseRounding { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the resulting placement coordinates are expressed in screen space.
    /// </summary>
    /// <remarks>
    /// When <see langword="false"/>, coordinates are typically in window/composition space.
    /// The interpretation is honored by the placement logic that consumes these parameters.
    /// </remarks>
    public virtual bool UseScreenCoordinates { get; set; }

    /// <summary>
    /// Gets or sets the target visual to position relative to, when different from <see cref="Visual"/>.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, the source <see cref="Visual"/> is used as the anchor.
    /// </remarks>
    public virtual Visual? Target { get; set; }

    /// <summary>
    /// Gets or sets the placement mode describing how the target is positioned relative to the anchor.
    /// </summary>
    /// <remarks>
    /// The specific interpretation of <see cref="Mode"/> depends on the caller that performs the placement.
    /// </remarks>
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
    /// <remarks>
    /// When provided, the function receives this <see cref="PlacementParameters"/> instance and should return
    /// the desired point as a <see cref="D2D_POINT_2F"/>. The result is interpreted in the coordinate space
    /// denoted by <see cref="UseScreenCoordinates"/>.
    /// </remarks>
    public virtual Func<PlacementParameters, D2D_POINT_2F>? CustomFunc { get; set; }

    /// <summary>
    /// Copies the current parameter values into another <see cref="PlacementParameters"/> instance.
    /// </summary>
    /// <param name="parameters">The destination parameters object to copy into.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="parameters"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// The destination instance must have been created with a valid <see cref="Visual"/>. This method does not change
    /// the destination's <see cref="Visual"/> reference.
    /// </remarks>
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
