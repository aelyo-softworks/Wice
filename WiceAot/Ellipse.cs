namespace Wice;

/// <summary>
/// A vector <see cref="Shape"/> that renders an ellipse centered within its arranged bounds.
/// </summary>
/// <remarks>
/// - Backed by a <see cref="CompositionEllipseGeometry"/> created from the current <see cref="Window"/> compositor.
/// - By default, the ellipse auto-fits inside its arranged rectangle (excluding <see cref="Visual.Margin"/>),
///   subtracting half the <see cref="Shape.StrokeThickness"/> so the stroke remains fully visible.
/// - You can override the auto-fit by setting an explicit <see cref="Radius"/>, and fine-tune using <see cref="RadiusOffset"/>.
/// - Rendering is skipped until composition is initialized and the arranged rectangle is valid.
/// </remarks>
/// <seealso cref="SingleShape"/>
/// <seealso cref="Shape"/>
public partial class Ellipse : SingleShape
{
    /// <summary>
    /// Dynamic property descriptor for <see cref="Radius"/>.
    /// </summary>
    /// <remarks>
    /// Changing this property invalidates rendering (<see cref="VisualPropertyInvalidateModes.Render"/>).
    /// The default value is <c>Vector2.Zero</c>, which triggers auto-fit behavior; see <see cref="Radius"/>.
    /// </remarks>
    public static VisualProperty RadiusProperty { get; } = VisualProperty.Add(typeof(Ellipse), nameof(Radius), VisualPropertyInvalidateModes.Render, new Vector2(), ValidateEmptyVector2);

    /// <summary>
    /// Dynamic property descriptor for <see cref="RadiusOffset"/>.
    /// </summary>
    /// <remarks>
    /// Changing this property invalidates rendering (<see cref="VisualPropertyInvalidateModes.Render"/>).
    /// The default value is <c>Vector2.Zero</c>.
    /// </remarks>
    public static VisualProperty RadiusOffsetProperty { get; } = VisualProperty.Add(typeof(Ellipse), nameof(RadiusOffset), VisualPropertyInvalidateModes.Render, new Vector2(), ValidateEmptyVector2);

    /// <summary>
    /// Gets the underlying composition ellipse geometry for this shape, when available.
    /// </summary>
    /// <remarks>
    /// This value is set when the shape has an active <see cref="Window"/> and its <see cref="Compositor"/> can create
    /// a <see cref="CompositionEllipseGeometry"/>. It is <c>null</c> before composition is initialized or if composition is suspended.
    /// </remarks>
    public new CompositionEllipseGeometry? Geometry => (CompositionEllipseGeometry?)base.Geometry;

    /// <summary>
    /// Gets or sets the ellipse radius along the X and Y axes, in DIPs.
    /// </summary>
    /// <remarks>
    /// - When both components are <c>0</c> (the default), the radius is automatically computed to fit inside the
    ///   arranged bounds, centered within the available size (excluding <see cref="Visual.Margin"/>), and reduced by
    ///   half the <see cref="Shape.StrokeThickness"/> so the stroke remains fully inside the container.
    /// - When non-zero, the provided values are used verbatim (subject to clamping to <c>&gt;= 0</c> in <see cref="Render"/>).
    /// - The final radius applied to the geometry is <c>Radius + <see cref="RadiusOffset"/></c>.
    /// </remarks>
    [Category(CategoryLayout)]
    public Vector2 Radius { get => (Vector2)GetPropertyValue(RadiusProperty)!; set => SetPropertyValue(RadiusProperty, value); }

    /// <summary>
    /// Gets or sets the additive offset applied to the computed <see cref="Radius"/>.
    /// </summary>
    /// <remarks>
    /// This is useful when sizing relative to the parent or to fine-tune the auto-fit radius. The final radius used
    /// by the geometry is clamped to be non-negative in each dimension. Positive values expand the ellipse; negative
    /// values shrink it (down to zero).
    /// </remarks>
    public Vector2 RadiusOffset { get => (Vector2)GetPropertyValue(RadiusOffsetProperty)!; set => SetPropertyValue(RadiusOffsetProperty, value); }

    /// <summary>
    /// Creates the composition geometry for the ellipse using the current window's compositor.
    /// </summary>
    /// <returns>
    /// A new <see cref="CompositionEllipseGeometry"/> when the compositor is available; otherwise <c>null</c>.
    /// </returns>
    protected override CompositionGeometry? CreateGeometry() => Window?.Compositor?.CreateEllipseGeometry();

    /// <summary>
    /// Updates the composition geometry to reflect the current layout and stroke settings.
    /// </summary>
    /// <remarks>
    /// - Calls <see cref="Visual.Render()"/> to update common composition state.
    /// - Computes the geometry center as the midpoint of the arranged size excluding <see cref="Visual.Margin"/>.
    /// - Applies the radius:
    ///   - If <see cref="Radius"/> is <c>(0,0)</c>, auto-fits inside the container and subtracts half the
    ///     <see cref="Shape.StrokeThickness"/> so the stroke is fully contained; then applies <see cref="RadiusOffset"/>.
    ///   - Otherwise uses <see cref="Radius"/> plus <see cref="RadiusOffset"/>.
    /// - Each radius component is clamped to be non-negative.
    /// - No work is performed unless <see cref="Visual.ArrangedRect"/> is valid and <see cref="Geometry"/> is non-null.
    /// </remarks>
    protected override void Render()
    {
        base.Render();
        var ar = ArrangedRect;
        var radius = Radius;
        if (ar.IsValid && Geometry != null)
        {
            var size = ar.Size - Margin;
            Geometry.Center = new Vector2(size.width / 2, size.height / 2);
            if (radius.X == 0 && radius.Y == 0)
            {
                // make sure the circle is inside the container, so remove stroke's semi width
                Geometry.Radius = new Vector2(Math.Max(0, Geometry.Center.X - StrokeThickness / 2 + RadiusOffset.X), Math.Max(0, Geometry.Center.Y - StrokeThickness / 2 + RadiusOffset.Y));
            }
            else
            {
                Geometry.Radius = new Vector2(Math.Max(0, radius.X + RadiusOffset.X), Math.Max(0, radius.Y + RadiusOffset.Y));
            }
        }
    }
}
