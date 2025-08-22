namespace Wice;

/// <summary>
/// Vector path shape backed by a <see cref="CompositionPathGeometry"/>.
/// </summary>
/// <remarks>
/// - Setting <see cref="GeometrySource2D"/> updates the underlying <see cref="CompositionPathGeometry.Path"/> with
///   a new <see cref="CompositionPath"/> or clears it when set to <c>null</c>.
/// - Changes to <see cref="GeometrySource2D"/> invalidate rendering only (no layout).
/// </remarks>
/// <seealso cref="CompositionPathGeometry"/>
/// <seealso cref="IGeometrySource2D"/>
public partial class Path : SingleShape
{
    /// <summary>
    /// Dynamic property descriptor for <see cref="GeometrySource2D"/>.
    /// </summary>
    /// <remarks>
    /// - Invalidation: <see cref="VisualPropertyInvalidateModes.Render"/>.
    /// - Conversion: delegates to <see cref="SetGeo(BaseObject, object?)"/> to apply the value to the backing geometry.
    /// </remarks>
    public static VisualProperty GeometrySource2DProperty { get; } = VisualProperty.Add<IGeometrySource2D>(typeof(Path), nameof(GeometrySource2D), VisualPropertyInvalidateModes.Render, convert: SetGeo);

    private static object? SetGeo(BaseObject obj, object? value)
    {
        var path = (Path)obj;
        var geo = (IGeometrySource2D)value!;
        if (path.Geometry != null)
        {
            if (geo == null)
            {
                path.Geometry.Path = null;
            }
            else
            {
                path.Geometry.Path = new CompositionPath(geo);
            }
        }
        return value;
    }

    /// <summary>
    /// Gets or sets the 2D geometry source used to populate the underlying composition path.
    /// </summary>
    /// <remarks>
    /// - When non-null and a <see cref="Geometry"/> exists, a new <see cref="CompositionPath"/> is created and assigned.
    /// - When set to <c>null</c>, the existing <see cref="CompositionPathGeometry.Path"/> is cleared.
    /// </remarks>
    [Category(CategoryRender)]
    public IGeometrySource2D GeometrySource2D { get => (IGeometrySource2D)GetPropertyValue(GeometrySource2DProperty)!; set => SetPropertyValue(GeometrySource2DProperty, value); }

    /// <summary>
    /// Gets the strongly typed composition geometry for this shape.
    /// </summary>
    /// <remarks>
    /// This shadows the base <see cref="SingleShape.Geometry"/> and returns a <see cref="CompositionPathGeometry"/>.
    /// </remarks>
    [Category(CategoryRender)]
    public new CompositionPathGeometry? Geometry => (CompositionPathGeometry?)base.Geometry;

    /// <summary>
    /// Creates a <see cref="CompositionPathGeometry"/> using the window's compositor.
    /// </summary>
    /// <returns>A new <see cref="CompositionPathGeometry"/> instance or <c>null</c> when no compositor is available.</returns>
    protected override CompositionGeometry? CreateGeometry() => Window?.Compositor?.CreatePathGeometry();
}
