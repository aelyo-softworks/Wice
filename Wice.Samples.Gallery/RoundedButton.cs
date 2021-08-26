using System.Numerics;
using DirectN;

namespace Wice.Samples.Gallery
{
    public class RoundedButton : Button
    {
        private readonly RoundedRectangle _roundedRectangle = new RoundedRectangle();
        private readonly Canvas _canvas = new Canvas();

        public RoundedButton()
        {
            var child = Child; // by default, it's a Dock, replace by a Canvas

            HorizontalAlignment = Alignment.Near;
            VerticalAlignment = Alignment.Near;
            child.HorizontalAlignment = Alignment.Center;
            child.VerticalAlignment = Alignment.Center;

            _roundedRectangle.CornerRadius = new Vector2(Application.CurrentTheme.RoundedButtonCornerRadius);
            child.Remove(false);
            ClipFromParent = false; // for shadow

            _canvas.Children.Add(_roundedRectangle);
            _canvas.Children.Add(child);

            child.Arranged += (s, e) =>
            {
                var ds = child.ArrangedRect;
                Width = ds.Width;
                Height = ds.Height;
            };
            Child = _canvas;
        }

        protected override void UpdateStyle()
        {
            base.UpdateStyle();
            if (Compositor == null)
                return;

            _roundedRectangle.RenderBrush = Compositor.CreateColorBrush(GalleryWindow.ButtonColor);
            RenderBrush = null;
            Text.ForegroundBrush = new SolidColorBrush(_D3DCOLORVALUE.White);
            Icon.ForegroundBrush = Text.ForegroundBrush;

            if (IsEnabled)
            {
                var shadow = Compositor.CreateDropShadow();
                shadow.BlurRadius = 8;
                shadow.Color = GalleryWindow.ButtonShadowColor;
                shadow.Offset = new Vector3(0, 3, 0);
                _canvas.RenderShadow = shadow;
            }
        }
    }
}
