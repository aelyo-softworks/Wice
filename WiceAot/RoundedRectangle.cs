namespace Wice;

/// <summary>
/// Vector shape that renders a rounded rectangle using Windows Composition.
/// </summary>
/// <remarks>
/// - Creates a <see cref="CompositionRoundedRectangleGeometry"/> on demand.
/// - Updates geometry during <see cref="Render"/> based on the arranged rect and <see cref="Visual.Margin"/>.
/// </remarks>
public partial class RoundedRectangle : SingleShape
{
    /// <summary>
    /// Dynamic property descriptor for <see cref="CornerRadius"/>.
    /// </summary>
    /// <remarks>
    /// - Invalidation: <see cref="VisualPropertyInvalidateModes.Render"/>.
    /// - Default: <see cref="Vector2"/> (0, 0).
    /// - Validation: <c>ValidateEmptyVector2</c>.
    /// </remarks>
    public static VisualProperty CornerRadiusProperty { get; } =
        VisualProperty.Add(typeof(RoundedRectangle), nameof(CornerRadius), VisualPropertyInvalidateModes.Render, new Vector2(), ValidateEmptyVector2);

    /// <summary>
    /// Gets or sets the corner radii in DIPs for the rounded rectangle.
    /// X applies to the X radius; Y applies to the Y radius.
    /// </summary>
    [Category(CategoryRender)]
    public Vector2 CornerRadius
    {
        get => (Vector2)GetPropertyValue(CornerRadiusProperty)!;
        set => SetPropertyValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets the typed composition geometry for this shape if created.
    /// </summary>
    public new CompositionRoundedRectangleGeometry? Geometry => (CompositionRoundedRectangleGeometry?)base.Geometry;

    /// <summary>
    /// Creates the underlying <see cref="CompositionRoundedRectangleGeometry"/> using the window compositor.
    /// </summary>
    /// <returns>A new geometry instance, or <c>null</c> when the window/compositor is not available.</returns>
    protected override CompositionGeometry? CreateGeometry() => Window?.Compositor?.CreateRoundedRectangleGeometry();

    /// <summary>
    /// Updates the composition geometry to reflect the current arranged bounds and margin.
    /// </summary>
    /// <remarks>
    /// - Size = (ArrangedRect.Size - Margin).ToVector2()<br/>
    /// - Offset = (Margin.left, Margin.top)<br/>
    /// - CornerRadius = <see cref="CornerRadius"/>
    /// </remarks>
    protected override void Render()
    {
        base.Render();
        var ar = ArrangedRect;
        if (ar.IsValid && Geometry != null)
        {
            Geometry.Size = (ar.Size - Margin).ToVector2();
            Geometry.Offset = new Vector2(Margin.left, Margin.top);
            Geometry.CornerRadius = CornerRadius;
        }
    }
}
