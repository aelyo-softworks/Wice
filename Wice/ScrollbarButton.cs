using System;
using System.ComponentModel;
using Wice.Utilities;
using Windows.UI.Composition;

namespace Wice
{
    public class ScrollBarButton : ButtonBase
    {
        public static VisualProperty IsArrowOpenProperty = VisualProperty.Add(typeof(Visual), nameof(IsArrowOpen), VisualPropertyInvalidateModes.Render, true);
        public static VisualProperty ArrowRatioProperty = VisualProperty.Add(typeof(Visual), nameof(ArrowRatio), VisualPropertyInvalidateModes.Render, float.NaN);

        public ScrollBarButton(DockType type)
        {
#if DEBUG
            Name = nameof(ScrollBarButton) + type;
#endif

            Child = new Path();
#if DEBUG
            Child.Name = Name + nameof(Path);
#endif
            Dock.SetDockType(this, type);
            Child.Margin = Application.CurrentTheme.ScrollBarArrowMargin; // TODO: vary per scrollbar width/height?
        }

        [Browsable(false)]
        public new virtual Path Child { get => (Path)base.Child; set => base.Child = value; }

        [Category(CategoryBehavior)]
        public bool IsArrowOpen { get => (bool)GetPropertyValue(IsArrowOpenProperty); set => SetPropertyValue(IsArrowOpenProperty, value); }

        [Category(CategoryBehavior)]
        public float ArrowRatio { get => (float)GetPropertyValue(ArrowRatioProperty); set => SetPropertyValue(ArrowRatioProperty, value); }

        protected override void OnArranged(object sender, EventArgs e)
        {
            base.OnArranged(sender, e);

            var size = (Child.ArrangedRect - Child.Margin).Size;

            var open = IsArrowOpen;
            var type = Dock.GetDockType(this);
            var geoSource = Application.Current.ResourceManager.GetScrollBarButtonGeometrySource(type, size.width, ArrowRatio, open);
            Child.GeometrySource2D = geoSource.GetIGeometrySource2();
        }

        protected override void OnAttachedToComposition(object sender, EventArgs e)
        {
            base.OnAttachedToComposition(sender, e);
            Child.Shape.StrokeThickness = Application.CurrentTheme.ScrollBarButtonStrokeThickness;
            Child.StrokeBrush = Compositor.CreateColorBrush(Application.CurrentTheme.ScrollBarButtonStrokeColor.ToColor());
            Child.RenderBrush = Child.StrokeBrush;
        }
    }
}
