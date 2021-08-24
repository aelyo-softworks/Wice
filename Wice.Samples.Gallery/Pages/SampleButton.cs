using System;
using DirectN;
using Wice.Samples.Gallery.Samples;

namespace Wice.Samples.Gallery.Pages
{
    // a visual button for a given sample
    public class SampleButton : ButtonBase
    {
        public SampleButton(SampleList sample)
        {
            if (sample == null)
                throw new ArgumentNullException(nameof(sample));

            Margin = D2D_RECT_F.Thickness(5);
            DoWhenAttachedToComposition(() => RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.LightGray));
            Width = 300;
            Height = 150;

            Child = new Dock();
            Child.Margin = D2D_RECT_F.Thickness(10);

            if (!string.IsNullOrEmpty(sample.IconText))
            {
                var icon = new TextBox();
                icon.Margin = D2D_RECT_F.Thickness(5);
                icon.Text = sample.IconText;
                icon.FontSize = 20;
                icon.FontFamilyName = Application.CurrentTheme.SymbolFontName;
                icon.VerticalAlignment = Alignment.Near;
                Dock.SetDockType(icon, DockType.Left);
                Child.Children.Add(icon);
            }

            var title = new TextBox();
            title.Margin = D2D_RECT_F.Thickness(5, 0, 5, 5);
            title.ForegroundBrush = new SolidColorBrush(GalleryWindow.ButtonColor);
            title.Text = sample.Title;
            title.FontSize = 20;
            Dock.SetDockType(title, DockType.Top);
            Child.Children.Add(title);

            if (!string.IsNullOrEmpty(sample.Header))
            {
                var desc = new TextBox();
                desc.Margin = D2D_RECT_F.Thickness(5);
                desc.FontWeight = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_SEMI_LIGHT;
                desc.WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WHOLE_WORD;
                desc.Text = sample.Header;
                desc.ToolTipContentCreator = tt => Window.CreateDefaultToolTipContent(tt, desc.Text);
                Dock.SetDockType(desc, DockType.Top);
                Child.Children.Add(desc);
            }
        }
    }
}
