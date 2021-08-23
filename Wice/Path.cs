using System.ComponentModel;
using Windows.Graphics;
using Windows.UI.Composition;

namespace Wice
{
    public class Path : SingleShape
    {
        public static VisualProperty GeometrySource2DProperty = VisualProperty.Add<IGeometrySource2D>(typeof(Path), nameof(GeometrySource2D), VisualPropertyInvalidateModes.Render, convert: SetGeo);

        private static object SetGeo(BaseObject obj, object value)
        {
            var path = (Path)obj;
            var geo = (IGeometrySource2D)value;
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

        [Category(CategoryRender)]
        public IGeometrySource2D GeometrySource2D { get => (IGeometrySource2D)GetPropertyValue(GeometrySource2DProperty); set => SetPropertyValue(GeometrySource2DProperty, value); }

        [Category(CategoryRender)]
        public new CompositionPathGeometry Geometry => (CompositionPathGeometry)base.Geometry;

        protected override CompositionGeometry CreateGeometry() => Window?.Compositor?.CreatePathGeometry();
    }
}
