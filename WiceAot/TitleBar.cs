namespace Wice
{
    public partial class TitleBar : Dock
    {
        private bool _isMain;

        public event EventHandler? Updated;

        public TitleBar()
        {
            Canvas.SetTop(this, 0);
            SetDockType(this, DockType.Top);
            CloseButton = CreateCloseButton();
            if (CloseButton != null)
            {
#if DEBUG
                CloseButton.Name = nameof(CloseButton);
#endif
                SetDockType(CloseButton, DockType.Right);
                Children.Add(CloseButton);
            }

            MaxButton = CreateMaxButton();
            if (MaxButton != null)
            {
#if DEBUG
                MaxButton.Name = nameof(MaxButton);
#endif

                SetDockType(MaxButton, DockType.Right);
                Children.Add(MaxButton);
            }

            MinButton = CreateMinButton();
            if (MinButton != null)
            {
#if DEBUG
                MinButton.Name = nameof(MinButton);
#endif

                SetDockType(MinButton, DockType.Right);
                Children.Add(MinButton);
            }

            Title = CreateTitle();
            if (Title != null)
            {
                Title.Margin = D2D_RECT_F.Thickness(10, 0, 0, 0);
                Title.FontSize = 12;
                Title.ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER;
#if DEBUG
                Title.Name = nameof(Title);
#endif
                SetDockType(Title, DockType.Left);
                Children.Add(Title);

                TitlePadding = CreatePadding();
                if (TitlePadding != null)
                {

#if DEBUG
                    TitlePadding.Name = nameof(TitlePadding);
#endif
                    SetDockType(TitlePadding, DockType.Left);
                    Children.Add(TitlePadding);
                }
            }
        }

        [Browsable(false)]
        public TextBox? Title { get; }

        [Browsable(false)]
        public Visual? TitlePadding { get; } // so we can say Title.IsVisible = false

        [Browsable(false)]
        public Visual? MinButton { get; }

        [Browsable(false)]
        public Visual? MaxButton { get; }

        [Browsable(false)]
        public Visual? CloseButton { get; }

        [Category(CategoryBehavior)]
        public virtual bool IsMain
        {
            get => _isMain;
            set
            {
                if (_isMain == value)
                    return;

                _isMain = value;
                SetWindowMain();
            }
        }

        private void SetWindowMain()
        {
            var window = Window;
            if (window == null)
                return;

            if (_isMain)
            {
                window.MainTitleBar = this;
                window.Style |= WINDOW_STYLE.WS_THICKFRAME | WINDOW_STYLE.WS_CAPTION | WINDOW_STYLE.WS_SYSMENU | WINDOW_STYLE.WS_MAXIMIZEBOX | WINDOW_STYLE.WS_MINIMIZEBOX;
            }
            else
            {
                if (window.MainTitleBar == this)
                {
                    window.MainTitleBar = null;
                }
            }
        }

        protected virtual TextBox CreateTitle() => new();
        protected virtual Visual CreatePadding() => new();
        protected virtual Visual CreateMinButton() => new TitleBarButton { ButtonType = TitleBarButtonType.Minimize };
        protected virtual Visual CreateMaxButton() => new TitleBarButton { ButtonType = TitleBarButtonType.Maximize };
        protected virtual Visual CreateCloseButton() => new TitleBarButton { ButtonType = TitleBarButtonType.Close };

        protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
        {
            var window = Window;
            if (window != null)
            {
                var buttonSize = TitleBarButton.GetDpiAdjustedCaptionButtonSize(window);
                Height = buttonSize.cy;
            }

            var size = base.MeasureCore(constraint);
            return size;
        }

        protected virtual void OnUpdated(object sender, EventArgs e) => Updated?.Invoke(sender, e);

        protected virtual internal void Update()
        {
            var window = Window;
            if (window == null || Compositor == null)
                return;

            var buttonSize = TitleBarButton.GetDpiAdjustedCaptionButtonSize(window);
            Height = buttonSize.cy;

            if (CloseButton != null)
            {
                CloseButton.Height = buttonSize.cy;
                CloseButton.Width = buttonSize.cx;
                CloseButton.HoverRenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Red.ToColor());
            }

            if (MinButton != null)
            {
                MinButton.Height = buttonSize.cy;
                MinButton.Width = buttonSize.cx;
                MinButton.HoverRenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.LightGray.ToColor());
            }

            var zoomed = false;
            if (Title != null)
            {
                if (Parent is ITitleBarParent tbp)
                {
                    Title.Text = tbp.Title;
                    zoomed = tbp.IsZoomed;
                }
                else if (IsMain)
                {
                    Title.Text = window.Title;
                    zoomed = window.IsZoomed;
                }
            }

            if (MaxButton != null)
            {
                MaxButton.Height = buttonSize.cy;
                MaxButton.Width = buttonSize.cx;
                if (MaxButton is TitleBarButton tbb)
                {
                    tbb.ButtonType = zoomed ? TitleBarButtonType.Restore : TitleBarButtonType.Maximize;
                }
                MaxButton.HoverRenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.LightGray.ToColor());
            }

            OnUpdated(this, EventArgs.Empty);
        }

        protected override void OnAttachedToComposition(object? sender, EventArgs e)
        {
            base.OnAttachedToComposition(sender, e);
            Update();
            SetWindowMain();
        }
    }
}
