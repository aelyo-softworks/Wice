using System.ComponentModel;
using System.Numerics;
using Wice.Utilities;
using Windows.UI.Composition;

namespace Wice
{
    public class RoundedRectangle : SingleShape
    {
        public static VisualProperty CornerRadiusProperty = VisualProperty.Add(typeof(RoundedRectangle), nameof(CornerRadius), VisualPropertyInvalidateModes.Render, new Vector2());

        [Category(CategoryRender)]
        public Vector2 CornerRadius { get => (Vector2)GetPropertyValue(CornerRadiusProperty); set => SetPropertyValue(CornerRadiusProperty, value); }

        public new CompositionRoundedRectangleGeometry Geometry => (CompositionRoundedRectangleGeometry)base.Geometry;

        protected override CompositionGeometry CreateGeometry() => Window?.Compositor?.CreateRoundedRectangleGeometry();

        protected override void Render()
        {
            base.Render();
            var ar = ArrangedRect;
            if (ar.IsValid)
            {
                Geometry.Size = Extensions.ToVector2(ar.Size - Margin);
                Geometry.Offset = new Vector2(Margin.left, Margin.top);
                Geometry.CornerRadius = CornerRadius;
            }
        }
    }
}
