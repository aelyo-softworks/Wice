using System;
using DirectN;
using Wice.Samples.Gallery.Samples;
using Wice.Samples.Gallery.Utilities;

namespace Wice.Samples.Gallery.Pages
{
    public class SampleVisual : Dock
    {
        public SampleVisual(Sample sample)
        {
            if (sample == null)
                throw new ArgumentNullException(nameof(sample));

            var sampleTb = new TextBox();
            sampleTb.FontSize = 20;
            sampleTb.Margin = D2D_RECT_F.Thickness(0, 0, 0, 10);
            sampleTb.Text = sample.Description;
            SetDockType(sampleTb, DockType.Top);
            Children.Add(sampleTb);

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
                sample.Layout(dock);

                var text = sample.GetSampleText();
                CodeBox code = null;
                if (text != null)
                {
                    code = new CodeBox();
                    code.Margin = D2D_RECT_F.Thickness(0, 10, 0, 0);
                    code.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.White);
                    code.Options |= TextHostOptions.WordWrap;
                    code.Padding = 5;
                    code.CodeLanguage = WiceLanguage.Default.Id; // init & put in repo
                    SetDockType(code, DockType.Top);
                    code.CodeText = text;
                    dock.Children.Add(code);
                }
            });
        }
    }
}
