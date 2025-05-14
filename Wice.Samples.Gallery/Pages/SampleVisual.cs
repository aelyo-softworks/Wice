using System;
using DirectN;
using Wice.Samples.Gallery.Samples;
using Wice.Samples.Gallery.Utilities;
using Wice.Utilities;

namespace Wice.Samples.Gallery.Pages
{
    public class SampleVisual : Dock
    {
        public SampleVisual(Sample sample)
        {
            if (sample == null)
                throw new ArgumentNullException(nameof(sample));

            var desc = sample.Description.Nullify();
            if (desc != null)
            {
                var sampleTb = new TextBox();
                sampleTb.FontSize = 20;
                sampleTb.Margin = D2D_RECT_F.Thickness(0, 0, 0, 10);
                sampleTb.Text = sample.Description;
                SetDockType(sampleTb, DockType.Top);
                Children.Add(sampleTb);
            }

            var sampleBorder = new Border();
            sampleBorder.BorderThickness = 1;
            sampleBorder.Padding = 10;
            sampleBorder.BorderBrush = new SolidColorBrush(_D3DCOLORVALUE.LightGray);
            SetDockType(sampleBorder, DockType.Top);
            Children.Add(sampleBorder);

            var dock = new Dock();
            sampleBorder.Children.Add(dock);

            DoWhenAttachedToComposition(() =>
            {
                sample.Compositor = Compositor;
                sample.Window = Window;
                sample.Layout(dock);

                var text = sample.GetSampleText();
                if (text != null)
                {
                    var codeBox = new CodeBox();
                    codeBox.Margin = D2D_RECT_F.Thickness(0, 10, 0, 0);
                    codeBox.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.White.ToColor());
                    codeBox.Options |= TextHostOptions.WordWrap;
                    codeBox.Padding = 5;
                    codeBox.CodeLanguage = WiceLanguage.Default.Id; // init & put in repo
                    SetDockType(codeBox, DockType.Top);
                    codeBox.CodeText = text;
                    dock.Children.Add(codeBox);
                }
            });
        }
    }
}
