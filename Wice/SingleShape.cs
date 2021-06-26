using System;
using System.ComponentModel;
using Windows.UI.Composition;

namespace Wice
{
    public abstract class SingleShape : Shape
    {
        protected SingleShape()
        {
        }

        [Category(CategoryRender)]
        public CompositionGeometry Geometry { get; private set; }

        [Category(CategoryRender)]
        public CompositionSpriteShape Shape { get; private set; }

        protected abstract CompositionGeometry CreateGeometry();
        protected virtual CompositionSpriteShape CreateShape() => Window.Compositor.CreateSpriteShape(Geometry);

        protected override void OnAttachedToComposition(object sender, EventArgs e)
        {
            Geometry = CreateGeometry();
            if (Geometry == null)
                throw new InvalidOperationException();

#if DEBUG
            Geometry.Comment = Name;
#endif

            Shape = CreateShape();
            if (Shape == null)
                throw new InvalidOperationException();

#if DEBUG
            Shape.Comment = Name;
#endif
            CompositionVisual.Shapes.Add(Shape);
            base.OnAttachedToComposition(sender, e);
        }
    }
}
