using Windows.UI.Composition;

namespace Wice
{
    public class Rectangle : SingleShape
    {
        public new CompositionRectangleGeometry Geometry => (CompositionRectangleGeometry)base.Geometry;

        protected override CompositionGeometry CreateGeometry() => Window?.Compositor?.CreateRectangleGeometry();

        protected override void Render()
        {
            base.Render();
            var ar = ArrangedRect;
            if (ar.IsValid)
            {
                Geometry.Size = Utilities.Extensions.ToVector2(ar.Size - Margin);
            }
        }
    }
}
