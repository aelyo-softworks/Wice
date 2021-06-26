using System;

namespace Wice
{
    public class HorizontalScrollBar : ScrollBar
    {
        public HorizontalScrollBar()
        {
            Canvas.SetLeft(this, 0);
            Canvas.SetBottom(this, 0);
            SetDockType(this, DockType.Bottom);
            Height = Application.CurrentTheme.HorizontalScrollBarHeight;

            SmallDecrease.Width = Height; // square
            SetDockType(LargeDecrease, DockType.Left);
            SmallIncrease.Width = Height; // square
            SetDockType(LargeIncrease, DockType.Right);
            Thumb.Height = Height;
        }

        protected override ScrollBarButton CreateSmallDecrease() => new ScrollBarButton(DockType.Left);
        protected override ButtonBase CreateLargeDecrease() => new ButtonBase();
        protected override ScrollBarButton CreateSmallIncrease() => new ScrollBarButton(DockType.Right);
        protected override ButtonBase CreateLargeIncrease() => new ButtonBase();
        protected override Thumb CreateThumb() => new Thumb();

        protected override void OnRendered(object sender, EventArgs e)
        {
            base.OnRendered(sender, e);
            if (IsOverlay)
            {
                if (IsMouseOver)
                {
                    Thumb.Height = Height - 4; //keep an empty border on bottom
                }
                else
                {
                    Thumb.Height = Application.CurrentTheme.ScrollBarOverlaySize;
                }
            }
            else
            {
                Thumb.Height = Height;
            }
        }
    }
}
