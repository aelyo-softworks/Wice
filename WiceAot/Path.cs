namespace Wice;

/// <summary>
/// Vector path shape backed by a <see cref="CompositionPathGeometry"/>.
/// </summary>
public partial class Path : SingleShape
{
    /// <summary>
    /// Dynamic property descriptor for <see cref="GeometrySource2D"/>.
    /// </summary>
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
    [Category(CategoryRender)]
    public IGeometrySource2D GeometrySource2D { get => (IGeometrySource2D)GetPropertyValue(GeometrySource2DProperty)!; set => SetPropertyValue(GeometrySource2DProperty, value); }

    /// <summary>
    /// Gets the strongly typed composition geometry for this shape.
    /// </summary>
    [Category(CategoryRender)]
    public new CompositionPathGeometry? Geometry => (CompositionPathGeometry?)base.Geometry;

    /// <inheritdoc/>
    protected override CompositionGeometry? CreateGeometry() => Window?.Compositor?.CreatePathGeometry();
}
