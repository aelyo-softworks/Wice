using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice
{
    public partial class TitleBar : Dock
    {
        private bool _isMain;

        public TitleBar()
        {
            Canvas.SetTop(this, 0);
            SetDockType(this, DockType.Top);
            CloseButton = CreateCloseButton();
            if (CloseButton == null)
                throw new InvalidOperationException();

#if DEBUG
            CloseButton.Name = nameof(CloseButton);
#endif
            CloseButton.ButtonType = TitleBarButtonType.Close;
            SetDockType(CloseButton, DockType.Right);
            Children.Add(CloseButton);

            MaxButton = CreateMaxButton();
            if (MaxButton == null)
                throw new InvalidOperationException();

#if DEBUG
            MaxButton.Name = nameof(MaxButton);
#endif

            MaxButton.ButtonType = TitleBarButtonType.Maximize;
            SetDockType(MaxButton, DockType.Right);
            Children.Add(MaxButton);

            MinButton = CreateMinButton();
            if (MinButton == null)
                throw new InvalidOperationException();

#if DEBUG
            MinButton.Name = nameof(MinButton);
#endif

            MinButton.ButtonType = TitleBarButtonType.Minimize;
            SetDockType(MinButton, DockType.Right);
            Children.Add(MinButton);

            Title = CreateTitle();
            if (Title == null)
                throw new InvalidOperationException();

            Title.Margin = D2D_RECT_F.Thickness(10, 0, 0, 0);
            Title.FontSize = 12;
            Title.ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER;
#if DEBUG
            Title.Name = nameof(Title);
#endif
            SetDockType(Title, DockType.Left);
            Children.Add(Title);

            TitlePadding = CreatePadding();
            if (TitlePadding == null)
                throw new InvalidOperationException();

#if DEBUG
            TitlePadding.Name = nameof(TitlePadding);
#endif
            SetDockType(TitlePadding, DockType.Left);
            Children.Add(TitlePadding);
        }

        [Browsable(false)]
        public TextBox Title { get; }

        [Browsable(false)]
        public Visual TitlePadding { get; } // so we can say Title.IsVisible = false

        [Browsable(false)]
        public TitleBarButton MinButton { get; }

        [Browsable(false)]
        public TitleBarButton MaxButton { get; }

        [Browsable(false)]
        public TitleBarButton CloseButton { get; }

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
                window.Style |= WS.WS_THICKFRAME | WS.WS_CAPTION | WS.WS_SYSMENU | WS.WS_MAXIMIZEBOX | WS.WS_MINIMIZEBOX;
            }
            else
            {
                if (window.MainTitleBar == this)
                {
                    window.MainTitleBar = null;
                }
            }
        }

        protected virtual TextBox CreateTitle() => new TextBox();
        protected virtual Visual CreatePadding() => new Visual();
        protected virtual TitleBarButton CreateMinButton() => new TitleBarButton();
        protected virtual TitleBarButton CreateMaxButton() => new TitleBarButton();
        protected virtual TitleBarButton CreateCloseButton() => new TitleBarButton();

        protected virtual internal void Update()
        {
            var native = Window?.Native;
            if (native == null)
                return;

            var bounds = new tagRECT();
            WindowsFunctions.DwmGetWindowAttribute(native.Handle, DWMWINDOWATTRIBUTE.DWMWA_CAPTION_BUTTON_BOUNDS, ref bounds, Marshal.SizeOf<tagRECT>());
            Height = bounds.Height;

            // we have 3 buttons. not sure this is always ok...
            var width = (bounds.Width - 1) / 3;

            CloseButton.Height = bounds.Height;
            CloseButton.Width = width;
            CloseButton.HoverRenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Red);

            MinButton.Height = bounds.Height;
            MinButton.Width = width;
            MinButton.HoverRenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.LightGray);

            var zoomed = false;
            if (Parent is ITitleBarParent tbp)
            {
                Title.Text = tbp.Title;
                zoomed = tbp.IsZoomed;
            }
            else if (IsMain)
            {
                Title.Text = Window.Title;
                zoomed = Window.IsZoomed;
            }

            MaxButton.Height = bounds.Height;
            MaxButton.Width = width;
            MaxButton.ButtonType = zoomed ? TitleBarButtonType.Restore : TitleBarButtonType.Maximize;
            MaxButton.HoverRenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.LightGray);
        }

        protected override void OnAttachedToComposition(object sender, EventArgs e)
        {
            base.OnAttachedToComposition(sender, e);
            Update();
            SetWindowMain();
        }
    }
}
