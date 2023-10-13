using System;
using System.ComponentModel;
using DirectN;
using Wice.Utilities;

namespace Wice
{
    public class Button : ButtonBase
    {
        public Button()
        {
            IsFocusable = true;
            Child = CreatePanel();
            Cursor = Cursor.Hand;

            Icon = CreateIcon();
            if (Icon == null)
                throw new InvalidOperationException();

            Icon.PropertyChanged += OnIconPropertyChanged;
#if DEBUG
            Icon.Name = nameof(Icon);
#endif
            Child.Children.Add(Icon);

            // not stretch, for text
            Text = CreateText();
            if (Text == null)
                throw new InvalidOperationException();

            Text.PropertyChanged += OnTextPropertyChanged;
#if DEBUG
            Text.Name = nameof(Text);
#endif
            Child.Children.Add(Text);

            // to ensure button size is equal to content's size
            HorizontalAlignment = Alignment.Center;
            VerticalAlignment = Alignment.Center;
            //Child.HorizontalAlignment = Alignment.Center;
            //Child.VerticalAlignment = Alignment.Center;
        }

        [Browsable(false)]
        public TextBox Icon { get; }

        [Browsable(false)]
        public TextBox Text { get; }

        private void OnIconPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TextBox.TextProperty.Name)
            {
                UpdateMargins();
            }
        }

        private void OnTextPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TextBox.TextProperty.Name)
            {
                UpdateMargins();
            }
        }

        protected override void UpdateStyle()
        {
            base.UpdateStyle();
            var compositor = Compositor;
            if (compositor != null)
            {
                RenderBrush = compositor.CreateColorBrush(Application.CurrentTheme.ButtonColor.ToColor());
            }
        }

        protected virtual void UpdateMargins()
        {
            if (!string.IsNullOrEmpty(Icon.Text) && !string.IsNullOrEmpty(Text.Text))
            {
                Text.Margin = D2D_RECT_F.Thickness(0, Application.CurrentTheme.ButtonMargin, Application.CurrentTheme.ButtonMargin, Application.CurrentTheme.ButtonMargin);
                Icon.Margin = D2D_RECT_F.Thickness(Application.CurrentTheme.ButtonMargin, Application.CurrentTheme.ButtonMargin - 2, Application.CurrentTheme.ButtonMargin, Application.CurrentTheme.ButtonMargin);
            }
            else if (!string.IsNullOrEmpty(Text.Text))
            {
                Text.Margin = D2D_RECT_F.Thickness(Application.CurrentTheme.ButtonMargin, Application.CurrentTheme.ButtonMargin, Application.CurrentTheme.ButtonMargin, Application.CurrentTheme.ButtonMargin);
                Icon.Margin = new D2D_RECT_F();
            }
            else
            {
                Text.Margin = new D2D_RECT_F();
                Icon.Margin = new D2D_RECT_F();
            }
        }

        protected virtual Visual CreatePanel() => new Dock();
        protected virtual TextBox CreateIcon()
        {
            var tb = new TextBox();
            tb.FontFamilyName = Application.CurrentTheme.SymbolFontName;
            tb.ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER;
            tb.DrawOptions = D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT;
            return tb;
        }

        protected virtual TextBox CreateText()
        {
            var tb = new TextBox();
            tb.ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER;
            tb.DrawOptions = D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT;
            return tb;
        }
    }
}
