using System;
using System.Numerics;
using DirectN;

namespace Wice.Samples.Gallery
{
    public class RoundedButton : Button
    {
        private static _D3DCOLORVALUE _buttonColor;
        private static _D3DCOLORVALUE _buttonShadowColor;

        private readonly RoundedRectangle _rr = new RoundedRectangle();
        private readonly Canvas _canvas = new Canvas();

        static RoundedButton()
        {
            _buttonColor = new _D3DCOLORVALUE(0xFF0078D7);
            _buttonShadowColor = _buttonColor.ChangeAlpha(0x7F);
        }

        public RoundedButton()
        {
            var child = Child; // by default, it's a Dock

            HorizontalAlignment = Alignment.Near;
            VerticalAlignment = Alignment.Near;
            child.HorizontalAlignment = Alignment.Center;
            child.VerticalAlignment = Alignment.Center;

            _rr.CornerRadius = new Vector2(Application.CurrentTheme.RoundedButtonCornerRadius);
            child.Remove(false);
            ClipFromParent = false; // for shadow

            _canvas.Children.Add(_rr);
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

            _rr.RenderBrush = Compositor.CreateColorBrush(_buttonColor);
            RenderBrush = null;
            Text.ForegroundBrush = new SolidColorBrush(_D3DCOLORVALUE.White);
            Icon.ForegroundBrush = Text.ForegroundBrush;

            if (IsEnabled)
            {
                var shadow = Compositor.CreateDropShadow();
                shadow.BlurRadius = 8;
                shadow.Color = _buttonShadowColor;
                shadow.Offset = new Vector3(0, 3, 0);
                _canvas.RenderShadow = shadow;
            }
        }
    }
}
