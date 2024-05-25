namespace Wice
{
    public abstract class ScrollBar : Dock
    {
        public static VisualProperty ModeProperty { get; } = VisualProperty.Add(typeof(ScrollBar), nameof(Mode), VisualPropertyInvalidateModes.Measure, ScrollBarMode.Standard);

        public event EventHandler<EventArgs> SmallIncreaseClick;
        public event EventHandler<EventArgs> SmallDecreaseClick;
        public event EventHandler<EventArgs> LargeIncreaseClick;
        public event EventHandler<EventArgs> LargeDecreaseClick;

        protected ScrollBar()
        {
            SmallDecrease = CreateSmallDecrease();
            if (SmallDecrease == null)
                throw new InvalidOperationException();

            SmallDecrease.Click += OnSmallDecreaseClick;
            SmallDecrease.FocusIndex = 0;
            SmallDecrease.HandleOnClick = false;
            Children.Add(SmallDecrease);

            LargeDecrease = CreateLargeDecrease();
            if (LargeDecrease == null)
                throw new InvalidOperationException();

            LargeDecrease.Click += OnLargeDecreaseClick;
            LargeDecrease.FocusIndex = 1;
            LargeDecrease.HandleOnClick = false;
            Children.Add(LargeDecrease);
#if DEBUG
            LargeDecrease.Name = nameof(LargeDecrease);
#endif

            SmallIncrease = CreateSmallIncrease();
            if (SmallIncrease == null)
                throw new InvalidOperationException();

            SmallIncrease.Click += OnSmallIncreaseClick;
            SmallIncrease.FocusIndex = 4;
            SmallIncrease.HandleOnClick = false;
            Children.Add(SmallIncrease);

            LargeIncrease = CreateLargeIncrease();
            if (LargeIncrease == null)
                throw new InvalidOperationException();

            LargeIncrease.Click += OnLargeIncreaseClick;
            LargeIncrease.FocusIndex = 3;
            LargeIncrease.HandleOnClick = false;
            Children.Add(LargeIncrease);
#if DEBUG
            LargeIncrease.Name = nameof(LargeIncrease);
#endif

            Thumb = CreateThumb();
            if (Thumb == null)
                throw new InvalidOperationException();

            Thumb.FocusIndex = 2;
            Children.Add(Thumb);
#if DEBUG
            Thumb.Name = nameof(Thumb);
#endif
        }

        [Browsable(false)]
        public ButtonBase SmallDecrease { get; }

        [Browsable(false)]
        public ButtonBase LargeDecrease { get; }

        [Browsable(false)]
        public Thumb Thumb { get; }

        [Browsable(false)]
        public ButtonBase LargeIncrease { get; }

        [Browsable(false)]
        public ButtonBase SmallIncrease { get; }

        internal ScrollViewer View => Parent as ScrollViewer;
        internal bool IsOverlay => View?.ScrollMode == ScrollViewerMode.Overlay;

        protected abstract ScrollBarButton CreateSmallDecrease();
        protected abstract ButtonBase CreateLargeDecrease();
        protected abstract Thumb CreateThumb();
        protected abstract ButtonBase CreateLargeIncrease();
        protected abstract ScrollBarButton CreateSmallIncrease();

        protected virtual void OnSmallIncreaseClick(object? sender, EventArgs e) => SmallIncreaseClick?.Invoke(sender, e);
        protected virtual void OnSmallDecreaseClick(object? sender, EventArgs e) => SmallDecreaseClick?.Invoke(sender, e);
        protected virtual void OnLargeIncreaseClick(object? sender, EventArgs e) => LargeIncreaseClick?.Invoke(sender, e);
        protected virtual void OnLargeDecreaseClick(object? sender, EventArgs e) => LargeDecreaseClick?.Invoke(sender, e);

        public ScrollBarMode Mode { get => (ScrollBarMode)GetPropertyValue(ModeProperty); set => SetPropertyValue(ModeProperty, value); }

        protected internal virtual void UpdateCorner(ScrollViewer view)
        {
            // should only be implemented by vsb
        }

        protected override void IsMouseOverChanged(bool newValue)
        {
            base.IsMouseOverChanged(newValue);
            if (IsOverlay)
            {
                // force redraw when overlay to show expansion
                Invalidate(VisualPropertyInvalidateModes.Render, new PropertyInvalidateReason(IsMouseOverProperty));
            }
        }

        protected override void OnRendered(object? sender, EventArgs e)
        {
            base.OnRendered(sender, e);
            var compositor = Window.Compositor;
            if (compositor == null)
                return;

            if (IsOverlay)
            {
                RenderBrush = compositor.CreateColorBrush(D3DCOLORVALUE.Transparent.ToColor());
                Thumb.RenderBrush = compositor.CreateColorBrush(Application.CurrentTheme.ScrollBarOverlayThumbColor.ToColor());
                Thumb.CornerRadius = new Vector2(Application.CurrentTheme.ScrollBarOverlayCornerRadius);
                SmallDecrease.IsVisible = false;
                SmallIncrease.IsVisible = false;
            }
            else
            {
                RenderBrush = compositor.CreateColorBrush(Application.CurrentTheme.ScrollBarBackgroundColor.ToColor());
                Thumb.RenderBrush = compositor.CreateColorBrush(Application.CurrentTheme.ScrollBarThumbColor.ToColor());
                Thumb.CornerRadius = new Vector2(0);
                SmallDecrease.IsVisible = true;
                SmallIncrease.IsVisible = true;
            }
        }
    }
}
